using AISIGA.Program.IGA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.AIS.VALIS
{
    static class VALIS
    {
        public static void AssingAGClassByVoting(List<Antibody> antibodies, List<Antigen> antigens)
        {
            Dictionary<Antigen, double[]> antigenVotes = new Dictionary<Antigen, double[]>();
            // initiate the vote dictionary
            foreach (Antigen antigen in antigens)
            {
                antigenVotes.Add(antigen, new double[LabelEncoder.ClassCount]);
            }

            // Cast the votes
            foreach (Antibody antibody in antibodies)
            {
                (List<Antigen> matchedAntigens, double[] _) = FitnessFunctions.GetMatchedAntigens(antibody, antigens);

                for (int i = 0; i < matchedAntigens.Count; i++)
                {
                    Antigen matchedAntigen = matchedAntigens[i];
                    antigenVotes[matchedAntigen][antibody.GetClass()] += antibody.GetFitness().GetTotalFitness();
                }
            }

            // Count the votes, and decide the class
            foreach ((Antigen antigen, double[] votes) in antigenVotes)
            {
                double maxVote = votes.Max();
                int maxVoteIndex = Array.IndexOf(votes, maxVote);
                antigen.SetAssignedClass(maxVoteIndex);
            }

        }
    }
}
