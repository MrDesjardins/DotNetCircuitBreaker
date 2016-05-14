using System;

namespace DotNetCircuitBreaker
{
    public class HalfOpenState : CircuitBreakerState
    {
        public HalfOpenState(CircuitBreaker circuitBreaker) : base(circuitBreaker) { }

        public override void ActUponException(Exception e)
        {
            base.ActUponException(e);
            this.circuitBreaker.MoveToOpenState();
        }

        public override void ProtectedCodeHasBeenCalled()
        {
            base.ProtectedCodeHasBeenCalled();
            this.circuitBreaker.MoveToClosedState();
        }
    }
}