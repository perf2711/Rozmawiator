﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Call
{
    public static class CallMessageExtensions
    {
        public static CallMessage NewUser(this CallMessage message, Guid userId)
        {
            message.Type = CallMessageType.NewUser;
            return (CallMessage) message.AddContent(userId.ToByteArray());
        }

        public static CallMessage UserDeclined(this CallMessage message, Guid userId)
        {
            message.Type = CallMessageType.UserDeclined;
            return (CallMessage) message.AddContent(userId.ToByteArray());
        }

        public static CallMessage Bye(this CallMessage message, string reason)
        {
            message.Type = CallMessageType.Bye;
            return (CallMessage) message.AddContent(reason);
        }

        public static CallMessage UserLeft(this CallMessage message, Guid userId)
        {
            message.Type = CallMessageType.UserLeft;
            return (CallMessage)message.AddContent(userId.ToByteArray());
        }

        public static CallMessage Audio(this CallMessage message, byte[] audioData)
        {
            message.Type = CallMessageType.Audio;
            return (CallMessage) message.AddContent(audioData);
        }
    }
}
