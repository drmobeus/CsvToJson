using System;
using CsvToJson.Processors;
using CsvToJson.UnitTests.Helpers;
using CsvToJson.Writers;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using NUnit.Framework;

namespace CsvToJson.UnitTests
{
    [TestFixture]
    public class BaseTestSetups
    {
        public Mock<CloudBlobContainer> MockBlobJsonContainer;
        public Mock<CloudBlobContainer> MockBlobCsvContainer;
        public Mock<CloudBlobContainer> MockBlobErrorContainer;
        public Mock<CloudBlockBlob> MockCloudBlockJsonBlob;
        public Mock<CloudBlockBlob> MockCloudBlockCsvBlob;
        public Mock<CloudBlockBlob> MockCloudBlockErrorBlob;
        public ExecutionContext Context;
        public TestLogger Logger;
        public IProcessor ParallelProcessor;
        public IProcessor LinearProcessor;

        [SetUp]
        public void Setup()
        {
            Logger = new TestLogger("Test");

            MockBlobJsonContainer = new Mock<CloudBlobContainer>(new Uri("http://tempuri.org/blob"));
            MockBlobCsvContainer = new Mock<CloudBlobContainer>(new Uri("http://tempuri.org/blobout"));
            MockBlobErrorContainer = new Mock<CloudBlobContainer>(new Uri("http://tempuri.org/bloberror"));

            MockCloudBlockJsonBlob = new Mock<CloudBlockBlob>(new Uri("http://tempuri.org/blob"));
            MockCloudBlockCsvBlob = new Mock<CloudBlockBlob>(new Uri("http://tempuri.org/blobout"));
            MockCloudBlockErrorBlob = new Mock<CloudBlockBlob>(new Uri("http://tempuri.org/bloberror"));

            MockBlobJsonContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockJsonBlob.Object);

            MockBlobErrorContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockErrorBlob.Object);

            Context = new ExecutionContext();
            Context.FunctionAppDirectory = @"C:\Workspace\CsvToJson\bin\Debug\netcoreapp3.1";
            Context.FunctionDirectory = @"C:\Workspace\CsvToJson\bin\Debug\netcoreapp3.1\CsvToJson";
            Context.FunctionName = "CsvToJson";
            Context.InvocationId = Guid.NewGuid();

            ParallelProcessor = new ParallelProcessor(new OutputBlobWriter());

            LinearProcessor = new LinearProcessor(new OutputBlobWriter());
        }
    }
}