using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.SystemConfigurations;
using CSIRO.Metaheuristics.Objectives;
using CSIRO.Sys;
using CSIRO.Metaheuristics.Utils;

namespace CSIRO.Metaheuristics.Tests
{
    [Serializable]
    public class TestHyperCube : HyperCube<double>
    {
        public TestHyperCube(int p, double val, double min, double max)
            : base(TestHyperCube.ramp(p))
        {
            foreach (var varName in this.GetVariableNames())
            {
                this.SetMinValue(varName, min);
                this.SetMaxValue(varName, max);
                this.SetValue(varName, val);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (var varName in this.GetVariableNames())
            {
                sb.Append(this.GetValue(varName));
                sb.Append(',');
            }
            sb.Append("{");
            return sb.ToString();
           
        }

        public void SetValues(params double[] values)
        {
            var varNames = this.GetVariableNames();
            for (int i = 0; i < values.Length; i++)
            {
                this.SetValue(varNames[i], values[i]);
            }
        }

        public void SetAllValues(double value)
        {
            var varNames = this.GetVariableNames();
            for (int i = 0; i < varNames.Length; i++)
            {
                this.SetValue(varNames[i], value);
            }
        }

        private TestHyperCube(int p)
            : base(TestHyperCube.ramp(p))
        {
        }

        private static string[] ramp(int p)
        {
            string[] result = new string[p];
            for (int i = 0; i < p; i++)
            {
                result[i] = i.ToString();
            }
            return result;
        }

        public IObjectiveScores<T> EvalParaboloidScore<T>(T sysConfig, double bestParam) where T: IHyperCube<double>
        {
            double result = CalculateParaboloid(sysConfig, bestParam);
            return MetaheuristicsHelper.CreateSingleObjective<T>(sysConfig, result, "Paraboloid");
        }

        public static double CalculateParaboloid(IHyperCube<double> sysConfig, double bestParam)
        {
            var names = sysConfig.GetVariableNames();
            double result = 0;
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                double v = (sysConfig.GetValue(name) - bestParam);
                result += v * v;
            }
            return result;
        }
        public override IHyperCube<double> HomotheticTransform(IHyperCube<double> point, double factor)
        {
            var pt = point as TestHyperCube;
            HyperCube<double> result = new TestHyperCube(this.Dimensions);
            performHomoteticTransform(point, factor, ref result);
            return result;
        }

        protected override double reflect(double point, double reference, double factor)
        {
            return reference + ((point - reference) * factor);
        }

        public override void ApplyConfiguration(object system)
        {
            throw new NotImplementedException();
        }
    }

    public class IdentityObjEval : IObjectiveEvaluator<TestHyperCube>
    {
        public IObjectiveScores<TestHyperCube> EvaluateScore(TestHyperCube sysConfig)
        {
            var names = sysConfig.GetVariableNames();
            IObjectiveScore[] scores = new IObjectiveScore[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                scores[i] = new DoubleObjectiveScore(name, sysConfig.GetValue(name), maximise: false);
            }
            return new MultipleScores<TestHyperCube>(scores, sysConfig);
        }
    }

    public class ParaboloidObjEval<T> : IClonableObjectiveEvaluator<T>
        where T : IHyperCube<double>

    {
        protected double bestParam;

        public ParaboloidObjEval(double bestParam=0)
        {
            this.bestParam = bestParam;
        }
        IObjectiveScores<T> IObjectiveEvaluator<T>.EvaluateScore(T systemConfiguration)
        {
            return evalScore(systemConfiguration);
        }

        private IObjectiveScores<T> evalScore(T sysConfig)
        {
            var names = sysConfig.GetVariableNames();
            double result = 0;
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                double v = (sysConfig.GetValue(name) - bestParam);
                result += v * v;
            }
            var dblScore = new DoubleObjectiveScore("Paraboloid", result, maximise: false);
            return new MultipleScores<T>(new IObjectiveScore[] { dblScore }, sysConfig);
        }

        bool ICloningSupport.SupportsDeepCloning
        {
            get { return true; }
        }

        bool ICloningSupport.SupportsThreadSafeCloning
        {
            get { return true; }
        }

        protected virtual IClonableObjectiveEvaluator<T> deepClone()
        {
            return new ParaboloidObjEval<T>(bestParam: this.bestParam);
        }

        IClonableObjectiveEvaluator<T> ICloningSupport<IClonableObjectiveEvaluator<T>>.Clone()
        {
            return deepClone();
        }
    }

    public class ParaboloidObjEvalTest : ParaboloidObjEval<TestHyperCube>
    {
        public ParaboloidObjEvalTest(double bestParam = 0) : base(bestParam: bestParam)
        {
        }

        protected override IClonableObjectiveEvaluator<TestHyperCube> deepClone()
        {
            return new ParaboloidObjEvalTest(bestParam: this.bestParam);
        }
    }
}
