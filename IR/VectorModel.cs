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

        public void GenerateInvertedFile()
        {
           // todo Vector
        }
    }
}
