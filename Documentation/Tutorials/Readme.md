# Tutorials

This folder gathers code samples illustrating how to use optimisation tools in this framework.

You can use Visual Studio (Express for Windows, or Professional or above) to open the solution file Tutorials.sln

## Hello World

```bat
cd C:\src\blah\metaheuristics\Documentation\Tutorials\HelloWorld\bin\Debug
HelloWorld.exe
```

While the traditional Hello World sample is contrived for this toolset, it can still be formulated as a problem that illustrates the fundamental concepts in this framework.

If you have to remember *one* thing from this tutorial, it is the three essential players 

- the tunable parameters of the optimisation problem: `ISystemConfiguration`, here implemented by `StringSpecification` for this 'Hello World' problem
- the object evaluating the score(s) for a given "system configuration" (`IObjectiveEvaluator<T>`) where T is the parameterization type name, here `StringSpecification`
- the optimisation algorithm, implementing `IEvolutionEngine<T>`

It is recommended that you declare variables typed as interfaces, not concrete classes, whenever possible, including in an 'Hello World' sample...

```CSharp
IObjectiveEvaluator<StringSpecification> evaluator;
IEvolutionEngine<StringSpecification> finder;
```

The body of the main function in the program is simple. The rest of the code is implemented almost purely on an as-needed basis, in a top-down fashion (indeed this is exactly how the tutorial code was written).

```CSharp
string strToFind = args.Length == 0 ? "Hello, World!" : args[0];
// TODO: implement concrete evaluator and finder;
var result = finder.Evolve();
Console.WriteLine(MetaheuristicsHelper.GetHumanReadable(result));
```

## Point time series model

```bat
cd C:\src\github_jm\metaheuristics\Documentation\Tutorials\AWBM_URS\bin\Debug
AWBM_URS.exe
```

Placeholder section - demonstrates a crude but structurally meaningful calibration of a point time series model.

## Calibrating a model written in a native language

There are many cases where simulation models are written in a native. From the point of view of the calibration, 
there is almost no difference with the previous tutorial; indeed this is a deliberate aim of the approach proposed.

This tutorial thus does not cover anything new in terms of optimisation, but it should bring much information on 
how to interoperate with a simulation model written in a native language.

The program to use as entry point is named NativeModelSample. You should be able to run it, 
provided you place the native library NativeModelCpp.dll alongside the main program.

.NET offers some good features for interoperability with native code. The technology used in this sample 
is P/Invoke. One only needs to 

```CSharp
[DllImport("NativeModelCpp.dll", EntryPoint = "CreateSimulation", CallingConvention = CallingConvention.Cdecl)]
internal static extern IntPtr CreateSimulation();
```

Specifying how to handle more complicated types such as strings and arrays can require some non-obvious know-how. 
The code in this sample should provide sample code for common cases.

```C
NATIVE_AWBM_API AwbmSimulation * CreateSimulation();
```

You are encouraged to use a similar pattern for the C API of your model, for naming preprocessor macros.

