using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace CSIRO.Metaheuristics.DataModel
{
    public class OptimizationResultsContext : DbContext
    {
        static OptimizationResultsContext()
        {
            Database.SetInitializer<OptimizationResultsContext>(
                new DropCreateDatabaseIfModelChanges<OptimizationResultsContext>());
        }

        public OptimizationResultsContext() : base("name=MH.Results") { }

        public DbSet<ObjectiveScore> ObjectiveScoreSet { get; set; }
        public DbSet<ObjectivesResultsCollection> ObjectivesResultsCollectionSet { get; set; }
        public DbSet<ObjectiveScoreCollection> ObjectiveScoresSet { get; set; }
        public DbSet<TagCollection> TagCollectionSet { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurationSet { get; set; }
        public DbSet<HyperCube> HyperCubeSet { get; set; }
        public DbSet<VariableSpecification> VariableSpecification { get; set; }
    }
}
