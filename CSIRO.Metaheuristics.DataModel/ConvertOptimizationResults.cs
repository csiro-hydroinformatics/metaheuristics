using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.DataModel
{
    public class ConvertOptimizationResults
    {
        public static ObjectivesResultsCollection Convert(IEnumerable<IObjectiveScores> results, string resultsSetName = "", IDictionary<string, string> attributes = null)
        {
            var resultsSet = new ObjectivesResultsCollection();
            resultsSet.Name = resultsSetName;
            var listOfPoints = new List<ObjectiveScoreCollection>();
            foreach (var item in results)
            {
                var newObjScore = new ObjectiveScoreCollection();
                newObjScore.Scores = Convert(item);
                newObjScore.SysConfiguration = Convert(item.GetSystemConfiguration());
                listOfPoints.Add(newObjScore);
            }
            resultsSet.ScoresSet = listOfPoints;
            resultsSet.Tags = Convert(attributes);
            return resultsSet;
        }

        private static TagCollection Convert(IDictionary<string, string> attributes)
        {
            if (attributes == null)
                return null;
            var result = new TagCollection();
            result.Tags = new List<Tag>();
            foreach (var item in attributes)
            {
                result.Tags.Add(new Tag { Name = item.Key, Value = item.Value });
            }
            return result;
        }

        public static SystemConfiguration Convert(ISystemConfiguration sysConfig)
        {
            var hc = sysConfig as IHyperCube<double>;
            if (hc == null)
                throw new NotSupportedException("Can only represent systen configurations that are hypercubes, as yet");
            var varNames = hc.GetVariableNames();
            var result = new HyperCube();
            result.Name = hc.GetConfigurationDescription();
            // TODO
            //result.Tags.Tags.Add(new Tag() { Name = "", Value = "" });
            result.Variables = new List<VariableSpecification>();
            foreach (var varName in varNames)
            {
                result.Variables.Add(new VariableSpecification 
                { 
                    Name = varName, 
                    Value = (float)hc.GetValue(varName),
                    Minimum = (float)hc.GetMinValue(varName),
                    Maximum = (float)hc.GetMaxValue(varName)
                });
            }
            return result;
        }

        private static List<ObjectiveScore> Convert(IObjectiveScores scores)
        {
            var scoreList = new List<ObjectiveScore>();
            for (int i = 0; i < scores.ObjectiveCount; i++)
            {
                var singleObjective = scores.GetObjective(i);
                scoreList.Add(
                    new ObjectiveScore
                    {
                        Name = singleObjective.Name,
                        Maximize = singleObjective.Maximise,
                        Value = singleObjective.ValueComparable.ToString()
                    });
            }
            return scoreList;
        }
    }
}
