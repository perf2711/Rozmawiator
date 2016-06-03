using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Shared;

namespace Rozmawiator.ClientApi
{
    public class RemoteClient
    {
        public short Id { get; }
        public string Nickname { get; }

        public RemoteClient(short id, string nickname)
        {
            Id = id;
            Nickname = nickname;
        }
    }
}
