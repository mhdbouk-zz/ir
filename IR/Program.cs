using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IR
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"init.. {DateTime.Now}");

            DocumentTerms terms = new DocumentTerms();

            StopList stopList = new StopList(AppConstant.StopListPath);

            string[] files = Utilities.GetFilesFromDirectory(AppConstant.DocumentDirectory, new List<string> { AppConstant.DocumentExtension });

            List<Document> documents = files.Select(f => new Document(f, terms)).ToList();
            List<Task> tasks = new List<Task>();

            documents.ForEach(x =>
            {
                tasks.Add(x.GenerateStpFileAsync(stopList));
            });

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Done Phase 1 - StopWords removal, please check generated files {DateTime.Now}");

            tasks = new List<Task>();

            documents.ForEach(x =>
            {
                tasks.Add(x.GenerateStemmedFileAsync());
            });

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Done Phase 2 - Suffix removal, please check generated files {DateTime.Now}");

            InvertedModel model = new InvertedModel(terms, documents);
            model.GenerateInvertedFile();
            model.GenerateTFIDFValuesFile();

            Console.WriteLine($"Done Phase 3 - Generate Inverted File (Boolean & TFIDF), please check generated files {DateTime.Now}");

            Console.Write("Enter Query: ");
            string query = Console.ReadLine();

            Document queryDocument = new Document(terms, query);
            Task.WaitAll(queryDocument.GenerateStpFileAsync(stopList));
            Task.WaitAll(queryDocument.GenerateStemmedFileAsync());


            model.SubmitQuery(queryDocument);
        }
    }
}
