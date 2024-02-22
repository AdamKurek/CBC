using CBC.Server;
using CBC.Shared;
using Duende.IdentityServer.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

public class VideoChatHub : Hub
{
    private const string QueueUserKey = "U";

    private static readonly TalkersQueues users = new(18,60);
    static bool firstClient = true;
    public override async Task OnConnectedAsync()
    {
        if (firstClient)
        {
            Console.WriteLine("1. Print users.Males..  count");
            Console.WriteLine("2. Print users.Males.. [0].recents");
            Console.WriteLine("3. Print users.Males.. [0].disliked");
            Console.WriteLine("4. Print users.Males.. [0].unacceptable");
            
            firstClient = false;
            _ = Task.Run(() =>
                {
                    for(; ; )
                    { 
                    try { 
                        char key = Console.ReadKey().KeyChar;
                        Console.WriteLine(); 
                        switch (key)
                        {
                            case '1':
                                var line = Console.ReadLine();
                                if (line.Contains("-"))
                                    {
                                        var lines = line.Split('-');
                                        var ageParsedMin = int.Parse(lines[0]);
                                        var ageParsedMax = int.Parse(lines[1]);
                                        for(int i = ageParsedMin;i < ageParsedMax; i++) { 
                                            Console.WriteLine("cleintwo: " + users.Males[i - 18].Count().ToString());
                                        }
                                        break;
                                    }
                                    var agePreMin = int.Parse(line);
                                Console.WriteLine("cleintwo: " + users.Males[agePreMin - 18].Count().ToString());
                                break;
                            case '2':
                                line = Console.ReadLine();
                                agePreMin = int.Parse(line);
                                Console.WriteLine($"Recents of {users.Males[agePreMin - 18].First().value.preferences.ConnectionId}:");
                                foreach (WeakReference<InQueueStatus> v in users.Males[agePreMin - 18].First().value.recent)
                                {
                                    if(v.TryGetTarget(out var target)){
                                        Console.WriteLine(target.preferences.ConnectionId);
                                    }
                                }
                                break;
                            case '3':
                                line = Console.ReadLine();

                                agePreMin = int.Parse(line);

                                Console.WriteLine($"Dilikeds of {users.Males[agePreMin - 18].First().value.preferences.ConnectionId}:");
                                foreach (WeakReference<InQueueStatus> v in users.Males[agePreMin - 18].First().value.disliked)
                                {
                                    if (v.TryGetTarget(out var target))
                                    {
                                        Console.WriteLine(target.preferences.ConnectionId);
                                    }
                                }
                                break;
                            case '4':
                                    line = Console.ReadLine();

                                    agePreMin = int.Parse(line);

                                    IEnumerable<string> NotAcceptable = users.Males[agePreMin - 18].First().value.recent
                                    .Select(item => item.TryGetTarget(out var status) ? status.preferences.ConnectionId : null).Concat(
                                                users.Males[agePreMin - 18].First().value.disliked
                                    .Select(item => item.TryGetTarget(out var status) ? status.preferences.ConnectionId : null)
                                                ).Where(item => item != null)!;
                                foreach(var v in NotAcceptable)
                                    { Console.WriteLine(v); }
                                break;
                                default:
                                    Console.WriteLine("Invalid option.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in testThread:" + ex.ToString());
                    }
                }
            });
        }

        await base.OnConnectedAsync();
        Console.WriteLine("connected");

        //Context.Items.Add(ageString, 24);
        //Context.Items.Add(isFemString, false);
        //await Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionId", Context.ConnectionId);
    }

   

    internal class InQueueStatus
    {
        internal QueueUser user { get; set; }
        internal bool InQueue { get; set; } = false;
        public UserPreferences preferences { get; internal set; }
        public SearchableQueue<WeakReference<InQueueStatus>> recent { get; set; } = new(3);//3//maybe change to 5 or 10 or 100 for premium or sth 
        public SearchableQueue<WeakReference<InQueueStatus>> disliked { get; set; } = new(3);//maybe it can be SearchableQueue of strings

        internal InQueueStatus(QueueUser ur, UserPreferences flt)
        {
            user = ur;
            preferences = flt;
        }

        ~InQueueStatus()
        {
            if (InQueue)
            {
                users.RemoveUser(user, preferences.ConnectionId);
            }
        }
    }

    public async Task Skip(string s = null, string userString = null)
    {
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        if (userString != null) { SetUser(ref user, userString); }
        if (user is null) { return; }
        var preferences = user.preferences;
        UserPreferences fromSerialization = JsonConvert.DeserializeObject<UserPreferences>(s);
        if(fromSerialization != null)
        {
            preferences.MinAge = fromSerialization.MinAge; 
            preferences.MaxAge = fromSerialization.MaxAge;
            preferences.AcceptMale = fromSerialization.AcceptMale; 
            preferences.AcceptFemale = fromSerialization.AcceptFemale;
        }
        InQueueStatus foundMatch = null;
        lock (user.user)
        {
            if (user.InQueue)
            {
                _ = users.RemoveUser(user.user, Context.ConnectionId);//use ensure user in queue and update only on preferences change
                user.InQueue = false;
            }
            try {
                foundMatch = users.GetId(user);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (foundMatch == null)
            {
                user.InQueue = true;
                JoinQueue(user);
                return;
            }
        }
        await ConnectUsers(user, foundMatch);
    }

    private void SetUser(ref InQueueStatus user, string s)
    {
        QueueUser queueUser = JsonConvert.DeserializeObject<QueueUser>(s);
        if (user == null)
        {
            if(queueUser == null)
            {
                queueUser = new QueueUser() { Age = -1 };
            }
            user = new InQueueStatus(queueUser, new(Context.ConnectionId));
            Context.Items.Add(QueueUserKey, user);
            return;
        }
        lock (user.user)
        {
            if (user.InQueue)
            {
                users.RemoveUser(user.user, Context.ConnectionId);
                user.InQueue = false;
            }
            user.user = queueUser!;
        }
    }

    public async Task Dodge()
    {
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        if (user is null) { return; }
        lock (user.user)
        {
            if (user.InQueue)
            {
                _ = users.RemoveUser(user.user, Context.ConnectionId);
                user.InQueue = false;
            }
        }
    }

    private void JoinQueue(InQueueStatus status)
    {
        users.Push(status);
    }

    public async Task Dislike(string DislikedId)
    {
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        if (user is null) { return; }
        _ = user.recent.SearchFromMostRecent(status => {
            if(status.TryGetTarget(out var inQStatus))
            {
                if(inQStatus.preferences.ConnectionId == DislikedId)
                {
                    user.disliked.Enqueue(new(inQStatus));// you can't enqueue the same weakPointer
                    return true;
                }
                return false;
            }
            return false;
        });
    }

    private async Task ConnectUsers(InQueueStatus user1, InQueueStatus user2)
    {
        try {
            await Clients.Client(user1.preferences.ConnectionId).SendAsync("MatchFound", user2.preferences.ConnectionId, true);
            await Clients.Client(user2.preferences.ConnectionId).SendAsync("MatchFound", user1.preferences.ConnectionId, false);
            user1.recent.Enqueue(new(user2));
            user2.recent.Enqueue(new(user1));
        }
        catch (Exception ex)
        {
        }
    }

    public async Task CallUser(string callto)
    {
        try
        {
            await Clients.Client(callto).SendAsync("ReceiveCall", Context.ConnectionId);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task AcceptCall(string accepted)
    {
        try
        {
            await Clients.Client(accepted).SendAsync("AcceptedCall", Context.ConnectionId);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task DenyCall(string denied)
    {
        try
        {
            await Clients.Client(denied).SendAsync("DeniedCall", Context.ConnectionId);
        }
        catch (Exception ex)
        {
        }
    }

    public async ValueTask MarkAsRemoved()
    {
        try
        {
            InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
            lock (user.user)
            {
                user.InQueue = false;
            }
        }
        catch (Exception ex)
        {
        }
    }
   
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
            lock (user.user)
            {
                if (user.InQueue) {
                    users.RemoveUser(user.user, Context.ConnectionId);
                    user.InQueue = false;
                }
            }
        }
        catch (Exception ex)
        {
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendOffer(string targetUsername, string offer)
    {
        try
        {
            await Clients.Client(targetUsername).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task NSendOffer(string targetSender, string targetUsername, string offer)
    {
        try
        {
            await Clients.Client(targetUsername).SendAsync("ReceiveOffer", targetSender, offer);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task SendAnswer(string targetUsername, SessionDescription answer)
    {
        try
        {
            await Clients.Client(targetUsername).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task ToPeer(string target, string SerializedOffer)
    {
        try
        {
            await Clients.Client(target).SendAsync("ReceiveOffer", Context.ConnectionId, SerializedOffer);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task SenderToPeer(string sender,string target, string SerializedOffer)
    {
        try
        {
            await Clients.Client(target).SendAsync("ReceiveOffer", sender, SerializedOffer);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task ToPeerAndConnect(string target, string SerializedOffer)
    {
        try
        {
            await Clients.Client(target).SendAsync("ReceiveOfferAndConnect", Context.ConnectionId, SerializedOffer);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task SendIceCandidate(string targetConnectionID, string candidate)
    {
        try
        {
            await Clients.Client(targetConnectionID).SendAsync("CReceiveIceCandidate", candidate);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task Reeport(string targetConnectionID)
    {

    }

}
