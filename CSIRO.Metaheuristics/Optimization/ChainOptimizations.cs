using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Optimization
{
    public class ChainOptimizations<T> : IEvolutionEngine<T> where T: ISystemConfiguration
    {
        private string description;

        public ChainOptimizations(string description, Func<IOptimizationResults<T>> function)
        {
            endOfChain = new Lazy<IOptimizationResults<T>>(function);
            this.description = description;
        }

        Lazy<IOptimizationResults<T>> endOfChain;
        public IOptimizationResults<T> Evolve()
        {
            return endOfChain.Value;
        }

        public string GetDescription()
        {
            return description;
        }

        public void Cancel()
        {
            throw new NotSupportedException();
        }

        public int CurrentGeneration
        {
            get { throw new NotImplementedException(); }
        }
    }
}
