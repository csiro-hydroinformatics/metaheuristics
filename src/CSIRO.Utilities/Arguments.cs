/*
 * 
 * J-M, 2007-04-25
 * Using a class for processing arguments. There is no easily reusable such thing 
 * in TIME. Evaluate usefulness, if satisfactory this should be promoted and/or built upon.
 * Added a storate for arguments that are not preceded by a 'switch' (e.g. main input file)
 */


/*
* Arguments class: application arguments interpreter
*
* Authors:		R. LOPES
* Contributors:	R. LOPES
* Created:		25 October 2002
* Modified:		28 October 2002
*
* Version:		1.0
*/

using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace CSIRO.Utilities
{
    /// <summary>
    /// Arguments class. NOTE: Consider also CommandLineParser (this present is simpler however)
    /// </summary>
    /// <remarks>
    /// Arguments class: application arguments interpreter
    ///
    /// Authors:		R. LOPES
    /// Contributors:	R. LOPES
    /// Created:		25 October 2002
    /// Modified:		28 October 2002
    ///
    /// Version:		1.0
    /// </remarks>
    public class Arguments
    {
        // Variables
        private Dictionary<string, string> Parameters;
        private List<string> ParameterlessArguments;

        private string originalCommandLineArgs;
        public string OriginalCommandLineArgs
        {
            get { return originalCommandLineArgs; }
        }

        // Constructor
        public Arguments(string[] Args)
        {
            captureOriginalCmdLineArgs(Args);
            Parameters = new Dictionary<string, string>();
            ParameterlessArguments = new List<string>();
            // Regex Spliter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            // 2013-11 per202. I disable the use of 'slash' as a start for options. This is a nuisance running on Linux.
            // Regex Spliter = new Regex(@"^-{1,2}|^/|=", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex Spliter = new Regex(@"^-{1,2}|=", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string Parameter = null;
            string[] Parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
            foreach (string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
                Parts = Spliter.Split(Txt, 3);
                switch (Parts.Length)
                {
                    // Found a value (for the last parameter found (space separator))
                    case 1:
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                            {
                                Parts[0] = Remover.Replace(Parts[0], "$1");
                                Parameters.Add(Parameter, Parts[0]);
                            }
                            Parameter = null;
                        }
                        else // this is an argument with no parameters, e.g. file.txt in a 
                        // command line input such as 'runmodel -param1 1.2 file.txt'
                        {
                            ParameterlessArguments.Add(Parts[0]);
                        }
                        break;
                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter)) Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];
                        break;
                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter)) Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];
                        // Remove possible enclosing characters (",')
                        if (!Parameters.ContainsKey(Parameter))
                        {
                            Parts[2] = Remover.Replace(Parts[2], "$1");
                            Parameters.Add(Parameter, Parts[2]);
                        }
                        Parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (Parameter != null)
            {
                if (!Parameters.ContainsKey(Parameter)) Parameters.Add(Parameter, "true");
            }
        }

        private void captureOriginalCmdLineArgs(string[] Args)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < Args.Length; i++)
            {
                s.Append(Args[i]);
                s.Append(" ");
            }
            originalCommandLineArgs = s.ToString();
        }

        public bool ContainsKey(string param)
        {
            return Parameters.ContainsKey(param);
        }

        public ICollection<string> Keys
        {
            get { return Parameters.Keys; }
        }

        // Retrieve a parameter value if it exists
        public string this[string Param]
        {
            get
            {
                return (Parameters[Param]);
            }
        }

        public string GetSingleArgument(int index)
        {
            return ParameterlessArguments[index];
        }

        public int SingleArgumentCount
        {
            get { return ParameterlessArguments.Count; }
        }

        public int ParameterArgumentCount
        {
            get { return Parameters.Count; }
        }
    }
}
