using System;

namespace DotNetCircuitBreaker
{
    public class OpenState : CircuitBreakerState
    {
        private readonly DateTime openDateTime;
        public OpenState(CircuitBreaker circuitBreaker)
            : base(circuitBreaker)
        {
            this.openDateTime = DateTime.UtcNow;
        }


        public override CircuitBreaker ProtectedCodeIsAboutToBeCalled()
        {
            base.ProtectedCodeIsAboutToBeCalled();
            this.Update();
            return base.circuitBreaker;

        }

        public override CircuitBreakerState Update()
        {
            base.Update();
            if (DateTime.UtcNow >= this.openDateTime + base.circuitBreaker.Timeout)
            {
                return this.circuitBreaker.MoveToHalfOpenState();
            }
            return this;
        }
    }
}