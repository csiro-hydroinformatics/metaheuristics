using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Objectives
{
    /// <summary>
    /// A structure to serialize the definition of an objective that works on a large number of 
    /// objectives, e.g. across numerous catchments. Use case for R, not excluded 
    /// to use another package.
    /// </summary>
    /// <remarks>
    /// The main use case for this is the calibration of AWRA-L across 300 catchments, 
    /// with a composite, "averaging" scores across these catchments (300 * (NSE + log-bias)) 
    /// </remarks>
    public class RexpObjectiveDefinition
    {
        /// <summary>
        /// The directory containing the R.dll file, e.g. "C:\bin\R\R-2.13.0\bin\x64"
        /// </summary>
        public string DirectoryForRLibrary { get; set; }

        /// <summary>
        /// Names to give to the objective scores (R vectors) for use in the final compound formula. E.g. NSE, logbiases
        /// </summary>
        public string[] VariableNames { get; set; }

        /// <summary>
        /// The (R) function that calculated an overall score, e.g. mean(quantile(1-NSE, probs = c(0.1, .25, .50, .75), na.rm = FALSE, names = FALSE, type = 7)) + mean(logbiases)
        /// </summary>
        public string CompoundingFunction { get; set; }

        public string CompoundingFunctionName { get; set; }

        public bool CompoundingFunctionIsMaximizable { get; set; }
    
    }
}
