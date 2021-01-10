using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataProcessingExam
{
    public class DataProcessorService
    {
        private eServerState m_state;
        private Stopwatch stopwatch;
        private readonly ILogger<DataProcessorService> logger;
        private readonly FileConfiguration config;
        private List<WordAnalysis> wordAnalyses;
        public DataProcessorService(ILogger<DataProcessorService> logger, IOptions<FileConfiguration> config)
        {
            this.logger = logger;
            this.config = config.Value;
        }

        public async Task<List<WordAnalysis>> ProcessData()
        {
            m_state = eServerState.Running;
            stopwatch = Stopwatch.StartNew();

            wordAnalyses = await ProcessFile();

            stopwatch.Stop();
            m_state = eServerState.Completed;

            return wordAnalyses;
        }

        public List<WordAnalysis> GetLastResult => wordAnalyses;

        private async Task<List<WordAnalysis>> ProcessFile()
        {
            logger.LogInformation($"Initiating process {config.GetFileForProcessing()}");
            //TODO: Implement!

            if (!File.Exists(config.GetFileForProcessing()))
                throw new Exception();
            ConcurrentDictionary<char, int> dictionary = new ConcurrentDictionary<char, int>();
            foreach (var line in File.ReadLines(config.GetFileForProcessing()).AsParallel().WithDegreeOfParallelism(System.Environment.ProcessorCount))
            {
                foreach (var i in line.ToUpper())
                {
                    if (i >= 'A' && i <= 'Z')
                    {
                        dictionary.AddOrUpdate(i, 1, (key, oldValue) => oldValue + 1);
                    }
                }
            }

            //using (StreamReader sr = new StreamReader(config.GetFileForProcessing(), System.Text.Encoding.Default))
            //{
            //    string line;
            //    while ((line = await sr.ReadLineAsync()) != null)
            //    {
            //        foreach (var i in line.ToUpper())
            //        {
            //            if (i >= 'A' && i <= 'Z')
            //            {
            //                dictionary.AddOrUpdate(i, 1, (key, oldValue) => oldValue + 1);
            //            }
            //        }
            //    }
            //}

            List<WordAnalysis> answer = new List<WordAnalysis>();
            foreach (var d in dictionary)
            {
                answer.Add(new WordAnalysis { Letter = d.Key, NumberOfOccurrences = d.Value });
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5)); //Simulates the process time

            logger.LogInformation($"Process completed {config.GetFileForProcessing()}. Execution time {stopwatch.Elapsed}");

            return new List<WordAnalysis> {
                new WordAnalysis { Letter = 'B', NumberOfOccurrences = 22 },
                new WordAnalysis { Letter = 'A', NumberOfOccurrences = 50 }
            };
        }

        public ServerState GeteServerState() => new ServerState
        {
            ProcessTime = stopwatch?.Elapsed ?? TimeSpan.Zero,
            State = m_state,
        };
    }
}
