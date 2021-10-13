using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CsvToJson.UnitTests
{
    [TestFixture]
    public class LoggingTests: BaseTestSetups
    {
        [Test]
        public async Task WhenAValidCsvFileIsAddedToInputContainer_ThenTheExpectedLoggingMessagesOccur()
        {
            string csvFile = "Id,Name\r\n1,Fey";

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(ParallelProcessor).Run(s, "test.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            Logger.GetLogMessages()[0].FormattedMessage.Should().Be("CSV to JSON Blob trigger function Processing blob\n Name:test.csv");
            Logger.GetLogMessages()[1].FormattedMessage.Should().Be("Blob written successfully\n From:test.csv To:test1.json");
            Logger.GetLogMessages()[2].FormattedMessage.Should().Be("All records in test.csv have now been processed");
            Logger.GetLogMessages()[3].FormattedMessage.Should().Be("Blob deleted successfully from CSV container\n Name:test.csv");
            Logger.GetLogMessages().Count.Should().Be(4);
        }

        [Test]
        public async Task WhenANotCsvFileIsAddedToInputContainer_ThenTheExpectedLoggingMessagesOccur()
        {
            string invalidFileName = "piccy.png";

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();
            await new CsvToJsonFunction(ParallelProcessor).Run(s, invalidFileName, Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            Logger.GetLogMessages()[0].FormattedMessage.Should().Be("CSV to JSON Blob trigger function Processing blob\n Name:piccy.png");
            Logger.GetLogMessages()[1].FormattedMessage.Should().Be("Blob does not have '.csv' suffix, so it was moved. \n Name:piccy.png to bloberror");
            Logger.GetLogMessages().Count.Should().Be(2);
        }

        [Test]
        public async Task WhenACsvFileWithInvalidContentIsAddedToInputContainer_ThenTheExpectedLoggingMessagesOccur()
        {
            string csvFile = "I am not csv data";

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(ParallelProcessor).Run(s, "test.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);

            Logger.GetLogMessages()[0].FormattedMessage.Should().Be("CSV to JSON Blob trigger function Processing blob\n Name:test.csv");
            Logger.GetLogMessages()[1].FormattedMessage.Should().Be("Cannot process Blob, so it was moved. \n Name:test.csv to bloberror");
            Logger.GetLogMessages().Count.Should().Be(2);
        }
    }
}