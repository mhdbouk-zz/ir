using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IR
{
    public class BooleanModel
    {
        private readonly DocumentTerms _terms;
        private readonly List<Document> _documents;
        private List<InvertedValue> _invertedValues;

        public BooleanModel(DocumentTerms terms, List<Document> documents)
        {
            _terms = terms;
            _documents = documents;
        }

        public void GenerateInvertedFIle()
        {
            _invertedValues = new List<InvertedValue>();

            StringBuilder rowsStringBuilder = new StringBuilder();
            for (int i = 0; i < _documents.Count; i++)
            {
                Document document = _documents[i];
                rowsStringBuilder.Append($"{document.FileNameWithoutExtension}{AppConstant.CsvDelimiter}");
                foreach (string term in _terms.Terms)
                {
                    bool found = document.IsTermExist(term);
                    int termFrequency = document.Count(term);
                    rowsStringBuilder.Append($"{(found ? 1 : 0)}{AppConstant.CsvDelimiter}");

                    _invertedValues.Add(new InvertedValue
                    {
                        Document = document.FileNameWithoutExtension,
                        Term = term,
                        Value = found,
                        TermFrequency = termFrequency
                    });
                }
                if (i < _documents.Count - 1)
                {
                    rowsStringBuilder.AppendLine();
                }
            }

            foreach (InvertedValue item in _invertedValues)
            {
                item.DocumentFrequency = _invertedValues.Count(x => x.Term == item.Term && x.Value);
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder documentFreqStringBuilder = new StringBuilder();
            sb.Append(AppConstant.CsvDelimiter);
            documentFreqStringBuilder.Append($"DocFreq{AppConstant.CsvDelimiter}");
            for (int i = 0; i < _terms.Terms.Count; i++)
            {
                string term = _terms.Terms[i];
                sb.Append($"{term}{AppConstant.CsvDelimiter}");
                documentFreqStringBuilder.Append($"{_invertedValues.FirstOrDefault(x => x.Term == term).DocumentFrequency}{AppConstant.CsvDelimiter}");
            }

            sb.AppendLine();
            sb.AppendLine(rowsStringBuilder.ToString());
            sb.Append(documentFreqStringBuilder.ToString());

            File.WriteAllText(AppConstant.BooleanInvertedFile, sb.ToString());
        }

        public void GenerateTFIDFValuesFile()
        {
            StringBuilder rowsStringBuilder = new StringBuilder();
            for (int i = 0; i < _documents.Count; i++)
            {
                Document document = _documents[i];
                rowsStringBuilder.Append($"{document.FileNameWithoutExtension}{AppConstant.CsvDelimiter}");
                foreach (string term in _terms.Terms)
                {
                    // calculate TFIDF
                    InvertedValue invertedValue = _invertedValues.FirstOrDefault(x => x.Term == term && x.Document == document.FileNameWithoutExtension);
                    double idf = Math.Log10((double)_documents.Count / invertedValue.DocumentFrequency);
                    double tfidf = invertedValue.TermFrequency * idf;

                    rowsStringBuilder.Append($"{Math.Round(tfidf, 2)}{AppConstant.CsvDelimiter}");
                }
                if (i < _documents.Count - 1)
                {
                    rowsStringBuilder.AppendLine();
                }
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder documentFreqStringBuilder = new StringBuilder();
            sb.Append(AppConstant.CsvDelimiter);
            documentFreqStringBuilder.Append($"DocFreq{AppConstant.CsvDelimiter}");
            for (int i = 0; i < _terms.Terms.Count; i++)
            {
                string term = _terms.Terms[i];
                sb.Append($"{term}{AppConstant.CsvDelimiter}");
                documentFreqStringBuilder.Append($"{_invertedValues.FirstOrDefault(x => x.Term == term).DocumentFrequency}{AppConstant.CsvDelimiter}");
            }

            sb.AppendLine();
            sb.AppendLine(rowsStringBuilder.ToString());
            sb.Append(documentFreqStringBuilder.ToString());

            File.WriteAllText(AppConstant.TFIDFInvertedFile, sb.ToString());
        }
    }
}
