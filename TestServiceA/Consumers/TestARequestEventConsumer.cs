using Link.Cloud.Core.Shared.DTOs;
using Link.Cloud.Core.Shared.Events;
using Link.Cloud.Core.Shared.Interfaces;
using MassTransit;
using System.Text.Json;

namespace TestServiceA.Consumers
{
    public class TestARequestEventConsumer : IConsumer<TestARequestEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<TestARequestEventConsumer> _logger;

        public TestARequestEventConsumer(IPublishEndpoint publishEndpoint, ILogger<TestARequestEventConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TestARequestEvent> context)
        {
            _logger.LogInformation($"Id ({context.Message.CorrelationId}) TestARequestEvent Consume ediliyor .");
            int testNumber = 1;
            await Task.Delay(3000);


            if (testNumber >= 10)
            {
                var testAFailedEvent = new TestAFailedEvent(context.Message.CorrelationId)
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    Data = context.Message.PreviousData,
                    Message = "TestA request işlenirken hata meydana geldi.",
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Token = context.Message.Token

                };
                await _publishEndpoint.Publish(testAFailedEvent);
                _logger.LogInformation($"Id ({context.Message.CorrelationId}) TestAFailed event gönderiliyor .");
            }
            else
            {
                var testASucceedEvent = new TestASucceedEvent(context.Message.CorrelationId)
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    PreviousData = context.Message.PreviousData,
                    UpdatedData = context.Message.UpdatedData,
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Token = context.Message.Token
                };
                _logger.LogInformation($"Id ({context.Message.CorrelationId}) TestASucceed event gönderiliyor .");
                await _publishEndpoint.Publish(testASucceedEvent);
            }
        }
    }
}
