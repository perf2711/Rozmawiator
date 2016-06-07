using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication
{
    public interface IMessage
    {
        MessageCategory Category { get; }
        Guid SenderId { get; set; }
        byte MessageType { get; set; }
        byte[] Content { get; set; }
    }
}
