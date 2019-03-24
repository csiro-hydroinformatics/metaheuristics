# A simplified metaheuristics framework for .NET

master: [![Build status](https://ci.appveyor.com/api/projects/status/g03tt0taej6s4i35/branch/master?svg=true)](https://ci.appveyor.com/project/jmp75/metaheuristics/branch/master)
testing: [![Build status](https://ci.appveyor.com/api/projects/status/g03tt0taej6s4i35/branch/testing?svg=true)](https://ci.appveyor.com/project/jmp75/metaheuristics/branch/testing)

## Purpose

This framework is designed to incorporate the state of the art in metaheuristics software frameworks, yet limiting the software complexity to users who are interested in applying it without advanced knowledge of software or optimisation research. It has been used mainly to calibrate environmental models, mostly hydrology models.

The purpose of this framework is to define a set of programming interfaces, rather than replicate optimisation algorithms found in other optimisation frameworks.

## License

This software is released under the LGPL v2.1. See LICENSE.txt.

This code depends on third party libraries which may have a different license.

## Requirements

This framework is written in C#, and designed to compile to target .NET v4.0. Some projects depend on NUnit and Entity Framework, using NuGet packages.

It is developed and tested on Microsoft .NET 4.0 for Windows and for Linux on using Mono 3.0.x or 3.2.x and MonoDevelop 4.2. It is likely to work as is on MacOS using a similar Mono/MonoDevelop setup, but has not been tested.

Most projects in the solution use "nuget" packages to handle dependencies on third party libraries (e.g. Entity Framework, NUnit, log4net). See http://docs.nuget.org for further documentation.

The project CSIRO.Metaheuristics.Parallel uses a modified version the library MPI.NET, currently hosted at https://github.com/jmp75/MPI.NET. Due to platform and version specific aspects of the native MPI libraries, a successful compilation is not trivial. Introductory documentation will not depend on this feature until a build process is documented, so you can ignore CSIRO.Metaheuristics.Parallel for the time being. 

## Getting started

You will find a series of tutorials under the Documentation folder. 

## Related work

[A C++ metaheuristics framework](https://github.com/jmp75/wila)

## Building

```bat
cd C:\path\to\Metaheuristics.NET
:: Once migrated to .net standard
:: dotnet restore Metaheuristics.sln
:: assuming msbuild is in your PATH
msbuild Metaheuristics.sln /p:Platform="Any CPU" /p:Configuration=Release /consoleloggerparameters:ErrorsOnly
```

Nuget packages:

```bat
cd C:\path\to\metaheuristics\build\
.\build_mh_nuget.cmd
```
