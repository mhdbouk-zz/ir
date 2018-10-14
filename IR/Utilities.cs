using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IR
{
    public static class Utilities
    {
        /// <summary>
        /// Get list of file path from directory
        /// </summary>
        /// <param name="directoryPath">Directory Path to look for files</param>
        /// <param name="ext">Filter by extesions</param>
        /// <returns></returns>
        public static string[] GetFilesFromDirectory(string directoryPath, List<string> ext = null)
        {
            var result = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            if (ext != null && ext.Count > 0)
            {
                result = result.Where(s => ext.Contains(Path.GetExtension(s))).ToArray();
            }
            return result;
        }
    }
}
