using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub(IUnitOfWork uow, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        HttpContext? httpContext = Context.GetHttpContext();
        string otherUserId = httpContext?.Request.Query["userId"].ToString() ?? throw new HubException("Cannot get other user");
        string groupName = GetGroupName(GetUserId(), otherUserId);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName);

        var messages = await uow.MessageRepository.GetMessageThread(GetUserId(), otherUserId);
        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        Member? sender = await uow.MemberRepository.GetMemberByIdAsync(GetUserId());
        Member? recipient = await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);
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
        var group = await uow.MessageRepository.GetMessageGroup(groupName);
        bool userInGroup = group is not null && group.Connections.Any(x => x.UserId == message.RecipientId);

        if (userInGroup)
        {
            message.DateRead = DateTime.UtcNow;
        }

        uow.MessageRepository.AddMessage(message);
        if (await uow.CompleteAsync())
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
        await uow.MessageRepository.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> AddToGroup(string groupName)
    {
        var group = await uow.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, GetUserId());

        if (group is null)
        {
            group = new Group(groupName);
            uow.MessageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);
        return await uow.CompleteAsync();
    }

    private string GetGroupName(string caller, string otherUser)
    {
        bool stringCompare = string.CompareOrdinal(caller, otherUser) < 0;
        return stringCompare ? $"{caller}-{otherUser}" : $"{otherUser}-{caller}";
    }

    private string GetUserId() => Context.User?.GetMemberId() ?? throw new HubException("Cannot get user ID");
}