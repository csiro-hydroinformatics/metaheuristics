mh - Access the CSIRO.Metaheuristics framework from R
=================

**mh** is an R package to access a metaheuristics framework [CSIRO.Metaheuristics](https://github.com/jmp75/metaheuristics) running on Microsoft .NET and Mono.

Note that as of 2014-04, this package is not at a stage intended for use by many developers. The build process is far from streamlined and well documented.

## Prerequisites and package dependencies

The mh package uses the .NET framework SDK to build some C# code. Typically if you have on your machine the file "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe", you can skip this paragraph. Otherwise you need to install the [Microsoft Windows SDK for Windows 7 and .NET Framework 4](http://www.microsoft.com/en-us/download/details.aspx?id=8279). An overview of list of Microsoft SDKs is available [here](http://msdn.microsoft.com/en-us/vstudio/hh487283.aspx)

The interoperability of R and .NET code relies on the [rClr](http://r2clr.codeplex.com/) R package. As of 2013-08-19 you can download an installable R package for windows (zip file). Make sure to at least skim through the [installation instructions](http://r2clr.codeplex.com/wikipage?title=Installing%20R%20packages&referringTitle=Documentation).

```S
install.packages("zoo") # you don't need to run this command if you already have the zoo package installed.
```

## Install

### Install from precompiled mh

### Compile and install from source

Check out the mh package from its [github repository](http://someaddress.net) to a folder of your choice. Note that, if you already use the `devtools` package, using the `install_github` function to install mh is is _not yet_ possible; you need a customisation step as follows.

The source code in C# used by the package uses NuGet to manage its dependencies. 

Building from the Windows CMD prompt:

```bat
F:
cd F:\path\to\the\package
REM R.exe for either architecture is fine.
set R="c:\Program Files\R\R-3.0.0\bin\x64\R.exe"
REM optionally, but preferably, check the package
%R% CMD check mh
%R% CMD INSTALL mh
```

Or, if you use devtools:

```S
library(devtools)
mhdir <- 'F:/path/to/the/package' # i.e. F:/path/to/the/package contains the package folder mh
install(file.path(mhdir, 'mh'))
```

## Examples

```S
require(mh)
```

## Resources

## Acknowledgements
