using CBC.Server;
using CBC.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Runtime.Intrinsics.X86;

public class VideoChatHub : Hub
{
    //private static readonly ConcurrentDictionary< , QueueUser> Users = new ConcurrentDictionary<string, QueueUser>();
    //private static readonly ConcurrentQueue<QueueUser> Users = new();
    private static readonly UsersMultiversumQueue users = new(18,60);
    private static readonly ConcurrentDictionary<string, QueueUser> MapOfUsers = new();
    string cringeid;
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("connected");
        await base.OnConnectedAsync();
        MapOfUsers.TryAdd(new(Context.ConnectionId),  new QueueUser() { Age = 24, IsFemale = false });
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionId", Context.ConnectionId);

    }
    public async Task Skip(string userId)
    {
        cringeid = Context.ConnectionId;
        UserPreferences preferences = new UserPreferences() { AcceptFemale = true, AcceptMale = true, MaxAge = 25, MinAge = 23, ConnectionId = Context.ConnectionId };
        if(! await FindMatchingUser(Context.ConnectionId, preferences)) { 
        
            await JoinQueue(preferences,24,false);
            Console.WriteLine("dolonczyl do queuq");
        }else{ 
            Console.WriteLine("znalazl");
        }
    }
    public async Task JoinQueue(UserPreferences preferences,int age,bool female)
    {
        var query = Context.GetHttpContext().Request.Query;
        Console.WriteLine($"pushje {age} latka czy femalem {female}");
        users.Push(age, female, preferences);

        //Users.TryAdd(, user);
        //Console.WriteLine(query["id"] + "ID"); 
        //Console.WriteLine(Context.ConnectionId + "cnnd");
        //cringeid = query["id"];
        //if (!int.TryParse(query["age"], out var age) ||
        //    !bool.TryParse(query["female"], out var isFemale) ||
        //    !bool.TryParse(query["acceptMale"], out var acceptMale) ||
        //    !bool.TryParse(query["acceptFemale"], out var acceptFemale) ||
        //    !int.TryParse(query["minAge"], out var minAge) ||
        //    !int.TryParse(query["maxAge"], out var maxAge))
        //{
        //    await Clients.Caller.SendAsync("Error", "Invalid input parameters.");
        //    int.TryParse(query["id"], out var xd);
        //    Console.WriteLine(query["id"].GetType());
        //    Console.WriteLine(query.First().ToString());
        //    return;
        //}

        //if (!Context.User.Identity.IsAuthenticated && (user.AcceptFemale || user.AcceptMale))
        //{
        //    await Clients.Caller.SendAsync("Error", "Unauthenticated users cannot set gender preferences.");
        //   return;
        //}

        //Users.TryAdd(user.username, new QueueUser
        //{
        //    ConnectionId = Context.ConnectionId,
        //    Age = age,
        //    IsFemale = isFemale,
        //    AcceptMale = acceptMale,
        //    AcceptFemale = acceptFemale,
        //    MinAge = minAge,
        //    MaxAge = maxAge
        //});
    }

    private async Task ConnectUsers(string user1, string user2)
    {
        Console.WriteLine("czad");
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
        Console.WriteLine("username szuka");
        if (MapOfUsers.TryGetValue(username, out var user))
        {
            Console.WriteLine("jesttu");
            var otherId = users.GetId(preferences, user);
            if (otherId != null)
            {
                Console.WriteLine("2");
                await ConnectUsers(username, otherId);
                return true;
            }
        }
        else
        {
            Console.WriteLine($"user {username} not connected somehow");
        }
        Console.WriteLine("tu false");
        return false;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            MapOfUsers.TryRemove(cringeid, out _);
        }
        catch (Exception ex)
        {
            // Handle the exception, log it, or take appropriate action.
            // For example: Log.Error($"Error handling disconnection for {Context.ConnectionId}: {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendOffer(string targetUsername, string offer)
    {
        try
        {
            await Clients.Client(targetUsername).SendAsync("ReceiveOffer", cringeid, offer);
        }
        catch (Exception ex)
        {
            // Handle the exception, log it, or take appropriate action.
            // For example: Log.Error($"Error sending offer to {targetUsername}: {ex.Message}");
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
            // Handle the exception, log it, or take appropriate action.
            // For example: Log.Error($"Error sending offer to {targetUsername}: {ex.Message}");
        }
    }

    public async Task SendAnswer(string targetUsername, SessionDescription answer)
    {
        try
        {
            await Clients.Client(targetUsername).SendAsync("ReceiveAnswer", cringeid, answer);
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
