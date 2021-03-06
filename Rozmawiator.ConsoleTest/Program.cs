﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.ClientApi;
using Rozmawiator.Shared;

namespace Rozmawiator.ConsoleTest
{
    class Program
    {
        private static Client _client;

        private static void Main(string[] args)
        {
            _client = new Client();
            Console.Write("Nickname: ");
            _client.Nickname = Console.ReadLine();
            _client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
            _client.Connected += ClientOnConnected;
            _client.NewMessage += ClientOnNewMessage;

            while (true)
            {
                Console.Write("Receiver: ");
                var receiver = Console.ReadLine();
                Console.Write("Message: ");
                var text = Console.ReadLine();
                var message = new Message().DirectText(receiver, text);
                _client.Send(message);
                LogSelfMessage(message);
            }
        }

        private static void ClientOnNewMessage(Client client, Message message)
        {
            LogMessage(message);
        }

        private static void LogMessage(Message message, bool appendContent = true)
        {
            Log($"<{message.Type}> {message.GetDirectTextNickname() ?? ""} " +  (appendContent ? message.GetTextContent() : ""));
        }

        private static void LogSelfMessage(Message message, bool appendContent = true)
        {
            Log($"You <{message.Type}> " + (appendContent ? message.GetTextContent() : ""));
        }

        private static void Log(string text, bool appendDate = true)
        {
            Console.WriteLine((appendDate ? $"[{DateTime.Now.ToString("T")}]" : "") + $" {text}\n");
        }

        private static void ClientOnConnected(Client client, Message message)
        {
            LogMessage(message);
        }
    }
}
