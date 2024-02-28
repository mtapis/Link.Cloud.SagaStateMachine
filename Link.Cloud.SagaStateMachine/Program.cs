using Link.Cloud.Core.Shared.Constants;
using Link.Cloud.SagaStateMachine;
using Link.Cloud.SagaStateMachine.Constants;
using Link.Cloud.SagaStateMachine.Models;
using Link.Cloud.SagaStateMachine.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
    {
        services.AddMassTransit(cfg =>
        {

            cfg.AddSagaStateMachine<EventStateMachine, EventStateInstance>().EntityFrameworkRepository(opt =>
            {
                opt.AddDbContext<DbContext, EventStateDbContext>((provider, builder) =>
                {
                    builder.UseSqlServer(hostContext.Configuration.GetConnectionString(SagaStateMachineAppSettingsConnStringsConst.SqlConnection), m =>
                    {
                        m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    });
                });
            });

            var hostName = hostContext.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.Hostname];
            var port = ushort.Parse(hostContext.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.Port]);
            var virtualHost = hostContext.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.VirtualHost];
            var username = hostContext.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.Username];
            var password = hostContext.Configuration.GetSection(AppSettingsRabbitMqConst.Name)[AppSettingsRabbitMqConst.Password];

            cfg.UsingRabbitMq((context, cnfg) =>
            {
                cnfg.Host(hostName, port, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cnfg.ReceiveEndpoint(EventQueueConsts.SirketUpdatedEventQueue, e =>
                {
                    e.ConfigureSaga<EventStateInstance>(context);
                });

                cnfg.ReceiveEndpoint(EventQueueConsts.IsletmeUpdatedEventQueue, e =>
                {
                    e.ConfigureSaga<EventStateInstance>(context);
                });
            });
        });

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
