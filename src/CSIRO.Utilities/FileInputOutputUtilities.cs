using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CSIRO.Utilities
{
    public class FileInputOutputUtilities
    {

        public static string[][] LoadDelimitedFile( string fileName, char delimiter, StringSplitOptions splitOptions = StringSplitOptions.None )
        {
            return LoadDelimitedFile( fileName, new char[] { delimiter }, splitOptions );
        }

        public static string[][] LoadDelimitedFile( string fileName, char[] separators, StringSplitOptions splitOptions = StringSplitOptions.None )
        {
            if( !System.IO.File.Exists( fileName ) )
                throw new ArgumentException( "Cannot find delimited file: " + fileName );
            List<string[]> result = new List<string[]>( );
            string line = string.Empty;
            using( StreamReader sr = new StreamReader( fileName ) )
            {
                while( true )
                {
                    line = sr.ReadLine( );
                    if( line == string.Empty || line == null )
                        break;
                    result.Add( line.Split( separators, splitOptions ) );
                }
            }
            return result.ToArray( );
        }

        public static void SaveText( string text, string fileName )
        {
            using( StreamWriter sw = new StreamWriter( fileName ) )
            {
                sw.Write( text );
            }
        }
    }
}
