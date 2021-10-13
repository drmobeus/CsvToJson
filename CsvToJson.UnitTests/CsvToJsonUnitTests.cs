using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;


namespace CsvToJson.UnitTests
{
    [TestFixture]
    public class CsvToJsonUnitTests:BaseTestSetups
    {
        [Test]
        public async Task WhenAValidCsvFileIsAddedToInputContainer_ThenTheExpectedJsonFileAppearsInOutputContainer()
        {
            string csvFile = "Id,Name\r\n1,Fey";

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(ParallelProcessor).Run(s, "test.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);


            var expectedOutput = "{\"Id\":1,\"Name\":\"Fey\"}";
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput), Times.Once);
        }

        [Test]
        public async Task WhenAValidCsvFileIsAddedToInputContainer_ThenItIsRemovedFromTheCsvContainer()
        {
            string csvFile = "Id,Name\r\n1,Fey";

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(ParallelProcessor).Run(s, "test.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            MockCloudBlockCsvBlob.Verify(d => d.DeleteAsync(), Times.Once);
        }

        [Test]
        public async Task WhenANotCsvFileIsAddedToInputContainer_ThenItIsMovedToTheErrorContainer()
        {
            string invalidFileName = "piccy.png";

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();
            await new CsvToJsonFunction(ParallelProcessor).Run(s, invalidFileName, Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(It.IsAny<string>()), Times.Never);

            MockCloudBlockErrorBlob.Verify(x => x.StartCopyAsync(MockCloudBlockCsvBlob.Object), Times.Once);
        }

        [Test]
        public async Task WhenANotCsvFileIsAddedToInputContainer_ThenItIsRemovedFromTheCsvContainer()
        {
            string invalidFileName = "piccy.png";

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();
            await new CsvToJsonFunction(ParallelProcessor).Run(s, invalidFileName, Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            MockCloudBlockCsvBlob.Verify(d => d.DeleteAsync(), Times.Once);
        }

        [Test]
        public async Task WhenAnInvalidCsvFileIsAddedToInputContainer_ThenItIsMovedToTheErrorContainer()
        {
            string csvFile = "I am not csv data";

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(ParallelProcessor).Run(s, "test.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);


            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(It.IsAny<string>()), Times.Never);

            MockCloudBlockErrorBlob.Verify(x => x.StartCopyAsync(MockCloudBlockCsvBlob.Object), Times.Once);
        }

        [Test]
        public async Task WhenAnInvalidCsvFileIsAddedToInputContainer_ThenItIsRemovedFromTheCsvContainer()
        {
            string csvFile = "I am not csv data";

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(ParallelProcessor).Run(s, "test.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            MockCloudBlockCsvBlob.Verify(d => d.DeleteAsync(), Times.Once);
        }

        [Test]
        public async Task WhenMultipleValidCsvFileIsAddedToInputContainer_ThenTheExpectedJsonFilesAppearsInOutputContainer()
        {
            string csvFileA = "Id,Name\r\n1,Fey";
            string csvFileB = "Id,Name\r\n2,Hobbes";
            string csvFileC = "Id,Name\r\n3,Martha";

            MockCloudBlockCsvBlob.SetupSequence(x => x.DownloadTextAsync()).ReturnsAsync(csvFileA)
                .ReturnsAsync(csvFileB)
                .ReturnsAsync(csvFileC);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            //Assume triggered three times
            await new CsvToJsonFunction(ParallelProcessor).Run(s, "testA.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);
            await new CsvToJsonFunction(ParallelProcessor).Run(s, "testB.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);
            await new CsvToJsonFunction(ParallelProcessor).Run(s, "testC.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("testA1.json"), Times.Once);
            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("testB1.json"), Times.Once);
            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("testC1.json"), Times.Once);


            var expectedOutput1 = "{\"Id\":1,\"Name\":\"Fey\"}";
            var expectedOutput2 = "{\"Id\":2,\"Name\":\"Hobbes\"}";
            var expectedOutput3 = "{\"Id\":3,\"Name\":\"Martha\"}";
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput1), Times.Once);
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput2), Times.Once);
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput3), Times.Once);
        }

        [Test]
        public async Task WhenMultipleValidAndOneInvalidCsvFileIsAddedToInputContainer_ThenOneFileAppearsInTheErrorContainer()
        {
            //TODO: Break this down - too much going on here!!
            //TODO: this and previous - make clearer what's going on.

            string csvFileA = "Id,Name\r\n1,Fey";
            string invalidCsvFileB = "Not Valid CSV,Hobbes";
            string csvFileC = "Id,Name\r\n3,Martha";

            MockCloudBlockCsvBlob.SetupSequence(x => x.DownloadTextAsync()).ReturnsAsync(csvFileA)
                .ReturnsAsync(invalidCsvFileB)
                .ReturnsAsync(csvFileC);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            //Assume triggered three times
            await new CsvToJsonFunction(ParallelProcessor).Run(s, "testA.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);
            await new CsvToJsonFunction(ParallelProcessor).Run(s, "invalidTestB.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);
            await new CsvToJsonFunction(ParallelProcessor).Run(s, "testC.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            MockBlobErrorContainer.Verify(_ => _.GetBlockBlobReference("invalidTestB.csv"), Times.Once);
            MockCloudBlockErrorBlob.Verify(x => x.UploadTextAsync(invalidCsvFileB), Times.Never);

            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("testA1.json"), Times.Once);
            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("invalidTestB.json"), Times.Never);
            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("testC1.json"), Times.Once);


            var expectedOutput1 = "{\"Id\":1,\"Name\":\"Fey\"}";
            var expectedOutput2 = "{\"Id\":2,\"Name\":\"Hobbes\"}";
            var expectedOutput3 = "{\"Id\":3,\"Name\":\"Martha\"}";
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput1), Times.Once);
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput2), Times.Never);
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput3), Times.Once);
        }

        [Ignore("Enable this if we decide that putting the same file through twice does not do an Upsert")]
        [Test]
        public async Task WhenMultipleValidCsvFilesWithTheSameNameAreAddedToInputContainer_ThenTheExpectedJsonFileIsWrttenOnceOnlyAndNotOverwritten()
        {
            string csvFileA = "Id,Name\r\n1,Fey";
            string csvFileB = "Id,Name\r\n1,Hobbes";

            MockCloudBlockCsvBlob.SetupSequence(x => x.DownloadTextAsync()).ReturnsAsync(csvFileA)
                .ReturnsAsync(csvFileB);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            //Assume triggered three times
            await new CsvToJsonFunction(ParallelProcessor).Run(s, "testA.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);
            await new CsvToJsonFunction(ParallelProcessor).Run(s, "testA.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("testA1.json"), Times.Exactly(1));

            var expectedOutput = "{\"Id\":1,\"Name\":\"Fey\"}";
            var unexpectedOutput = "{\"Id\":2,\"Name\":\"Hobbes\"}";

            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput), Times.Once);
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(unexpectedOutput), Times.Never);
        }

        [Test]
        public async Task WhenAValidCsvFileWithMultipleRowsAddedToInputContainer_ThenTheExpectedJsonFilesAppearsInOutputContainer()
        {
            string csvFile = "Id,Name\r\n1,Fey\r\n2,Hobbes\r\n3,Martha";

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(ParallelProcessor).Run(s, "test.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("test1.json"), Times.Once);
            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("test2.json"), Times.Once);
            MockBlobJsonContainer.Verify(_ => _.GetBlockBlobReference("test3.json"), Times.Once);

            var expectedOutput1 = "{\"Id\":1,\"Name\":\"Fey\"}";
            var expectedOutput2 = "{\"Id\":2,\"Name\":\"Hobbes\"}";
            var expectedOutput3 = "{\"Id\":3,\"Name\":\"Martha\"}";
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput1), Times.Once);
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput2), Times.Once);
            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(expectedOutput3), Times.Once);
        }
    }
}
