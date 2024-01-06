using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace VerbalFluencyAnalysis
{
    internal class Program
    {
        static void Main(string[] args)
        {

            List<Participant> participantData = new List<Participant>();

            using (var reader = new StreamReader("data_concat.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    int i = 1;
                    List<string> words = new List<string>();
                    while (i < 52 && csv.GetField<string>($"{i}.") != "")
                    {
                        words.Add(csv.GetField<string>($"{i}."));
                        i++;
                    }

                    participantData.Add(new Participant()
                    {
                        Id = csv.GetField<int>("Sorszam"),
                        Gender = csv.GetField<string>("neme"),
                        Age = csv.GetField<int>("életkor"),
                        Education = csv.GetField<string>("iskolázottság"),
                        Diagnosis = csv.GetField<string>("alapbetegség"),
                        Words = words
                    });
                }
            }




            // words' frequency
            List<WordMetric> wordMetrics = new List<WordMetric>();

            // going through all participants' answers
            participantData.ForEach(p =>
            {
                int i = 0;
                // going through all words in a result
                p.Words.ForEach(w =>
                {
                    i++;

                    // checking if we have metrics associated with this word
                    var index = wordMetrics.FindIndex(m => m.Word == w);
                    // if not, we create a new one
                    if (index != -1)
                    {
                        wordMetrics[index].Frequency++;
                        wordMetrics[index].TotalPositionMetric += i;
                    }
                    // if yes, we update the values with the new find
                    else
                        wordMetrics.Add(new WordMetric()
                        {
                            Word = w,
                            Frequency = 1,
                            TotalPositionMetric = i,
                            TotalNumberOfWordsInStore = participantData.Count
                        });
                });
            });


            // distance matrix
            DistanceMetric[,] distanceMatrix = new DistanceMetric[wordMetrics.Count, wordMetrics.Count];

            var wordsList = wordMetrics.Select(w => w.Word).ToList();

            participantData.ForEach(p =>
            {
                for (int i = 0; i < p.Words.Count; i++)
                {
                    for (int j = i + 1; j < p.Words.Count; j++)
                    {
                        int x = wordsList.IndexOf(p.Words[i]);
                        int y = wordsList.IndexOf(p.Words[j]);

                        if (distanceMatrix[x, y] == null)
                            distanceMatrix[x, y] = new DistanceMetric();

                        distanceMatrix[x, y].Numerator += j - i;
                        distanceMatrix[x, y].Denominator++;
                    }
                }
            });

            // what is the right cluster treshold?
            // interesting: when trying out the treshold with different values, the number of clusters have a local minimum at 7.
            // this might be because of the average cluster size should be around the capacity of the working memory of a healthy adult.
            const int clusterTreshold = 7;

            // trying to find clusters from dataset
            List<List<WordMetric>> clusters = new List<List<WordMetric>>();
            bool[] clustered = new bool[wordMetrics.Count];

            // go through the columns of the matrix
            for (int i = 0; i < wordMetrics.Count; i++)
            {
                // if we have the word in a cluster already, skip
                if (clustered[i]) continue;

                // if we don't, create a new cluster...
                var cluster = new List<int>() { i };
                clustered[i] = true;
                int clusterIndex = 0;

                // ...and check which other words belong in this cluster (closer to the word than the set treshold)
                for (int j = i + 1; j < wordMetrics.Count; j++)
                    if (distanceMatrix[i, j] != null && distanceMatrix[i, j].Distance < clusterTreshold)
                    {
                        cluster.Add(j);
                        clustered[j] = true;
                    }

                // go through the found words in the new cluster and get the words near them too
                while (clusterIndex < cluster.Count - 1)
                {
                    clusterIndex++;

                    if (clustered[cluster[clusterIndex]]) continue;

                    for (int j = cluster[clusterIndex] + 1; j < wordMetrics.Count; j++)
                        if (distanceMatrix[cluster[clusterIndex], j] != null && distanceMatrix[cluster[clusterIndex], j].Distance < clusterTreshold)
                        {
                            cluster.Add(j);
                            clustered[j] = true;
                        }

                }

                // throw away clusters that are smaller than the cluster size (probably not enough data)
                //if (cluster.Count < clusterTreshold) continue;

                clusters.Add(cluster.ConvertAll(w => { return wordMetrics[w]; }));
            }

            // calculating participants' clusters
            participantData.ForEach(data =>
            {
                Console.WriteLine();
                Console.WriteLine($"Diagnosis: {data.Diagnosis}");
                for (int i = 0; i < data.Words.Count; i++)
                {
                    var clusterID = clusters.FindIndex(c => c.Exists(w => w.Word == data.Words[i]));
                    data.ClusterMap.Add(clusterID);
                    if (!data.ClusterIDs.Contains(clusterID)) data.ClusterIDs.Add(clusterID);

                    if (i != 0 && clusterID != data.ClusterMap[i - 1]) data.NumberOfClusterSwitches++;
                    Console.Write($"{clusterID} ");
                }
                Console.WriteLine();
                Console.WriteLine($"Number of words: {data.Words.Count}");
                Console.WriteLine($"Number of clusters: {data.ClusterIDs.Count}");
                Console.WriteLine($"Average cluster size: {data.AverageSizeOfClusters:f2}");
                Console.WriteLine($"Number of cluster switches: {data.NumberOfClusterSwitches}");

            });

            // calculating averages based on diagnosis
            List<DiagnosisMetric> diagnoses = new List<DiagnosisMetric>();
            participantData.ForEach(data =>
            {
                var diagnosis = diagnoses.Find(d => d.Name == data.Diagnosis);

                if (diagnosis != null)
                    diagnosis.AddDataPoint(data);
                else diagnoses.Add(new DiagnosisMetric(data));
            });

            Console.WriteLine();
            foreach (var item in diagnoses)
            {
                Console.WriteLine($"Diagnosis: {item.Name}");
                Console.WriteLine($"Average number of words: {item.AvgWordNo:f2}");
                Console.WriteLine($"Average number of clusters: {item.AvgClusterNo:f2}");
                Console.WriteLine($"Average size of clusters: {item.AvgClusterSize:f2}");
                Console.WriteLine();
            }

            // display all words sorted by frequency
            var sortedWords = from entry in wordMetrics orderby entry.FrequencyPercentage descending select entry;
            foreach (var word in sortedWords)
                Console.WriteLine($"{word.Word}        {word.FrequencyPercentage * 100:f2}% ({word.Frequency}/{word.TotalNumberOfWordsInStore})");

            Console.ReadLine();
        }
    }
}
