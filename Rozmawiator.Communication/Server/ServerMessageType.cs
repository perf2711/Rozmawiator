using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Server
{
    public enum ServerMessageType
    {
        Hello,
        Bye,
        KeepAlive,
        Ok,
        Error,
        CreateConversation
    }
}
