using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MoreLinq;
using Rozmawiator.Controls;
using Rozmawiator.Data;
using Rozmawiator.Models;

namespace Rozmawiator.PartialViews
{
    /// <summary>
    /// Interaction logic for ChatLogControl.xaml
    /// </summary>
    public partial class MessageDisplayControl : UserControl
    {
        public Conversation Conversation { get; }

        public MessageDisplayControl(Conversation conversation, Guid? priorTo = null)
        {
            InitializeComponent();
            Conversation = conversation;
            DisplayMessages(priorTo);
        }

        private async void DisplayMessages(Guid? priorTo = null)
        {
            await Conversation.GetMoreMessages();

            var priorToMessage = Conversation.Messages.FirstOrDefault(m => m.Id == priorTo);
            var messages = priorToMessage != null
                ? Conversation.Messages.Where(m => m.Timestamp < priorToMessage.Timestamp).OrderBy(m => m.Timestamp)
                : Conversation.Messages.OrderBy(m => m.Timestamp);
            
            foreach (var message in messages)
            {
                AddMessageControl(message);
            }

            SetSenderInfoVisibility();
        }

        public void AddMessageControl(TextMessage message)
        {
            var lastBubble = MessagesPanel.Children.OfType<MessageControl>().Any()
                ? MessagesPanel.Children.OfType<MessageControl>().MaxBy(m => m.Message.Timestamp)
                : null;
            
            var bubble = new MessageControl(message)
            {
                Margin = new Thickness(5),
            };

            if (bubble.Message.Sender == UserService.LoggedUser)
            {
                bubble.IsSent = true;
                bubble.Bubble.Background = new SolidColorBrush(Colors.LightGray);
            }
            else if (bubble.Message.Sender == lastBubble?.Message.Sender)
            {
                bubble.SenderInfoVisibility = false;
            }

            MessagesPanel.Children.Add(bubble);
        }

        public void RemoveMessageControl(TextMessage message)
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
                if (bubble.Message.Sender == UserService.LoggedUser)
                {
                    bubble.SenderInfoVisibility = false;
                    previousMessage = bubble;
                    continue;
                }
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
            var oldMessages = args.OldItems as IList<TextMessage>;
            var newMessages = args.NewItems as IList<TextMessage>;

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
