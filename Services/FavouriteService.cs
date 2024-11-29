using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal;
using backend.Model;
using backend.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace backend.Services
{
    public class FavouriteService : IFavRepo
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly Table _table;
        private const string GSIFav = "Favourites-index";

        public FavouriteService()
        {
            _dynamoDbClient = Helper.CreateDynamoDBClient();
            _table = Table.LoadTable(_dynamoDbClient, "Users");
        }

        //get user's favourites by their id
        public async Task<List<FavouriteDTO>> GetUsersFavByUserId(string Uid, string tag)
        {
            List<Document> filteredFavs;
            Document user = await _table.GetItemAsync(Uid);

            if (user == null)
            {
                return null;
            }

            var favourites = user["Favourites"].AsListOfDocument();
            List<FavouriteDTO> favs = new List<FavouriteDTO>();

            if (favourites == null)
            {
                return favs;
            }

            if (string.IsNullOrEmpty(tag))
            {
                foreach (Document favDoc in favourites)
                {
                    FavouriteDTO fav = new FavouriteDTO
                    {
                        Fid = favDoc["Fid"].AsString(),
                        QuoteBy = favDoc["QuoteBy"].AsString(),
                        QuoteContents = favDoc["QuoteContents"].AsString(),
                        Tags = favDoc["Tags"].AsString(),
                    };
                    favs.Add(fav);
                }
                return favs;
            }
            else
            {
                filteredFavs = favourites.Where(m => 
                { 
                    List<string> t = m["Tags"].AsString().Split(',').ToList();
                    return t.Any(a => a.Trim().Equals(tag, StringComparison.OrdinalIgnoreCase));
                }).ToList();

                List<FavouriteDTO> filteredResult = new List<FavouriteDTO>();

                foreach (Document favDoc in filteredFavs)
                {
                    FavouriteDTO fav = new FavouriteDTO
                    {
                        Fid = favDoc["Fid"].AsString(),
                        QuoteBy = favDoc["QuoteBy"].AsString(),
                        QuoteContents = favDoc["QuoteContents"].AsString(),
                        Tags = favDoc["Tags"].AsString(),
                    };
                    filteredResult.Add(fav);
                }
                return filteredResult;
            }
        }


        //get quote by Id
        public async Task<FavouriteDTO> GetFavQuoteById(string Uid, string Fid)
        {
            Document user = await _table.GetItemAsync(Uid);

            if (user == null)
            {
                return null;
            }

            if(!user.ContainsKey("Favourites"))
            {
                return null;
            }

            List<Document> favourites = user["Favourites"].AsListOfDocument();
            List<FavouriteDTO> favs = new List<FavouriteDTO>();

            if (favourites == null)
            {
                return null;
            }
            
            
            foreach(Document favDoc in favourites)
            {
                FavouriteDTO fav = new FavouriteDTO
                {
                    Fid = favDoc["Fid"].AsString(),
                    QuoteBy = favDoc["QuoteBy"].AsString(),
                    QuoteContents = favDoc["QuoteContents"].AsString(),
                    Tags = favDoc["Tags"].AsString(),
                };
                favs.Add(fav);
            }

            var quote = favs.FirstOrDefault(f => f.Fid == Fid);

            return quote;
            
        }


        //add quote to favourites list
        public async Task<bool> AddQuoteToFavAsync(string Uid, QuoteDTO quote)
        {
            string fid = Guid.NewGuid().ToString();

            Dictionary<string, AttributeValue> quoteDetails = new Dictionary<string, AttributeValue>
            {
                {"Fid", new AttributeValue { S = fid } },
                {"Qid", new AttributeValue { S = quote.Qid } },
                {"QuoteContents", new AttributeValue { S = quote.Quote } },
                {"QuoteBy", new AttributeValue { S = quote.QuoteBy } },
                {"Tags", new AttributeValue { S = quote.Tags } },
            };

            List<AttributeValue> quoteList = new List<AttributeValue>
            {
                new AttributeValue { M = quoteDetails}
            };

            UpdateItemRequest request = new UpdateItemRequest
            {
                TableName = "Users",
                Key = new Dictionary<string, AttributeValue>
                {
                    {"Uid", new AttributeValue {S = Uid} },
                },
                UpdateExpression = "SET Favourites = list_append(Favourites, :quote)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":quote", new AttributeValue { L = quoteList } }
                }

            };

            UpdateItemResponse result = await _dynamoDbClient.UpdateItemAsync(request);
            return result.HttpStatusCode == HttpStatusCode.OK;
        }


        //remove a quote from user's favourites list
        public async Task<bool> DeleteQuoteById(string Uid, string Fid)
        {
            Document user = await _table.GetItemAsync(Uid);

            if(user == null || !user.Contains("Favourites"))
            {
                return false;
            }

            var favourites = user["Favourites"].AsListOfDocument();
            
            var quoteToRemove = favourites.FirstOrDefault(m => m["Fid"].AsString() == Fid);
            
            if(quoteToRemove == null)
            {
                return false;
            }

            favourites.Remove(quoteToRemove);

            user["Favourites"] = favourites;

            await _table.UpdateItemAsync(user);

            return true;

        }

        //update favourite quote tags
        public async Task<bool> UpdateQuoteTagsById(string Uid, string Fid, string tags)
        {
            var req = new GetItemRequest
            {
                TableName = "Users",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Uid", new AttributeValue { S = Uid } }
                }
            };

            var response = await _dynamoDbClient.GetItemAsync(req);

            if (response.Item == null || !response.Item.ContainsKey("Favourites"))
            {
                return false;
            }

            var user = new User
            {
                Uid = response.Item["Uid"].S,
                Favourites = new List<Favourite>(),
            };

            if (response.Item.ContainsKey("Favourites"))
            {
                foreach (var q in response.Item["Favourites"].L)
                {
                    user.Favourites.Add(new Favourite
                    {
                        Fid = q.M["Fid"].S,
                        Qid = q.M["Qid"].S,
                        QuoteBy = q.M["QuoteBy"].S,
                        QuoteContents = q.M["QuoteContents"].S,
                        Tags = q.M["Tags"].S
                    });
                }
            }

            var qToUpdate = user.Favourites.FirstOrDefault(m => m.Fid == Fid);

            if (qToUpdate != null)
            {
                qToUpdate.Tags = tags;
            }

            var request = new UpdateItemRequest
            {
                TableName = "Users",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Uid", new AttributeValue { S = user.Uid } }
                },
                UpdateExpression = "SET Favourites = :favourites",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":favourites", new AttributeValue
                        {
                            L = user.Favourites.Select(m => new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    { "Fid", new AttributeValue { S = m.Fid } },
                                    { "Qid", new AttributeValue { S = m.Qid } },
                                    { "QuoteBy", new AttributeValue { S = m.QuoteBy } },
                                    { "QuoteContents", new AttributeValue { S = m.QuoteContents } },
                                    { "Tags", new AttributeValue { S = m.Tags } }
                                }
                            }).ToList()
                        }
                    }
                }
            };

            UpdateItemResponse result = await _dynamoDbClient.UpdateItemAsync(request);
            return result.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
