## Note to self: see the roxygen vignette for examples

## update the documentation with:
# library(roxygen2) ; library(devtools) ; document('F:/src/github_jm/metaheuristics/R/pkgs/mh')
# library(roxygen2) ; roxygenize('F:/src/github_jm/metaheuristics/R/pkgs/mh')

#' An R programming interface and utilities to access the metaheuristics framework
#' 
#' \tabular{ll}{
#' Package: \tab mh\cr
#' Type: \tab Package\cr
#' Version: \tab 0.4-9 \cr
#' Date: \tab 2015-08-11 \cr
#' Release Notes: \tab Upgrade dependency on R.NET 1.6.5 \cr
#' License: \tab LGPL 3\cr
#' }
#'
#' \tabular{lll}{
#' Version \tab Date \tab Notes \cr
#' 0.4-8: \tab 2015-06-23 \tab Fix to AsDataFrame, which was checking for HyperCube<double> instead of IHyperCube<double> \cr
#' 0.4-7: \tab 2015-06-18 \tab Fix SCE option parameter controlling SCE behaviorwas not passed to the engine. Update to use latest R.NET and Metaheuristics nugets \cr
#' 0.4-6: \tab 2015-05-07 \tab Fix issue building on Ubuntu and R 3.2 \cr
#' 0.4-5: \tab 2015-04-04 \tab Update to use CSIRO.Metaheuristics 0.4.9 - use parallel.foreach to work around Mono 3.12.1 \cr
#' }
#' 
#' 
#' @import rClr
#' @import ggplot2
#' @import stringr
#'
#' @name mh-package
#' @aliases mh
#' @docType package
#' @title An R programming interface and utilities to access the metaheuristics framework
#' @author Jean-Michel Perraud \email{jean-michel.perraud_at_csiro.au}
#' @keywords metaheuristics CLR Mono .NET
NULL



