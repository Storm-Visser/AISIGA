using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Experiments
{
    abstract class AbstractExperimentConfig
    {
        public abstract bool UseUI { get; }
        public abstract int KFoldCount { get; }

        public abstract int NumberOfGenerations { get; }
        public abstract double PopulationSizeFractionOfDatapoints { get; }
        public abstract double PercentageOfParents { get; }
        public abstract int DataSetNr { get; }
        public abstract int NumberOfIslands { get; }

        public abstract double MutationRate { get; }
        public abstract double MutationFrequency { get; }
        public abstract double CrossoverRate { get; }
        public abstract double CrossoverFrequency { get; }
        public abstract double MigrationRate { get; }
        public abstract double MigrationFrequency { get; }
        public abstract double MasterMigrationFreq { get; }

        public abstract double aScoreMultiplier { get; }
        public abstract double bScoreMultiplier { get; }
        public abstract double cScoreMultiplier { get; }
        public abstract double dScoreMultiplier { get; }
        public abstract double eScoreMultiplier { get; }

        public abstract bool UseHyperSpheres { get; }
        public abstract bool UseHyperEllipsoids { get; }
        public abstract bool UseUnboundedRegions{ get; }
        public abstract bool UseAffinityMaturationMutation { get; }
        public abstract double ElitismPercentage { get; }
        public abstract bool UseTournamentSelection { get; }
        public abstract int TournamentSize { get; }
        public abstract bool UseClassRatioLocking { get; }
        public abstract bool UseUnboundedRatioLocking { get; }
        public abstract double RateOfUnboundedRegions { get; }
        public abstract bool DivideAntigens {  get; }

    }
}
