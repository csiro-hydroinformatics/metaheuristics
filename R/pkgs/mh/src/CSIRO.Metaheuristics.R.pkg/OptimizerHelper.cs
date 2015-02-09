using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CSIRO.Metaheuristics.CandidateFactories;
using CSIRO.Metaheuristics.Optimization;
using CSIRO.Utilities;

namespace CSIRO.Metaheuristics.R.Pkgs
{
    public sealed class OptimizerHelper
    {
        public static object CreateSceOptimizer(object evaluator, ICloneableSystemConfiguration template, Type sysConfigType = null, object terminationCondition = null, SceParameters sceParams = null)
        {
            var helper = getHelper();
            var t = sysConfigType ?? template.GetType();
            if (terminationCondition == null) terminationCondition = CreateMaxWalltime(t);
            return invokeGenericMethod("internalCreateSceOptimizer", t, new object[] { evaluator, template, terminationCondition });
        }

        public static object CreateMaxWalltime(Type sysConfigType, double maxHours = 0.1)
        {
            return invokeGenericMethod("internalCreateMaxWalltime", sysConfigType, new object[] { maxHours });
        }

        public static object CreateMaxNumShuffle(Type sysConfigType)
        {
            return invokeGenericMethod("internalCreateMaxNumShuffle", sysConfigType, new object[] {});
        }

        public static object CreateMarginalImprovementTermination(Type sysConfigType, double tolerance = 1e-6, int cutoffNoImprovement = 10, double maxHours = 0.1)
        {
            return invokeGenericMethod("internalCreateMaginalCheck", sysConfigType, new object[] { maxHours, tolerance, cutoffNoImprovement });
        }

        public static object CreateCoeffVariationTermination(Type sysConfigType, double cvThreshold = 0.025, double maxHours = 1)
        {
            return invokeGenericMethod("internalCreateCvTermination", sysConfigType, new object[] { cvThreshold, maxHours });
        }

        private static object invokeGenericMethod(string methodName, Type sysConfigType, object[] parameters)
        {
            var helper = getHelper();
            var method = helper.MakeGenericMethod(methodName, sysConfigType);
            return method.Invoke(null, parameters);
        }

        private static GenericTypesHelper getHelper()
        {
            var helper = new GenericTypesHelper(typeof(OptimizerHelper), BindingFlags.NonPublic | BindingFlags.Static);
            return helper;
        }

        private static ShuffledComplexEvolution<T> internalCreateSceOptimizer<T>(IClonableObjectiveEvaluator<T> evaluator, T template, ITerminationCondition<T> terminationCondition)
            where T : ICloneableSystemConfiguration, IHyperCube<double>
        {
            var populationInitializer = new UniformRandomSamplingFactory<T>(new CSIRO.Metaheuristics.RandomNumberGenerators.BasicRngFactory(0), template);
            var sce = new ShuffledComplexEvolution<T>(evaluator, populationInitializer,
                terminationCondition: terminationCondition);
            return sce;

        }

        private static ShuffledComplexEvolution<T>.MaxShuffleTerminationCondition internalCreateMaxNumShuffle<T>()
            where T : ICloneableSystemConfiguration, IHyperCube<double>
        {
            return new ShuffledComplexEvolution<T>.MaxShuffleTerminationCondition();
        }

        private static ShuffledComplexEvolution<T>.MaxWalltimeTerminationCondition internalCreateMaxWalltime<T>(double maxHours)
            where T : ICloneableSystemConfiguration, IHyperCube<double>
        {
            return new ShuffledComplexEvolution<T>.MaxWalltimeTerminationCondition(maxHours: maxHours);
        }

        private static ShuffledComplexEvolution<T>.CoefficientOfVariationTerminationCondition internalCreateCvTermination<T>(double cvThreshold, double maxHours)
            where T : ICloneableSystemConfiguration, IHyperCube<double>
        {
            return new ShuffledComplexEvolution<T>.CoefficientOfVariationTerminationCondition(threshold: cvThreshold, maxHours: maxHours);
        }

        private static ShuffledComplexEvolution<T>.MarginalImprovementTerminationCondition internalCreateMaginalCheck<T>(double maxHours, double tolerance, int cutoffNoImprovement)
            where T : ICloneableSystemConfiguration, IHyperCube<double>
        {
            return new ShuffledComplexEvolution<T>.MarginalImprovementTerminationCondition(maxHours: maxHours, tolerance: tolerance, cutoffNoImprovement: cutoffNoImprovement);
        }
    }
}
