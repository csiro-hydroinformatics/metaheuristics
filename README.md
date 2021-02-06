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

The project CSIRO.Metaheuristics.Parallel uses a modified version the library MPI.NET, currently hosted at [MPI.NET on github](https://github.com/mpidotnet/MPI.NET). Due to platform and version specific aspects of the native MPI libraries, a successful compilation is not trivial. Introductory documentation will not depend on this feature until a build process is documented, so you can ignore CSIRO.Metaheuristics.Parallel for the time being. 

## Getting started

You will find a series of tutorials under the Documentation folder. 

## Related work

[A C++ metaheuristics framework](https://github.com/csiro-hydroinformatics/wila)

## Building

### Windows, visual studio:

In a VS dev prompt for instance: 

```bat
%comspec% /k "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvars64.bat"
```

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

```bat
cd C:\src\github_jm\metaheuristics\build
cd output

:: csiro. *packages
set api_key=blahblahblahblahblahblahblahblahblahblahblahblah

:: Jan 2021 trying dotnet since nuget.exe does not work????
set pkg_ver=0.7.13
set push_opt= --api-key %api_key% --source https://api.nuget.org/v3/index.json

dotnet nuget push CSIRO.Utilities.%pkg_ver%.nupkg %push_opt%
dotnet nuget push CSIRO.Sys.%pkg_ver%.nupkg %push_opt%
dotnet nuget push CSIRO.Modelling.Core.%pkg_ver%.nupkg %push_opt%
dotnet nuget push CSIRO.Metaheuristics.%pkg_ver%.nupkg %push_opt%
dotnet nuget push CSIRO.Metaheuristics.DataModel.%pkg_ver%.nupkg %push_opt%
dotnet nuget push CSIRO.Metaheuristics.Logging.%pkg_ver%.nupkg %push_opt%
dotnet nuget push CSIRO.Metaheuristics.Parallel.%pkg_ver%.nupkg %push_opt%
dotnet nuget push CSIRO.Metaheuristics.R.%pkg_ver%.nupkg %push_opt%
dotnet nuget push CSIRO.Metaheuristics.Tests.%pkg_ver%.nupkg %push_opt%
```
