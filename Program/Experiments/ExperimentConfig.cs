using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Experiments
{
    abstract class ExperimentConfig
    {
        public abstract int NumberOfGenerations { get; }
        public abstract int PopulationSize { get; }
        public abstract int DataSetNr { get; }
        public abstract double TrainingTestSplit { get; }
        public abstract int NumberOfIslands { get; }

        public abstract double MutationRate { get; }
        public abstract double CrossoverRate { get; }
        public abstract double MigrationRate { get; }
        public abstract double MigrationFrequency { get; }

        public abstract double aScoreMultiplier { get; }
        public abstract double bScoreMultiplier { get; }
        public abstract double cScoreMultiplier { get; }
        public abstract double dScoreMultiplier { get; }

        public abstract bool UseHyperSpheres { get; }
        public abstract bool UseHyperEllipsoids { get; }
        public abstract bool UseUnboundedRegions{ get; }
        public abstract bool UseAffinityMaturationMutation { get; }
        public abstract double BaseRadius { get; }


    }
}
