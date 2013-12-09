using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Objectives
{
    [Serializable]
    public class DoubleObjectiveScore : RealValueObjectiveScore<double>
    {
        public DoubleObjectiveScore( string name, double value, bool maximise )
            : base( name, value, maximise )
        {
        }
    }

    [Serializable]
    public class FloatObjectiveScore : RealValueObjectiveScore<float>
    {
        public FloatObjectiveScore( string name, float value, bool maximise )
            : base( name, value, maximise )
        {
        }
    }

    [Serializable]
    public abstract class RealValueObjectiveScore<T> : IObjectiveScore<T>, IObjectiveScore 
        where T: IComparable
    {
        public RealValueObjectiveScore( string name, T value, bool maximise )
        {
            this.name = name;
            this.value = value;
            this.maximise = maximise;
        }
        string name;
        T value;
        bool maximise;

        public T Value
        {
            get { return value; }
        }

        public bool Maximise
        {
            get { return maximise; }
        }

        public string GetText( )
        {
            return name + " " + value.ToString( );
        }

        public IComparable ValueComparable
        {
            get { return value; }
        }

        public string Name
        {
            get { return name; }
        }
    }


}
