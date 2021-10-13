using Microsoft.WindowsAzure.Storage.Blob;

namespace CsvToJson
{
    public interface IBlobWriter
    {
        void WriteOutputBlob(CloudBlobContainer blobOutContainer, string jsonName, string jsonRecord);
    }
}