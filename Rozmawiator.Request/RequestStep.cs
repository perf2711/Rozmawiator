using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication;

namespace Rozmawiator.Request
{
    public class RequestStep
    {
        public enum StepType
        {
            Receive,
            Send
        }

        public StepType Type { get; set; }

        public Message Message { get; set; }

        public event Func<RequestStep, bool> Complete;

        public void Finish()
        {
            Complete?.Invoke(this);
        }
    }
}
