using System;
using System.Collections.Generic;
using System.Text;

namespace VerbalFluencyAnalysis
{
    internal class WordMetric
    {
        public string Word { get; set; }
        public int Frequency { get; set; }

        public int TotalPositionMetric { get; set; }
        public double AvgPosition { get { return (double)TotalPositionMetric / Frequency; } }

        public int TotalNumberOfWordsInStore { get; internal set; }

        public double FrequencyPercentage { get { return (double)Frequency / (double)TotalNumberOfWordsInStore; } }

        public override string ToString()
        {
            return Word;
        }
    }
}
