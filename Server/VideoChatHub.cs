using CBC.Server;
using CBC.Shared;
using Duende.IdentityServer.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

public class VideoChatHub : Hub
{
    private const string QueueUserKey = "U";

    private static readonly TalkersQueues users = new(18,60);
    static bool firstClient = true;
    public override async Task OnConnectedAsync()
    {
        if (firstClient)
        {
            Console.WriteLine("1. Print users.Males[24-18] count");
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
                                Console.WriteLine("cleintwo: " + users.Males[24 - 18].Count().ToString());
                                break;

                            default:
                                Console.WriteLine("Invalid option. Please enter a valid key.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            });
        }

        Console.WriteLine("connected");
        await base.OnConnectedAsync();
        Context.Items.Add(QueueUserKey, new InQueueStatus( new() { Age = 24, IsFemale = false }, new(Context.ConnectionId)));
        //Context.Items.Add(ageString, 24);
        //Context.Items.Add(isFemString, false);
        //await Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionId", Context.ConnectionId);
    }

    internal class InQueueStatus
    {
        internal QueueUser user { get; set; }
        internal bool InQueue { get; set; } = false;
        public UserPreferences preferences { get; internal set; }
        public SearchableQueue<WeakReference<InQueueStatus>> recent { get; set; } = new(3);//maybe change to 5 or 10 or 100 for premium or sth 
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

    public async Task Skip(string s)
    {
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
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
            foundMatch = users.GetId(preferences, user, preferences.ConnectionId);

            if (foundMatch == null)
            {
                user.InQueue = true;
                Console.WriteLine(5);

                JoinQueue(user);
                return;
            }
        }
        await ConnectUsers(user, foundMatch);
        //Context.GetHttpContext().Connection.RemoteIpAddress.ToString();
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

    private void JoinQueue(InQueueStatus preferences)
    {
        users.Push(preferences.user.Age, preferences.user.IsFemale, preferences);
    }

    public async Task Dislike(string DislikedId)
    {
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        if (user is null) { return; }
        var disliked = user.recent.SearchFromMostRecent(status => {
            if(status.TryGetTarget(out var inQStatus))
            {
                if(inQStatus.preferences.ConnectionId == DislikedId)
                {
                    return true;
                }
                return false;
            }
            return false;
        });
        user.disliked.Enqueue(disliked);
    }

    private async Task ConnectUsers(InQueueStatus user1, InQueueStatus user2)
    {
        //Console.WriteLine($"czad {user1} && {user2}");
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
        Console.WriteLine($"{Context.ConnectionId} dzwoni do {callto}");
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
        Console.WriteLine($"{accepted} accepted {Context.ConnectionId}");
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
        Console.WriteLine($"{denied} dnieodebal {Context.ConnectionId}");
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
        //Console.WriteLine($"{ target}    wysyla     {SerializedOffer} \n od {Context.ConnectionId}");
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
        //Console.WriteLine($"{target}    wysyla     {SerializedOffer} \n od {sender}");
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
       //Console.WriteLine($"{target}    wysylaAndConnect     {SerializedOffer} \n od {Context.ConnectionId}");
        try
        {
            //InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
            //lock (user.user)
            //{
            //    user.InQueue = false;
            //}
            await Clients.Client(target).SendAsync("ReceiveOfferAndConnect", Context.ConnectionId, SerializedOffer);
        }
        catch (Exception ex)
        {
        }
    }
    public async Task SendIceCandidate(string targetConnectionID, string candidate)
    {
        //Console.WriteLine($"wysyam ice do {targetConnectionID} tresc {candidate}");
        try
        {
            await Clients.Client(targetConnectionID).SendAsync("CReceiveIceCandidate", candidate);
        }
        catch (Exception ex)
        {
        }
    }
}
