using System;
using System.Threading;
using Xunit;

namespace DotNetCircuitBreakerUnitTest
{
    public class CircuitBreakerTest
    {
		private static void CallXAmountOfTimes(Action codeToCall, int timesToCall)
		{
			for (int i = 0; i < timesToCall; i++)
			{
				codeToCall();
			}
		}

		[Fact]
        public void GivenCircuitBreaker_WhenAttemptCallCallOnCloseCircuit_ThenCallMethod()
		{
            // Arrange
			bool protectedCodeWasCalled = false;
			Action protectedCode = () => protectedCodeWasCalled = true;
			var circuitBreaker = new  DotNetCircuitBreaker.CircuitBreaker(10, TimeSpan.FromMinutes(5));

            // Act
			circuitBreaker.AttemptCall(protectedCode);

            // Assert
			Assert.True(protectedCodeWasCalled);
		}

		[Fact]
		public void GivenCircuitBreaker_WhenAttemptCallCallOnCloseCircuit_ThenFailureCountRemainToZero()
		{
            // Arrange
			Action protectedCode = () => { return; };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(10, TimeSpan.FromMinutes(5));

            // Act
			circuitBreaker.AttemptCall(protectedCode);

            // Assert
			Assert.Equal(0, circuitBreaker.Failures);
		}

		[Fact]
		public void GivenCircuitBreaker_WhenConstructorWithInvalidThreshold_ThenThrowException()
		{
            Assert.Throws<ArgumentOutOfRangeException>(() => new DotNetCircuitBreaker.CircuitBreaker(0, TimeSpan.FromMinutes(5)));
		}

        [Fact]
		public void GivenCircuitBreaker_WhenConstructorWithInvalidTimeOut_ThenThrowException()
		{
            Assert.Throws<ArgumentOutOfRangeException>(() => new DotNetCircuitBreaker.CircuitBreaker(1, TimeSpan.Zero));
        }



		[Fact]
		public void GivenCircuitBreaker_WhenFailure_ThenFailureCountIncrease()
		{
            // Arrange
			Action protectedCode = () => { throw new Exception(); };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(10, TimeSpan.FromMinutes(5));

            // Act
			circuitBreaker.AttemptCall(protectedCode);

            // Assert
			Assert.Equal(1, circuitBreaker.Failures);
		}

        [Fact]
        public void GivenCircuitBreaker_WhenFailure_ThenLastExceptionIsKept()
        {
            // Arrange
            Action protectedCode = () => { throw new Exception("Message"); };
            var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(10, TimeSpan.FromMinutes(5));

            // Act
            circuitBreaker.AttemptCall(protectedCode);

            // Assert
            Assert.Equal("Message", circuitBreaker.GetExceptionFromLastAttemptCall().Message);
        }

        [Fact]
        public void GivenCircuitBreaker_WhenFailureThenSuccess_ThenLastExceptionIsNull()
        {
            // Arrange
            Action protectedCode = () => { throw new Exception("Message"); };
            var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(10, TimeSpan.FromMinutes(5));

            // Act
            circuitBreaker.AttemptCall(protectedCode);
            circuitBreaker.AttemptCall(() => { });

            // Assert
            Assert.Null(circuitBreaker.GetExceptionFromLastAttemptCall());
        }

		[Fact]
		public void GivenCircuitBreaker_WhenNew_ThenClose()
		{
            // Arrange & Act
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(5, TimeSpan.FromMinutes(5));

            // Assert
			Assert.True(circuitBreaker.IsClosed);
		}

		[Fact]
		public void GivenCircuitBreakerClose_WhenThresholdIsReached_ThenOpen()
		{
            // Arrange
		    const int THRESHOLD = 10;
			Action protectedCode = () => { throw new Exception(); };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMinutes(5));

            // Act
			CallXAmountOfTimes(() => circuitBreaker.AttemptCall(protectedCode), THRESHOLD);

            // Assert
			Assert.True(circuitBreaker.IsOpen);
		}

		[Fact]
		public void GivenCircuitBreakerOpen_WhenCallAgain_ThenNotCallNotExecuted()
		{
            // Arrange
		    const int THRESHOLD_INVOCATION = 10;
		    int countInvocation = 0;
			Action protectedCode = () =>
			{
			    countInvocation++;
                throw new Exception(); 
            };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD_INVOCATION, TimeSpan.FromMinutes(5));
			CallXAmountOfTimes(() => circuitBreaker.AttemptCall(protectedCode), THRESHOLD_INVOCATION);

            // Act
            Assert.Equal(THRESHOLD_INVOCATION, countInvocation);
		    circuitBreaker.AttemptCall(protectedCode);

            // Assert
            Assert.Equal(THRESHOLD_INVOCATION, countInvocation);
		}

		[Fact]
		public void GivenCircuitBreakerOpen_WhenTimeIfPassed_ThenHalfOpen()
		{
            // Arrange
		    const int THRESHOLD = 2;
			Action protectedCode = () => { throw new Exception(); };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMilliseconds(10));
			CallXAmountOfTimes(()=>circuitBreaker.AttemptCall(protectedCode), THRESHOLD);
			
            // Act
            Thread.Sleep(25);

            // Assert
			Assert.True(circuitBreaker.IsHalfOpen);
		}

		[Fact]
		public void GivenCircuitBreakerHalfOpen_WhenException_ThenOpen()
		{
            // Arrange
		    const int THRESHOLD = 2;
			Action protectedCode = () => {throw new Exception(); };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMilliseconds(10));
			CallXAmountOfTimes(()=>circuitBreaker.AttemptCall(protectedCode), THRESHOLD);
			Thread.Sleep(25); //This set is back to HalfOpen
            Assert.True(circuitBreaker.IsHalfOpen);

            // Act
		    circuitBreaker.AttemptCall(protectedCode);

            // Act
			Assert.True(circuitBreaker.IsOpen);
		}

		[Fact]
		public void GivenCircuitBreakerHalfOpen_WhenNoException_ThenClose()
		{
            // Arrange
		    const int THRESHOLD = 2;
			Action protectedCode = () => {throw new Exception(); };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMilliseconds(10));
			CallXAmountOfTimes(()=>circuitBreaker.AttemptCall(protectedCode), THRESHOLD);
			Thread.Sleep(25); //This set is back to HalfOpen
            Assert.True(circuitBreaker.IsHalfOpen);

            // Act
		    circuitBreaker.AttemptCall(() => { });
            
            // Act
			Assert.True(circuitBreaker.IsClosed);
		}

        [Fact]
		public void GivenCircuitBreakerHalfOpen_WhenNoException_ThenFailureCountZero()
		{
            // Arrange
		    const int THRESHOLD = 2;
			Action protectedCode = () => {throw new Exception(); };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMilliseconds(10));
			CallXAmountOfTimes(()=>circuitBreaker.AttemptCall(protectedCode), THRESHOLD);
			Thread.Sleep(25); //This set is back to HalfOpen
            Assert.True(circuitBreaker.IsHalfOpen);

            // Act
		    circuitBreaker.AttemptCall(() => { });
            
            // Act
			Assert.Equal(0, circuitBreaker.Failures);
		}

		[Fact]
		public void GivenCircuitBreakerOpen_WhenManuallyClose_ThenClosed()
		{
            // Arrange
		    const int THRESHOLD = 2;
			Action protectedCode = () => {throw new Exception(); };
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMinutes(5));
			CallXAmountOfTimes(()=>circuitBreaker.AttemptCall(protectedCode), THRESHOLD);
			Assert.True(circuitBreaker.IsOpen);

            // Act
			circuitBreaker.Close();

            // Assert
			Assert.True(circuitBreaker.IsClosed);
		}

		[Fact]
		public void GivenCircuitBreakerClose_WhenManuallyOpen_ThenOpen()
		{
            // Arrange
		    const int THRESHOLD = 2;
			var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMinutes(5));
			Assert.True(circuitBreaker.IsClosed);

            // Act
			circuitBreaker.Open();

            // Assert
			Assert.True(circuitBreaker.IsOpen);
		}

        [Fact]
        public void GivenCircuitBreakerClose_WhenOpenAndStateChangeSubscribed_ThenCallEventStateChangeOnce()
        {
            // Arrange
            int changeStateCount = 0;
            const int THRESHOLD = 2;
            var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMinutes(5));
            circuitBreaker.OnStateChange += (o, state) => { changeStateCount++; };
            Assert.True(circuitBreaker.IsClosed);

            // Act
            circuitBreaker.Open();

            // Assert
            Assert.Equal(1, changeStateCount);
        }

        [Fact]
        public void GivenCircuitBreakerClose_WhenOpenTwiceAndStateChangeSubscribed_ThenCallEventStateChangeOnce()
        {
            // Arrange
            int changeStateCount = 0;
            const int THRESHOLD = 2;
            Action protectedCode = () => { throw new Exception(); };
            var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMinutes(5));
            circuitBreaker.OnStateChange += (o, state) => { changeStateCount++; };
            Assert.True(circuitBreaker.IsClosed);

            // Act
            CallXAmountOfTimes(() => circuitBreaker.AttemptCall(protectedCode), THRESHOLD);
            CallXAmountOfTimes(() => circuitBreaker.AttemptCall(protectedCode), THRESHOLD);

            // Assert
            Assert.Equal(1, changeStateCount);
        }

        [Fact]
        public void GivenCircuitBreakerClose_WhenOpenAndCloseAndStateChangeSubscribed_ThenCallEventStateChangeTwice()
        {
            // Arrange
            int changeStateCount = 0;
            const int THRESHOLD = 2;
            Action protectedCode = () => { throw new Exception(); };
            var circuitBreaker = new DotNetCircuitBreaker.CircuitBreaker(THRESHOLD, TimeSpan.FromMilliseconds(10));
            circuitBreaker.OnStateChange += (o, state) => { changeStateCount++; };
            Assert.True(circuitBreaker.IsClosed);

            // Act
            CallXAmountOfTimes(() => circuitBreaker.AttemptCall(protectedCode), THRESHOLD);
            Thread.Sleep(25); //This set is back to HalfOpen
            circuitBreaker.AttemptCall(() => { });

            // Assert
            Assert.Equal(3, changeStateCount); //Close->Open, Open->Half, Half->Close
        }


    }
}
