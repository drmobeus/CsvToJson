using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CsvToJson
{
    public interface IProcessor
    {
        void ProcessRecords(string data, string csvFile, CloudBlobContainer blobOutContainer,
            ILogger log);

    }
}