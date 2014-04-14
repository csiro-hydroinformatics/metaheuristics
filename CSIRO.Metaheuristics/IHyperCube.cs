
﻿using System;

namespace CSIRO.Metaheuristics
{
    public interface IHyperCube<T> : ICloneableSystemConfiguration where T : IComparable
    {
        string[] GetVariableNames( );
        int Dimensions { get; }
        T GetValue( string variableName );
        T GetMaxValue( string variableName );
        T GetMinValue( string variableName );
        void SetValue( string variableName, T value );
        IHyperCube<T> HomotheticTransform( IHyperCube<T> point, double factor );
    }

    public interface IHyperCubeSetBounds<T> : IHyperCube<T> where T : IComparable
    {
        void SetMinValue(string variableName, T value);
        void SetMaxValue(string variableName, T value);
        void SetMinMaxValue(string variableName, T min, T max, T value);
    }

}
