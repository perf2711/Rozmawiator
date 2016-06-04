using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Rozmawiator.Controls;
using Rozmawiator.Models;

namespace Rozmawiator.PartialViews
{
    /// <summary>
    /// Interaction logic for ChatLogControl.xaml
    /// </summary>
    public partial class MessageDisplayControl : UserControl
    {
        public ObservableCollection<ChatMessage> Messages { get; }

        public MessageDisplayControl()
        {
            InitializeComponent();
            Messages = new ObservableCollection<ChatMessage>();
            Messages.CollectionChanged += OnMessagesChanged;
        }

        private void AddMessageControl(ChatMessage message)
        {
            var bubble = new MessageControl
            {
                Message = message,
                Margin = new Thickness(5)
            };

            MessagesPanel.Children.Add(bubble);
        }

        private void RemoveMessageControl(ChatMessage message)
        {
            var bubble = MessagesPanel.Children.OfType<MessageControl>().FirstOrDefault(b => b.Message == message);
            if (bubble != null)
            {
                MessagesPanel.Children.Remove(bubble);
            }
        }

        private void SetSenderInfoVisibility()
        {
            MessageControl previousMessage = null;

            foreach (var bubble in MessagesPanel.Children.OfType<MessageControl>())
            {
                if (previousMessage == null)
                {
                    previousMessage = bubble;
                    continue;
                }

                bubble.SenderInfoVisibility = previousMessage.Message.Sender != bubble.Message.Sender;
                previousMessage = bubble;
            }
        }

        private void OnMessagesChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var oldMessages = args.OldItems as IList<ChatMessage>;
            var newMessages = args.NewItems as IList<ChatMessage>;

            if (oldMessages != null)
            {
                foreach (var message in oldMessages)
                {
                    RemoveMessageControl(message);
                }
            }

            if (newMessages != null)
            {
                foreach (var message in newMessages)
                {
                    AddMessageControl(message);
                }
            }

            SetSenderInfoVisibility();
        }
    }
}
