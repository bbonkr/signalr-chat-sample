using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRChatSample.Shared.Core.EventHandlers;

namespace SignalRChatSample.Shared.Core
{
    public class ChatService
    {
        public event EventHandler<MessageEventArgs> OnReceiveMessage;
        public event EventHandler<MessageEventArgs> OnEnteredOrExisted;
        public event EventHandler<MessageEventArgs> OnConnectionClosed;

        public void Init(string urlRoot, bool useHttps)
        {
            random = new Random();

            var port = (urlRoot == "localhost" || urlRoot == "10.0.2.2") ? (useHttps ? ":5001" : ":5000") : String.Empty;

            var url = $"http{(useHttps ? "s" : String.Empty)}://{urlRoot}{port}/hub/chat";

            hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            hubConnection.Closed += async (error) =>
            {
                OnConnectionClosed?.Invoke(this, new MessageEventArgs("Connection closed...", String.Empty));
                IsConnected = false;
                await Task.Delay(random.Next(0, 5) * 1000);

                try
                {
                    await ConnectAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            };

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                OnReceiveMessage?.Invoke(this, new MessageEventArgs(message, user));
            });

            hubConnection.On<string>("Entered", (user) =>
            {
                OnEnteredOrExisted?.Invoke(this, new MessageEventArgs($"{user} entered...", user));
            });

            hubConnection.On<string>("Left", (user) =>
            {
                OnEnteredOrExisted?.Invoke(this, new MessageEventArgs($"{user} left...", user));
            });

            hubConnection.On<string>("EnterOrLeft", (message) =>
            {
                OnEnteredOrExisted?.Invoke(this, new MessageEventArgs(message, message));
            });
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
            {
                return;
            }

            try
            {
                await hubConnection.StartAsync();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected) { return; }

            try
            {
                await hubConnection.DisposeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            ActiveChannels.Clear();
            IsConnected = false;
        }

        public async Task LeaveChannelAsync(string group, string user)
        {
            if (!IsConnected || !ActiveChannels.ContainsKey(group))
            {
                return;
            }

            await hubConnection.SendAsync("RemoveFromGroup", group, user);

            ActiveChannels.Remove(group);
        }

        public async Task JoinChannelAsync(string group, string user)
        {
            if (!IsConnected || ActiveChannels.ContainsKey(group))
            {
                return;
            }

            await hubConnection.SendAsync("AddToGroup", group, user);
            ActiveChannels.Add(group, user);
        }

        public async Task SendMessageAsync(string group, string user, string message)
        {
            if (!IsConnected)
            {
                return;
            }

            await hubConnection.InvokeAsync("SendMessageGroup", group, user, message);
        }

        public List<string> GetRooms()
        {
            return new List<string>
            {
                ".NET",
                "ASP.NET",
                "Xamarin"
            };
        }

        private bool IsConnected { get; set; }
        private Dictionary<string, string> ActiveChannels { get; } = new Dictionary<string, string>();

        private HubConnection hubConnection;
        private Random random;

    }
}
