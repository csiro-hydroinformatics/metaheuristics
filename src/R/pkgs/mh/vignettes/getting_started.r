
## ----setup, include=FALSE------------------------------------------------
library(knitr)
opts_chunk$set(out.extra='style="display:block; margin: auto"', fig.align="center", fig.width=10, fig.height=8, dev='png')


## ----, eval=FALSE, include=FALSE-----------------------------------------
## library(swiftdev)
## setwd(file.path('F:/src/github_jm/metaheuristics/R/pkgs/mh', 'vignettes'))
## purlVignettes('.', '..')


## ------------------------------------------------------------------------
library(mh)


## ------------------------------------------------------------------------
mhDllDir <- system.file(package='mh', 'libs')
fnSampleEnv <- file.path(mhDllDir, "EnvModellingSample.dll" )
fnAdapter <- file.path(mhDllDir, "ModellingSampleAdapter.dll" )
stopifnot(file.exists(fnSampleEnv))
stopifnot(file.exists(fnAdapter))
clrLoadAssembly(fnSampleEnv)
clrLoadAssembly(fnAdapter)

clrGetTypesInAssembly("ModellingSampleAdapter")
clrGetStaticMethods("ModellingSampleAdapter.OptimizationAdapter")


## ------------------------------------------------------------------------
simul <- clrCallStatic("EnvModellingSample.SimulationFactory", "CreateAwbmSimulation", TRUE)
simul
clrCall(simul, 'Record', 'Runoff')


## ------------------------------------------------------------------------
p <- clrCallStatic("ModellingSampleAdapter.OptimizationAdapter", "BuildParameterSpace", simul)
p
pSetAsDataFrame(p)


## ------------------------------------------------------------------------
clrReflect(p)
clrGetMemberSignature(p, 'SetMaxValue')
clrCall(p, 'SetMaxValue', 'KSurf', 0.5)


## ------------------------------------------------------------------------
observation <- clrCallStatic("EnvModellingSample.DataHandling", "GetSampleRunoff")
observation[observation < 0] <- NA
plot(observation, type='l')

from <- clrCall(simul, 'GetStart') + 365L
to <- clrCall(simul, 'GetEnd')

objective <- clrCallStatic("ModellingSampleAdapter.OptimizationAdapter", "BuildEvaluator", simul, observation, from, to, "ss")

score <- getScore(objective, p)
score


## ------------------------------------------------------------------------
createSceOptimHelper <- function(objective, parameterizer, sysConfigType = NULL, terminationCriterion = NULL) {
  if(is.null(sysConfigType))  sysConfigType <- hyperCubeType()
  if(is.null(terminationCriterion)) terminationCriterion <- maxWallTimeTermination(sysConfigType)
  createSceOptim(objective, parameterizer, sysConfigType, terminationCriterion)
}

getMarginalTermination <- function(tolerance = 1e-06, cutoffNoImprovement = 10, maxHours = 0.05, sysConfigType=NULL) {
  if(is.null(sysConfigType))  sysConfigType <- hyperCubeType()
  mh::marginalImprovementTermination(sysConfigType, tolerance = tolerance, cutoffNoImprovement = cutoffNoImprovement, maxHours = maxHours) 
}

setCalibrationLogger <- function (optimizer) 
{
    calibLogger <- clrNew("CSIRO.Metaheuristics.Logging.InMemoryLogger")
    clrSet(optimizer, "Logger", calibLogger)
}


## ------------------------------------------------------------------------
optimizer <- createSceOptimHelper(objective, p, terminationCriterion = getMarginalTermination())
calibLogger <- setCalibrationLogger(optimizer)


## ------------------------------------------------------------------------
# clrSet(optimizer, "MaxDegreeOfParallelism", 3L)


## ------------------------------------------------------------------------
startTime <- now()
calibResults <- clrCall(optimizer, "Evolve")
endTime <- now()
calibWallTime <- endTime-startTime
print(paste( 'Optimization completed in ', calibWallTime, attr(calibWallTime, 'units')))


## ------------------------------------------------------------------------
logMh <- mh::processLogger(optimizer, fitness='Sum.Squares')  # TODO: illustrate where this name for the fitness comes from...
geomOps <- mh::subsetByMessage(logMh)
str(geomOps@data)


## ------------------------------------------------------------------------
pVarIds <- (pSetAsDataFrame(p))$Name
for (pVarId in pVarIds) {
	print(plotParamEvolution(geomOps, pVarId))
}

