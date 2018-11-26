using System.Collections.Generic;

namespace IR
{
    public class DocumentTerms
    {
        public List<string> Terms { get; private set; }
        public DocumentTerms()
        {
            Terms = new List<string>();
        }
        public void Add(string term)
        {
            if (!Terms.Contains(term))
            {
                Terms.Add(term);
            }
        }
    }
}
