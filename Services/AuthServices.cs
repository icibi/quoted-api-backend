using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using backend.Model;
using backend.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace backend.Services
{
    public class AuthServices : IAuthRepo
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private const string uTable = "Users";
        private const string GSIUsername = "Username-index";
        private const string GSIEmail = "Email-index";

        public AuthServices()
        {
            _dynamoDbClient = Helper.CreateDynamoDBClient();
        }


        //validate user credentials
        public async Task<UserDTO> ValidateUserAsync(string username, string password)
        {
            ScanRequest request = new ScanRequest
            {
                TableName = "Users",
                FilterExpression = "Username = :u and Password = :p",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":u", new AttributeValue { S = username } },
                    { ":p", new AttributeValue { S = password } }
                }
            };

            ScanResponse response = await _dynamoDbClient.ScanAsync(request);

            if (response.Items.Count == 1)
            {
                var userDetails = response.Items[0];
                return new UserDTO
                {
                    Uid = userDetails["Uid"].S,
                    Username = userDetails["Username"].S,
                    UserRole = userDetails["UserRole"].S
                };
            }

            return null;
        }

        //signup
        public async Task<bool> SignupAsync (SignupDTO signupRequest)
        {
            //query req for username
            QueryRequest reqU = new QueryRequest
            {
                TableName = uTable,
                IndexName = GSIUsername,
                KeyConditionExpression = "Username = :username",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":username", new AttributeValue { S = signupRequest.Username } }
                }
            };

            //query req for email
            QueryRequest reqE = new QueryRequest
            {
                TableName = uTable,
                IndexName = GSIEmail,
                KeyConditionExpression = "Email = :email",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":email", new AttributeValue { S = signupRequest.Email } }
                }
            };

            //check if either username and email already exists
            QueryResponse checkExist = await _dynamoDbClient.QueryAsync(reqU);
            QueryResponse checkExistEmail = await _dynamoDbClient.QueryAsync(reqE);

            //if (checkExistEmail != null) { return false; }
            if (checkExist.Count == 0  && checkExistEmail.Count == 0) 
            {
                //generate a unique Uid for the new user
                string uid = Guid.NewGuid().ToString();
                string role = "user";
                Document doc = new Document();

                //prepare the item to be added
                doc["Uid"] = uid;
                doc["Email"] = signupRequest.Email;
                doc["Username"] = signupRequest.Username;
                doc["Password"] = signupRequest.Password;
                doc["Favourites"] = new List<Document>();
                doc["ErrorReports"] = new List<Document>();
                doc["UserRole"] = role;

                Table t = Table.LoadTable(_dynamoDbClient, "Users");
                await t.PutItemAsync(doc);

                return true;
            }
            return false;
        }

        //user update their email and/or password
        public async Task<bool> PatchUserAsync(string Uid, UserUpdateDTO updatedUser)
        {
            List<string> updated = new List<string>();
            Dictionary<string, AttributeValue> attributeValues = new Dictionary<string, AttributeValue>();

            UpdateItemRequest request = new UpdateItemRequest
            {
                TableName = "Users",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Uid", new AttributeValue { S = Uid } }
                },
                UpdateExpression = "SET Email = :email, Password = :password",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":email", new AttributeValue { S = updatedUser.Email }},
                    { ":password", new AttributeValue { S = updatedUser.Password }},
                }
            };

            UpdateItemResponse result = await _dynamoDbClient.UpdateItemAsync(request);

            return result.HttpStatusCode == HttpStatusCode.OK;
        }


        //admin function
        //admin update accounts
        public async Task<bool> AdminUpdateUserAsync(string Uid, User updatedUser)
        {
            List<string> updated = new List<string>();
            Dictionary<string, AttributeValue> attributeValues = new Dictionary<string, AttributeValue>();

            UpdateItemRequest request = new UpdateItemRequest
            {
                TableName = "Users",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Uid", new AttributeValue { S = Uid } }
                },
                UpdateExpression = "SET Email = :email, Username = :username, Password = :password, UserRole = :userRole",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":email", new AttributeValue { S = updatedUser.Email }},
                    { ":username", new AttributeValue { S = updatedUser.Username }},
                    { ":password", new AttributeValue { S = updatedUser.Password }},
                    { ":userRole", new AttributeValue { S = updatedUser.UserRole }},

                }
            };

            UpdateItemResponse result = await _dynamoDbClient.UpdateItemAsync(request);

            return result.HttpStatusCode == HttpStatusCode.OK;
        }

        
        //get user by id
        public async Task<UserUpdateDTO> GetUserById(string Uid)
        {
            GetItemRequest request = new GetItemRequest
            {
                TableName = "Users",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Uid", new AttributeValue { S = Uid } }
                }
            };

            GetItemResponse response = await _dynamoDbClient.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
            {
                return null;
            }

            return new UserUpdateDTO
            {
                Email = response.Item["Email"].S,
                Password = response.Item["Password"].S,
            };
            
        }

        //admin remove a user
        public async Task<bool> DeleteUserById(string Uid)
        {
            Table t = Table.LoadTable(_dynamoDbClient, uTable);
            Document user = await t.GetItemAsync(Uid);

            if (user == null)
            {
                return false;
            }

            await t.DeleteItemAsync(user);

            return true;

        }
    }
}
