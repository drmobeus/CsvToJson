using System;
using System.IO;

namespace CsvToJson.UnitTests.Helpers
{
    public static class FileHelper
    {
        public static string GetFilePathAndName(string fileName)
        {
            return $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}{fileName}";
        }
    }
}