using CBC.Server;
using CBC.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public class VideoChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, QueueUser> Users = new ConcurrentDictionary<string, QueueUser>();
    string cringeid;
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("connected");
        await base.OnConnectedAsync();
        await JoinQueue();
    }
    public async Task Skip(string userId)
    {
        cringeid = Context.ConnectionId;
        await FindMatchingUser(Context.ConnectionId);
        Console.WriteLine(cringeid + " dolaczyl");

    }
    public async Task JoinQueue()
    {
        Console.WriteLine(cringeid + " zyje dalek");
        var user = new QueueUser() { MaxAge = 60, MinAge = 18, Age = 20, AcceptFemale = true, AcceptMale = true, IsFemale = false};
        var query = Context.GetHttpContext().Request.Query;
        Users.TryAdd(Context.ConnectionId, user);
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

        if (!Context.User.Identity.IsAuthenticated && (user.AcceptFemale || user.AcceptMale))
        {
            await Clients.Caller.SendAsync("Error", "Unauthenticated users cannot set gender preferences.");
            return;
        }

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
            await Clients.Client(user1).SendAsync("MatchFound", user2,true);
            await Clients.Client(user2).SendAsync("MatchFound", user1,false);
        }
        catch(Exception ex)
        {

        }
    }

    private async Task FindMatchingUser(string username)
    {
        Console.WriteLine("0");

        if (Users.TryGetValue(username, out var user))
        {
            Users[Context.ConnectionId].WaitingForTalk = true;
             //     var matchingUser = Users.Values.FirstOrDefault(u =>
             //     pair.Value.WaitingForTalk &&
             //     u != user &&
             //     ((u.IsFemale && user.AcceptFemale) || (!u.IsFemale && user.AcceptMale)) &&
             //     u.Age >= user.MinAge && u.Age <= user.MaxAge &&
             //     ((user.IsFemale && u.AcceptFemale) || (!user.IsFemale && u.AcceptMale)) &&
             //     user.Age >= u.MinAge && user.Age <= u.MaxAge);

             var matchingUserKey = Users.FirstOrDefault(pair =>
                pair.Value.WaitingForTalk &&
                pair.Value != user &&
                ((pair.Value.IsFemale && user.AcceptFemale) || (!pair.Value.IsFemale && user.AcceptMale)) &&
                pair.Value.Age >= user.MinAge && pair.Value.Age <= user.MaxAge &&
                ((user.IsFemale && pair.Value.AcceptFemale) || (!user.IsFemale && pair.Value.AcceptMale)) &&
                user.Age >= pair.Value.MinAge && user.Age <= pair.Value.MaxAge
            ).Key;

            if (matchingUserKey != null)
            {
                Console.WriteLine("2");
                await ConnectUsers(username, matchingUserKey);
                return;
            }
        }
        else
        {
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            Users.TryRemove(cringeid, out _);
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
        if (Users.TryGetValue(targetUsername, out var targetUser))
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
        else
        {
            // Handle the case where the targetUsername is not found in the Users dictionary.
            // For example: Log.Warning($"User with username {targetUsername} not found for SendOffer.");
        }
    }

    public async Task NSendOffer(string targetSender, string targetUsername, string offer)
    {
        if (Users.TryGetValue(targetUsername, out var targetUser))
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
        else
        {
            // Handle the case where the targetUsername is not found in the Users dictionary.
            // For example: Log.Warning($"User with username {targetUsername} not found for SendOffer.");
        }
    }

    public async Task SendAnswer(string targetUsername, SessionDescription answer)
    {
        if (Users.TryGetValue(targetUsername, out var targetUser))
        {
            try
            {
                await Clients.Client(targetUsername).SendAsync("ReceiveAnswer", cringeid, answer);
            }
            catch (Exception ex)
            {
            }
        }
        else
        {
        }
    }

    public async Task ToPeer(string target, string SerializedOffer)
    {
        Console.WriteLine($"{ target}    wysyla     {SerializedOffer} \n od {cringeid}");
        if (Users.TryGetValue(target, out var targetUser))
        {
            try
            {
                await Clients.Client(target).SendAsync("ReceiveOffer", cringeid, SerializedOffer);
            }
            catch (Exception ex)
            {
            }
        }
        else
        {
        }
    }

    public async Task NToPeer(string sender,string target, string SerializedOffer)
    {
        Console.WriteLine($"{target}    wysyla     {SerializedOffer} \n od {cringeid}");
        if (Users.TryGetValue(target, out var targetUser))
        {
            try
            {
                await Clients.Client(target).SendAsync("ReceiveOffer", sender, SerializedOffer);
            }
            catch (Exception ex)
            {
            }
        }
        else
        {
        }
    }

    public async Task ToPeerAndConnect(string target, string SerializedOffer)
    {
        Console.WriteLine($"{target}    wysylaAndConnect     {SerializedOffer} \n od {cringeid}");
        if (Users.TryGetValue(target, out var targetUser))
        {
            try
            {
                await Clients.Client(target).SendAsync("ReceiveOfferAndConnect", cringeid, SerializedOffer);
            }
            catch (Exception ex)
            {
            }
        }
        else
        {
        }
    }
    public async Task SendIceCandidate(string targetConnectionID, string candidate)
    {
        Console.WriteLine($"wysyam ice do {targetConnectionID} tresc {candidate}");

        if (Users.TryGetValue(targetConnectionID, out var targetUser))
        {
            try
            {
                await Clients.Client(targetConnectionID).SendAsync("CReceiveIceCandidate", candidate);
            }
            catch (Exception ex)
            {
            }
        }
        else
        {
        }
    }



}
