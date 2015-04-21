mh - Access the CSIRO.Metaheuristics framework from R
=================

**mh** is an R package to access a metaheuristics framework [CSIRO.Metaheuristics](https://github.com/jmp75/metaheuristics) running on Microsoft .NET and Mono.

As of 2015-04, this package is used by the author for project work calibrating hydrologic models, on Windows and Linux. The build process has significantly improved over the past few months, but this remains a complicated package to use. It currently requires rClr 0.7-4 at least.

## Prerequisites and package dependencies

### Windows

The mh package uses the .NET framework SDK to build some C# code. Typically if you have on your machine the file "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe", you can skip this paragraph. Otherwise you need to install the [Microsoft Windows SDK for Windows 7 and .NET Framework 4](http://www.microsoft.com/en-us/download/details.aspx?id=8279), or you can install one of the community or commercial versions of Visual Studio. An overview of list of Microsoft SDKs is available [here](http://msdn.microsoft.com/en-us/vstudio/hh487283.aspx)

The interoperability of R and .NET code relies on the [rClr](http://rclr.codeplex.com/) R package. As of 2015-01 you can download an installable R package for windows (zip file). Make sure to at least skim through the [installation instructions](http://r2clr.codeplex.com/wikipage?title=Installing%20R%20packages&referringTitle=Documentation).

### Linux

As of 2015-04, the package has been used in a research and development, running on Linux with Mono 3.12.1.

some of the tools of the Mono implementation are required to install **mh**. One of the package dependencies, the rClr package, also requires it anyway. You will find suitable instructions at [Building from source rClr](https://github.com/jmp75/rClr#from-source).

## Install

### Install from precompiled mh

Placeholder for installation of Windows binary package.

### Compile and install from source

Check out the mh package from its [github repository](https://github.com/jmp75/metaheuristics) to a folder of your choice. Note that, if you already use the `devtools` package, using the `install_github` function to install mh is is _not yet_ possible; you need a customisation step as follows.

The source code in C# used by the package uses NuGet to manage its dependencies. 

#### Windows

Building from the Windows CMD prompt:

```bat
F:
cd F:\path\to\the\package
REM R.exe for either architecture is fine.
set R="c:\Program Files\R\R-3.0.0\bin\x64\R.exe"
REM optionally, but preferably, check the package
REM %R% CMD check mh
%R% CMD build mh
%R% R CMD INSTALL mh_0.4-5.tar.gz
```

Or, if you use devtools:

```S
library(devtools)
mhdir <- 'F:/path/to/the/package' # i.e. F:/path/to/the/package contains the package folder mh
install(file.path(mhdir, 'mh'))
```

#### Linux

```sh
cd src
cd github_jm/
git clone https://github.com/jmp75/metaheuristics.git
cd metaheuristics/
cd R
cd pkgs/
# git checkout master
# git pull
nuget restore mh/src/mh.sln 
R CMD build mh 
R CMD INSTALL mh_0.4-5.tar.gz # or subsequent version number
```

## Examples

You will find at least one introduction vignette as of 2015-01; from your R session use:

```S
browseVignette('mh')
```

## Resources


