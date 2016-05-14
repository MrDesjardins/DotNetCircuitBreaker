using System;

namespace DotNetCircuitBreaker
{
    public interface ICircuitBreaker
    {
        event Action<object, CircuitBreakerState> OnStateChange;
        int Failures { get; }
        int Threshold { get; }
        TimeSpan Timeout { get; }
        bool IsClosed { get; }
        bool IsOpen { get; }
        bool IsHalfOpen { get; }
        bool IsThresholdReached();
        Exception GetExceptionFromLastAttemptCall();

        /// <summary>
        /// This should be used as followed : myCircuitBreaker.AttemptCall(()=>{yourCode();}).IsClosed?"AllFine":"Something wrong";
        /// </summary>
        /// <param name="protectedCode"></param>
        /// <returns></returns>
        CircuitBreaker AttemptCall(Action protectedCode);

        void Close();
        void Open();
    }
}