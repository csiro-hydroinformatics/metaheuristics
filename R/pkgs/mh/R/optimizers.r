###### Manipulate optimisation algorithms

optimHelper <- 'CSIRO.Metaheuristics.R.Pkgs.OptimizerHelper'

#' Build an SCE optimizer
#'
#' Build an SCE optimizer
#'
#' @param objective an objective calculator
#' @param parameterizer a type that is implementing IHyperCube<double>
#' @param sysConfigType The type of the system configuration to use for Generics... type of parameterizer if this is the default value NULL
#' @param maxHours the maximum walltime for the calibration 
#' @return a Shuffled Complex Evolution optimizer
#' @export
createSceOptim <- function(objective, parameterizer, sysConfigType=NULL, maxHours=0.1) { 
  clrCallStatic(optimHelper, 'CreateSceOptimizer', objective, parameterizer, sysConfigType, maxHours)
}

#' Sets a logger onto 
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





