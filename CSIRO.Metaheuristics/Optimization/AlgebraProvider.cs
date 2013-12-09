using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.Optimization
{
    public interface IAlgebraProvider
    {
        IBase CreateBase( int numDim );
        IVector CreateVector( int numDim );
    }

    public interface IBase
    {
        IVector CreateVector( );
        int NumDimensions { get; }
        IVector this[int i] { get; }
        void Orthonormalize( int p );
        void SetBaseVector( int p, IVector pathOfStage );
        IVector GetBaseVector( int p );

        IBase CreateNew( int numDim );
    }

    public interface IVector
    {
        double this[int i] { get; set; }
        int Length { get; }

        bool IsOrthogonal( IVector pathOfStage );

        bool IsNullVector { get; }

        void SetAllComponents( double value );
    }


}
