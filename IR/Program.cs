using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IR
{
    internal class Program
    {
        private static void Main(string[] args)
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

            Console.WriteLine($"Done Phase 1 - StopWords removal, please check generated files. ({DateTime.Now})");

            tasks = new List<Task>();

            documents.ForEach(x =>
            {
                tasks.Add(x.GenerateStemmedFileAsync());
            });

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Done Phase 2 - Suffix removal, please check generated files. ({DateTime.Now})");

            InvertedModel model = new InvertedModel(terms, documents);
            Console.WriteLine($"\tStart Generating Boolean Inverted File. ({DateTime.Now})");
            model.GenerateInvertedFile();
            Console.WriteLine($"\tDone Generating Boolean Inverted File. ({DateTime.Now})");
            Console.WriteLine();
            Console.WriteLine($"\tStart Generating TFIDF Inverted File. ({DateTime.Now})");
            model.GenerateTFIDFValuesFile();
            Console.WriteLine($"\tDone Generating TFIDF Inverted File. ({DateTime.Now})");
            Console.WriteLine();
            Console.WriteLine($"Done Phase 3 - Generate Inverted File (Boolean & TFIDF), please check generated files. ({DateTime.Now})");

            int queryNumber = 1;
            while (true)
            {
                Console.Write("Enter Query (enter EXIST to quit): ");
                string query = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(query) || query.ToUpper() == "EXIST")
                {
                    break;
                }
                Console.WriteLine($"Working on it... ({DateTime.Now})");
                Document queryDocument = new Document(terms, query);
                Task.WaitAll(queryDocument.GenerateStpFileAsync(stopList));
                Task.WaitAll(queryDocument.GenerateStemmedFileAsync());

                model.SubmitQuery(queryDocument, query, queryNumber);
                Console.WriteLine($"Done, check generated file. ({DateTime.Now})");
                queryNumber++;
            }


        }
    }
}
