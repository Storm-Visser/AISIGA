using AISIGA.Program.AIS;
using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.IGA
{
    static class EVOFunctions
    {
        public static ExperimentConfig? Config { get; set; }

        public static Antibody ElitismSelection(List<Antibody> population)
        {
            Antibody bestContestant = population.OrderByDescending(a => a.GetFitness().GetTotalFitness()).First();

            return bestContestant;
        }

        public static Antibody TournamentSelect(List<Antibody> population, int tournamentSize)
        {
            // Randomly select 'tournamentSize' antibodies
            List<Antibody> tournamentContestants = new List<Antibody>();
            Random rand = new Random();

            for (int i = 0; i < tournamentSize; i++)
            {
                int randomIndex = rand.Next(population.Count);
                tournamentContestants.Add(population[randomIndex]);
            }

            // Select the best antibody from the tournament
            Antibody bestContestant = tournamentContestants.OrderByDescending(a => a.GetFitness().GetTotalFitness()).First();

            return bestContestant;
        }


        public static Antibody MutateAntibody(Antibody antibody, List<Antigen> allAntigens)
        {
            if (Config == null)
            {
                throw new Exception("Config is not set. Please set the config before calling this function.");
            }

            if (Config.UseAffinityMaturationMutation)
            {
                return AffinityMaturationMutation(antibody, allAntigens);
            }

            //Mutate class
            if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationFrequency)
            {
                //get a list of possible classes (not the current class) from encoding
                List<int> possibleClasses = Enumerable.Range(0, LabelEncoder.ClassCount)
                    .Where(c => c != antibody.GetClass())
                    .ToList();
                //randomly select a class from the list
                antibody.SetClass(possibleClasses[RandomProvider.GetThreadRandom().Next(possibleClasses.Count)]);
            }


            //Mutate base radius
            if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationFrequency)
            {
                antibody.SetBaseRadius(antibody.GetBaseRadius() * (RandomProvider.GetThreadRandom().NextDouble() * 1.9) + 0.1);
            }


            //Mutate feature values
            double[] featureValues = antibody.GetFeatureValues();
            for (int i = 0; i < featureValues.Length; i++)
            {
                if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationFrequency)
                {
                    // Mutate the values with a number between 0.1 and 2
                    double mutation = (RandomProvider.GetThreadRandom().NextDouble() * 1.9) + 0.1;
                    featureValues[i] *= mutation;
                }
            }

            if (Config.UseHyperEllipsoids || Config.UseUnboundedRegions)
            {
                //Mutate feature multipliers
                double[] featureMultipliers = antibody.GetFeatureMultipliers();
                for (int i = 0; i < featureMultipliers.Length; i++)
                {
                    if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationFrequency)
                    {
                        // Mutate the values with a number between 0.1 and 2
                        double mutation = (RandomProvider.GetThreadRandom().NextDouble() * 1.9) + 0.1;
                        featureMultipliers[i] *= mutation;
                    }
                }
            }

            if (Config.UseUnboundedRegions)
            {
                //Mutate feature dim types
                int[] featureDimTypes = antibody.GetFeatureDimTypes();
                for (int i = 0; i < featureDimTypes.Length; i++)
                {
                    if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationFrequency)
                    {
                        switch (featureDimTypes[i])
                        {
                            case 0:
                                featureDimTypes[i] = RandomProvider.GetThreadRandom().Next(1, 3);
                                break;
                            case 1:
                                featureDimTypes[i] = RandomProvider.GetThreadRandom().Next(0, 2) * 2;
                                break;
                            case 2:
                                featureDimTypes[i] = RandomProvider.GetThreadRandom().Next(0, 2);
                                break;
                            default:
                                throw new Exception("Invalid feature dim type");
                        }
                    }
                }
            }
            antibody.GetFitness().SetIsCalculated(false);
            return antibody;
        }

        public static (Antibody Child1, Antibody Child2) CrossoverAntibodies(Antibody Parent1, Antibody Parent2)
        {
            //todo: Implement crossover logic Make sure they are NEW antibodies
            if (Config == null)
            {
                throw new Exception("Config is not set. Please set the config before calling this function.");
            }

            // Create new antibodies
            Antibody child1 = new Antibody(0,0, Parent1.GetFeatureMultipliers().Length);
            Antibody child2 = new Antibody(0,0, Parent2.GetFeatureMultipliers().Length);

            //Select class
            if (RandomProvider.GetThreadRandom().NextDouble() < Config.CrossoverFrequency)
            {
                child1.SetClass(Parent2.GetClass());
                child2.SetClass(Parent1.GetClass());
            }
            else
            {
                child1.SetClass(Parent1.GetClass());
                child2.SetClass(Parent2.GetClass());
            }


            //Select base radius
            if (RandomProvider.GetThreadRandom().NextDouble() < Config.CrossoverFrequency)
            {
                child1.SetBaseRadius(Parent2.GetBaseRadius());
                child2.SetBaseRadius(Parent1.GetBaseRadius());
            }
            else
            {
                child1.SetBaseRadius(Parent1.GetBaseRadius());
                child2.SetBaseRadius(Parent2.GetBaseRadius());
            }


            //Select feature values
            double[] featureValues = Parent1.GetFeatureValues();
            for (int i = 0; i < featureValues.Length; i++)
            {
                if (RandomProvider.GetThreadRandom().NextDouble() < Config.CrossoverFrequency)
                {
                    child1.GetFeatureValues()[i] = Parent2.GetFeatureValues()[i];
                    child2.GetFeatureValues()[i] = Parent1.GetFeatureValues()[i];
                }
                else
                {
                    child1.GetFeatureValues()[i] = Parent1.GetFeatureValues()[i];
                    child2.GetFeatureValues()[i] = Parent2.GetFeatureValues()[i];
                }
            }

            if (Config.UseHyperEllipsoids || Config.UseUnboundedRegions)
            {
                //Select feature multipliers
                double[] featureMultipliers = Parent1.GetFeatureMultipliers();
                for (int i = 0; i < featureMultipliers.Length; i++)
                {
                    if (RandomProvider.GetThreadRandom().NextDouble() < Config.CrossoverFrequency)
                    {
                        child1.GetFeatureMultipliers()[i] = Parent2.GetFeatureMultipliers()[i];
                        child2.GetFeatureMultipliers()[i] = Parent1.GetFeatureMultipliers()[i];
                    }
                    else
                    {
                        child1.GetFeatureMultipliers()[i] = Parent1.GetFeatureMultipliers()[i];
                        child2.GetFeatureMultipliers()[i] = Parent2.GetFeatureMultipliers()[i];
                    }
                }
            }

            if (Config.UseUnboundedRegions)
            {
                //Select feature multipliers
                int[] featureDimTypes = Parent1.GetFeatureDimTypes();
                for (int i = 0; i < featureDimTypes.Length; i++)
                {
                    if (RandomProvider.GetThreadRandom().NextDouble() < Config.CrossoverFrequency)
                    {
                        child1.GetFeatureDimTypes()[i] = Parent2.GetFeatureDimTypes()[i];
                        child2.GetFeatureDimTypes()[i] = Parent1.GetFeatureDimTypes()[i];
                    }
                    else
                    {
                        child1.GetFeatureDimTypes()[i] = Parent1.GetFeatureDimTypes()[i];
                        child2.GetFeatureDimTypes()[i] = Parent2.GetFeatureDimTypes()[i];
                    }
                }
            }

            
            return (child1, child2);
        }

        public static Antibody AffinityMaturationMutation(Antibody child, List<Antigen> allAntigens)
        {
            return child;
        }
    }
}
