###### Manipulation and application of parameter sets (system configurations)

sysConfigHelper <- 'CSIRO.Metaheuristics.R.Pkgs.SysConfigHelper'
nameCol <- 'Name'
valueCol <- 'Value'
maxCol <- 'Max'
minCol <- 'Min'
descCol <- 'Description'

#' Configure a hypercube as per a data frame specification
#'
#' Configure a hypercube as per a data frame specification
#'
#' @param paramSet An object that implements IHyperCubeSetBounds<double>
#' @param specsFrame a data frame with column names Name,Value,Min,Max. Min and Max are optional if setBounds is FALSE
#' @param setBounds if TRUE, use the Min and Max columns to modify the bounds of the hypercube. Defaults to FALSE.
#' @export
setHyperCube <- function(paramSet, specsFrame, setBounds=FALSE) {
  pnames <- specsFrame[,nameCol]   ; stopifnot(is.character(pnames))
  vals <- specsFrame[,valueCol]  ; stopifnot(is.numeric(vals))
  if(!setBounds) {
    invisible(clrCallStatic(sysConfigHelper, 'SetHyperCube', paramSet, pnames, vals))
  } else {
    mins <- specsFrame[,minCol]    ; stopifnot(is.numeric(mins))
    maxs <- specsFrame[,maxCol]    ; stopifnot(is.numeric(maxs))
    invisible(clrCallStatic(sysConfigHelper, 'SetHyperCube', paramSet, pnames, vals, mins, maxs))
  }
}

#' Change the minimum bound(s) of a hypercube parameter set
#'
#' Change the minimum bound(s) of a hypercube parameter set
#'
#' @param paramSet An object that implements IHyperCubeSetBounds<double>
#' @param varName character vector, name(s) of the variable(s) to change
#' @param value numeric vector, value(s) to set the variable(s) minimum bound(s) to.
#' @export
setMinValue <- function(paramSet, varName, value) {
  setParamSetValue(paramSet, varName, value, methodName='SetMinValue')
}

#' Change the maximum bound(s) of a hypercube parameter set
#'
#' Change the maximum bound(s) of a hypercube parameter set
#'
#' @param paramSet An object that implements IHyperCubeSetBounds<double>
#' @param varName character vector, name(s) of the variable(s) to change
#' @param value numeric vector, value(s) to set the variable(s) maximum bound(s) to.
#' @export
setMaxValue <- function(paramSet, varName, value) {
  setParamSetValue(paramSet, varName, value, methodName='SetMaxValue')
}

#' Change the value(s) of a hypercube parameter set
#'
#' Change the value(s) of a hypercube parameter set
#'
#' @param paramSet An object that implements IHyperCubeSetBounds<double>
#' @param varName character vector, name(s) of the variable(s) to change
#' @param value numeric vector, value(s) to set the variable(s) value(s) to.
#' @export
setValue <- function(paramSet, varName, value) {
  setParamSetValue(paramSet, varName, value, methodName='SetValue')
}

#' Get the value(s) of a hypercube parameter set
#'
#' Get the value(s) of a hypercube parameter set
#'
#' @param paramSet An object that implements at least IHyperCube<double>
#' @param varName character vector, name(s) of the variable(s) to get
#' @return numeric vector, value(s) of the variable(s) value(s).
#' @export
getValue <- function(paramSet, varName) {
  getParamSetValue(paramSet, varName, methodName='GetValue')
}

#' Get the minimum value(s) of a hypercube parameter set
#'
#' Get the minimum value(s) of a hypercube parameter set
#'
#' @param paramSet An object that implements at least IHyperCube<double>
#' @param varName character vector, name(s) of the variable(s) to get
#' @return numeric vector, value(s) of the minimum value(s) of the variable(s).
#' @export
getMinValue <- function(paramSet, varName) {
  getParamSetValue(paramSet, varName, methodName='GetMinValue')
}

#' Get the maximum value(s) of a hypercube parameter set
#'
#' Get the maximum value(s) of a hypercube parameter set
#'
#' @param paramSet An object that implements at least IHyperCube<double>
#' @param varName character vector, name(s) of the variable(s) to get
#' @return numeric vector, value(s) of the maximum value(s) of the variable(s).
#' @export
getMaxValue <- function(paramSet, varName) {
  getParamSetValue(paramSet, varName, methodName='GetMaxValue')
}

setParamSetValue <- function(paramSet, varName, value, methodName) {
  stopifnot(length(varName) == length(value))
  stopifnot(is.character(varName))
  stopifnot(is.numeric(value))
  for (i in 1:length(varName)) {
    clrCall(paramSet, methodName, varName[i], value[i])
  }
}

getParamSetValue <- function(paramSet, varName, methodName) {
  stopifnot(is.character(varName))
  res <- numeric(length(varName))
  for (i in 1:length(varName)) {
    res[i] <- clrCall(paramSet, methodName, varName[i])
  }
  return(res)
}

#' Gets the CLR type for IHyperCubeSetBounds<double>
#'
#' Gets the CLR type for IHyperCubeSetBounds<double>. This function is sometimes needed to pass arguments to some functions requiring an explicit type of system configuration, e.g. createSceOptim.
#'
#' @return a System.Type
#' @export
hyperCubeSetBoundsType <- function() {
  return(clrGet(sysConfigHelper, 'TypeofIHyperCubeSetBounds'))
}

#' Gets the CLR type for IHyperCube<double>
#'
#' Gets the CLR type for IHyperCube<double>. This function is sometimes needed to pass arguments to some functions requiring an explicit type of system configuration, e.g. createSceOptim.
#'
#' @return a System.Type
#' @export
hyperCubeType <- function() {
  return(clrGet(sysConfigHelper, 'TypeofIHyperCube'))
}

#' Builds a parameter set from a line in a data frame.
#'
#' Builds a parameter set from a line in a data frame, typically a line from a calibration log.
#'
#' @param paramSet A parameter set template, an object that implements IHyperCubeSetBounds<double>
#' @param dataFrame a data frame with at least column names including all the names of the template parameter set.
#' @param rownum The row index to use; defaults to 1
#' @return A new parameter set, cloned from the template and with updated values
#' @export
buildParamSet <- function(paramSet, dataFrame, rownum=1) {
  p <- clrCall(paramSet, 'Clone')
  psdf <- pSetAsDataFrame(p)
  pn <- rownames(psdf)
  psdf[pn,'Value'] <- as.numeric(dataFrame[rownum,pn])
  setHyperCube(p, psdf)
  p
}

#' Apply a system configuration to a compatible model  
#'
#' Apply a system configuration to a compatible model  
#'
#' @param sysConfig a CLR object that implements ISystemConfiguration
#' @param modelSystem the modelling system to configure
#' @export
applySysConfig <- function(sysConfig, modelSystem) {
  invisible(clrCall(sysConfig, 'ApplyConfiguration', modelSystem))
}

#' Convert a hypercube to a simple data frame representation
#'
#' Convert a hypercube to a simple data frame representation
#'
#' @param paramSet a CLR object that implements IHyperCube
#' @return a data frame with column names Name,Value,Min,Max,Description
#' @export
pSetAsDataFrame <- function(paramSet) {
  # Consider whether as.data.frame.cobjRef is acceptable. Problem is that it would not really support anything other than hypercubes.
  p <- clrCallStatic(sysConfigHelper, 'ToDataFrame', paramSet)
  psdf <- data.frame(Name=clrGet(p,nameCol), Value=clrGet(p,valueCol), Min=clrGet(p,minCol), Max=clrGet(p,maxCol), Description=clrGet(p,descCol), stringsAsFactors=FALSE)
  rownames(psdf) <- psdf[,nameCol]
  psdf
}
