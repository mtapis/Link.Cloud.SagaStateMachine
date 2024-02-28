using Link.Cloud.Core.Shared.Constants;
using MassTransit;
using TestServiceB.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure =>
{
    var hostName = builder.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.Hostname];
    var port = ushort.Parse(builder.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.Port]!);
    var virtualHost = builder.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.VirtualHost];
    var username = builder.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.Username];
    var password = builder.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.Password];

    configure.AddConsumer<TestBRequestEventConsumer>();
    configure.AddConsumer<TestBRollBackRequestMessageConsumer>();
    configure.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(hostName, port, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ReceiveEndpoint(EventQueueConsts.TestBRequestEventQueue, e =>
        {
            e.ConfigureConsumer<TestBRequestEventConsumer>(ctx);
        });
        cfg.ReceiveEndpoint(EventQueueConsts.TestBRollBackRequestMessageQueue, e =>
        {
            e.ConfigureConsumer<TestBRollBackRequestMessageConsumer>(ctx);
        });

    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
