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
        await base.OnConnectedAsync();
    }
    public async Task JoinQueue()
    {
        var query = Context.GetHttpContext().Request.Query;

        if (!int.TryParse(query["age"], out var age) ||
            !bool.TryParse(query["female"], out var isFemale) ||
            !bool.TryParse(query["acceptMale"], out var acceptMale) ||
            !bool.TryParse(query["acceptFemale"], out var acceptFemale) ||
            !int.TryParse(query["minAge"], out var minAge) ||
            !int.TryParse(query["maxAge"], out var maxAge))
        {
            await Clients.Caller.SendAsync("Error", "Invalid input parameters.");
            return;
        }

        var username = query["username"];

        if (!Context.User.Identity.IsAuthenticated && (acceptMale || acceptFemale))
        {
            await Clients.Caller.SendAsync("Error", "Unauthenticated users cannot set gender preferences.");
            return;
        }

        Users.TryAdd(username, new QueueUser
        {
            ConnectionId = Context.ConnectionId,
            Age = age,
            IsFemale = isFemale,
            AcceptMale = acceptMale,
            AcceptFemale = acceptFemale,
            MinAge = minAge,
            MaxAge = maxAge
        });
    }

    private async Task ConnectUsers(QueueUser user1, QueueUser user2)
    {
        try { 
            await Clients.Client(user1.ConnectionId).SendAsync("MatchFound", user2.ConnectionId);
            await Clients.Client(user2.ConnectionId).SendAsync("MatchFound", user1.ConnectionId);
        }catch(Exception ex)
        {

        }
    }

    private async Task FindMatchingUser(string username)
    {
        if (Users.TryGetValue(username, out var user))
        {
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
        else
        {
            // Handle the case where the username is not found in the Users dictionary.
            // For example: Log.Warning($"User with username {username} not found.");
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            var usernameEntry = Users.FirstOrDefault(x => x.Value.ConnectionId == Context.ConnectionId);
            if (!string.IsNullOrEmpty(usernameEntry.Key))
            {
                Users.TryRemove(usernameEntry.Key, out _);
            }
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
            await Clients.Client(targetUser.ConnectionId).SendAsync("ReceiveOffer", Context.UserIdentifier, offer);
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
                await Clients.Client(targetUser.ConnectionId).SendAsync("ReceiveAnswer", Context.UserIdentifier, answer);
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
        if (Users.TryGetValue(targetUsername, out var targetUser))
        {
            try
            {
                await Clients.Client(targetUser.ConnectionId).SendAsync("ReceiveIceCandidate", Context.UserIdentifier, candidate);
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
