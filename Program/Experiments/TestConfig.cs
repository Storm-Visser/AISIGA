using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Experiments
{
    class TestConfig : ExperimentConfig
    {
        public override bool UseUI => true;

        public override int NumberOfGenerations => 200;
        public override double PopulationSizeFractionOfDatapoints => 1.0;
        public override double PercentageOfParents => 0.2;
        public override int DataSetNr => 0;
        public override int NumberOfIslands => 4;

        public override double MutationRate => 0.1;
        public override double MutationFrequency => 0.05;
        public override double CrossoverRate => 0.8;
        public override double CrossoverFrequency => 0.5;
        public override double MigrationRate => 0.1;
        public override double MigrationFrequency => 0.05;
        public override double MasterMigrationFreq => 1;

        public override double BaseRadius => 0.3;
        public override double aScoreMultiplier => 2.5;
        public override double bScoreMultiplier => 1.0;
        public override double cScoreMultiplier => 1.2;
        public override double dScoreMultiplier => 0.0;
        public override double eScoreMultiplier => 1.0;

        public override bool UseHyperSpheres => true;
        public override bool UseHyperEllipsoids => false;
        public override bool UseUnboundedRegions => false;
        public override bool UseAffinityMaturationMutation => false;
        public override bool UseTournamentSelection => false;
        public override int TournamentSize => 2;
        public override bool UseClassRatioLocking => false;
        public override bool UseUnboundedRatioLocking => false;
        public override double RateOfUnboundedRegions => 0.2;   

    }
}
