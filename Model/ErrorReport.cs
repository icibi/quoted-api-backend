using Amazon.DynamoDBv2.DataModel;

namespace backend.Model
{
    [DynamoDBTable("Users")]
    public class ErrorReport
    {
        public string ERid { get; set; }
        public string Error { get; set; }
        public string ErrorType { get; set; }
        public string QuoteId { get; set; }
    }
}
