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
        await FindMatchingUser(Context.ConnectionId);
        Console.WriteLine(Context.ConnectionId + " dołączył");

    }
    public async Task JoinQueue()
    {
        var user = new QueueUser() { MaxAge = 60, MinAge = 18, Age = 20, AcceptFemale = true, AcceptMale = true, IsFemale = false};
        var query = Context.GetHttpContext().Request.Query;
        Users.TryAdd(Context.ConnectionId, user);
        //Console.WriteLine(query["id"] + "ID"); 
        //Console.WriteLine(Context.ConnectionId + "cnnd");
        cringeid = query["id"];
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
            Users.TryRemove(Context.ConnectionId, out _);
        }
        catch (Exception ex)
        {
            // Handle the exception, log it, or take appropriate action.
            // For example: Log.Error($"Error handling disconnection for {Context.ConnectionId}: {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendOffer(string targetUsername, SessionDescription offer)
    {
        if (Users.TryGetValue(targetUsername, out var targetUser))
        {
            try
            {
                await Clients.Client(targetUsername).SendAsync("ReceiveOffer", Context.UserIdentifier, offer);
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
                await Clients.Client(targetUsername).SendAsync("ReceiveAnswer", Context.UserIdentifier, answer);
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
        if (Users.TryGetValue(target, out var targetUser))
        {
            try
            {
                Console.WriteLine($"{ target}    wysyla     {SerializedOffer}");
                await Clients.Client(target).SendAsync("ReceiveOffer", Context.UserIdentifier, SerializedOffer);
            }
            catch (Exception ex)
            {
            }
        }
        else
        {
        }
    }
    

    public async Task SendIceCandidate(string targetUsername, SessionDescription candidate)
    {
        Console.WriteLine("jeste w ice");

        if (Users.TryGetValue(targetUsername, out var targetUser))
        {
            try
            {
                Console.WriteLine("wysyam ice");
                await Clients.Client(targetUsername).SendAsync("ReceiveIceCandidate", Context.UserIdentifier, candidate);
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
