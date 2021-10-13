using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace CsvToJson
{
    public class ParallelProcessor:IProcessor
    {
        private IBlobWriter _blobWriter;
        public ParallelProcessor(IBlobWriter blobWriter)
        {
            _blobWriter = blobWriter;
        }

        public void ProcessRecords(string data, string csvFile, CloudBlobContainer blobOutContainer, ILogger log)
        {
            int recordId = 0;
            var name = csvFile.Split(".")[0];

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLowerInvariant(),
                NewLine = Environment.NewLine,
            };

            using TextReader textReader = new StringReader(data);
            using var csvInput = new CsvReader(textReader, csvConfig);
            {
                var records = csvInput.GetRecords<CsvRecordType>();

                if (records != null)
                    Parallel.ForEach(records, record =>
                    {
                        var jsonRecord = JsonConvert.SerializeObject(record);

                        var jsonName = $"{name}{Interlocked.Increment(ref recordId)}.json";

                        _blobWriter.WriteOutputBlob(blobOutContainer, jsonName, jsonRecord);

                        log.LogInformation($"Blob written successfully\n From:{csvFile} To:{jsonName}");
                    });
            }
        }
    }
}