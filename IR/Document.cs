using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR
{
    public class Document
    {
        private readonly string _documentPath;
        private readonly string _fileNameWithoutExtension;
        private List<string> _documentWords;
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
        public async Task GenerateStopListAsync(StopList stopList)
        {
            IsStopListValid(stopList);

            var documentStpWords = _documentWords.Where(x => !stopList.StopWords.Any(s => s == x)).ToList();

            await File.WriteAllLinesAsync($"{AppConstant.StpDirectory}\\{_fileNameWithoutExtension}{AppConstant.StpExtension}", documentStpWords.ToArray());
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
    }
}
