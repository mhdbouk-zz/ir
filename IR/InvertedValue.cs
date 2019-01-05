namespace IR
{
    public class InvertedValue
    {
        public string Term { get; set; }
        public string Document { get; set; }
        public bool Value { get; set; }
        public int DocumentFrequency { get; set; }
        public int TermFrequency { get; set; }
        public double TFIDF { get; set; }
        public double QueryTFIDF { get; set; }
    }

    public class TermDocumentFrequency
    {
        public string Term { get; set; }
        public int DocumentFrequency { get; set; }
    }

    public class RelevantDocument
    {
        public int QueryId { get; set; }
        public int DocumentId { get; set; }
    }

    public class Query
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
    }
}
