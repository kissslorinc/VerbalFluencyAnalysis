using System;
using System.Collections.Generic;
using System.Text;

namespace VerbalFluencyAnalysis
{
    internal class DiagnosisMetric
    {

        public DiagnosisMetric(string name)
        {
            Name = name;
        }

        public DiagnosisMetric(Participant firstParticipant)
        {
            Name = firstParticipant.Diagnosis;
            _clusterNoMetric += firstParticipant.NumberOfClusters;
            _clusterSizeMetric += firstParticipant.AverageSizeOfClusters;
            _wordNoMetric += firstParticipant.NumberOfWords;

            _dataCount++;
        }
        public string Name { get; set; }

        private double _clusterNoMetric = 0;
        public double AvgClusterNo { get { return _clusterNoMetric / _dataCount; } }

        private double _clusterSizeMetric = 0;
        public double AvgClusterSize { get { return _clusterSizeMetric / _dataCount; } }


        private double _wordNoMetric = 0;
        public double AvgWordNo { get { return _wordNoMetric / _dataCount; } }

        private int _dataCount = 0;

        public void AddDataPoint(int numberOfClusters, int avgClusterSize, int numberOfWords)
        {
            _clusterNoMetric += numberOfClusters;
            _clusterSizeMetric += avgClusterSize;
            _wordNoMetric += numberOfWords;

            _dataCount++;
        }

        public void AddDataPoint(Participant dataPoint)
        {
            _clusterNoMetric += dataPoint.NumberOfClusters;
            _clusterSizeMetric += dataPoint.AverageSizeOfClusters;
            _wordNoMetric += dataPoint.NumberOfWords;

            _dataCount++;
        }

    }
}
