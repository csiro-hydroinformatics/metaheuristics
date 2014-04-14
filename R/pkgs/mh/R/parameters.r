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
