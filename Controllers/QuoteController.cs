using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly IConfiguration _config;

        public QuotesController(IAmazonDynamoDB dynamoDbClient, IConfiguration config)
        {
            _dynamoDbClient = Helper.CreateDynamoDBClient();
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuotes()
        {
            var quotes = new List<AllQuotes>();

            try
            {
                var request = new ScanRequest
                {
                    TableName = "AllQuotes"
                };

                ScanResponse response = await _dynamoDbClient.ScanAsync(request);

                foreach (var item in response.Items)
                {
                    var quote = new AllQuotes
                    {
                        Qid = item["Qid"].S,
                        ApprovalStatus = item["ApprovalStatus"].S,
                        Categories = item["Categories"].S,
                        DateSubmitted = item["DateSubmitted"].S,
                        FavouritedCount = item["FavouritedCount"].S,
                        Quote = item["Quote"].S,
                        QuoteBy = item["QuoteBy"].S,
                        SubmittedBy = item["SubmittedBy"].S,
                        Tags = item["Tags"].S
                    };

                    quotes.Add(quote);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (quotes == null)
            {
                return NotFound();
            }
            return Ok(quotes);
        }
    }
}
