using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Metaheuristics.DataModel
{
    public class ObjectiveScore
    {
        public int ObjectiveScoreId { get; set; }
        public string Name { get; set; }
        public bool Maximize { get; set; }
        public string Value { get; set; }
        public virtual ObjectiveScoreCollection ObjectiveScores{ get; set; }
    }

    public class ObjectivesResultsCollection
    {
        public string Name { get; set; }
        public int ObjectivesResultsCollectionId { get; set; }
        public virtual ICollection<ObjectiveScoreCollection> ScoresSet { get; set; }
        public virtual TagCollection Tags { get; set; }
    }

    public class ObjectiveScoreCollection
    {

        public virtual SystemConfiguration SysConfiguration { get; set; }
        public int ObjectiveScoreCollectionId { get; set; }
        public virtual ObjectivesResultsCollection Collection { get; set; }
        public virtual ICollection<ObjectiveScore> Scores { get; set; }
    }

    public class TagCollection
    {
        public int TagCollectionId { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<ObjectivesResultsCollection> ResultsCollections { get; set; }
        public virtual ICollection<SystemConfiguration> SysConfigs { get; set; }
    }

    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public abstract class SystemConfiguration
    {
        public int SystemConfigurationId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ObjectiveScoreCollection> Scores { get; set; }
        public virtual TagCollection Tags { get; set; }
    }

    public class HyperCube : SystemConfiguration
    {
        //public int HyperCubeId { get; set; }
        public virtual ICollection<VariableSpecification> Variables { get; set; }
    }

    public class VariableSpecification
    {
        public int VariableSpecificationId { get; set; }
        public string Name { get; set; }
        public float Minimum { get; set; }
        public float Maximum { get; set; }
        public float Value { get; set; }
    }
}
