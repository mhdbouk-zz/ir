using System;
using System.Collections.Generic;
using System.Text;

namespace IR
{
    public static class AppConstant
    {
        public const string StopListPath = @"assets\StopList.txt";
        public const string DocumentDirectory = @"assets\documents";
        public const string DocumentExtension = ".txt";
        public const string StpExtension = ".stp";

        private static readonly string[] delimiters = new string[]{ "\r\n",".\r\n", ".\r", "\t", ";" , " & ",
                                            " ^ ",". "," | "," @","@ "," @ "," [ "," ] "," : ",": "," :",
                                            " \" "," ' ","?"," < "," > " ,",","\\","! "," #"," # "," % ","^ "," * ","  ","\t",
                                            " ( "," ) "," - "," _ "," + "," = "," { "," } "," [ "," ] ",":"," ", "“", "”", "—"};

        public static string[] GetDelimiters()
        {
            return delimiters;
        }

        public const string StpDirectory = @"assets\stp";
    }
}
