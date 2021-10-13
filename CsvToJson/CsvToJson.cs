using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
//using System.Text.Json;     //We need to talk about this!?
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

[assembly: FunctionsStartup(typeof(CsvToJson.Startup))]
namespace CsvToJson
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IBlobWriter, OutputBlobWriter>();

            builder.Services.AddScoped<IProcessor, ParallelProcessor>();
        }
    }

    public class CsvToJsonFunction
    {
        private IProcessor _processor;

        public CsvToJsonFunction(IProcessor processor)
        {
            _processor = processor;
        }

        [FunctionName("CsvToJson")]
        public  async Task Run([BlobTrigger("csv-in/{csvFileName}", Connection = "AzureWebJobsStorage")]Stream jsonSteStream, string csvFileName, ILogger log, ExecutionContext context,
            [Blob("json-out", FileAccess.Write, Connection = "AzureWebJobsStorageOutput")] CloudBlobContainer blobOutContainer,
            [Blob("csv-in", FileAccess.Read, Connection = "AzureWebJobsStorage")] CloudBlobContainer blobInContainer,
            [Blob("error-out", FileAccess.Write, Connection = "AzureWebJobsStorageError")] CloudBlobContainer blobErrorContainer)
        {
            log.LogInformation($"CSV to JSON Blob trigger function Processing blob\n Name:{csvFileName}");

            
            await blobOutContainer.CreateIfNotExistsAsync();
            await blobErrorContainer.CreateIfNotExistsAsync();

           
            if (csvFileName.EndsWith(".csv"))
            {
                try
                {
                    CloudBlockBlob csvblob = blobInContainer.GetBlockBlobReference(csvFileName);
                    var data = await csvblob.DownloadTextAsync();

                    _processor.ProcessRecords(data, csvFileName, blobOutContainer, log);

                    log.LogInformation($"All records in {csvFileName} have now been processed");

                    await csvblob.DeleteAsync(); 

                    log.LogInformation($"Blob deleted successfully from CSV container\n Name:{csvFileName}");
                }
                catch
                {
                    await MoveBlobToErrorsContainers(blobErrorContainer, blobInContainer, csvFileName);
                    log.LogError($"Cannot process Blob, so it was moved. \n Name:{csvFileName} to {blobErrorContainer.Name}");
                }
                
            }
            else
            {
                await MoveBlobToErrorsContainers(blobErrorContainer, blobInContainer, csvFileName);
                log.LogError($"Blob does not have '.csv' suffix, so it was moved. \n Name:{csvFileName} to {blobErrorContainer.Name}");
            }
        }

        private static async Task MoveBlobToErrorsContainers(CloudBlobContainer blobErrorContainer,
            CloudBlobContainer blobInContainer, string sourceFile)
        {
            CloudBlockBlob csvblob = blobInContainer.GetBlockBlobReference(sourceFile);
            var errorBlob = blobErrorContainer.GetBlockBlobReference($"{sourceFile}");
            await errorBlob.StartCopyAsync(csvblob);
            await csvblob.DeleteAsync();
        }
    }
}
