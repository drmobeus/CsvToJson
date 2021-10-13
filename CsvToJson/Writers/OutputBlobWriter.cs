using Microsoft.WindowsAzure.Storage.Blob;

namespace CsvToJson.Writers
{
    public class OutputBlobWriter : IBlobWriter
    {
        public void WriteOutputBlob(CloudBlobContainer blobOutContainer, string jsonName, string jsonRecord)
        {
            var jsonBlob = blobOutContainer.GetBlockBlobReference(jsonName);
            jsonBlob.UploadTextAsync(jsonRecord);
        }
    }
}