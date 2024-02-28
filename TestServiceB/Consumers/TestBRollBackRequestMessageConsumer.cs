using Link.Cloud.Core.Shared.Constants;
using Link.Cloud.Core.Shared.Events;
using Link.Cloud.Core.Shared.Interfaces;
using Link.Cloud.Core.Shared.Messages;
using MassTransit;
using MassTransit.Transports;

namespace TestServiceB.Consumers
{
    public class TestBRollBackRequestMessageConsumer : IConsumer<ITestBRollBackRequestMessage>
    {

        private readonly ILogger<TestBRollBackRequestMessageConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public TestBRollBackRequestMessageConsumer(ILogger<TestBRollBackRequestMessageConsumer> logger, ISendEndpointProvider sendEndpointProvider)
        {

            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<ITestBRollBackRequestMessage> context)
        {
            _logger.LogInformation($"Id ({context.Message.TenantId}) TestBRollBackRequestMessage Consume ediliyor .");
             await Task.Delay(5000);

            _logger.LogInformation($"Id ({context.Message.TenantId}) TestBRollBackRequestMessage Consume edildi .");
            //TestA nin rollback için dinleyeceği mesaj, sıralı servislerde birbirlerine farklı verilerl aktarılıyorsa sırayla geri gidilmeli rollback edilmeli C-B-A-Sirket

            var testARollbackMessage = new TestARollBackRequestMessage() 
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                Message = context.Message.Message,
                Data = context.Message.Data
            };
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{EventQueueConsts.TestARollBackRequestMessageQueue}"));
            await sendEndpoint.Send<ITestARollBackRequestMessage>(testARollbackMessage);

        }
    }
}
