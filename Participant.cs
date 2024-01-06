using System;
using System.Collections.Generic;
using System.Text;

namespace VerbalFluencyAnalysis
{
    internal class Participant
    {
        public int Id { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }

        public string Education { get; set; }

        public string Diagnosis { get; set; }

        public List<String> Words { get; set; }

        public int NumberOfWords { get { return Words.Count; } }

        public List<int> ClusterMap { get; set; } = new List<int>();

        public List<int> ClusterIDs { get; set; } = new List<int>();

        public int NumberOfClusters { get { return ClusterIDs.Count; } }

        public double AverageSizeOfClusters { get { return (double)NumberOfWords / NumberOfClusters; } }

        public int NumberOfClusterSwitches { get; set; } = 0;
    }
}
