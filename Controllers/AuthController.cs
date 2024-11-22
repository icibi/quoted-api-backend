using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using backend.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public AuthController(IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient; 
        }

        //login function
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var isValidUser = await ValidateUserAsync(loginRequest.Username, loginRequest.Password);

            if (isValidUser)
            {
                return Ok(new { Message = "Login successful" });
            }
            else
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }
        }

        //validate user credentials
        private async Task<bool> ValidateUserAsync(string username, string password)
        {
            try
            {
                var request = new ScanRequest
                {
                    TableName = "Users",
                    FilterExpression = "Username = :u and Password = :p",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":u", new AttributeValue { S = username } },
                        { ":p", new AttributeValue { S = password } }
                    }
                };

                var response = await _dynamoDbClient.ScanAsync(request);

                //check if user exists
                return response.Items.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return false;
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User signupRequest)
        {
            try
            {
                //validate inputs
                if (string.IsNullOrWhiteSpace(signupRequest.Email) ||
                    string.IsNullOrWhiteSpace(signupRequest.Username) ||
                    string.IsNullOrWhiteSpace(signupRequest.Password))
                {
                    return BadRequest(new { Message = "All fields are required." });
                }

                //generate a unique Uid for the new user
                var uid = Guid.NewGuid().ToString();

                //prepare the item to be added
                var item = new Dictionary<string, AttributeValue>
        {
            { "Uid", new AttributeValue { S = uid } },
            { "Email", new AttributeValue { S = signupRequest.Email } },
            { "Username", new AttributeValue { S = signupRequest.Username } },
            { "Password", new AttributeValue { S = signupRequest.Password } }
        };

                //add the item to the DynamoDB table
                var request = new PutItemRequest
                {
                    TableName = "Users",
                    Item = item
                };

                var response = await _dynamoDbClient.PutItemAsync(request);

                //confirm successful signup
                return Ok(new { Message = "Signup successful", UserId = uid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred while signing up: {ex.Message}" });
            }
        }


    }

    //request body for login
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
