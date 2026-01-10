using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




// Redis Service Registration
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = new ConfigurationOptions{
                EndPoints= {Environment.GetEnvironmentVariable("REDIS_ENDPOINT")},
                User= Environment.GetEnvironmentVariable("REDIS_USER"),
                Password= Environment.GetEnvironmentVariable("REDIS_PASSWORD"),
                Ssl = false,
                ConnectTimeout = 5000,
                SyncTimeout = 5000
            };
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});


builder.Services.AddSingleton<IRateLimiterService, RedisRateLimiter>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseMiddleware<RateLimitingMiddleware>();
app.MapControllers();
app.Run();

