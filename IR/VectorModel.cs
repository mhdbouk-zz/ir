using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IR
{
    public class VectorModel
    {
        private readonly DocumentTerms _terms;
        private readonly List<Document> _documents;

        public VectorModel(DocumentTerms terms, List<Document> documents)
        {
            _terms = terms;
            _documents = documents;
        }

        public void GenerateInvertedFile(List<InvertedValue> booleanInvertedFiles)
        {
            List<InvertedValue> result = new List<InvertedValue>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _terms.Terms.Count; i++)
            {
                if (i != 0)
                {
                    sb.Append(_terms.Terms[i]);
                }

                sb.Append(AppConstant.CsvDelimiter);
            }

            StringBuilder rowsStringBuilder = new StringBuilder();
            foreach (var document in _documents)
            {
                rowsStringBuilder.Append($"{document.FileNameWithoutExtension}{AppConstant.CsvDelimiter}");
                List<InvertedValue> documentValues = new List<InvertedValue>();
                foreach (var term in _terms.Terms)
                {
                    bool found = document.IsTermExist(term);
                    rowsStringBuilder.Append($"{(found ? 1 : 0)}{AppConstant.CsvDelimiter}");

                    documentValues.Add(new InvertedValue
                    {
                        Document = document.FileNameWithoutExtension,
                        Term = term,
                        Value = found
                    });
                }
                result.AddRange(documentValues);
                rowsStringBuilder.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine(rowsStringBuilder.ToString());

            File.WriteAllText(AppConstant.VectorInvertedFile, sb.ToString());
        }
    }
}
}
