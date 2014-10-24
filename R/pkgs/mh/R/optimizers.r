###### Manipulate optimisation algorithms

optimHelper <- 'CSIRO.Metaheuristics.R.Pkgs.OptimizerHelper'

#' Build an SCE optimizer
#'
#' Build an SCE optimizer
#'
#' @param objective an objective calculator
#' @param parameterizer a type that is implementing IHyperCube<double>
#' @param sysConfigType The type of the system configuration to use for Generics... type of parameterizer if this is the default value NULL
#' @param terminationCriterion An object that can be passed to SCE for testing the completion of the algorithm. 
#' @param sceParameters An object of CLR type SceParameters, or NULL for defaults.
#' @return a Shuffled Complex Evolution optimizer
#' @export
createSceOptim <- function(objective, parameterizer, sysConfigType=NULL, terminationCriterion = maxWallTimeTermination(), sceParameters = NULL) {
  if( is.null(sceParameters)) {
    sceParameters <- createSceParameters()
  }
  clrCallStatic(optimHelper, 'CreateSceOptimizer', objective, parameterizer, sysConfigType, terminationCriterion, sceParameters)
}

#' Builds an SCE parameterization for a problem with N degrees of freedom
#'
#' Builds an SCE parameterization for a problem with N degrees of freedom. This is based on rule of thumbs published in (REF to add)
#'
#' @param n degrees of freedom
#' @return a Shuffled Complex Evolution parameters object
#' @export
createSceParamForDimension <- function(n)
{
  if(n < 0) stop('number of dimensions of an optimisation problem must be positive')
  clrCallStatic('CSIRO.Metaheuristics.Optimization.SceParameters', 'CreateForProblemOfDimension', as.integer(n), -1L);
}

#' Builds a configuration for SCE optimizer
#'
#' Builds a configuration for SCE optimizer
#'
#' @param alpha Number of geometrical transformation for each subcomplex
#' @param beta Number of evolution steps taken by each sub-complex before shuffling occurs
#' @param p Number of complexes
#' @param pmin Minimum number of complexes (populations of points)
#' @param m Number of points per complex
#' @param q Number of points per SUB-complex
#' @param reflectRatio The homothetic ratio used in the reflection phase of the complex evolution: default -1.0
#' @param contractRatio The homothetic ratio used in the contraction phase of the complex evolution: default 0.5
#' @param trapezDensity a coefficient between 0 and 2 (though between 1 and 2 only makes practical sense) defining the probability for assigning points to subcomplexes. Defaults to 2
#' @param numShuffles maximum number of shuffles. -1 by default means that the termination criterion ignores it.
#' @return a Shuffled Complex Evolution parameters object
#' @export
createSceParameters <- function(alpha=1, beta=9, contractRatio=0.5, reflectRatio=-1, m=9, p=5, pmin=5, q=5, trapezDensity=2, numShuffles=-1) {
  sceP <- clrNew('CSIRO.Metaheuristics.Optimization.SceParameters')
  clrSet(sceP, 'Alpha', as.integer(alpha))                      
  clrSet(sceP, 'Beta', as.integer(beta))                        
  clrSet(sceP, 'ContractionRatio', as.numeric(contractRatio))   
  clrSet(sceP, 'M', as.integer(m))                              
  clrSet(sceP, 'NumShuffle', as.integer(numShuffles))           
  clrSet(sceP, 'P', as.integer(p))                              
  clrSet(sceP, 'Pmin', as.integer(pmin))                        
  clrSet(sceP, 'Q', as.integer(q))                              
  clrSet(sceP, 'ReflectionRatio', as.numeric(reflectRatio))     
  clrSet(sceP, 'TrapezoidalDensityParameter', as.numeric(trapezDensity))
  return(sceP)
}

#' Create an termination criterion with a max wall time
#'
#' Create an termination criterion with a max wall time
#'
#' @param sysConfigType The type of the system configuration to use for Generics.
#' @param maxHours the maximum wall time runtime for the optimisation
#' @return a CLR termination criterion.
#' @export
maxWallTimeTermination <- function(sysConfigType, maxHours = 0.1) {
  clrCallStatic(optimHelper, 'CreateMaxWalltime', sysConfigType, as.numeric(maxHours))
}

#' Create an termination criterion based on the rate of marginal fitness improvement
#'
#' Create an termination criterion based on the rate of marginal fitness improvement
#'
#' @param sysConfigType The type of the system configuration to use for Generics.
#' @param tolerance the maximum wall time runtime for the optimisation
#' @param cutoffNoImprovement the maximum wall time runtime for the optimisation
#' @param maxHours the maximum wall time runtime for the optimisation
#' @return a CLR termination criterion.
#' @export
marginalImprovementTermination <- function(sysConfigType, tolerance = 1e-6, cutoffNoImprovement = 10, maxHours = 0.1) {
  clrCallStatic(optimHelper, 'CreateMarginalImprovementTermination', sysConfigType, as.numeric(tolerance), as.integer(cutoffNoImprovement), as.numeric(maxHours))
}

#' Create an termination criterion based on a coefficient of variation of the parameter sets
#'
#' Create an termination criterion based on a coefficient of variation of the parameter sets
#'
#' @param sysConfigType The type of the system configuration to use for Generics.
#' @param cvThreshold CV of the parameter sets, below which the process has converged. It is applied to a subset of the population of points (the "better" half of it)
#' @param maxHours the maximum wall time runtime for the optimisation
#' @return a CLR termination criterion.
#' @export
coeffVariationTermination <- function(sysConfigType, cvThreshold = 0.025, maxHours = 0.1) {
  clrCallStatic(optimHelper, 'CreateCoeffVariationTermination', sysConfigType, as.numeric(cvThreshold), as.numeric(maxHours))
}

#' Sets a logger onto an optimizer
#'
#' @param optimizer an IOptimizer
#' @param calibLogger an object implementing ILoggerMh. May be a missing argument, in which case a defaut in-memory logger is created.
#' @export
setLogger <- function(optimizer, calibLogger) { 
  if(missing(calibLogger)) {
    calibLogger <- clrNew('CSIRO.Metaheuristics.Logging.InMemoryLogger')
  }
  clrSet(optimizer, 'Logger', calibLogger)
}

#' Gets the content of a calibration log as a data frame
#'
#' Gets the content of a calibration log as a data frame
#'
#' @param calibLogger an object implementing ILoggerMh. May be a missing argument, in which case a defaut in-memory logger is created.
#' @return a data frame
#' @export
getLoggerContent <- function(calibLogger) { 
  clrCallStatic(sysConfigHelper, 'GetContent', calibLogger)
}





