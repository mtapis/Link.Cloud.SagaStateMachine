using Link.Cloud.Core.Shared.Constants;
using Link.Cloud.Core.Shared.Events;
using Link.Cloud.Core.Shared.Interfaces;
using Link.Cloud.Core.Shared.Messages;
using MassTransit;
using MassTransit.Transports;

namespace TestServiceC.Consumers
{
    public class TestCRollBackRequestMessageConsumer : IConsumer<ITestCRollBackRequestMessage>
    {

        private readonly ILogger<TestCRollBackRequestMessageConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public TestCRollBackRequestMessageConsumer(ILogger<TestCRollBackRequestMessageConsumer> logger, ISendEndpointProvider sendEndpointProvider)
        {

            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<ITestCRollBackRequestMessage> context)
        {
            _logger.LogInformation($"Id ({context.Message.TenantId}) TestCRollBackRequestMessage Consume ediliyor .");
             await Task.Delay(5000);

            _logger.LogInformation($"Id ({context.Message.TenantId}) TestCRollBackRequestMessage Consume edildi .");
            //TestB nin rollback için dinleyeceği mesaj, sıralı servislerde birbirlerine farklı verilerl aktarılıyorsa sırayla geri gidilmeli rollback edilmeli C-B-A-Muhasebe

            var testBRollbackMessage = new TestBRollBackRequestMessage() 
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                Message = context.Message.Message,
                Data = context.Message.Data
            };
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{EventQueueConsts.TestBRollBackRequestMessageQueue}"));
            await sendEndpoint.Send<ITestBRollBackRequestMessage>(testBRollbackMessage);

        }
    }
}
