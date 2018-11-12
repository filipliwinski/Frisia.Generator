# Frisia.Generator
Parameter values generator for parameterized unit tests.

### Prerequisites

- .NET Core 2.1

### Safety warning

Frisia.Generator uses [Roslyn Scripting API](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples) 
to examine provided code samples. In current state it does not use any mocking 
framework or sandbox for executing C# code. It is highly recommended to check if
the code provided is safe to run before executing Fisia.Generator.

The code you provide will be executed _as is_ and may cause damage to your environment.

### How to build

Frisia.Generator uses two other projects: [Frisia.Rewriter](https://github.com/filipliwinski/Frisia.Rewriter) and
[Frisia.Solver](https://github.com/filipliwinski/Frisia.Solver). Both are required to successfuly build Frisia.Generator 
and Frisia.Generator.Demo projects. After downloading all the source code, 
include Rewriter and Solver projects in Frisia.Generator solution by using 
_Add > Existing project..._ command in Visual Studio.




