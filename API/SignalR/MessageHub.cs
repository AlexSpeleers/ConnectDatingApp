using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub(IMessageRepository messageRepository, IMemberRepository memberRepository, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        HttpContext? httpContext = Context.GetHttpContext();
        string otherUserId = httpContext?.Request.Query["userId"].ToString() ?? throw new HubException("Cannot get other user");
        string groupName = GetGroupName(GetUserId(), otherUserId);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName);

        var messages = await messageRepository.GetMessageThread(GetUserId(), otherUserId);
        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        Member? sender = await memberRepository.GetMemberByIdAsync(GetUserId());
        Member? recipient = await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);
        if (recipient is null || sender is null || sender.Id == createMessageDto.RecipientId)
            throw new HubException("Cannot send this message");

        Message message = new()
        {
            Sender = sender,
            Recipient = recipient,
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.Id, recipient.Id);
        var group = await messageRepository.GetMessageGroup(groupName);
        bool userInGroup = group is not null && group.Connections.Any(x => x.UserId == message.RecipientId);

        if (userInGroup)
        {
            message.DateRead = DateTime.UtcNow;
        }

        messageRepository.AddMessage(message);
        if (await messageRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.Id);
            if (connections is not null && connections.Count > 0 && !userInGroup)
            {
                await presenceHub.Clients.Clients(connections)
                    .SendAsync("NewMessageReceived", message.ToDto());
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await messageRepository.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> AddToGroup(string groupName)
    {
        var group = await messageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, GetUserId());

        if (group is null)
        {
            group = new Group(groupName);
            messageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);
        return await messageRepository.SaveAllAsync();
    }

    private string GetGroupName(string caller, string otherUser)
    {
        bool stringCompare = string.CompareOrdinal(caller, otherUser) < 0;
        return stringCompare ? $"{caller}-{otherUser}" : $"{otherUser}-{caller}";
    }

    private string GetUserId() => Context.User?.GetMemberId() ?? throw new HubException("Cannot get user ID");
}