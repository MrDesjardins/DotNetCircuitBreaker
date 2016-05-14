using System;

namespace DotNetCircuitBreaker
{
    public abstract class CircuitBreakerState
    {
        protected readonly CircuitBreaker circuitBreaker;

        protected CircuitBreakerState(CircuitBreaker circuitBreaker)
        {
            this.circuitBreaker = circuitBreaker;
        }

        public virtual CircuitBreaker ProtectedCodeIsAboutToBeCalled()
        {
            return this.circuitBreaker;
        }
        public virtual void ProtectedCodeHasBeenCalled() { }
        public virtual void ActUponException(Exception e)
        {
            this.circuitBreaker.IncreaseFailureCount();
        }

        public virtual CircuitBreakerState Update()
        {
            return this;
        }
    }
}
