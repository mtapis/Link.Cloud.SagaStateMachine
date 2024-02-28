using Link.Cloud.Core.Shared.DTOs;
using Link.Cloud.Core.Shared.Events;
using Link.Cloud.Core.Shared.Interfaces;
using MassTransit;
using System.Text.Json;

namespace TestServiceC.Consumers
{
    public class TestCRequestEventConsumer : IConsumer<ITestCRequestEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<TestCRequestEventConsumer> _logger;

        public TestCRequestEventConsumer(IPublishEndpoint publishEndpoint, ILogger<TestCRequestEventConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ITestCRequestEvent> context)
        {
            _logger.LogInformation($"Id ({context.Message.CorrelationId}) TestCRequestEvent Consume ediliyor .");
            int testNumber = 1;
            await Task.Delay(3000);


            if (testNumber >= 10)
            {
                var testCFailedEvent = new TestCFailedEvent(context.Message.CorrelationId)
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    Data = context.Message.PreviousData,
                    Message = "TestC request işlenirken hata meydana geldi.",
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Token = context.Message.Token
                };
                await _publishEndpoint.Publish(testCFailedEvent);
            }
            else
            {
                var testCSucceedEvent = new TestCSucceedEvent(context.Message.CorrelationId)
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    PreviousData = context.Message.PreviousData,
                    UpdatedData = context.Message.UpdatedData,
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Token = context.Message.Token
                };
                await _publishEndpoint.Publish(testCSucceedEvent);
                _logger.LogInformation($"Id ({context.Message.CorrelationId}) TestCSucceedEvent gönderiliyor.");
            }
        }
    }
}
