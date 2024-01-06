using System;
using System.Collections.Generic;
using System.Text;

namespace VerbalFluencyAnalysis
{
    internal class DistanceMetric
    {
        public int Numerator { get; set; } = 0;
        public int Denominator { get; set; } = 0;

        public double Distance { get {  return (double)Numerator / Denominator; } }
    }
}
