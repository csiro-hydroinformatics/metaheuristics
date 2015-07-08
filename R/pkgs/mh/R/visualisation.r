###### Plotting functions

numColname <- 'PointNumber'


#' Load a CSV log file of an optimisation
#'
#' Load a CSV log file of an optimisation
#'
#' @param fn the file name of the CSV
#' @return a data frame, as loaded with read.csv, and an added column 'PointNumber'
#' @export
loadMhLog <- function(fn) {
  x <- read.csv(fn)
  x[[numColname]] <- 1:nrow(x)
  x
}

#' min/max bound numeric values 
#'
#' min/max bound numeric values 
#'
#' @param x a numeric vector
#' @param lim a num vector of the min/max limits to apply, for instance c(0, 1)
#' @return a numeric vector
#' @export
boundValues <- function(x, lim=NULL) {
  if(!is.null(lim)) {
    return(pmax( lim[1], pmin( x, lim[2])))
  } else {
    return(x)
  }
}

#' min/max bound a column in a data frame
#'
#' min/max bound a column in a data frame
#'
#' @param x a data frame
#' @param colname a character vector, name of the column to bound
#' @param lim a num vector of the min/max limits to apply, for instance c(0, 1)
#' @return a data frame
#' @export
boundValuesDf <- function(x, colname, lim=c(0,1)) {
  x[[colname]] <- boundValues(x[[colname]], lim)
  x
}

#' Plot the value of a parameter along the optimisation process
#'
#' Plot the value of a parameter along the optimisation process. 
#' The color scale is the objective score. Useful to check the behavior of the optimisation process.
#'
#' @param logMh an mhData object
#' @param paramName the exact name of one of the model parameters
#' @param objLims optional bounds to apply to the objective function value prior to plotting (e.g. c(0,1) for Nash-Sutcliffe efficiency)
#' @return a ggplot object
#' @export
#' @examples
#' \dontrun{
#' logFileName <- 'F:/path/to/419016_IhacresClassic_19390101_19481231.csv'
#' logSce <- loadMhLog(logFileName)
#' logMh <- new("mhData",
#'    data= logSce,  
#'    fitness = "NSE.logbias",
#'    messages = "Message", 
#'    categories = "Category")
#'    
#' geomOps <- subsetByMessage(logMh)
#' d <- plotParamEvolution(geomOps, 'Tq', objLims=c(0,1))
#' d
#' }
#' @export
plotParamEvolution <- function(logMh, paramName, objLims=NULL, title="Evolution of parameter values", xlab="Logged point", ylab=paramName) {
  d <- boundFitness(logMh, objLims)
  ggplot(d, aes_string(x = numColname, y = paramName, colour=logMh@fitness)) + 
    geom_point() + labs( title=title, x=xlab, y=ylab) +
    scale_colour_continuous(low="blue", high="red")
}


#' Plot the value of a parameter along the optimisation process
#'
#' Plot the value of a parameter along the optimisation process. 
#' The color scale is the message associated with the point, for instance a geometric transformation. 
#' An example message is 'Contracted point in the subcomplex' for the Shuffled complex algorithm. 
#' Useful to check the behavior of the optimisation process.
#'
#' @param logMh an mhData object 
#' @param paramName the exact name of one of the model parameters
#' @return a ggplot object
#' @export
#' @examples
#' \dontrun{
#' logFileName <- 'F:/path/to/419016_IhacresClassic_19390101_19481231.csv'
#' logSce <- loadMhLog(logFileName)
#' logMh <- new("mhData",
#'    data= logSce,  
#'    fitness = "NSE.logbias",
#'    messages = "Message", 
#'    categories = "Category")
#'    
#' geomOps <- subsetByMessage(logMh)
#' d <- plotParamEvolutionMsg(geomOps, 'Tq')
#' d
#' # If there is overplotting, one can use facets to have a clearer view of the optimiser behavior
#' d + facet_wrap( as.formula(paste("~", geomOps@@messages, sep=' ')) )
#' }
#' @export
plotParamEvolutionMsg <- function(logMh, paramName) {
  x <- logMh@data
  ggplot(x, aes_string(x = numColname, y = paramName, colour=logMh@messages)) + 
    geom_point() + ggtitle("Evolution of parameter values") + xlab("Logged point") + ylab(paramName) 
}

#' Facetted bi-parameter scatter plots of the value of a parameter along the optimisation process
#'
#' Plot the value of a parameter along the optimisation process. 
#' The color scale is the objective score. Useful to check the behavior of the optimisation process.
#'
#' @param logMh an mhData object 
#' @param x the exact name of one of the model parameters
#' @param y the exact name of a second model parameter
#' @return a ggplot object
#' @export
#' @import ggplot2
plotShuffles <- function(logMh, x, y, objLims=NULL) {
  d <- boundFitness(logMh, objLims)
  ggplot(d, aes_string(x=x, y=y, colour=logMh@fitness)) + 
    geom_point() + ggtitle("Population at shuffling stages") + xlab(x) + ylab(y) +
    facet_wrap( as.formula(paste("~", logMh@categories, sep=' ')) ) +
    scale_colour_continuous(low="blue", high="red")
}

# ggsave(d, file=file.path(outputDir, shortFileName), height = 12, width = 16)

#' A copy constructor for mhData objects
#'
#' A copy constructor for mhData objects
#'
#' @param src the object to use as a source for the object member values.
#' @param newData a data frame
#' @return mhData object
#' @export
copyMhData <- function( src, newData ) {
  new("mhData",
   data= newData,  
   fitness = src@fitness,
   messages = src@messages, 
   categories = src@categories)
}

#' Rolling max function
#'
#' Rolling max function
#'
#' @param x a numeric vector
#' @return the rolling maximum over the input vector
#' @export
rollingMax <- function(x) {
  result <- x
  for ( i in 2:length(x)) {
    result[i] = max(x[i],result[i-1], na.rm=TRUE)
  }
  result
}

#' Plots a custom view of the fitness (defaults: max score so far)
#'
#' Plots a custom view of the fitness (defaults: max score so far)
#'
#' @param logMh an mhData object 
#' @param FUN a function to apply to the 'fitness' column as specified by the mhData object
#' @return ggplot object
#' @export
#' @examples
#' \dontrun{
#' logFileName <- 'F:/path/to/419016_IhacresClassic_19390101_19481231.csv'
#' logSce <- loadMhLog(logFileName)
#' logMh <- new("mhData",
#'    data= logSce,  
#'    fitness = "NSE.logbias",
#'    messages = "Message", 
#'    categories = "Category")
#'    
#' geomOps <- subsetByMessage(logMh)
#' geomOps <- copyMhData( geomOps, boundValuesDf(geomOps@@data, geomOps@@fitness, c(0,1)))
#' d <- plotMaxScore(geomOps)
#' d
#' }
#' @export
plotScore <- function(logMh, FUN=rollingMax) { 
  x <- logMh@data
  maxObj <- data.frame(FUN(x[[logMh@fitness]]))
  names(maxObj) <- logMh@fitness
  maxObj[[numColname]] <- x[[numColname]]
  ggplot(maxObj, aes_string(x=numColname, y=logMh@fitness)) + 
    geom_line() + ggtitle("Best score evolution") + xlab('Logged point') + ylab('Fitness score') 
    # +
    # ylim(c(min(maxObj[[logMh@fitness]]), max(maxObj[[logMh@fitness]]) + 0.05))
}

#' Subset a data frame based on string pattern matching
#'
#' Subset a data frame based on string pattern matching on one of its character columns. Uses the function \code{\link{str_detect}}
#'
#' @param x the data frame
#' @param colname column name
#' @param pattern a pattern suitable for use by \code{\link{str_detect}}, for instance 'Initial.*|Reflec.*|Contrac.*|Add.*'
#' @return a data frame. Unnecessary levels have been dropped from factor columns.
#' @import stringr
#' @export
subsetByPattern <- function(x, colname, pattern) {
  criterion <- x[[colname]]
  lvls = levels(criterion)
  if(is.null(criterion)) stop('Subsetting vector is not a factor')
  indices <- criterion %in% lvls[stringr::str_detect(lvls, pattern)]
  droplevels(subset(x,indices))
}

#' Subset an mhData object based on string pattern matching
#'
#' Subset an mhData object based on string pattern matching on the column for 'messages'
#'
#' @param logMh an mhData object 
#' @param pattern a pattern suitable for use by \code{\link{str_detect}}, for instance 'Initial.*|Reflec.*|Contrac.*|Add.*'
#' @return an mhData object 
#' @export
subsetByMessage <- function(logMh, pattern='Initial.*|Reflec.*|Contrac.*|Add.*') {
  copyMhData(logMh, subsetByPattern(logMh@data, logMh@messages, pattern))
}

#' Subset an mhData object based on string pattern matching
#'
#' Subset an mhData object based on string pattern matching on the column for 'categories'
#'
#' @param logMh an mhData object 
#' @param pattern a pattern suitable for use by \code{\link{str_detect}}, for instance 'Initial.*|Reflec.*|Contrac.*|Add.*'
#' @return an mhData object 
#' @export
subsetByCategory <- function(logMh, pattern='Initial.*|Shuffling.*') {
  copyMhData(logMh, subsetByPattern(logMh@data, logMh@categories, pattern))
}

#' Extract information from an optimisation logger
#'
#' Extract information from an optimisation logger. This retrieves data and transforms it into a format more amenable to visualization.
#'
#' @param optimizer the instance of the optimizer
#' @param fitness the name of the fitness score that was used for the optimization, e.g. 'NSE'
#' @param messages
#' @param categories
#' @return an object for graphing through the 'mh' package functions
#' @export
processLogger <- function(optimizer, fitness = "NSE", messages = "Message", categories = "Category") {
  calibLogger <- clrGet(optimizer, 'Logger')
  if(is.null(calibLogger)) stop("no logger is associated with the optimizer")
  d <- mh::getLoggerContent(calibLogger)
  d$PointNumber = 1:nrow(d)
  mkOptimLog(d, fitness=fitness, messages=messages, categories=categories)
}

#' Extract information from an optimisation logger
#'
#' Extract information from an optimisation logger. This retrieves data and transforms it into a format more amenable to visualization.
#'
#' @param logDataFrame a data frame, extracted from an optimization logger with mh::getLoggerContent 
#' @param fitness the name of the fitness score that was used for the optimization, e.g. 'NSE'
#' @param messages
#' @param categories
#' @return an object for graphing through the 'mh' package functions
#' @export
mkOptimLog <- function(logDataFrame,fitness = "NSE", messages = "Message", categories = "Category")
{
  dfNames <- names(logDataFrame)
  checkValidName <- function(x, name) {
    if(!(fitness %in% dfNames)) {
      d <- paste(dfNames, collapse=', ', sep=', ')
      stop(paste0('specified string "', x, '" for "', name, '" not found in data frame names: ', d))
    }
  }
  checkValidName(fitness, 'fitness')
  checkValidName(messages, 'messages')
  checkValidName(categories, 'categories')
  return(new("mhData",
    data= logDataFrame,
    fitness = fitness,
    messages = messages,
    categories = categories))
}

boundFitness <- function(logMh, objLims=NULL) {
  d <- logMh@data
  if(!is.null(objLims)) {
    d <- boundValuesDf(d, logMh@fitness, objLims)
  }
  d
}


# library(ggplot2)
# logFileName <- 'F:/Shared/CalibrationGwDrought/gw_drought/calibResults/caliblog/419016_IhacresClassic_19390101_19481231.csv'
# objFctName <- 'NSE.logbias'
# logSce <- loadMhLog(logFileName)

# logMh <- new("mhData",
   # data= logSce,  
   # fitness = "NSE.logbias",
   # messages = "Message", 
   # categories = "Category")
   
# geomOps <- subsetByMessage(logMh)
# d <- plotParamEvolution(geomOps, 'Tq', objLims=c(0,1))
# d <- plotParamEvolutionMsg(geomOps, 'Tq')

# tmp <- copyMhData( logMh, boundValuesDf(logMh@data, logMh@fitness, c(0,1)))
# plotShuffles(subsetByCategory(tmp), 'L', 'Tq')

#################
