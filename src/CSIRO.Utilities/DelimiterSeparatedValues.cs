using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CSIRO.Utilities
{
    public class DelimiterSeparatedValues
    {
        private const char COMMA_CHARACTER = ',';

        public static string BuildReportingTable( IDataFrameInfoProvider[] tableProviders )
        {
            ReportTableBuilder tableBuilder = new ReportTableBuilder();
            for( int i = 0; i < tableProviders.Length; i++ )
            {
                tableBuilder.AddRow( tableProviders[i] );
            }
            return tableBuilder.GetReportTable( );
        }

        public static Dictionary<string, string> ParseOneLineDictionary(string line, char[] itemSeparators, char[] keyValueSeparator)
        {
            var entries = Array.ConvertAll(line.Trim().Split(itemSeparators), (x => x.Split(keyValueSeparator,2))); // Assume the key goes to the first occurance, and if there are more occurances assume they are part of the actual value.
            return entries.ToDictionary((x => x[0]), (x => x[1]));
        }

        public static Dictionary<string, string> ParseOneLineDictionary(string line)
        {
            char[] defaultItemSep = new char[] { '|' };
            char[] defaultKeyValueSep = new char[] { ',', ':' };
            return ParseOneLineDictionary(line, defaultItemSep, defaultKeyValueSep);
        }

        public static string[][] GetColumns(string fileName, int[] columnNumbers, char[] delimiters, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
        {
            return getColumns( fileName, columnNumbers, delimiters, 0, stringSplitOptions: stringSplitOptions );
        }

        public static string[] GetColumn( string fileName, int columnNumber, char delimiter )
        {
            return GetColumns( fileName, new int[] { columnNumber }, delimiter )[0];
        }

        public static string[][] GetColumns( string fileName, int[] columnNumbers, char delimiter, bool removeHeader )
        {
            return GetColumns( fileName, columnNumbers, delimiter, ( removeHeader ? 1 : 0 ) );
        }

        public static string[][] GetColumns( string fileName, int[] columnNumbers, char delimiter, int startLineIndex )
        {
            return getColumns( fileName, columnNumbers, new char[] { delimiter }, startLineIndex, stringSplitOptions: StringSplitOptions.None );
        }

        private static string[][] getColumns( string fileName, int[] columnNumbers, char[] separators, int startLineIndex, StringSplitOptions stringSplitOptions )
        {
            string[][] values = FileInputOutputUtilities.LoadDelimitedFile( fileName, separators, splitOptions: stringSplitOptions );
            string[][] result = new string[columnNumbers.Length][];

            for( int col = 0; col < columnNumbers.Length; col++ )
            {
                result[col] = new string[values.Length - startLineIndex];
                for( int i = startLineIndex; i < values.Length; i++ )
                    result[col][i - startLineIndex] = values[i][columnNumbers[col] - 1];
            }
            return result;
        }

        public static string[][] GetColumns( string fileName, int[] columnNumbers, char delimiter )
        {
            return GetColumns( fileName, columnNumbers, delimiter, false );
        }

        public static string[] GetColumnFromCommaSeparated( string fileName, int columnNumber )
        {
            return GetColumn( fileName, columnNumber, COMMA_CHARACTER );
        }

        /// <summary>
        /// Gets a set of columns from a CSV file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="columnNumbers">A array of column numbers (one-based indexing) to extract</param>
        /// <returns>An array of arrays, i.e. an array of columns in the 
        /// order specified by the indices passed as arguments to this function.</returns>
        public static string[][] GetColumnsFromCommaSeparated( string fileName, int[] columnNumbers )
        {
            return GetColumnsFromCommaSeparated( fileName, columnNumbers, false );
        }

        /// <summary>
        /// Gets a set of columns from a CSV file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="columnNumbers">A array of column numbers (one-based indexing) to extract</param>
        /// <returns>An array of arrays, i.e. an array of columns in the 
        /// order specified by the indices passed as arguments to this function.</returns>
        public static string[][] GetColumnsFromCommaSeparated( string fileName, int[] columnNumbers, bool removeHeader )
        {
            return GetColumns( fileName, columnNumbers, COMMA_CHARACTER, removeHeader );
        }

        public static string[][] LoadFromCommaSeparated( string fileName )
        {
            return FileInputOutputUtilities.LoadDelimitedFile( fileName, COMMA_CHARACTER );
        }

        public static string[][] MergeDictionaryTextFiles( string[] dictionaryFiles, string[] columnNames, char delimiter )
        {
            Dictionary<string, string>[] dicts = LoadDictionaries( dictionaryFiles, delimiter );
            return MergeDictionaries( dicts, columnNames );
        }

        public static string[][] MergeDictionaries( Dictionary<string, string>[] dicts, string[] columnNames )
        {
            List<string> keys = getUniqueKeys( dicts );
            List<string[]> result = new List<string[]>( );
            List<string> header = new List<string>( );
            header.Add( "Key" );
            for( int i = 0; i < columnNames.Length; i++ )
                header.Add( columnNames[i] );

            result.Add( header.ToArray( ) );

            foreach( string key in keys )
            {
                List<string> entries = new List<string>( );
                entries.Add( key );
                for( int i = 0; i < dicts.Length; i++ )
                {
                    entries.Add( dicts[i].ContainsKey( key ) ? dicts[i][key] : string.Empty );
                }
                result.Add( entries.ToArray( ) );
            }
            return result.ToArray( );
        }

        public static Dictionary<string, string>[] LoadDictionaries( string[] dictionaryFiles, string delimiter )
        {
            return LoadDictionaries( dictionaryFiles, Convert.ToChar( delimiter ) );
        }

        public static Dictionary<string, string>[] LoadDictionaries( string[] dictionaryFiles, char delimiter )
        {
            Dictionary<string, string>[] result = new Dictionary<string, string>[dictionaryFiles.Length];
            for( int i = 0; i < dictionaryFiles.Length; i++ )
                result[i] = LoadDictionary( dictionaryFiles[i], delimiter );

            return result;
        }

        public static Dictionary<string, string> LoadDictionary( string fileName, string delimiter )
        {
            return LoadDictionary( fileName, Convert.ToChar( delimiter ) );
        }

        public static Dictionary<string, string> LoadDictionary( string fileName, char delimiter )
        {
            Dictionary<string, string> result = new Dictionary<string, string>( );
            string[][] content = FileInputOutputUtilities.LoadDelimitedFile( fileName, delimiter );
            for( int i = 0; i < content.Length; i++ )
            {
                if( content[i].Length != 2 )
                    throw new Exception( "Expected exactly 2 columns in file " + fileName + " , but line number " + ( i + 1 ) + " has " + content[i].Length );
                if( result.ContainsKey( content[i][0] ) )
                    throw new Exception( "File " + fileName + ": found duplicate key '" + content[i][0] + "'" );
                result.Add( content[i][0], content[i][1] );
            }
            return result;
        }

        public static Dictionary<string, string> LoadDictionaryFromCommaSeparated( string fileName )
        {
            return LoadDictionary( fileName, COMMA_CHARACTER );
        }

        public static string SerialiseAsCommaSeparated( double[] values )
        {
            return SerialiseAsDelimiterSeparated( values, COMMA_CHARACTER );
        }

        public static string SerialiseAsDelimiterSeparated( double[] values, char delimiter )
        {
            StringBuilder s = new StringBuilder( );
            for( int i = 0; i < values.Length; i++ )
            {
                s.Append( values[i].ToString( CultureInfo.InvariantCulture ) );
                if( i < ( values.Length - 1 ) )
                    s.Append( delimiter );
            }
            return s.ToString( );
        }

        public static string SerialiseAsCommaSeparated( string[][] values )
        {
            StringBuilder s = new StringBuilder( );
            for( int i = 0; i < values.Length; i++ )
                s.AppendLine( SerialiseAsCommaSeparated( values[i] ) );
            return s.ToString( );
        }

        public static string SerialiseAsCommaSeparated( List<List<string>> lines )
        {
            return SerialiseAsCommaSeparated( toArray( lines ) );
        }

        private static string[][] toArray( List<List<string>> lines )
        {
            List<string[]> result = new List<string[]>( );
            for( int i = 0; i < lines.Count; i++ )
                result.Add( lines[i].ToArray( ) );
            return result.ToArray( );
        }

        public static string SerialiseAsCommaSeparated( string[] values )
        {
            return SerialiseAsDelimiterSeparated( values, COMMA_CHARACTER );
        }

        public static string SerialiseAsColumn( string[] values )
        {
            StringBuilder sb = new StringBuilder( );
            for( int i = 0; i < values.Length; i++ )
            {
                sb.AppendLine( values[i] );
            }
            return sb.ToString( );
        }

        public static string SerialiseAsOneColumn( string[] values )
        {
            StringBuilder s = new StringBuilder( );
            for( int i = 0; i < values.Length; i++ )
            {
                s.Append( values[i].ToString( CultureInfo.InvariantCulture ) );
                if( i < ( values.Length - 1 ) )
                    s.Append( Environment.NewLine );
            }
            return s.ToString( );
        }

        public static string SerialiseAsDelimiterSeparated( string[] values, char delimiter )
        {
            StringBuilder s = new StringBuilder( );
            for( int i = 0; i < values.Length; i++ )
            {
                s.Append( values[i].ToString( CultureInfo.InvariantCulture ) );
                if( i < ( values.Length - 1 ) )
                    s.Append( delimiter );
            }
            return s.ToString( );
        }

        private static List<string> getUniqueKeys( Dictionary<string, string>[] dicts )
        {
            List<string> result = new List<string>( );
            for( int i = 0; i < dicts.Length; i++ )
            {
                foreach( string key in dicts[i].Keys )
                    if( !result.Contains( key ) )
                        result.Add( key );
            }
            return result;
        }

        public static string[][] SplitAllLines( string[] dataLines, char[] separators, StringSplitOptions stringSplitOptions )
        {
            return Array.ConvertAll<string, string[]>( dataLines, ( x => x.Split( separators, stringSplitOptions ) ) );
        }
    }
}
