using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Treefrog
{
    public enum MessageType
    {
        None,
        Info,
        Warning,
        Error,
    }

    public class MessageInfo
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public MessageType Type { get; set; }
    }

    public interface IMessageService
    {
        void ShowMessage (string message);
        void ShowMessage (MessageInfo info);
    }

    public class MessageService : IMessageService
    {
        public void ShowMessage (string message)
        {
            MessageBox.Show(message);
        }

        public void ShowMessage (MessageInfo info)
        {
            string title = info.Title ?? MessageTypeToTitle(info.Type);
            MessageBox.Show(info.Message, title, MessageBoxButton.OK, MessageTypeToIcon(info.Type));
        }

        private string MessageTypeToTitle (MessageType type)
        {
            switch (type) {
                case MessageType.Info:
                    return "Message";
                case MessageType.Warning:
                    return "Warning";
                case MessageType.Error:
                    return "Error";
                default:
                    return "";
            }
        }

        private MessageBoxImage MessageTypeToIcon (MessageType type)
        {
            switch (type) {
                case MessageType.Info:
                    return MessageBoxImage.Information;
                case MessageType.Warning:
                    return MessageBoxImage.Warning;
                case MessageType.Error:
                    return MessageBoxImage.Error;
                default:
                    return MessageBoxImage.None;
            }
        }
    }
}
