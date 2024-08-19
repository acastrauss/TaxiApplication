using AzureStorageWrapper.Entities;
using Contracts.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Models.Chat;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.ServiceModel.Channels;

namespace TaxiWeb
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        private readonly IBussinesLogic bussinesLogic;
        public ChatHub(IBussinesLogic bussinesLogic) 
        {
            this.bussinesLogic = bussinesLogic;
        }

        [Authorize]
        public override async Task OnConnectedAsync()
        {
            var userEmailClaim = Context.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            if (userEmailClaim == null)
            {
                return;
            }
            bool added = ConnectedUsers.TryAdd(Context.ConnectionId, userEmailClaim.Value);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedUsers.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }

        [Authorize]
        public async Task CreateNewOrGetExistingChat(Models.Chat.Chat chat)
        {
            var userEmailClaim = Context.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            if (userEmailClaim == null) 
            {
                return;
            }

            var createdChat = await bussinesLogic.CreateNewOrGetExistingChat(chat);

            if (createdChat == null) 
            {
                return;
            }

            var connectionId = ConnectedUsers.FirstOrDefault(x => x.Value == userEmailClaim.Value).Key;
            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("CreateOrGetChat", userEmailClaim.Value, chat);
            }
        }

        [Authorize]
        public async Task SendMessage(Models.Chat.ChatMessage message)
        {
            var userEmailClaim = Context.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            if (userEmailClaim == null)
            {
                return;
            }
            
            var createdMessage = await bussinesLogic.AddNewMessageToChat(message);

            if(createdMessage == null)
            {
                return;
            }
            var connectionIdClient = ConnectedUsers.FirstOrDefault(x => x.Value == createdMessage.ClientEmail).Key;
            if (connectionIdClient != null)
            {
                await Clients.Client(connectionIdClient).SendAsync("ReceiveMessage", createdMessage.ClientEmail, createdMessage);
            }
            
            var connectionIdDriver = ConnectedUsers.FirstOrDefault(x => x.Value == createdMessage.DriverEmail).Key;
            if (connectionIdDriver != null)
            {
                await Clients.Client(connectionIdDriver).SendAsync("ReceiveMessage", createdMessage.DriverEmail, createdMessage);
            }
        }
    }
}
