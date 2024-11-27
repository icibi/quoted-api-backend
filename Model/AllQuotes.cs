using Amazon.DynamoDBv2.DataModel;

namespace backend.Model
{
    [DynamoDBTable("AllQuotes")]
    public class AllQuotes
    {
        public string Qid { get; set; }
        public string ApprovalStatus { get; set; }
        public string DateSubmitted { get; set; }
        public string FavouritedCount { get; set; }
        public string Quote { get; set; }
        public string QuoteBy { get; set; }
        public string SubmittedBy { get; set; }
        public string Tags { get; set; }
    }
}