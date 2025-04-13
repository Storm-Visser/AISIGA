using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Experiments
{
    class TestConfig : ExperimentConfig
    {
        public override int NumberOfGenerations => 10;
        public override int PopulationSize => 10;
        public override int DataSetNr => 0; 
        public override double TrainingTestSplit => 0.8;
        public override int NumberOfIslands => 4;

        public override double MutationRate => 0.1;
        public override double CrossoverRate => 0.5;
        public override double MigrationRate => 0.1;
        public override double MigrationFrequency => 0.1;

        public override double aScoreMultiplier => 1.0;
        public override double bScoreMultiplier => 1.0;
        public override double cScoreMultiplier => 1.0;
        public override double dScoreMultiplier => 1.0;

        public override bool UseHyperSpheres => true;
        public override bool UseHyperEllipsoids => false;
        public override bool UseUnboundedRegions => false;
        public override bool UseAffinityMaturationMutation => false;
    }
}
