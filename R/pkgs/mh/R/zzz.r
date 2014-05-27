#' Function called when loading the package with 'library'. 
#'
#' Function called when loading the package with 'library'. 
#'
#' @rdname dotOnLoad.Rd
#' @name dotOnLoad
#' @param libname the path to the library from which the package is loaded
#' @param pkgname the name of the package.
.onLoad <- function(libname='~/R', pkgname='mh') {
  libLocation<-system.file(package=pkgname)
  libpath <- file.path(libLocation, 'libs')
  f <- file.path(libpath, 'CSIRO.Metaheuristics.R.Pkgs.dll')
  if( !file.exists(f) ) {
    packageStartupMessage('Could not find path to CSIRO.Metaheuristics.R.Pkgs.dll, you will have to load it manually')
  } else {
    clrLoadAssembly(f)
  }
}


# create the mhData class
setClass(
    "mhData",
     representation(
       data= "data.frame",  
       fitness = "character",             # the colunm name in the data frame of the measure of fitness 
       messages = "character",       #  the colunm name in the data frame of the messages 
       categories = "character"    #  the colunm name in the data frame identifying the categories of data points
       ))


# TODO?       
# setGeneric("mhData.plotParamEvolution", function(this, paramName, objLims) standardGeneric("mhData.plotParamEvolution"))
# setMethod(
    # "mhData.plotParamEvolution", 
    # "mhData",
# )
