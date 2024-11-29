using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using backend.Model;
using backend.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace backend.Services
{
    public class QuoteService : IQuoteRepo
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public QuoteService(IAmazonDynamoDB dynamoDbClient) { 
        
            _dynamoDbClient = Helper.CreateDynamoDBClient();
        }

        //get all quotes
        public async Task<List<AllQuotes>> GetAllQuotesAsync()
        {
            var quotes = new List<AllQuotes>();

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
                    DateSubmitted = item["DateSubmitted"].S,
                    FavouritedCount = item["FavouritedCount"].S,
                    Quote = item["Quote"].S,
                    QuoteBy = item["QuoteBy"].S,
                    SubmittedBy = item["SubmittedBy"].S,
                    Tags = item["Tags"].S
                };
                quotes.Add(quote);
            }
            return quotes;
        }

        //get quote by Id
        public async Task<QuoteDTO> GetQuoteById(string Qid)
        {
            try
            {
                var request = new GetItemRequest
                {
                    TableName = "AllQuotes",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "Qid", new AttributeValue { S = Qid } }
                    }
                };

                var response = await _dynamoDbClient.GetItemAsync(request);

                if (response.Item == null || response.Item.Count == 0)
                {
                    return null; 
                }

                return new QuoteDTO
                {
                    Qid = Qid,
                    Quote = response.Item["Quote"].S,
                    QuoteBy = response.Item["QuoteBy"].S,
                    Tags = response.Item["Tags"].S,
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        //add new quote
        public async Task<bool> AddQuoteAsync(string Uid, AddQuoteDTO quote)
        {
            if(Uid == null || quote == null) return false;

            //generate a unique id for the new quote
            string qid = Guid.NewGuid().ToString();

            Document doc = new Document();

            //prepare the item to be added
            doc["Qid"] = qid;
            doc["Quote"] = quote.Quote;
            doc["QuoteBy"] = quote.QuoteBy;
            doc["Tags"] = quote.Tags;
            doc["FavouritedCount"] = "0";
            doc["SubmittedBy"] = Uid;
            doc["DateSubmitted"] = DateTime.Now.ToString();
            doc["ApprovalStatus"] = "false";

            Table t = Table.LoadTable(_dynamoDbClient, "AllQuotes");
            Document response = await t.PutItemAsync(doc);

            return true;
        }



        //admin update part(s) of a quote in All Quotes
        public async Task<bool> PatchQuoteAsync(string Qid, Dictionary<string, string> updatedQuote)
        {
            var updated = new List<string>();
            var attributeValues = new Dictionary<string, AttributeValue>();

            foreach (var detail in updatedQuote)
            {
                updated.Add($"{detail.Key} = :{detail.Key}");
                attributeValues[($":{detail.Key}")] = new AttributeValue { S = detail.Value };
            }

            var request = new UpdateItemRequest
            {
                TableName = "AllQuotes",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Qid", new AttributeValue { S = Qid } }
                },
                UpdateExpression = "SET " + string.Join(", ", updated),
                ExpressionAttributeValues = attributeValues
            };

            var result = await _dynamoDbClient.UpdateItemAsync(request);

            return result.HttpStatusCode == HttpStatusCode.OK;
        }

        //admin update an entire quote
        public async Task<bool> UpdateQuoteAsync(string Qid, AllQuotes quoteUpdated)
        {
            var request = new UpdateItemRequest
            {
                TableName = "AllQuotes",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Qid", new AttributeValue { S = quoteUpdated.Qid } }
                },
                UpdateExpression = "SET Quote = :Quote, QuoteBy = :QuoteBy, Tags = :Tags, ApprovalStatus = :ApprovalStatus, DateSubmitted = :DateSubmitted, FavouritedCount = :FavouritedCount, SubmittedBy = :SubmittedBy",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":Quote", new AttributeValue { S = quoteUpdated.Quote } },
                    { ":QuoteBy", new AttributeValue { S = quoteUpdated.QuoteBy } },
                    { ":Tags", new AttributeValue { S = quoteUpdated.Tags } },
                    { ":ApprovalStatus", new AttributeValue { S = quoteUpdated.ApprovalStatus } },
                    { ":DateSubmitted", new AttributeValue { S = quoteUpdated.DateSubmitted } },
                    { ":SubmittedBy", new AttributeValue { S = quoteUpdated.SubmittedBy } },
                    { ":FavouritedCount", new AttributeValue { S = quoteUpdated.FavouritedCount } }
                }
            };
            var result = await _dynamoDbClient.UpdateItemAsync(request);
            return result.HttpStatusCode == HttpStatusCode.OK;
        }

        //delete quote
        public async Task<bool> DeleteQuoteByIdAsync(string Qid)
        {
            var allQuotesTable = Table.LoadTable(_dynamoDbClient, "AllQuotes");
            Document quote = await allQuotesTable.GetItemAsync(Qid);

            if (quote == null)
            {
                return false;
            }
            await allQuotesTable.DeleteItemAsync(quote);

            return true;
        }

        //admin function, might not keep
        //updates a quote in all users' list if changes were made to the quote in All Quotes
        public async Task<bool> UpdateQuoteInUserFavAsync(QuoteDTO quoteUpdated)
        {
            var scanRequest = new ScanRequest
            {
                TableName = "Users"
            };

            var scanResponse = await _dynamoDbClient.ScanAsync(scanRequest);

            foreach (var user in scanResponse.Items)
            {
                
                if (user.ContainsKey("Favourites"))
                {
                    var favoriteQuotesList = user["Favourites"].L;

                    var quoteToUpdate = favoriteQuotesList.FirstOrDefault(m => m.M["Qid"].N == quoteUpdated.Qid);

                    if (quoteToUpdate != null)
                    {
                        var updateRequest = new UpdateItemRequest
                        {
                            TableName = "Users",
                            Key = new Dictionary<string, AttributeValue>
                            {
                                { "Uid", new AttributeValue { S = user["Uid"].S } }
                            },
                            UpdateExpression = "SET Favourites = list_append(list_remove(Favourites, :quote), :quoteUpdated)",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                            {
                                { ":quote", quoteToUpdate },
                                { ":quoteUpdated", new AttributeValue { M = new Dictionary<string, AttributeValue>
                                    {
                                        { ":Quote", new AttributeValue { S = quoteUpdated.Quote } },
                                        { ":QuoteBy", new AttributeValue { S = quoteUpdated.QuoteBy } }
                                    }
                                }
                             }
                            }
                        };
                        var updateResult = await _dynamoDbClient.UpdateItemAsync(updateRequest);

                        if (updateResult.HttpStatusCode != HttpStatusCode.OK)
                        {
                            return false; 
                        }
                    }
                }
            }
            return true; 
        }
    }
}
