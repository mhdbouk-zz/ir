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

            StopList stopList = new StopList(AppConstant.StopListPath);

            string[] files = Utilities.GetFilesFromDirectory(AppConstant.DocumentDirectory, new List<string> { AppConstant.DocumentExtension });

            List<Document> documents = files.Select(f => new Document(f)).ToList();
            List<Task> tasks = new List<Task>();

            documents.ForEach(x =>
            {
                tasks.Add(x.GenerateStopListAsync(stopList));
            });

            Task.WaitAll(tasks.ToArray());
        }
    }
}
