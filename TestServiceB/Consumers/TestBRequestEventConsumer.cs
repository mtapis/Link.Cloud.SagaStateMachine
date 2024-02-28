using Link.Cloud.Core.Shared.DTOs;
using Link.Cloud.Core.Shared.Events;
using Link.Cloud.Core.Shared.Interfaces;
using MassTransit;
using System.Text.Json;

namespace TestServiceB.Consumers
{
    public class TestBRequestEventConsumer : IConsumer<ITestBRequestEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<TestBRequestEventConsumer> _logger;

        public TestBRequestEventConsumer(IPublishEndpoint publishEndpoint, ILogger<TestBRequestEventConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ITestBRequestEvent> context)
        {
            _logger.LogInformation($"Id ({context.Message.CorrelationId}) TestBRequestEvent Consume ediliyor .");
            int testNumber = 1;
            await Task.Delay(3000);


            if (testNumber >= 10)
            {
                var testBFailedEvent = new TestBFailedEvent(context.Message.CorrelationId)
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    Data = context.Message.PreviousData,
                    Message = "TestB request işlenirken hata meydana geldi.",
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Token = context.Message.Token
                };
                await _publishEndpoint.Publish(testBFailedEvent);
            }
            else
            {
                var testBSucceedEvent = new TestBSucceedEvent(context.Message.CorrelationId)
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    PreviousData = context.Message.PreviousData,
                    UpdatedData = context.Message.UpdatedData,
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Token = context.Message.Token
                };
                _logger.LogInformation($"Id ({context.Message.CorrelationId}) TestBSucceedEvent gönderiliyor..");
                await _publishEndpoint.Publish(testBSucceedEvent);
                
            }
        }
    }
}
