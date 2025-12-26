var builder = WebApplication.CreateBuilder(args);

//Add All the required services here
builder.Services.AddSingleton<ITokenBucketStore, InMemoryTokenBucketStore>();
builder.Services.AddSingleton<IRateLimitPolicyResolver,
    StaticRateLimitPolicyResolver>();
builder.Services.AddSingleton<IRateLimitIdentityResolver,
    CompositeIdentityResolver>();
// builder.Services.AddSingleton<IRateLimiter>(sp =>
//     new TokenBucketRateLimiter(
//         sp.GetRequiredService<ITokenBucketStore>(),
//         capacity: 5, //reduce the latency for testing
//         windowTimeSeconds: 60
//     ));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RateLimitingMiddleware>();
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
