using Microsoft.WindowsAzure.Storage.Blob;

namespace CsvToJson
{
    public class OutputBlobWriter : IBlobWriter
    {
        public OutputBlobWriter()
        {

        }

        public void WriteOutputBlob(CloudBlobContainer blobOutContainer, string jsonName, string jsonRecord)
        {
            var jsonBlob = blobOutContainer.GetBlockBlobReference(jsonName);
            jsonBlob.UploadTextAsync(jsonRecord);
        }
    }
}