using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public AuthController()
        {
            _dynamoDbClient = Helper.CreateDynamoDBClient();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Invalid login request.");
            }

            try
            {
                var request = new ScanRequest
                {
                    TableName = "Users",
                    FilterExpression = "Username = :username AND Password = :password",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":username", new AttributeValue { S = loginRequest.Username } },
                        { ":password", new AttributeValue { S = loginRequest.Password } }
                    }
                };

                var response = await _dynamoDbClient.ScanAsync(request);

                if (response.Items.Count > 0)
                {
                    return Ok(new { Message = "Login successful.", UserDetails = response.Items });
                }

                return Unauthorized(new { Message = "Invalid Username or Password." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest signupRequest)
        {
            if (signupRequest == null ||
                string.IsNullOrEmpty(signupRequest.Email) ||
                string.IsNullOrEmpty(signupRequest.Username) ||
                string.IsNullOrEmpty(signupRequest.Password))
            {
                return BadRequest("Invalid signup request.");
            }

            try
            {
                // Generate a unique identifier for the new user
                string uid = Guid.NewGuid().ToString();

                // Define user attributes, explicitly initializing empty lists
                var item = new Dictionary<string, AttributeValue>
        {
            { "Uid", new AttributeValue { S = uid } },
            { "Email", new AttributeValue { S = signupRequest.Email } },
            { "Username", new AttributeValue { S = signupRequest.Username } },
            { "Password", new AttributeValue { S = signupRequest.Password } },
            { "ErrorReports", new AttributeValue { L = new List<AttributeValue> { new AttributeValue { NULL = true } } } },
            { "Favourites", new AttributeValue { L = new List<AttributeValue> { new AttributeValue { NULL = true } } } }
        };

                var request = new PutItemRequest
                {
                    TableName = "Users",
                    Item = item
                };

                // Add the new user to the table
                await _dynamoDbClient.PutItemAsync(request);

                return Ok(new { Message = "Signup successful.", Uid = uid });
            }
            catch (AmazonDynamoDBException ex)
            {
                return StatusCode(500, new { Message = "DynamoDB error occurred.", Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }



    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignupRequest
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}