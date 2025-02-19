using System.Reflection;
using DataCollectionService;
using DataCollectionService.Contracts;
using DataCollectionService.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register IHttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IMessageHandleService, MessageHandleService>();
builder.Services.AddSingleton<IApiGatewayService, ApiGatewayService>();
builder.Services.AddSingleton<MongoDbContext>(); // Register MongoDbContext as a singleton
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
// Add Kafka consumer as a background service
builder.Services.AddHostedService<KafkaConsumerService>();
// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Метод для проверки клиента
app.MapPost("/api/datacollection", () =>
{
    // Типа ответ от Anti-Fraud system
    Console.WriteLine($"Stub method");

    // Возвращаем успешный ответ
    return Results.Ok("Swagger works!");
});

app.Run();
