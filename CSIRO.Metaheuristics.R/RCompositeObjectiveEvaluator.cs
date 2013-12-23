using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.Objectives;
using CSIRO.Metaheuristics.Parallel.Objectives;
using CSIRO.Metaheuristics.Parallel.SystemConfigurations;
using CSIRO.Utilities;
using RDotNet;
using RDotNet.Internals;
using RDotNet.NativeLibrary;
using Environment = System.Environment;

namespace CSIRO.Metaheuristics.R
{
    /// <summary>
    ///   Class that calculates a composite objective score from a set of scores using a data driven R function.
    /// </summary>
    public class RCompositeObjectiveEvaluator<TSysConfig> : CompositeObjectiveCalculation<TSysConfig> where TSysConfig : ISystemConfiguration
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RexpObjectiveDefinition objectiveDefinition;
        private readonly REngine rEngine;

        protected override bool IsMaximisable
        {
            get { return objectiveDefinition.CompoundingFunctionIsMaximizable; }
        }

        private string ObjectiveExpression
        {
            get { return objectiveDefinition.CompoundingFunction; }
        }

        protected override string ObjectiveName
        {
            get { return objectiveDefinition.CompoundingFunctionName; }
        }

        protected override string[] VariableNames
        {
            get { return objectiveDefinition.VariableNames; }
        }

        #region Creation

        /// <summary>
        ///   Initializes a new instance of the <see cref="MultiCatchmentCompositeObjectiveEvaluator" /> class.
        /// </summary>
        /// <param name="objDefn"> The objective definition. </param>
        private RCompositeObjectiveEvaluator(RexpObjectiveDefinition objDefn)
        {
            objectiveDefinition = objDefn;
            string pathToRDynLib = objDefn.DirectoryForRLibrary;
            string rhome = Environment.GetEnvironmentVariable("R_HOME");

            if (!Directory.Exists(pathToRDynLib))
            {
                REngine.SetEnvironmentVariables();
            }
            else
            {
                if (string.IsNullOrEmpty(rhome))
                {
                    var plat = NativeUtility.GetPlatform();
                    switch (plat)
                    {
                        case PlatformID.Win32NT:
                            if (string.IsNullOrEmpty(rhome))
                            {
                                rhome = Path.Combine(pathToRDynLib, @"..\.."); // Assume R more recent than 2.12 : binaries under bin\x64 for instance; folder containing 'bin' is R_HOME
                                Log.WarnFormat("R_HOME environment variable not set. Setting R_HOME = {0}", rhome);
                            }
                            break;
                        case PlatformID.Unix:
                            Log.Debug("R init: detected Unix platform");
                            if (String.IsNullOrEmpty(rhome))
                                throw new Exception("R_HOME environment variable is not set");
                            break;
                        default:
                            throw new NotSupportedException(string.Format("Platform not supported: {0}", plat));
                    }
                }
                Log.Debug("R init: R_HOME = " + rhome);
                REngine.SetEnvironmentVariables(pathToRDynLib, rhome);
            }

            Log.Debug("R init: creating R engine");
            rEngine = REngine.CreateInstance("RDotNet");
            Log.Debug("R init: initialising R engine");
            StartupParameter rStartParams = new StartupParameter
            {
                Quiet = true,
                SaveAction = StartupSaveAction.NoSave,
                Slave = false,
                Interactive = true,
                Verbose = false,
                LoadInitFile = true,
                LoadSiteFile = true,
                RestoreAction = StartupRestoreAction.NoRestore,
                NoRenviron = false
            };
            rEngine.Initialize(rStartParams);
            Log.Debug("Created rEngine: " + rEngine.ToString());
        }

        private static RCompositeObjectiveEvaluator<TSysConfig> singleton = null;

        /// <summary>
        ///   Factory method for creating an instance of the <see cref="MultiCatchmentCompositeObjectiveEvaluator" /> class.
        /// </summary>
        /// <param name="objectiveDefinitionFileInfo"> The objective definition file info. </param>
        /// <returns> The new instance. </returns>
        public static RCompositeObjectiveEvaluator<TSysConfig> Create(FileInfo objectiveDefinitionFileInfo)
        {
            if (!objectiveDefinitionFileInfo.Exists)
                throw new ArgumentException("Cannot find objective definition file " +
                                            objectiveDefinitionFileInfo.FullName);

            var objDefn =
                XmlSerializeHelper.DeserializeFromXML<RexpObjectiveDefinition>(objectiveDefinitionFileInfo.FullName);
            return Create(objDefn);
        }

        public static RCompositeObjectiveEvaluator<TSysConfig> Create(RexpObjectiveDefinition objDefn)
        {
            if (singleton == null)
            {
                singleton = new RCompositeObjectiveEvaluator<TSysConfig>(objDefn);
            }
            return singleton;
        }

        #endregion

        /// <summary>
        ///   Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"> <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    rEngine.Dispose();
                }
                disposed = true;
            }
        }

        protected override double calculateComposite(double[][] objValues)
        {
            for (int i = 0; i < VariableNames.Length; i++)
            {
                rEngine.SetSymbol(VariableNames[i], rEngine.CreateNumericVector(objValues[i]));
            }
            Log.InfoFormat("Objective expression: {0}", ObjectiveExpression);
            return rEngine.Evaluate(ObjectiveExpression).AsNumeric()[0];
        }
    }
}