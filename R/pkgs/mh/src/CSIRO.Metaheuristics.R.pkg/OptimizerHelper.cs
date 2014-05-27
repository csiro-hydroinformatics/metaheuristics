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
        public static object CreateSceOptimizer(object evaluator, ICloneableSystemConfiguration template, Type sysConfigType=null, double maxHours = 0.1)
        {
            var helper = new GenericTypesHelper(typeof(OptimizerHelper), BindingFlags.NonPublic | BindingFlags.Static);
            var parameters = new object[] { evaluator, template, maxHours };
            var method = helper.MakeGenericMethod("internalCreateSceOptimizer", sysConfigType ?? template.GetType());
            return method.Invoke( null, parameters );
        }

        private static ShuffledComplexEvolution<T> internalCreateSceOptimizer<T>(IClonableObjectiveEvaluator<T> evaluator, T template, double maxHours) 
            where T : ICloneableSystemConfiguration, IHyperCube<double>
        {
            var populationInitializer = new UniformRandomSamplingFactory<T>(new CSIRO.Metaheuristics.RandomNumberGenerators.BasicRngFactory(0), template);
            var sce = new ShuffledComplexEvolution<T>(evaluator, populationInitializer,
                terminationCondition: new ShuffledComplexEvolution<T>.CoefficientOfVariationTerminationCondition(maxHours: maxHours));
            //sce.Logger = new Log4netAdapter();
            return sce;

        }

    }
}
