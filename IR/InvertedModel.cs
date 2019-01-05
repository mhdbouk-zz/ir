using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR
{
    public class InvertedModel
    {
        private readonly DocumentTerms _terms;
        private readonly List<Document> _documents;
        private List<InvertedValue> _invertedValues;
        private readonly List<TermDocumentFrequency> _termDocumentFrequencies;

        public InvertedModel(DocumentTerms terms, List<Document> documents)
        {
            _terms = terms;
            _documents = documents;
            _termDocumentFrequencies = _terms.Terms.Select(x => new TermDocumentFrequency { Term = x }).ToList();
        }

        /// <summary>
        /// Generate CSV file with all terms for all document display 1 / 0 if found and not. Also generate the DocumentFreq
        /// </summary>
        public void GenerateInvertedFile()
        {
            _invertedValues = new List<InvertedValue>();

            StringBuilder rowsStringBuilder = new StringBuilder();
            List<Task> tasks = new List<Task>();
            string lastFile = _documents.LastOrDefault().FileNameWithoutExtension;

            foreach (Document document in _documents)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    GenerateForOneDocument(document, lastFile);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            foreach (Document document in _documents)
            {
                rowsStringBuilder.Append(document.RowsStringBuilder.ToString());
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder documentFreqStringBuilder = new StringBuilder();
            sb.Append(AppConstant.CsvDelimiter);
            documentFreqStringBuilder.Append($"DocFreq{AppConstant.CsvDelimiter}");
            foreach (TermDocumentFrequency term in _termDocumentFrequencies)
            {
                sb.Append($"{term.Term}{AppConstant.CsvDelimiter}");
                documentFreqStringBuilder.Append($"{term.DocumentFrequency}{AppConstant.CsvDelimiter}");
            }

            sb.AppendLine();
            sb.AppendLine(rowsStringBuilder.ToString());
            sb.AppendLine(documentFreqStringBuilder.ToString());
            File.WriteAllText(AppConstant.BooleanInvertedFile, sb.ToString());
        }

        private void GenerateForOneDocument(Document document, string lastFile)
        {
            List<InvertedValue> values = new List<InvertedValue>();
            document.RowsStringBuilder.Append($"{document.FileNameWithoutExtension}{AppConstant.CsvDelimiter}");
            foreach (TermDocumentFrequency term in _termDocumentFrequencies)
            {
                bool found = document.IsTermExist(term.Term);
                int termFrequency = document.Count(term.Term);
                document.RowsStringBuilder.Append($"{(found ? 1 : 0)}{AppConstant.CsvDelimiter}");

                values.Add(new InvertedValue
                {
                    Document = document.FileNameWithoutExtension,
                    Term = term.Term,
                    Value = found,
                    TermFrequency = termFrequency
                });
                if (found)
                {
                    term.DocumentFrequency++;
                }
            }
            if (document.FileNameWithoutExtension != lastFile)
            {
                document.RowsStringBuilder.AppendLine();
            }
            document.InvertedValues = values;
        }

        /// <summary>
        /// Generate CSV file with all terms for all document display The TFIDF. Also generate the DocumentFreq
        /// </summary>
        public void GenerateTFIDFValuesFile()
        {
            StringBuilder rowsStringBuilder = new StringBuilder();
            List<Task> tasks = new List<Task>();
            string lastFile = _documents.LastOrDefault().FileNameWithoutExtension;

            foreach (Document document in _documents)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    GenerateTFIDFForOneDocument(document, lastFile);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            foreach (Document document in _documents)
            {
                rowsStringBuilder.Append(document.RowsStringBuilder.ToString());
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder documentFreqStringBuilder = new StringBuilder();
            sb.Append(AppConstant.CsvDelimiter);
            documentFreqStringBuilder.Append($"DocFreq{AppConstant.CsvDelimiter}");
            foreach (TermDocumentFrequency term in _termDocumentFrequencies)
            {
                sb.Append($"{term.Term}{AppConstant.CsvDelimiter}");
                documentFreqStringBuilder.Append($"{term.DocumentFrequency}{AppConstant.CsvDelimiter}");
            }

            sb.AppendLine();
            sb.AppendLine(rowsStringBuilder.ToString());
            sb.AppendLine(documentFreqStringBuilder.ToString());
            File.WriteAllText(AppConstant.TFIDFInvertedFile, sb.ToString());
        }

        private void GenerateTFIDFForOneDocument(Document document, string lastFile)
        {
            document.RowsStringBuilder = new StringBuilder();
            document.RowsStringBuilder.Append($"{document.FileNameWithoutExtension}{AppConstant.CsvDelimiter}");
            foreach (TermDocumentFrequency term in _termDocumentFrequencies)
            {
                // calculate TFIDF
                int docFreq = term.DocumentFrequency;
                double idf = Math.Log10((double)_documents.Count / docFreq);
                double tfidf = Math.Round(document.Count(term.Term) * idf, 3);
                document.RowsStringBuilder.Append($"{tfidf}{AppConstant.CsvDelimiter}");
            }
            if (document.FileNameWithoutExtension != lastFile)
            {
                document.RowsStringBuilder.AppendLine();
            }
        }

        /// <summary>
        /// Apply COS method for the query generated by the user
        /// </summary>
        /// <param name="query">Query should be cleaned (sufix and stoplist removal) before apply the query</param>
        public void SubmitQuery(Document query, string queryString, int queryNumber)
        {
            List<Task> tasks = new List<Task>();

            foreach (Document document in _documents)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    GenerateCosForOneDocument(document, query);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Query {queryNumber}: " + queryString);
            sb.AppendLine("=============================================");
            sb.AppendLine($"Document Name{AppConstant.CsvDelimiter}COS Value");
            List<(string fileName, double cosValue)> list = _documents.OrderByDescending(x => x.CosValue).Select(x => (x.FileNameWithoutExtension, x.CosValue)).ToList();
            foreach ((string fileName, double cosValue) in list)
            {
                sb.AppendLine($"{fileName}{AppConstant.CsvDelimiter}{cosValue}");
            }
            File.WriteAllText(string.Format(AppConstant.QueryCosFile, queryNumber), sb.ToString());
        }

        void GenerateCosForOneDocument(Document document, Document query)
        {
            double CosNumerator = 0;
            double CosDocumentDenominator = 0;
            double CosQueryDenominator = 0;
            foreach (TermDocumentFrequency term in _termDocumentFrequencies)
            {
                // calculate TFIDF
                int docFreq = term.DocumentFrequency;
                double idf = Math.Log10((double)_documents.Count / docFreq);
                double queryTFIDF = Math.Round(query.Count(term.Term) * idf, 3);
                double documentTFIDF = Math.Round(document.Count(term.Term) * idf, 3);

                CosNumerator += documentTFIDF * queryTFIDF;
                CosDocumentDenominator += Math.Pow(documentTFIDF, 2);
                CosQueryDenominator += Math.Pow(queryTFIDF, 2);
            }
            document.CosValue = Math.Round(CosNumerator / Math.Sqrt(CosDocumentDenominator * CosQueryDenominator), 3);
        }
    }
}
