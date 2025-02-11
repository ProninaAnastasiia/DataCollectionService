using DataCollectionService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register IHttpClientFactory
builder.Services.AddHttpClient();

// Add Kafka consumer as a background service
builder.Services.AddHostedService<KafkaConsumerService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
