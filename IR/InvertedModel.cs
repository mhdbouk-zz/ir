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
        private readonly List<TermDocumentFrequency> _termDocumentFrequencies;
        private List<RelevantDocument> _relevantDocuments;

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
        public void SubmitQuery(Document query, string queryString, int queryNumber, string fileName, bool testCollection = false)
        {
            List<Task> tasks = new List<Task>();

            if (testCollection)
            {
                string[] files = File.ReadAllLines(AppConstant.RelevantDocumentsPath);

                _relevantDocuments = files.Select(x => x.Split("\t")).Select(x => new RelevantDocument { DocumentId = int.Parse(x[1]), QueryId = int.Parse(x[0]) }).ToList();
            }

            foreach (Document document in _documents)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    GenerateCosForOneDocument(document, query, testCollection);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Query {queryNumber}: " + queryString);
            sb.AppendLine("=============================================");
            sb.AppendLine();
            StringBuilder output = new StringBuilder($"Document Name{AppConstant.CsvDelimiter}COS Value");
            if (testCollection)
            {
                output.Append($"{AppConstant.CsvDelimiter}Relevant{AppConstant.CsvDelimiter}Precision{AppConstant.CsvDelimiter}Recall");
            }
            sb.AppendLine(output.ToString());
            List<(string name, double cosValue, bool relevant)> list = _documents.OrderByDescending(x => x.CosValue).Select(x => (x.FileNameWithoutExtension, x.CosValue, x.Relevant)).ToList();
            if (testCollection)
            {
                list = list.Where(x => x.cosValue > 0).ToList();
            }
            int relevantRetrieved = 0;
            int totlaRelevantCount = list.Sum(x => x.relevant ? 1 : 0);
            List<(double precision, double recall)> precisionAndRecalls = new List<(double precision, double recall)>();
            for (int i = 0; i < list.Count; i++)
            {
                (string name, double cosValue, bool relevant) = list[i];
                output = new StringBuilder($"{name}{AppConstant.CsvDelimiter}{cosValue}");
                if (testCollection)
                {
                    if (relevant)
                    {
                        relevantRetrieved++;
                    }
                    output.Append($"{AppConstant.CsvDelimiter}{(relevant ? "R" : "")}");
                    double precision = Math.Round((double)relevantRetrieved / (i + 1), 3);
                    double recall = Math.Round((double)relevantRetrieved / totlaRelevantCount, 3);
                    precisionAndRecalls.Add((precision, recall));
                    output.Append($"{AppConstant.CsvDelimiter}{precision}");
                    output.Append($"{AppConstant.CsvDelimiter}{recall}");
                }
                sb.AppendLine(output.ToString());
            }

            File.WriteAllText(fileName, sb.ToString());

            if (testCollection)
            {
                sb.Clear();

                // Calculate the Average Precision per unique Recall
                List<(double uniqueRecall, double avaragePrecision)> avarage = new List<(double uniqueRecall, double avaragePrecision)>();
                List<double> uniqueRecall = precisionAndRecalls.Select(x => x.recall).Distinct().ToList();
                foreach (double recall in uniqueRecall)
                {
                    double avaragePrecision = precisionAndRecalls.Where(x => x.recall == recall).Select(x => x.precision).Average();
                    avarage.Add((recall, avaragePrecision));
                }


                sb.AppendLine($"Average Precision{AppConstant.CsvDelimiter}Recall");
                foreach ((double recall, double avaragePrecision) in avarage)
                {
                    sb.AppendLine($"{avaragePrecision}{AppConstant.CsvDelimiter}{recall}");
                }

                File.WriteAllText(string.Format(AppConstant.AveragePath, query.FileNameWithoutExtension), sb.ToString());
            }
        }

        private void GenerateCosForOneDocument(Document document, Document query, bool testCollection = false)
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
            if (testCollection)
            {
                document.Relevant = _relevantDocuments.Any(x => x.DocumentId == document.Id && x.QueryId == query.Id);
            }
        }

        public void RunAllQueries(StopList stopList)
        {
            string[] files = Utilities.GetFilesFromDirectory(AppConstant.QueriesDirectory, new List<string> { AppConstant.QueryExtension });

            List<Document> documents = files.Select((f, index) => new Document(f, _terms)).ToList();
            List<Task> tasks = new List<Task>();

            documents.ForEach(x =>
            {
                tasks.Add(x.GenerateStpFileAsync(stopList));
            });

            Task.WaitAll(tasks.ToArray());

            tasks = new List<Task>();

            documents.ForEach(x =>
            {
                tasks.Add(x.GenerateStemmedFileAsync());
            });

            Task.WaitAll(tasks.ToArray());

            //tasks = new List<Task>();
            Console.WriteLine($"Query count: {documents.Count}");
            documents.ForEach(x =>
            {
                //tasks.Add(Task.Factory.StartNew(() =>
                //{
                string fileName = $"{AppConstant.QueryCosDirectory}\\{x.FileNameWithoutExtension}-cos.csv";
                SubmitQuery(x, x.Query, x.Id, fileName, testCollection: true);
                Console.WriteLine(x.FileNameWithoutExtension);
                //}));
            });

            //Task.WaitAll(tasks.ToArray());
        }
    }
}
