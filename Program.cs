using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using backend.Repository;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Register AWS services
builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    // Create and configure the DynamoDB client
    var config = builder.Configuration;
    var client = new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1);
    return client;
});

// Register IDynamoDBContext
builder.Services.AddSingleton<IDynamoDBContext>(sp =>
{
    var dynamoDbClient = sp.GetRequiredService<IAmazonDynamoDB>();
    return new DynamoDBContext(dynamoDbClient); // Use DynamoDBContext implementation
});

//register the repositories
builder.Services.AddSingleton<IQuoteRepo, QuoteService>();
builder.Services.AddSingleton<IAuthRepo, AuthServices>();
builder.Services.AddSingleton<IFavRepo, FavouriteService>();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Allow requests from React app
              .AllowAnyMethod() // Allow all HTTP methods (GET, POST, etc.)
              .AllowAnyHeader(); // Allow all headers
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<FavouriteService>();
builder.Services.AddScoped<AuthServices>();
builder.Services.AddControllers();



var app = builder.Build();

// Enable Swagger (optional, for API testing)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.UseDeveloperExceptionPage();

app.MapControllers();

app.Run();
