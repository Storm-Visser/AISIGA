using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Experiments
{
    class TestConfig : ExperimentConfig
    {
        public override int NumberOfGenerations => 1000;
        public override int PopulationSize => 300;
        public override double PercentageOfParents => 0.3;
        public override int DataSetNr => 0; 
        public override double TrainingTestSplit => 0.8;
        public override int NumberOfIslands => 4;

        public override double MutationRate => 0.01;
        public override double MutationFrequency => 0.05;
        public override double CrossoverRate => 0.8;
        public override double CrossoverFrequency => 0.5;
        public override double MigrationRate => 0.2;
        public override double MigrationFrequency => 0.1;

        public override double aScoreMultiplier => 10.0;
        public override double bScoreMultiplier => 1.0;
        public override double cScoreMultiplier => 0.0;
        public override double dScoreMultiplier => 0.0;
        public override double eScoreMultiplier => 0.0;

        public override bool UseHyperSpheres => true;
        public override bool UseHyperEllipsoids => true;
        public override bool UseUnboundedRegions => false;
        public override bool UseAffinityMaturationMutation => false;
        public override double BaseRadius => 0.5;
    }
}
