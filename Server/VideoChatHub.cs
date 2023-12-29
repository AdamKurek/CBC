using Blazorise;
using CBC.Server;
using CBC.Server.ConcurrentLinkedListQueue;
using CBC.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Runtime.Intrinsics.X86;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public class VideoChatHub : Hub
{
    //private const string isFemString = "IsFemale";
   //private const string ageString = "Age";
    private const string QueueUserKey = "U";

    //private static readonly ConcurrentDictionary< , QueueUser> Users = new ConcurrentDictionary<string, QueueUser>();
    //private static readonly ConcurrentQueue<QueueUser> Users = new();
    private static readonly UsersMultiversumQueue users = new(18,60);
    //private static readonly ConcurrentDictionary<string, QueueUser> MapOfUsers = new();
    static bool firstClient = true;
    public override async Task OnConnectedAsync()
    {
        if (firstClient)
        {
            _ = Task.Run(() =>
            {
                firstClient = false;
                while (true)
                {
                    try { 
                        Console.WriteLine("cleintwo: " + users.Males[24-18].Count().ToString());
                    }catch(Exception ex) { Console.WriteLine(ex.ToString()); }
                        Thread.Sleep(1000);
                }
            });
        }

        Console.WriteLine("connected");
        await base.OnConnectedAsync();
        Context.Items.Add(QueueUserKey, new InQueueStatus( new() { Age = 24, IsFemale = false }, new(Context.ConnectionId)));
        //Context.Items.Add(ageString, 24);
        //Context.Items.Add(isFemString, false);
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionId", Context.ConnectionId);
    }
    internal class InQueueStatus
    {
        internal QueueUser user { get; set; }
        internal bool InQueue { get; set; } = false;
        public UserPreferences preferences { get; internal set; }
        public takolejka<string> disliked { get; set; } = new(3);//maybe change to 5 or 10 or 100 for premium or sth 

        internal InQueueStatus(QueueUser ur, UserPreferences flt)
        {
            user = ur;
            preferences = flt;
        }
        ~InQueueStatus()
        {
            lock (user) { 
                if (InQueue)
                {
                    users.RemoveUser(user, preferences.ConnectionId);
                }
            }
        }
    }
    public async Task SetParameters(int age, bool isfemale)
    {
        //Context.Items.Add(ageString, age);
        //Context.Items.Add(isFemString, isfemale);
    }
    public async Task Skip(string s)
    {
        Console.WriteLine(Context.ConnectionId + "Skipped ");

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
        //Console.WriteLine("String: " + fromSerialization);

        //Console.WriteLine("Skipping " + preferences);

        if (preferences == null) {
            preferences = user.preferences;
        }
        string foundMatch = null;
        lock (user.user)
        {
            if (user.InQueue)
            {
                _ = users.RemoveUser(user.user, Context.ConnectionId);
                user.InQueue = false;
            }
            foundMatch = FindMatchingUser(preferences);
            if (foundMatch == null)
            {
                user.InQueue = true;
                //Console.WriteLine("dolonczyl do queuq");
                JoinQueue(preferences, user.user);
                return;
            }
        }
        await ConnectUsers(Context.ConnectionId, foundMatch);
        //Context.GetHttpContext().Connection.RemoteIpAddress.ToString();

    }

    public async Task Dodge()
    {
        Console.WriteLine(Context.ConnectionId+ "doddged");
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
        private void JoinQueue(UserPreferences preferences,QueueUser user)
    {
        //Console.WriteLine($"pushje {user.Age} latka czy femalem {user.IsFemale}");
        users.Push(user.Age,user.IsFemale, preferences);
    }
    public async Task Dislike(string Disliked)
    {
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        if (user is null) { return; }
        user.disliked.Push(Disliked);
    }
    private async Task ConnectUsers(string user1, string user2)
    {
        //Console.WriteLine($"czad {user1} && {user2}");
        try {
            //TODO maybe store that user1 is about to be marked as out of queue// also update MarkAsRemoved() 
            await Clients.Client(user1).SendAsync("MatchFound", user2, true);
            await Clients.Client(user2).SendAsync("MatchFound", user1, false);
        }
        catch(Exception ex)
        {
        }
    }
    public async Task CallUser(string caller)
    {
        Console.WriteLine($"{Context.ConnectionId} dzwoni do {caller}");
        try
        {
            await Clients.Client(caller).SendAsync("ReceiveCall", Context.ConnectionId);

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
    private string FindMatchingUser(UserPreferences preferences)
    {

        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        return users.GetId(preferences, user.user, preferences.ConnectionId); ;
    }
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
            lock (user.user)
            {
                while(users.RemoveUser(user.user, Context.ConnectionId))
                {
                    ;
                }
                
                user.InQueue = false;
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
