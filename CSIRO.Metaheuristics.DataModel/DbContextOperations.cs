using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.DataModel
{
    public class DbContextOperations
    {
        public static int SaveObjectiveResultsSet(IOptimizationResults<ISystemConfiguration> resultsSet)
        {
            return SaveObjectiveResultsSet(ConvertOptimizationResults.Convert(resultsSet));
        }

        public static int SaveObjectiveResultsSet(ObjectivesResultsCollection resultsSet)
        {
            using (var db = new OptimizationResultsContext())
            {
                db.ObjectivesResultsCollectionSet.Add(resultsSet);
                int recordsAffected = db.SaveChanges();
            }
            return resultsSet.ObjectivesResultsCollectionId;
        }

        public static ObjectivesResultsCollection[] FindResultsTagged(IDictionary<string, string> tags)
        {
            using (var db = new OptimizationResultsContext())
            {
                return db.ObjectivesResultsCollectionSet
                    .AsNoTracking()
                    .Where((x => ContainsAllTags(x, tags))).ToArray();//.Tags.Tags.FirstOrDefault((y => y.TagId).FirstOrDefault().Value );
            }
        }

        public static bool ContainsAllTags(ObjectivesResultsCollection x, IDictionary<string, string> tags)
        {
            var xTags = x.Tags.Tags;
            return tags.All((dictTag =>
                (xTags.Any((xTag =>
                    (dictTag.Key == xTag.Name && dictTag.Value == xTag.Value))))));
        }
    }
}
