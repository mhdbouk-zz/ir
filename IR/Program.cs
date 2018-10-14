using System;

namespace IR
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"init.. {DateTime.Now}");

            StopList stopList = new StopList(@"assets\StopList.txt");
            

        }
    }
}
