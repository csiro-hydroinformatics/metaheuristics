using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.DataModel;
using System.IO;
using IronPython.Hosting;

namespace CSIRO.Metaheuristics.Logging
{
    public class LoggingUtils
    {

        public static void WriteLoggerContent(InMemoryLogger logger, string outputCsvLogFile, string binFolder = @"//chrome-bu/c$/MH/bin/LogSce", 
            string strSysPaths = @"['.', '//chrome-bu/c$/cygwin/home/per202', '//chrome-bu/c$/bin/IronPython/Lib', '//chrome-bu/c$/bin/IronPython/DLLs', '//chrome-bu/c$/bin/IronPython', '//chrome-bu/c$/bin/IronPython/lib/site-packages']",
            string resultsName = "")
        {

            List<ObjectivesResultsCollection> result = new List<ObjectivesResultsCollection>();
            while (logger.Count > 0)
            {
                var item = logger.Dequeue();
                ObjectivesResultsCollection resultsSet = createResultsSet(item, resultsName);
                if (resultsSet != null)
                    result.Add(resultsSet);
            }
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();

            scope.SetVariable("binFolder", binFolder);
            engine.CreateScriptSourceFromString("strSysPaths = " + strSysPaths + Environment.NewLine).Execute(scope);
            engine.CreateScriptSourceFromString("print strSysPaths").Execute(scope);
            scope.SetVariable("outputCsvLogFile", outputCsvLogFile);
            engine.CreateScriptSourceFromString(firstPart).Execute(scope);
            scope.SetVariable("allResultsSets", result.ToArray());
            engine.CreateScriptSourceFromString(secondPart).Execute(scope);
        }

        public static string MakePythonSysPathsString(string[] paths)
        {
            StringBuilder sb= new StringBuilder();
            sb.Append("[ ");
            for (int i = 0; i < (paths.Length-1); i++)
			{
                sb.Append("'");
                sb.Append( paths[i] );
                sb.Append("' ,");
			}
            sb.Append("'");
            sb.Append(paths[paths.Length-1]);
            sb.Append("'");
            sb.Append(" ]");
            return sb.ToString();
        }

        private static ObjectivesResultsCollection createResultsSet(SysConfigLogInfo item, string resultsName = "")
        {
            IObjectiveScores[] arrayScores = item.Scores as IObjectiveScores[];
            if (arrayScores != null)
            {
                var tags = item.Tags;
                var result = ConvertOptimizationResults.Convert(arrayScores, attributes: tags);
                result.Name = resultsName;
                return result;
            }
            return null;
        }

                    private static string secondPart = @"keys = []
rows = []
i = 0
for s in allResultsSets:
    for scores in s.ScoresSet:
        line = []
        line.extend([(x.Name, x.Value) for x in scores.SysConfiguration.Variables])
        line.extend([(x.Name, x.Value) for x in scores.Scores])
        line.extend([(x.Name, x.Value) for x in s.Tags.Tags])
        line = dict(line)
        rows.append(line)
        keys.extend(line.keys())
    i = i+1
    if (i%(allResultsSets.Length/10) == 0): print i, '/', allResultsSets.Length


keys = unique(keys)
keys.sort()
print 'Keys of dataframe:', keys
print 'Exporting', len(rows) , 'rows to csv' , outputCsvLogFile
exportToCsvFile( rows, outputCsvLogFile, keys)
";



        private static string firstPart = @"import clr
import sys
sys.path = strSysPaths # ['.', '//chrome-bu/c$/cygwin/home/per202', '//chrome-bu/c$/bin/IronPython/Lib', '//chrome-bu/c$/bin/IronPython/DLLs', '//chrome-bu/c$/bin/IronPython', '//chrome-bu/c$/bin/IronPython/lib/site-packages']
import os, csv
clr.AddReference('System.Core')
import System
clr.ImportExtensions(System.Linq) # This needs to happen *before* the functions are defined.


assemblies = ['CSIRO.Metaheuristics.dll', 'CSIRO.Metaheuristics.DataModel.dll']

for assembly in assemblies:
    clr.AddReferenceToFileAndPath(os.path.join(binFolder, assembly))

from CSIRO.Metaheuristics.DataModel import ConvertOptimizationResults, DbContextOperations, OptimizationResultsContext

def exportToCsvFile(resultsToExport, outputFileName, keys=None):
    f = open(outputFileName,'wb') # Open as binary to prevent ending up with two carriage returns on Windows: /r/r/n.
    if (keys is None):
        keys = resultsToExport[0].keys()
    dw = csv.DictWriter(f, delimiter=',', fieldnames=keys)
    dw.writeheader()
    dw.writerows(resultsToExport)
    f.close()

def getTags(objectivesResultsCollection):
    tags = [x for x in objectivesResultsCollection.Tags.Tags]
    return dict([(x.Name, x.Value) for x in tags])


def hasTags( objectivesResultsCollection, tagsAsDicts ):
    d = getTags(objectivesResultsCollection)
    for key in tagsAsDicts:
        if (d.has_key(key) == False): return False
        if (d[key] != tagsAsDicts[key]): return False
    return True

def unique(seq): return list(set(seq))

";
    }
}
