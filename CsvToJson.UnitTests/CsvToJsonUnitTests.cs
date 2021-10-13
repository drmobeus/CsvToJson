using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
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

            Logger.GetLogMessages().Count.Should().Be(4);
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

            Logger.GetLogMessages().Count.Should().Be(2);
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

            Logger.GetLogMessages().Count.Should().Be(2);
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
    }
}
