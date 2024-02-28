using Link.Cloud.Core.Shared.Constants;
using Link.Cloud.Core.Shared.Interfaces;
using Link.Cloud.Core.Shared.Messages;
using MassTransit;
using MassTransit.Transports;

namespace TestServiceA.Consumers
{
    public class TestARollBackRequestMessageConsumer:IConsumer<ITestARollBackRequestMessage>
    {
        private readonly ILogger<TestARollBackRequestMessageConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public TestARollBackRequestMessageConsumer(ILogger<TestARollBackRequestMessageConsumer> logger, ISendEndpointProvider sendEndpointProvider)
        {
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<ITestARollBackRequestMessage> context)
        {
            _logger.LogInformation($"Id ({context.Message.TenantId}) TestARollBackRequestMessage Consume ediliyor .");
             await Task.Delay(5000);
            _logger.LogInformation($"Id ({context.Message.TenantId}) TestARollBackRequestMessage Consume edildi .");
            var isletmeUpdateRollbackMessage = new IsletmeUpdateRollBackRequestMessage()
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                Message = context.Message.Message,
                Data = context.Message.Data
            };
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{EventQueueConsts.IsletmeUpdateRollBackRequestMessageQueue}"));
            await sendEndpoint.Send<IIsletmeUpdateRollBackRequestMessage>(isletmeUpdateRollbackMessage);

        }
    }
}
