using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRChatSample.Backend.Hubs
{
    public class ChatHub : Hub
    {
        public ChatHub()
        {

        }

        /// <summary>
        /// 채팅방에 들어옴
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task AddToGroup(string groupName, string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Entered", user);
        }

        /// <summary>
        /// 채팅방에서 나감
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task RemoveFromGroup(string groupName, string user)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Left", user);
        }

        /// <summary>
        /// 채팅방에 메시지 전송
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }


    }
}
