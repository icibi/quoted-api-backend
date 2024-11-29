using Amazon.DynamoDBv2.DataModel;

namespace backend.Model
{
    [DynamoDBTable("Users")]
    public class User
    {
        public string Uid { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<ErrorReport> ErrorReports { get; set; } = new List<ErrorReport>();
        public List<Favourite> Favourites { get; set; } = new List<Favourite>();
        public string UserRole { get; set; }
    }
}
