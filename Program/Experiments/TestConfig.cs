using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Experiments
{
    class TestConfig : AbstractExperimentConfig
    {
        public override bool UseUI => true;
        public override int KFoldCount => 5;

        public override int NumberOfGenerations => 200;
        public override double PopulationSizeFractionOfDatapoints => 1;
        public override double PercentageOfParents => 0.2;
        public override int DataSetNr => 1;
        public override int NumberOfIslands => 4;

        public override double MutationRate => 0.4;
        public override double MutationFrequency => 0.8;
        public override double CrossoverRate => 0.8;
        public override double CrossoverFrequency => 0.5;
        public override double MigrationRate => 0.1;
        public override double MigrationFrequency => 0.1;
        public override double MasterMigrationFreq => 1;

        public override double aScoreMultiplier => 3.5;
        public override double bScoreMultiplier => 1.0;
        public override double cScoreMultiplier => 1.2;
        public override double dScoreMultiplier => 0.0;
        public override double eScoreMultiplier => 2.0;

        public override bool UseHyperSpheres => true;
        public override bool UseHyperEllipsoids => false;
        public override bool UseUnboundedRegions => false;
        public override bool UseAffinityMaturationMutation => false;
        public override double ElitismPercentage => 0.2;
        public override bool UseTournamentSelection => true;
        public override int TournamentSize => 2;
        public override bool UseClassRatioLocking => true;
        public override bool UseUnboundedRatioLocking => false;
        public override double RateOfUnboundedRegions => 0.2;   

    }
}
