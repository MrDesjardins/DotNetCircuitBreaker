using System;
using System.Diagnostics;

namespace DotNetCircuitBreaker
{
    public delegate void ChangedStateEventHandler(object sender, CircuitBreakerState state);

    /// <summary>
    /// Inspired by https://github.com/davybrion/CircuitBreaker/
    /// </summary>
    public class CircuitBreaker : ICircuitBreaker
    {
        private readonly object monitor = new object();
        private CircuitBreakerState state;
        public event Action<object, CircuitBreakerState> OnStateChange;
        public CircuitBreaker(int threshold, TimeSpan timeout)
        {
            if (threshold < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), @"Threshold should be greater than 0");
            }

            if (timeout.TotalMilliseconds < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), @"Timeout should be greater than 0");
            }

            this.Threshold = threshold;
            this.Timeout = timeout;
            this.MoveToClosedState();
        }

        public int Failures { get; private set; }
        public int Threshold { get; }
        public TimeSpan Timeout { get; }
        public bool IsClosed => this.state.Update() is ClosedState;

        public bool IsOpen => this.state.Update() is OpenState;

        public bool IsHalfOpen => this.state.Update() is HalfOpenState;

        internal CircuitBreakerState MoveToClosedState()
        {
            this.state = new ClosedState(this);
            this.NotifyStateChange(this.state);
            return this.state;
        }

        internal CircuitBreakerState MoveToOpenState()
        {
            this.state = new OpenState(this);
            this.NotifyStateChange(this.state);
            return this.state;
        }

        internal CircuitBreakerState MoveToHalfOpenState()
        {
            this.state = new HalfOpenState(this);
            this.NotifyStateChange(this.state);
            return this.state;
        }

        internal void IncreaseFailureCount()
        {
            this.Failures++;
        }

        internal void ResetFailureCount()
        {
            this.Failures = 0;
        }

        public bool IsThresholdReached()
        {
            return this.Failures >= this.Threshold;
        }

        private Exception exceptionFromLastAttemptCall = null;

        public Exception GetExceptionFromLastAttemptCall()
        {
            return this.exceptionFromLastAttemptCall;
        }

        /// <summary>
        /// This should be used as followed : myCircuitBreaker.AttemptCall(()=>{yourCode();}).IsClosed?"AllFine":"Something wrong";
        /// </summary>
        /// <param name="protectedCode"></param>
        /// <returns></returns>
        public CircuitBreaker AttemptCall(Action protectedCode)
        {
            this.exceptionFromLastAttemptCall = null;
            lock(this.monitor)
            {
                this.state.ProtectedCodeIsAboutToBeCalled();
                if (this.state is OpenState)
                {
                    return this; // Stop execution of this method
                }
            }

            try
            {
                protectedCode();
            }
            catch (Exception e)
            {
                this.exceptionFromLastAttemptCall = e;
                lock(this.monitor)
                {
                    this.state.ActUponException(e);
                }
                return this; // Stop execution of this method
            }

            lock (this.monitor)
            {
                this.state.ProtectedCodeHasBeenCalled();
            }
            return this;
        }

        public void Close()
        {
            lock (this.monitor)
            {
                this.MoveToClosedState();
            }
        }

        public void Open()
        {
            lock (this.monitor)
            {
                this.MoveToOpenState();
            }
        }

        /// <summary>
        /// Call the stage change event
        /// </summary>
        /// <param name="state"></param>
        private void NotifyStateChange(CircuitBreakerState state)
        {
            this.OnStateChange?.Invoke(this, state);
        }
    }
}