using System;
using System.Reflection;
using CSIRO.Sys;

namespace CSIRO.Metaheuristics.SystemConfigurations
{
    public class BasicHyperCube : HyperCube<double>
    {
        public BasicHyperCube (string[] variableNames) 
            : base(variableNames)
        {
        }

        public override void ApplyConfiguration (object system)
        {
            var t = system.GetType ();
            var names = this.GetVariableNames ();
            var map = ReflectionHelper.GetAccessorMap (t, names);
            foreach (var name in names) {
                map [name].SetValue (system, this.GetValue (name));
            }
        }

        public override IHyperCube<double> HomotheticTransform (IHyperCube<double> point, double factor)
        {
            var pt = point as BasicHyperCube;
            HyperCube<double> result = 
                (BasicHyperCube)this.Clone();
            performHomoteticTransform(point, factor, ref result);
            return result;
        }

        protected override double reflect (double point, double reference, double factor)
        {
            return HyperCubeOperations.Reflect (point, reference, factor);
        }
    }
}
