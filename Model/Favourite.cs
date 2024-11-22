using Amazon.DynamoDBv2.DataModel;

namespace backend.Model
{
    [DynamoDBTable("Users")]
    public class Favourite
    {
        public string Fid { get; set; }
        public string Qid { get; set; }
        public string QuoteBy { get; set; }
        public string QuoteCategories { get; set; }
        public string QuoteContents { get; set; }
        public string QuoteText { get; set; }
    }
}

