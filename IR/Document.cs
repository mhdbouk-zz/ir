using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IR
{
    public class Document
    {
        private readonly string _documentPath;
        private readonly string _fileNameWithoutExtension;
        private string _stpFile => $"{AppConstant.StpDirectory}\\{_fileNameWithoutExtension}{AppConstant.StpExtension}";
        private string _sfxFile => $"{AppConstant.SfxDirectory}\\{_fileNameWithoutExtension}{AppConstant.SfxExtension}";
        private List<string> _documentWords;
        private List<string> _documentStpWords;
        /// <summary>
        /// Create new instance of Document. We will use this document to apply the project steps on it
        /// </summary>
        /// <param name="path">Path of the document on disk</param>
        public Document(string path)
        {
            _documentPath = path;
            _fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            PrepareDocument();
        }
        /// <summary>
        /// Read all the text from the _documentPath and save them after remove all delimiters into list of _documentWords
        /// </summary>
        private void PrepareDocument()
        {
            string document = File.ReadAllText(_documentPath);

            if (string.IsNullOrWhiteSpace(document))
            {
                return;
            }

            _documentWords = document
                                // Trim on all delimiters and remove empty entries
                                .Split(AppConstant.GetDelimiters(), StringSplitOptions.RemoveEmptyEntries)
                                // Trim each word with the Delimiters for Trim list
                                .Select(x => x.Trim(AppConstant.GetDelimitersForTrim()))
                                .Where(x => !x.IsInteger())
                                .ToList();
        }
        /// <summary>
        /// Generate The document stp list and save it to disk with extension .stp
        /// </summary>
        /// <param name="stopList">Instance of existing stopList that has the stop words</param>
        /// <returns></returns>
        public async Task GenerateStpFileAsync(StopList stopList)
        {
            IsStopListValid(stopList);

            _documentStpWords = _documentWords.Where(x => !stopList.StopWords.Any(s => s == x)).Select(x => x.ToLower()).ToList();

            await File.WriteAllLinesAsync($"{_stpFile}", _documentStpWords.ToArray());
        }
        /// <summary>
        /// Check if stopList is not null, if null then throw exception
        /// </summary>
        /// <param name="stopList">Instance of stopList to check</param>
        private void IsStopListValid(StopList stopList)
        {
            if (stopList == null)
            {
                throw new ArgumentNullException(nameof(stopList));
            }
        }
        /// <summary>
        /// Generate new document with .sfx extension after removing the suffixes using porter algorithm
        /// </summary>
        /// <returns></returns>
        public async Task GenerateStemmedFileAsync()
        {
            if (_documentStpWords == null)
            {
                return;
            }

            List<string> stemmedTerms = _documentStpWords.Select(term => Porter2Stemmer.EnglishPorter2Stemmer.Instance.Stem(term).Value).Distinct().ToList();

            await File.WriteAllLinesAsync($"{_sfxFile}", stemmedTerms.ToArray());

        }
    }
}
