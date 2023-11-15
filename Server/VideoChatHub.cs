using CBC.Server;
using CBC.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Reflection;

public class VideoChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, QueueUser> Users = new ConcurrentDictionary<string, QueueUser>();

    public override async Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext().Request.Query["username"].ToString();
        var female = bool.Parse(Context.GetHttpContext().Request.Query["female"].ToString());
        var age = int.Parse(Context.GetHttpContext().Request.Query["age"].ToString());
        var acceptMale = bool.Parse(Context.GetHttpContext().Request.Query["acceptMale"].ToString());
        var acceptFemale = bool.Parse(Context.GetHttpContext().Request.Query["acceptFemale"].ToString());
        var minAge = int.Parse(Context.GetHttpContext().Request.Query["minAge"].ToString());
        var maxAge = int.Parse(Context.GetHttpContext().Request.Query["maxAge"].ToString());

        // Check if the user is authenticated and has the correct preferences
        if (!Context.User.Identity.IsAuthenticated && (acceptMale || acceptFemale))
        {
            // You can return an error message, ignore the request, or handle this situation as needed
            await Clients.Caller.SendAsync("Error", "Unauthenticated users cannot set gender preferences.");
            return;
        }

        Users.TryAdd(username, new QueueUser
        {
            ConnectionId = Context.ConnectionId,
            Age = age,
            IsFemale = female,
            AcceptMale = acceptMale,
            AcceptFemale = acceptFemale,
            MinAge = minAge,
            MaxAge = maxAge
        });

        //await FindMatchingUser(username);

        await base.OnConnectedAsync();
    }


    private async Task ConnectUsers(QueueUser user1, QueueUser user2)
    {
        await Clients.Client(user1.ConnectionId).SendAsync("MatchFound", user2.ConnectionId);
        await Clients.Client(user2.ConnectionId).SendAsync("MatchFound", user1.ConnectionId);
    }
   
    private async Task FindMatchingUser(string username)
    {
        var user = Users[username];
        var matchingUser = Users.Values.FirstOrDefault(u =>
            u.ConnectionId != user.ConnectionId &&
            ((u.IsFemale && user.AcceptFemale) || (!u.IsFemale && user.AcceptMale)) &&
            u.Age >= user.MinAge && u.Age <= user.MaxAge &&
            ((user.IsFemale && u.AcceptFemale) || (!user.IsFemale && u.AcceptMale)) &&
            user.Age >= u.MinAge && user.Age <= u.MaxAge);

        if (matchingUser != null)
        {
            await ConnectUsers(user, matchingUser);
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Users.FirstOrDefault(x => x.Value.ConnectionId == Context.ConnectionId).Key;
        Users.TryRemove(username, out _);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendOffer(string targetUsername, SessionDescription offer)
    {
        var targetConnectionId = Users[targetUsername].ConnectionId;
        await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", Context.UserIdentifier, offer);
    }

    public async Task SendAnswer(string targetUsername, SessionDescription answer)
    {
        var targetConnectionId = Users[targetUsername].ConnectionId;
        await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", Context.UserIdentifier, answer);
    }

    public async Task SendIceCandidate(string targetUsername, SessionDescription candidate)
    {
        var targetConnectionId = Users[targetUsername].ConnectionId;
        await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", Context.UserIdentifier, candidate);
    }
}
