using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CsvToJson.Processors
{
    public interface IProcessor
    {
        void ProcessRecords(string data, string csvFile, CloudBlobContainer blobOutContainer,
            ILogger log);

    }
}