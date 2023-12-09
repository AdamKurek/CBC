using CBC.Server;
using CBC.Server.ConcurrentLinkedListQueue;
using CBC.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("connected");
        await base.OnConnectedAsync();


        Context.Items.Add(QueueUserKey, new InQueueStatus(Context.ConnectionId, new() { Age = 24, IsFemale = false }));
        //Context.Items.Add(ageString, 24);
        //Context.Items.Add(isFemString, false);
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionId", Context.ConnectionId);
    }
    internal class InQueueStatus
    {
        internal QueueUser user { get; set; }
        internal bool InQueue { get; set; } = false;
        string connectionId;
        internal InQueueStatus(string ConnectionId, QueueUser ur)
        {
            connectionId = ConnectionId;
            user = ur;
        }
        ~InQueueStatus()
        {
            lock (user) { 
                if (InQueue)
                {
                    users.RemoveUser(user, connectionId);
                }
            }
        }
    }
    public async Task SetParameters(int age, bool isfemale)
    {
        //Context.Items.Add(ageString, age);
        //Context.Items.Add(isFemString, isfemale);
    }
    public async Task Skip(string userId)
    {
        UserPreferences preferences = new UserPreferences() { AcceptFemale = true, AcceptMale = true, MaxAge = 25, MinAge = 23, ConnectionId = Context.ConnectionId };
        if(! await FindMatchingUser(Context.ConnectionId, preferences)) {
            InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
            lock (user.user)
            {
                if (!user.InQueue)//todo make it so it checks if it's in queue and if not then adds
                {
                    JoinQueue(preferences, user.user);
                    user.InQueue = true;
                }
            }
            Console.WriteLine("dolonczyl do queuq");
        }else{ 
            Console.WriteLine("znalazl");
        }
    }
    private void JoinQueue(UserPreferences preferences,QueueUser user)
    {
        Console.WriteLine($"pushje {user.Age} latka czy femalem {user.IsFemale}");
        users.Push(user.Age,user.IsFemale, preferences);
    }

    private async Task ConnectUsers(string user1, string user2)
    {
        Console.WriteLine($"czad {user1} && {user2}");
        try { 
            await Clients.Client(user1).SendAsync("MatchFound", user2, true);
            await Clients.Client(user2).SendAsync("MatchFound", user1, false);
        }
        catch(Exception ex)
        {
        }
    }

    private async Task<bool> FindMatchingUser(string username, UserPreferences preferences)
    {
       
        InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
        //lock (user.user)
        //{
            var otherId = users.GetId(preferences, user.user, Context.ConnectionId);
            if (otherId != null)
            {
                Console.WriteLine("znalaz " + otherId);
                await ConnectUsers(username, otherId);
                return true;
            }

        return false;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
            lock (user.user)
            {
                var otherId = users.RemoveUser(user.user, Context.ConnectionId);
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
        Console.WriteLine($"{ target}    wysyla     {SerializedOffer} \n od {Context.ConnectionId}");
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
        Console.WriteLine($"{target}    wysyla     {SerializedOffer} \n od {sender}");
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
        Console.WriteLine($"{target}    wysylaAndConnect     {SerializedOffer} \n od {Context.ConnectionId}");
        try
        {
            InQueueStatus user = Context.Items[QueueUserKey] as InQueueStatus;
            lock (user.user)
            {
                user.InQueue = false;
            }
            await Clients.Client(target).SendAsync("ReceiveOfferAndConnect", Context.ConnectionId, SerializedOffer);
        }
        catch (Exception ex)
        {
        }
    }
    public async Task SendIceCandidate(string targetConnectionID, string candidate)
    {
        Console.WriteLine($"wysyam ice do {targetConnectionID} tresc {candidate}");
        try
        {
            await Clients.Client(targetConnectionID).SendAsync("CReceiveIceCandidate", candidate);
        }
        catch (Exception ex)
        {
        }
    }
}
