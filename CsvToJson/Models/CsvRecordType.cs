using CsvHelper.Configuration.Attributes;

namespace CsvToJson
{
    public class CsvRecordType
    {
        [Name("id")]
        public int Id { get; set; }
        [Name("name")]
        public string Name { get; set; }
    }
}