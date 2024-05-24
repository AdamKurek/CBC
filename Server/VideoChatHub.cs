using CBC.Server;
using CBC.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.X86;

public class VideoChatHub : Hub
{
    private const string QueueUserKey = "U";

    private static readonly TalkersQueues users = new(18, 60);
    static bool firstClient = true;

    private static InQueueStatus? firstUserConnected = null;
    public override async Task OnConnectedAsync()
    {
        if (firstClient)
        {
            firstClient = false;
            Console.WriteLine("1. Print users.Males..  count");
            Console.WriteLine("2. Print users.Males.. [0].recents");
            Console.WriteLine("3. Print users.Males.. [0].disliked");
            Console.WriteLine("4. Print users.Males.. [0].unacceptable");
            Console.WriteLine("5. Print users.undefined.Count");
            Console.WriteLine("6. Print users.males... banProgress");
            Console.WriteLine("7. Print users.males... Reeport power");
            Console.WriteLine("8. Print First InQueue user");

            _ = Task.Run(() =>
            {
                for (; ; )
                {
                    try
                    {
                        char key = Console.ReadKey().KeyChar;
                        Console.WriteLine();
                        switch (key)
                        {
                            case '1':
                                Console.WriteLine("what age/age range?    int / int-int");
                                var line = Console.ReadLine();
                                if (line.Contains("-"))
                                {
                                    var lines = line.Split('-');
                                    var ageParsedMin = int.Parse(lines[0]);
                                    var ageParsedMax = int.Parse(lines[1]);
                                    for (int i = ageParsedMin; i < ageParsedMax; i++)
                                    {
                                        Console.WriteLine(i + "cleint count: " + users.Males[i - 18].Count().ToString());
                                    }
                                    break;
                                }
                                var agePreMin = int.Parse(line);
                                Console.WriteLine(agePreMin + "cleint count: " + users.Males[agePreMin - 18].Count().ToString());
                                break;
                            case '2':
                                line = Console.ReadLine();
                                agePreMin = int.Parse(line);
                                Console.WriteLine($"Recents of {users.Males[agePreMin - 18].First().value.preferences.ConnectionId}:");
                                foreach (WeakReference<InQueueStatus> v in users.Males[agePreMin - 18].First().value.recent)
                                {
                                    if (v.TryGetTarget(out var target))
                                    {
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
                                foreach (var v in NotAcceptable)
                                { Console.WriteLine(v); }
                                break;
                            case '5':
                                Console.WriteLine(users.undefined.Count());
                                break;
                            case '6':
                                Console.WriteLine("what age/age range?    int / int-int");
                                line = Console.ReadLine();
                                if (line.Contains("-"))
                                {
                                    var lines = line.Split('-');
                                    var ageParsedMin = int.Parse(lines[0]);
                                    var ageParsedMax = int.Parse(lines[1]);
                                    for (int i = ageParsedMin; i < ageParsedMax; i++)
                                    {
                                        Console.WriteLine(i + "BanScore: " + users.Males[i - 18].First()?.value.BanScore);
                                    }
                                    break;
                                }
                                agePreMin = int.Parse(line);
                                Console.WriteLine(agePreMin + "BanScore: " + users.Males[agePreMin - 18].First()?.value.BanScore);
                                break;
                            case '7':
                                Console.WriteLine("what age/age range?    int / int-int");
                                line = Console.ReadLine();
                                if (line.Contains("-"))
                                {
                                    var lines = line.Split('-');
                                    var ageParsedMin = int.Parse(lines[0]);
                                    var ageParsedMax = int.Parse(lines[1]);
                                    for (int i = ageParsedMin; i < ageParsedMax; i++)
                                    {
                                        Console.WriteLine(i + "ReeportStrenth: " + users.Males[i - 18].First()?.value.ReeportStrenth);
                                    }
                                    break;
                                }
                                agePreMin = int.Parse(line);
                                Console.WriteLine(agePreMin + "ReeportStrenth: " + users.Males[agePreMin - 18].First()?.value.ReeportStrenth);
                                break;
                            case '8':
                                Console.WriteLine($"first user age {firstUserConnected.user.Age}, is female {firstUserConnected.user.IsFemale} age filters {firstUserConnected.preferences.MinAge}-{firstUserConnected.preferences.MaxAge}, accepts females {firstUserConnected.preferences.AcceptFemale}, accepts males {firstUserConnected.preferences.AcceptMale}, banScore{firstUserConnected.BanScore}, reeportStrenth {firstUserConnected.ReeportStrenth}");
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
        public SearchableQueue<WeakReference<InQueueStatus>> disliked { get; set; } = new(5);//maybe it can be SearchableQueue of strings
        public DateTime MostRecentConnect { get; set; } = DateTime.Now;
        internal double ReeportStrenth = 0.25;

        internal double BanScore = 1;

        internal InQueueStatus(QueueUser ur, UserPreferences flt)
        {
            user = ur;
            preferences = flt;
        }

        ~InQueueStatus()
        {
            if (InQueue)
            {
                try
                {
                    users.RemoveUser(user, preferences.ConnectionId);
                }
                catch { }
            }
        }
    }

    public async Task Skip(string s, string userString)
    {
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        if (user is null || userString != null) { SetUser(ref user, userString); }//TODO refactor SetUser and usage
        UserPreferences fromSerialization = JsonConvert.DeserializeObject<UserPreferences>(s);
        if (fromSerialization != null)
        {
            var preferences = user.preferences;
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
                user.InQueue = false;
                try
                {
                    _ = users.RemoveUser(user.user, Context.ConnectionId);//use ensure user in queue and update only on preferences change
                }
                catch { }
            }
            try
            {
                foundMatch = users.GetId(user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (foundMatch == null)
            {
                user.InQueue = true;
                JoinQueue(user);
                Console.WriteLine(user.preferences.ConnectionId + " could not find match");
                return;
            }
        }
        Console.WriteLine(user.preferences.ConnectionId + " found match " + foundMatch.preferences.ConnectionId);
        await ConnectUsers(user, foundMatch);
    }

    private void SetUser(ref InQueueStatus user, string s)
    {
        QueueUser queueUser = JsonConvert.DeserializeObject<QueueUser>(s);
        if (queueUser == null)
        {
            queueUser = new QueueUser() { Age = -1 };
        }
        if (user == null)
        {

            user = new InQueueStatus(queueUser, new(Context.ConnectionId));
            Context.Items.Add(QueueUserKey, user);

            if (firstUserConnected == null)///TODO it's only for testing it should not be in the main code
            {
                firstUserConnected = user;
            }

            return;
        }
        lock (user.user)
        {
            if (user.InQueue)
            {
                user.InQueue = false;
                try
                {
                    users.RemoveUser(user.user, Context.ConnectionId);
                }
                catch(Exception _) { }
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
                user.InQueue = false;
                try
                {
                    _ = users.RemoveUser(user.user, Context.ConnectionId);
                }
                catch { }
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
            if (status.TryGetTarget(out var inQStatus))
            {
                if (inQStatus.preferences.ConnectionId == DislikedId)
                {
                    user.disliked.Enqueue(new(inQStatus));// you can't enqueue the same weakPointer
                    inQStatus.BanScore -= 0.2 * user.ReeportStrenth;//lower ban Score but do not ban for being disliked
                    user.ReeportStrenth *= 0.9;
                    return true;
                }
                return false;
            }
            return false;
        });
    }

    private async Task ConnectUsers(InQueueStatus user1, InQueueStatus user2)
    {
        try
        {
            await Clients.Client(user1.preferences.ConnectionId).SendAsync("MatchFound", user2.preferences.ConnectionId, true);
            await Clients.Client(user2.preferences.ConnectionId).SendAsync("MatchFound", user1.preferences.ConnectionId, false);
            user1.recent.Enqueue(new(user2));
            user2.recent.Enqueue(new(user1));

            increseReeportStrenthAndConnectTime(user1);
            increseReeportStrenthAndConnectTime(user2);

            user1.MostRecentConnect = DateTime.Now;
            user2.MostRecentConnect = DateTime.Now;
            
          
        }
        catch (Exception ex)
        {
        }
    }
    private static void increseReeportStrenthAndConnectTime(InQueueStatus user)
    {
        increseReeportStrenth(user);
        user.MostRecentConnect = DateTime.Now;
        return;
        void increseReeportStrenth(InQueueStatus user)
        {
            if (user.MostRecentConnect.AddMinutes(1) > DateTime.Now)
            {
                user.ReeportStrenth += 0.01;
                if (user.ReeportStrenth >= 0.5) { user.ReeportStrenth = 0.5; }
                user.BanScore += 0.05;
                return;
            }
            if (user.MostRecentConnect.AddMinutes(4) > DateTime.Now)
            {
                user.ReeportStrenth += 0.5;
                if (user.ReeportStrenth >= 0.5) { user.ReeportStrenth = 0.5; }
                user.BanScore += 0.15;
                return;
            }
            if (user.MostRecentConnect.AddMinutes(10) > DateTime.Now)
            {
                user.ReeportStrenth += 0.2;
                if (user.ReeportStrenth >= 0.5) { user.ReeportStrenth = 0.5; }
                user.BanScore += 0.7;
                return;
            }
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

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
            if (user.recent.First().TryGetTarget(out var result)) {
                await RemoteDisconnectCall(Context.ConnectionId, result.preferences.ConnectionId);
            }
            lock (user.user)
            {
                if (user.InQueue)
                {
                    user.InQueue = false;
                    try
                    {
                        users.RemoveUser(user.user, Context.ConnectionId);
                    }
                    catch { }
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
    private async Task RemoteDisconnectCall(string who, string with)
    {
        try
        {
            await Clients.Client(with).SendAsync("ForceDisconnect", who);
        }
        catch (Exception ex) { }
    }
    public async Task Reeport(string ReeportedID)
    {
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        if (user is null) { return; }
        var weakReeported = user.recent.SearchFromMostRecent(status => {
            if (status.TryGetTarget(out var inQStatus))
            {
                if (inQStatus.preferences.ConnectionId == ReeportedID)
                {
                    return true;
                }
                return false;
            }
            return false;
        });

        if (weakReeported != null && weakReeported.TryGetTarget(out var inQStatus))
        {
            Console.WriteLine($"before reeport: {user.preferences.ConnectionId} reeported {inQStatus.preferences.ConnectionId} \n banscore {user.BanScore} reeport Strenth {user.ReeportStrenth}\n banscore {inQStatus.BanScore} reeport Strenth {inQStatus.ReeportStrenth} ");
            inQStatus.BanScore -= user.ReeportStrenth;
            user.ReeportStrenth *= 0.625;
            Console.WriteLine($"after  reeport: \n banscore {user.BanScore} reeport Strenth {user.ReeportStrenth} \n banscore {inQStatus.BanScore} reeport Strenth {inQStatus.ReeportStrenth} ");
            if (inQStatus.BanScore < 0)
            {
                await Clients.Client(inQStatus.preferences.ConnectionId).SendAsync("BanMe");
                Console.WriteLine("Banned " + inQStatus.preferences.ConnectionId);
            }
        }

    }

}
