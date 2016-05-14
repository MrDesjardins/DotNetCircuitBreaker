# Asp.Net Mvc Easy Routing

[![Build Status](https://travis-ci.org/MrDesjardins/DotNetCircuitBreaker.svg?branch=master)](https://travis-ci.org/MrDesjardins/DotNetCircuitBreaker)

[![DotNetCircuitBreaker MyGet Build Status](https://www.myget.org/BuildSource/Badge/DotNetCircuitBreaker?identifier=6e094bf3-a00b-4fff-a9de-215d8db5e30f)](https://www.myget.org/)

[![NuGet Badge](https://buildstats.info/nuget/DotNetCircuitBreaker)](https://www.nuget.org/packages/DotNetCircuitBreaker/)

[![Build history](https://buildstats.info/travisci/chart/MrDesjardins/DotNetCircuitBreaker)](https://travis-ci.org/MrDesjardins/DotNetCircuitBreaker)

.Net Circuit Breaker is a simple implementation of the circuit breaker pattern.

This allow to have Area and Controller based route in Asp.Net MVC in a Fluent way for multiple language (as this moment English and French hardcoded).

## Examples
Here is how to use the circuit breaker:

	//After 10 tries we wait 5 minutes
    var circuitBreaker = new  DotNetCircuitBreaker.CircuitBreaker(10, TimeSpan.FromMinutes(5));
	//Execute the protectedCode
	circuitBreaker.AttemptCall(protectedCode);

It's possible to know when it's been executed or not and log or do what ever is required.


	circuitBreaker.AttemptCall(protectedCode);
	if(circuitBreaker.IsOpen)
	{
		//Open = Something went wrong.
	}
	
You can also listen to an event when the state of the circuit breaker change:

	circuitBreaker.OnStateChange += (o, state) => { /*Do what you want*/ };

## Documentations

You can find more detail here:
 - [How to Create a Simple Circuit Breaker in C#](http://patrickdesjardins.com/blog/how-to-create-a-simple-circuit-breaker-in-c)

 
## Nuget Package
 You can find a Nuget package of this project at [Nuget.org](https://www.nuget.org/packages/DotNetCircuitBreaker/)