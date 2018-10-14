using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IR
{
    public class StopList
    {
        private readonly string _stopListPath;
        public string[] StopWords { get; private set; }
        /// <summary>
        /// Create new instance of the StopList engine
        /// </summary>
        /// <param name="path">Path of the StopList.txt</param>
        public StopList(string path)
        {
            _stopListPath = path;
            GetStopWords();
        }

        /// <summary>
        /// Generate list of stop words found in the stopList.txt (_stopListPath);
        /// </summary>
        private void GetStopWords()
        {
            StopWords = File.ReadAllLines(_stopListPath);
        }
    }
}
