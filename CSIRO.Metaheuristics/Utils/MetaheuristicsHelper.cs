using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using CSIRO.Metaheuristics.Objectives;

namespace CSIRO.Metaheuristics.Utils
{
    public sealed class MetaheuristicsHelper
    {
        private MetaheuristicsHelper( ) { }

        public static IObjectiveEvaluator<T> TryClone<T>( IObjectiveEvaluator<T> evaluator ) where T : ISystemConfiguration
        {
            if( evaluator == null )
                throw new ArgumentNullException( );
            IClonableObjectiveEvaluator<T> clonable = evaluator as IClonableObjectiveEvaluator<T>;
            if( clonable == null )
                throw new NotSupportedException( evaluator.GetType( ).Name + " does not implement IClonableObjectiveEvaluator" );
            return clonable.Clone( );
        }

        public static string GetHumanReadable( IObjectiveScores scores )
        {
            var sb = new StringBuilder( );
            for( int i = 0; i < scores.ObjectiveCount; i++ )
            {
                sb.AppendLine( scores.GetObjective( i ).GetText( ) );
            }
            sb.AppendLine( scores.GetSystemConfiguration( ).GetConfigurationDescription( ) );
            return sb.ToString( );
        }

        public static string GetHumanReadable( IOptimizationResults<ISystemConfiguration> results )
        {
            return GetHumanReadable( results.ToArray() );
        }

        public static string GetHumanReadable( IObjectiveScores[] scores )
        {
            var sb = new StringBuilder( );
            for( int i = 0; i < scores.Length; i++ )
            {
                sb.AppendLine( GetHumanReadable( scores[i] ) );
            }
            return sb.ToString( );
        }

        public static void SaveAsText( IObjectiveScores[] scores, string filename )
        {
            using( var stream = File.CreateText( filename ) )
            {
                for( int i = 0; i < scores.Length; i++ )
                {
                    stream.WriteLine( GetHumanReadable( scores[i] ) );
                    stream.WriteLine( string.Empty );
                }
            }
        }

        public static void SaveAsText( string content, string filename )
        {
            using( var stream = File.CreateText( filename ) )
            {
                stream.Write( content );
            }
        }

        public static void SaveAsCsv<T>(IEnumerable<IObjectiveScores> scores, string filename) where T : IHyperCube<double>
        {
            using (var stream = File.CreateText(filename))
            {
                stream.Write(BuildCsvFileContent<T>(scores));
            }
        }

        public static T[] ReadConfigsFromCsv<T>(string filename, T templateSpace) where T : IHyperCube<double>
        {
            using (var stream = File.OpenText(filename))
            {
                return ParseConfigsFromCsv(stream.ReadToEnd(), templateSpace);
            }
        }

        public static T[] ParseConfigsFromCsv<T>(string csvContent, T templateSpace) where T : IHyperCube<double>
        {
            string[] lines = csvContent.Split(new []{Environment.NewLine, "\n"}, StringSplitOptions.RemoveEmptyEntries);
            string[] numbers = new string[lines.Length - 1];
            for (int i = 0; i < numbers.Length; i++)
                numbers[i] = lines[i + 1];
            string[] header = lines[0].Split(new[] { "," }, StringSplitOptions.None);
            return Array.ConvertAll(numbers, x => parseLine(x, templateSpace, header));
        }

        private static T parseLine<T>(string line, T templateSpace, string[] header) where T : IHyperCube<double>
        {
            var t = (T)templateSpace.Clone();
            var values = line.Split(new[]{","}, StringSplitOptions.None);
            if (values.Length != header.Length)
                throw new Exception("Inconsistent numbers between CSV header and current line");
            var d = new Dictionary<string, string>();
            for (int i = 0; i < header.Length; i++)
                d[header[i]] = values[i];
            var varNames = t.GetVariableNames();
            foreach (var varName in varNames)
            {
                double value;
                if(!double.TryParse(d[varName], NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    throw new FormatException(string.Format("Could not parse numeric value '{0}' for parameter '{1}'", d[varName], varName));
                t.SetValue(varName, value);
            }
            return t;
        }

        public static string BuildCsvFileContent<T>(IEnumerable<IObjectiveScores> scores, bool writeHeader = true, Tuple<string, string>[] prepend = null) where T : IHyperCube<double>
        {
            var firstscore = scores.FirstOrDefault( );
            var firstpoint = (T)firstscore.GetSystemConfiguration();
            var varnames = firstpoint.GetVariableNames( );
            var objNames = new List<string>( );
            for( int i = 0; i < firstscore.ObjectiveCount; i++ )
                objNames.Add( firstscore.GetObjective( i ).Name );

            StringBuilder sb = new StringBuilder( );

            if (writeHeader)
            {
                prependColumnHeaders(prepend, sb);
                for (int i = 0; i < varnames.Length; i++)
                {
                    sb.Append(varnames[i]); sb.Append(',');
                }
                for (int i = 0; i < objNames.Count - 1; i++)
                {
                    sb.Append(objNames[i]); sb.Append(',');
                }
                sb.AppendLine(objNames[objNames.Count - 1]);
            }
            foreach( var score in scores )
            {
                var point = (T)score.GetSystemConfiguration();

                if (prepend != null)
                {
                    for (int i = 0; i < prepend.Length; i++)
                    {
                        sb.Append(prepend[i].Item2); sb.Append(',');
                    }
                }
                for (int i = 0; i < varnames.Length; i++)
                {
                    sb.Append( point.GetValue(varnames[i]) ); sb.Append( ',' );
                }
                for( int i = 0; i < objNames.Count - 1; i++ )
                {
                    sb.Append( score.GetObjective(i).ValueComparable.ToString() ); sb.Append( ',' );
                }
                sb.AppendLine( score.GetObjective( objNames.Count - 1 ).ValueComparable.ToString() );
            }

            return sb.ToString( );
        }

        public static string BuildCsvFileContent<T>(IEnumerable<T> points, bool writeHeader = true, Tuple<string,string>[] prepend = null) where T : IHyperCube<double>
        {
            var firstpoint = points.FirstOrDefault();
            var varnames = firstpoint.GetVariableNames();

            StringBuilder sb = new StringBuilder();

            if (writeHeader)
            {
                prependColumnHeaders(prepend, sb);
                for (int i = 0; i < varnames.Length - 1; i++)
                {
                    sb.Append(varnames[i]); sb.Append(',');
                }
                sb.AppendLine(varnames[varnames.Length - 1]);
            }
            foreach (var point in points)
            {
                if (prepend != null)
                {
                    for (int i = 0; i < prepend.Length; i++)
                    {
                        sb.Append(prepend[i].Item2); sb.Append(',');
                    }
                }
                for (int i = 0; i < varnames.Length - 1; i++)
                {
                    sb.Append(point.GetValue(varnames[i])); sb.Append(',');
                }
                sb.AppendLine(point.GetValue(varnames[varnames.Length - 1]).ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        private static void prependColumnHeaders(Tuple<string, string>[] prepend, StringBuilder sb)
        {
            if (prepend != null)
            {
                for (int i = 0; i < prepend.Length; i++)
                {
                    sb.Append(prepend[i].Item1); sb.Append(',');
                }
            }
        }

        public static double GetMaximum( params double[] values )
        {
            double result = double.NegativeInfinity;
            for( int i = 0; i < values.Length; i++ )
            {
                result = Math.Max( result, values[i] );
            }
            return result;
        }

        public static double GetMinimum( params double[] values )
        {
            double result = double.PositiveInfinity;
            for( int i = 0; i < values.Length; i++ )
            {
                result = Math.Min( result, values[i] );
            }
            return result;
        }

        public static bool CheckInBounds<T>( T value, T minValue, T maxValue, bool throwIfFalse=true) where T: IComparable
        {
            if( value.CompareTo( maxValue ) > 0 || value.CompareTo( minValue ) < 0 )
                if( throwIfFalse )
                    throw new ArgumentException( "Value to set is out of min-max bounds" );
                else
                    return false;
            return true;
        }

        public static IObjectiveScores[] InterpolateBetweenPoints<T>(IObjectiveEvaluator<T> evaluator, IEnumerable<IObjectiveScores> points, double stepSize)
            where T : IHyperCube<double>
        {
            if ( stepSize <= 0 || stepSize >= 1 )
                throw new ArgumentException("Step size must be in ]0,1[");
            var result = new List<IObjectiveScores>();
            var array = points.ToArray();
            for (int i = 0; i < array.Length-1; i++)
            {
                T a = (T)array[i].GetSystemConfiguration();
                T b = (T)array[i + 1].GetSystemConfiguration();
                result.Add(array[i]);
                double x = stepSize;
                while (x < 1.0)
                {
                    T interpolated = (T)a.HomotheticTransform(b, x);
                    result.Add(evaluator.EvaluateScore(interpolated));
                    x += stepSize;
                }
            }
            result.Add(array[array.Length-1]);
            return result.ToArray();
        }

        public static double[] GetValues(IHyperCube<double>[] pSets, string variableName)
        {
            return Array.ConvertAll(pSets, (x=> x.GetValue(variableName)));
        }

        public static IObjectiveScores<T> CreateSingleObjective<T>(T sysConfig, double result, string scoreName, bool maximise = false) where T : IHyperCube<double>
        {
            var dblScore = new DoubleObjectiveScore(scoreName, result, maximise: maximise);
            return new MultipleScores<T>(new IObjectiveScore[] { dblScore }, sysConfig);
        }

        public static T[][] MakeBins<T>(T[] population, int numBins)
        {
            int div = population.Length / numBins;
            int remainder = population.Length % numBins;
            T[][] result = new T[numBins][];
            int offset = 0;
            for (int i = 0; i < numBins; i++)
            {
                int len = (i < remainder ? div+1 : div);
                result[i] = new T[len];
                Array.Copy(population, offset, result[i], 0, len);
                offset += len;
            }
            return result;
        }
    }
}
