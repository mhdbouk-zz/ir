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

            options:
            Console.WriteLine("=====================");
            Console.WriteLine("Choose one of the following options:");
            Console.WriteLine("1. Run MEDIAN Test Collection (will generate COS & Precision and Recall");
            Console.WriteLine("2. Run custom query search (will generate COS)");
            Console.WriteLine("3. Exit");

            string optionStr = Console.ReadLine();
            int.TryParse(optionStr, out int option);
            if (option == 1)
            {
                Console.WriteLine($"Generating MEDIAN. ({DateTime.Now})");
                model.RunAllQueries(stopList);
                Console.WriteLine($"Done generating MEDIAN, check generated files. ({DateTime.Now})");
                goto options;
            }
            else if (option == 2)
            {
                int queryNumber = 1;
                while (true)
                {
                    Console.Write("Enter Query (EXIT to exit): ");
                    string query = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(query) || query.ToUpper() == "EXIT")
                    {
                        break;
                    }
                    Console.WriteLine($"Working on it... ({DateTime.Now})");
                    Document queryDocument = new Document(terms, query);
                    Task.WaitAll(queryDocument.GenerateStpFileAsync(stopList));
                    Task.WaitAll(queryDocument.GenerateStemmedFileAsync());

                    model.SubmitQuery(queryDocument, query, queryNumber, string.Format(AppConstant.QueryCosFile, queryNumber));

                    Console.WriteLine($"Done, check generated file. ({DateTime.Now})");
                    queryNumber++;
                }
                goto options;
            }
            else if (option == 3)
            {
                return;
            }
            else
            {
                goto options;
            }
        }
    }
}
