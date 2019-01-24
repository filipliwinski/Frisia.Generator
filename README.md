# Frisia.Generator
Parameter values generator for parameterized unit tests.

### Prerequisites

- [NET Core 2.1](https://www.microsoft.com/net/download)

### ⚠ Safety warning 

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
*Add > Existing project...* command in Visual Studio.

#### `Using dotnet`

For building with `dotnet build`, it is reqired to update project references manually, 
then in the Frisia.Generator.Demo project directory:

    dotnet build

You will get some warnings about *z3x64win* compatiblity but you can ignore it.

### How to run

#### `Using dotnet`
**Do not** execute `dotnet run` in the Frisia.Generator.Demo directory containing 
source files. Instead go to `bin\Debug\netcoreapp2.1` and run:

    dotnet .\Frisia.Generator.Demo.dll

Demo app will search for all .cs files in the current directory. You can specify certain 
file names as arguments:

    dotnet .\Frisia.Generator.Demo.dll file1.cs file2.cs

Results are saved in `fileName.cs.json` file.

[Here](https://github.com/filipliwinski/Frisia.Generator/raw/master/demo/win10-x64.zip) you can download compiled (self-contained) demo app for Windows.

#### Settings for Demo app

You can modify default settings in `appsettings.json` file.

| Setting | Default | Description |
| ------- | ------- | ----------- |
| LoopIterations | 1 | Number of iterations of loops content (more than 4 is not recommended). |
| VisitUnsatisfiablePaths | false | If true, Rewritter will folow unsatisfiable paths (used only for testing purposes). |
| VisitTimeoutPaths | false | If true, Rewritter will folow paths regardless of timeout. |
| LogFoundBranches | false | If true, found branches are written to console and file log. |
| WriteRewrittenCodeToFile | false | If true, code rewritten with Frisia.Rewriter is saved to file. |
| TimeoutInSeconds | 5 | Timeout value for performing runs with generated parameters. |

#### Limitations

Will be updated...


