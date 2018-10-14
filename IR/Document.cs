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
        private List<string> _documentStpWords;
        /// <summary>
        /// Create new instance of Document. We will use this document to apply the project steps on it
        /// </summary>
        /// <param name="path">Path of the document on disk</param>
        public Document(string path)
        {
            _documentPath = path;
            _documentWords = File.ReadAllText(path).Split(AppConstant.GetDelimiters(), StringSplitOptions.RemoveEmptyEntries).ToList();
            _fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        }

        public async Task GenerateStopListAsync(StopList stopList)
        {
            IsStopListValid(stopList);

            _documentStpWords = _documentWords.Where(x => !stopList.StopWords.Any(s => s == x)).ToList();
            _documentStpWords.ForEach(x =>
            {
                AppConstant.GetDelimiters().ToList().ForEach(d =>
                {
                    x = x.Replace(d, "");
                });
            });
            await File.WriteAllLinesAsync($"assets\\stp\\{_fileNameWithoutExtension}{AppConstant.StpExtension}", _documentStpWords.ToArray());
        }

        private void IsStopListValid(StopList stopList)
        {
            if (stopList == null)
            {
                throw new ArgumentNullException(nameof(stopList));
            }
        }
    }
}
