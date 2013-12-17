#!/bin/bash

THIS_DIR=./

src_nuspec=$THIS_DIR../CSIRO.Metaheuristics/CSIRO.Metaheuristics.nuspec

cp $src_nuspec $THIS_DIR../CSIRO.Utilities/CSIRO.Utilities.nuspec
cp $src_nuspec $THIS_DIR../CSIRO.Metaheuristics.DataModel/CSIRO.Metaheuristics.DataModel.nuspec
cp $src_nuspec $THIS_DIR../CSIRO.Metaheuristics.Logging/CSIRO.Metaheuristics.Logging.nuspec
cp $src_nuspec $THIS_DIR../CSIRO.Metaheuristics.Parallel/CSIRO.Metaheuristics.Parallel.nuspec
cp $src_nuspec $THIS_DIR../CSIRO.Metaheuristics.R/CSIRO.Metaheuristics.R.nuspec

nuget pack $THIS_DIR../CSIRO.Utilities/CSIRO.Utilities.csproj -IncludeReferencedProjects
nuget pack $THIS_DIR../CSIRO.Metaheuristics/CSIRO.Metaheuristics.csproj -IncludeReferencedProjects
nuget pack $THIS_DIR../CSIRO.Metaheuristics.DataModel/CSIRO.Metaheuristics.DataModel.csproj -IncludeReferencedProjects
nuget pack $THIS_DIR../CSIRO.Metaheuristics.Logging/CSIRO.Metaheuristics.Logging.csproj -IncludeReferencedProjects
nuget pack $THIS_DIR../CSIRO.Metaheuristics.Parallel/CSIRO.Metaheuristics.Parallel.csproj -IncludeReferencedProjects
nuget pack $THIS_DIR../CSIRO.Metaheuristics.R/CSIRO.Metaheuristics.R.csproj -IncludeReferencedProjects

