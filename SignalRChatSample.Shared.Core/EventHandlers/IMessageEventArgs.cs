using System;
namespace SignalRChatSample.Shared.Core.EventHandlers
{
    public interface IMessageEventArgs
    {
        string Message { get; }
    }
}
