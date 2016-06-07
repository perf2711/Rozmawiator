using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication;

namespace Rozmawiator.Request
{
    public class ComplexRequest
    {
        private int _currentIndex = 0;
        private readonly RequestStep[] _steps;

        public Guid Id { get; }
        public IEnumerable<RequestStep> Steps => _steps.AsEnumerable();
        public RequestStep CurrentStep => _steps[_currentIndex];

        public event Action<ComplexRequest, RequestStep> SendStep;

        public ComplexRequest(Guid id, params RequestStep[] steps)
        {
            Id = id;
            _steps = steps;
        }

        public void Receive(Message message)
        {
            if (message.RequestId != Id)
            {
                return;
            }

            CurrentStep.Finish();
            NextStep();
        }

        private void NextStep()
        {
            while (true)
            {
                _currentIndex++;
                if (CurrentStep.Type == RequestStep.StepType.Send)
                {
                    SendStep?.Invoke(this, CurrentStep);
                    continue;
                }
                break;
            }
        }
    }
}
