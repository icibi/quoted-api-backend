using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace backend
{
    public class Helper
    {
        public static IAmazonDynamoDB CreateDynamoDBClient()
        {
            var accessKeyID = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var region = RegionEndpoint.USEast1; // Or retrieve from app configuration if needed

            if (string.IsNullOrEmpty(accessKeyID) || string.IsNullOrEmpty(secretKey))
            {
                // If environment variables aren't found, fallback to appsettings or default credentials
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                accessKeyID = config["AWS:AccessKeyId"];
                secretKey = config["AWS:SecretAccessKey"];
                region = RegionEndpoint.GetBySystemName(config["AWS:Region"]);
            }

            var credentials = new BasicAWSCredentials(accessKeyID, secretKey);
            return new AmazonDynamoDBClient(credentials, region);
        }

    }
}