using System;
using System.Threading.Tasks;
using SignalRChatSample.Shared.Core;

namespace SignalRChatSample.ConsoleApp
{
    class Program
    {
        static private ChatService service;
        static Random random = new Random();

        public static async Task Main(string[] args)
        {
            var name = $"Console {random.Next(0, 10000)}";
            service = new ChatService();
            service.OnReceiveMessage += (s, e) =>
            {
                if (e.User == name) { return; }

                Console.WriteLine($"{e.User}: {Environment.NewLine}{e.Message}");
            };

            service.OnEnteredOrExisted += (s, e) =>
            {
                Console.WriteLine($"{e.User} entered.");
            };

            Console.WriteLine("Enter server Ip address:");
            var ipAddress = Console.ReadLine();

            service.Init(ipAddress, !IsLoopbackIpAddress(ipAddress));

            await service.ConnectAsync();
            Console.WriteLine("You are connected...");

            var room = await joinRoom(name);

            var keepConnected = true;

            do
            {
                var text = Console.ReadLine();
                if (text.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    keepConnected = false;
                }
                else if (text.Trim().Equals("leave", StringComparison.OrdinalIgnoreCase))
                {
                    await service.LeaveChannelAsync(room, name);

                    await joinRoom(name);
                }
                else
                {
                    await service.SendMessageAsync(room, name, text.Trim());
                }
            }
            while (keepConnected);
        }

        private static async Task<string> joinRoom(string name)
        {
            Console.WriteLine($"Enter room ({String.Join(", ", service.GetRooms())}):");
            var selectedRoom = Console.ReadLine();

            await service.JoinChannelAsync(selectedRoom, name);

            Console.WriteLine($"You join a Room {selectedRoom}");

            return selectedRoom;
        }

        private static bool IsLoopbackIpAddress(string ip)
        {
            var isLoopback = ip.Trim().Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                ip.Trim().Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                ip.Trim().Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase);

            return isLoopback;
        }
    }
}
