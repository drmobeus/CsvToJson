using System.IO;
using System.Threading.Tasks;
using CsvToJson.UnitTests.Helpers;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CsvToJson.UnitTests
{
    [TestFixture]
    public class ProcessorComparisonTests:BaseTestSetups
    {
        [Test]
        public async Task WhenAValid1000RowCsvFileIsAddedToInputContainer_AndTheParallelProcessorIsUsed_ThenTheExpected1000JsonFilesAppearsInOutputContainer()
        {
            string csvFile = File.ReadAllText(FileHelper.GetFilePathAndName("ThousandRows.csv"));

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(ParallelProcessor).Run(s, "ThousandRows.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);


            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(It.IsAny<string>()), Times.Exactly(1000));

            Logger.GetLogMessages().Count.Should().Be(1003);

            Logger.AllEntriesAreUnique().Should().BeTrue();
        }

        [Test]
        public async Task WhenAValid1000RowCsvFileIsAddedToInputContainer_AndTheLinearProcessorIsUsed_ThenTheExpected1000JsonFilesAppearsInOutputContainer()
        {
            string csvFile = File.ReadAllText(FileHelper.GetFilePathAndName("ThousandRows.csv"));

            MockCloudBlockCsvBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(csvFile);

            MockBlobCsvContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(MockCloudBlockCsvBlob.Object);

            Stream s = new MemoryStream();

            await new CsvToJsonFunction(LinearProcessor).Run(s, "ThousandRows.csv", Logger, Context, MockBlobJsonContainer.Object, MockBlobCsvContainer.Object, MockBlobErrorContainer.Object);


            MockCloudBlockJsonBlob.Verify(x => x.UploadTextAsync(It.IsAny<string>()), Times.Exactly(1000));

            Logger.GetLogMessages().Count.Should().Be(1003);

            Logger.AllEntriesAreUnique().Should().BeTrue();
        }
    }
}