using Link.Cloud.Core.Shared.Constants;
using Link.Cloud.Core.Shared.Events;
using Link.Cloud.Core.Shared.Interfaces;
using Link.Cloud.Core.Shared.Messages;
using Link.Cloud.SagaStateMachine.Models;
using MassTransit;

namespace Link.Cloud.SagaStateMachine.Services
{
    public class EventStateMachine: MassTransitStateMachine<EventStateInstance>
    {
        //Finsatın eventstate gönderdiği eventler
        public Event<ISirketUpdatedEvent> SirketUpdatedEvent { get; set; }
        public State SirketUpdated { get; private set; }

        public Event<ISirketUpdateSucceedEvent> SirketUpdateSucceedEvent { get; set; }
        public State SirketUpdateSucceed { get; private set; }

        public Event<ISirketUpdateFailedEvent> SirketUpdateFailedEvent { get; set; }
        public State SirketUpdateFailed { get; private set; }

        //Muhasebenin eventstate gönderdiği eventler
        public Event<IIsletmeUpdatedEvent> IsletmeUpdatedEvent { get; set; }
        public State IsletmeUpdated { get; private set; }

        public Event<IIsletmeUpdateSucceedEvent> IsletmeUpdateSucceedEvent { get; set; }
        public State IsletmeUpdateSucceed { get; private set; }

        public Event<IIsletmeUpdateFailedEvent> IsletmeUpdateFailedEvent { get; set; }
        public State IsletmeUpdateFailed { get; private set; }


        public Event<ITestASucceedEvent> TestASucceedEvent { get; set; }
        public State TestASucceed { get; private set; }
        public Event<ITestAFailedEvent> TestAFailedEvent { get; set; }
        public State TestAFailed { get; private set; }
        public Event<ITestBSucceedEvent> TestBSucceedEvent { get; set; }
        public State TestBSucceed { get; private set; }
        public Event<ITestBFailedEvent> TestBFailedEvent { get; set; }
        public State TestBFailed { get; private set; }
        public Event<ITestCSucceedEvent> TestCSucceedEvent { get; set; }
        public State TestCSucceed { get; private set; }
        public Event<ITestCFailedEvent> TestCFailedEvent { get; set; }
        public State TestCFailed { get; private set; }


        //-------------------------

        public EventStateMachine()
        {

            //InitializeEventStateMachineFromFinsat();
            //InitializeEventStateMachineFromMuhasebe();
            TestEventStateMachineFromFinsat();
            TestEventStateMachineFromMuhasebe();
        }

        private void TestEventStateMachineFromMuhasebe()
        {

            InstanceState(x => x.CurrentState);

            //taskId göre sorgulama yapıcak eğer yoksa veritabanında Instance correlationId oluştur.
            Event(() => IsletmeUpdatedEvent, y => y.CorrelateBy<Guid>(x => x.TaskId, z => z.Message.TaskId).SelectId(context => Guid.NewGuid()));

            Event(() => SirketUpdateSucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => SirketUpdateFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestASucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestAFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestBSucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestBFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestCSucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestCFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));


            Initially(When(IsletmeUpdatedEvent).Then(context =>
            {
                context.Saga.TaskId = context.Message.TaskId;
                context.Saga.UserRoleNames.AddRange(context.Message.UserRoleNames.Select(roleName =>
                new UserRoleName { Name = roleName, EventStateInstanceCorrelationId = context.Saga.CorrelationId, EventStateInstance = context.Saga }));
                context.Saga.UserId = context.Message.UserId;
                context.Saga.TenantId = context.Message.TenantId;
                context.Saga.Token = context.Message.Token;
                context.Saga.PreviousData = context.Message.PreviousData;
                context.Saga.UpdatedData = context.Message.UpdatedData;
                context.Saga.CreatedDate = DateTime.Now;

            }).Then(context =>
            {
                Console.WriteLine($"Muhasebe State before TestARequestEvent sent : {context.Saga.CurrentState}");

            }).TransitionTo(IsletmeUpdated)
            .Publish(context => new TestARequestEvent(context.Saga.CorrelationId) //şirketteki update işlemi için yolladık.
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                UserId = context.Message.UserId,
                Token = context.Message.Token,
                UserRoleNames = context.Message.UserRoleNames,
                PreviousData = context.Message.PreviousData,
                UpdatedData = context.Message.UpdatedData
            })      
            .Then(context =>
            {
                Console.WriteLine($"Muhasebe State after TestARequestEvent sent : {context.Saga.CurrentState}");
            }));

            //-----------------------
            During(IsletmeUpdated,
            When(TestASucceedEvent).TransitionTo(TestASucceed)
            .Publish(context => new TestBRequestEvent(context.Saga.CorrelationId)
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                UserId = context.Message.UserId,
                Token = context.Message.Token,
                UserRoleNames = context.Message.UserRoleNames,
                PreviousData = context.Message.PreviousData,
                UpdatedData = context.Message.UpdatedData
            })
            .Then(context =>
            {
                Console.WriteLine($"Muhasebe State after TestBRequestEvent sent : {context.Saga.CurrentState}");
            }),
            When(TestAFailedEvent).TransitionTo(TestAFailed)
            .Send(new Uri($"queue:{EventQueueConsts.IsletmeUpdateRollBackRequestMessageQueue}"), context => new IsletmeUpdateRollBackRequestMessage() //TestB nin rollback için dinleyeceği mesaj
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                Message = context.Message.Message,
                Data = context.Message.Data
            })
            .Then(context =>
            {
                Console.WriteLine($"Muhasebe State after IsletmeUpdateRollBackRequestMessage sent : {context.Saga.CurrentState}");
            })
            );

            //--------------------
            During(TestASucceed,            
            Ignore(TestASucceedEvent),
            When(TestBSucceedEvent).TransitionTo(TestBSucceed)
            
            .Publish(context => new TestCRequestEvent(context.Saga.CorrelationId)
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                PreviousData = context.Message.PreviousData,
                UpdatedData = context.Message.UpdatedData,               

            }).Then(context =>
            {
                Console.WriteLine($"Muhasebe State after TestCRequestEvent sent : {context.Saga.CurrentState}");
            })
            , When(TestBFailedEvent).TransitionTo(TestBFailed)
            .Send(new Uri($"queue:{EventQueueConsts.TestARollBackRequestMessageQueue}"), context => new TestARollBackRequestMessage() //TestA servisinden bir önceki servis şirkete rollback yolla.
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    Token = context.Message.Token,
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Message = context.Message.Message,
                    Data = context.Message.Data
                })
          .Then(context =>
          {
              Console.WriteLine($"Muhasebe State after TestARollBackRequestMessage sent : {context.Saga.CurrentState}");
          }));

            //-----------------------
            During(TestBSucceed,
            Ignore(TestBSucceedEvent),
           When(TestCSucceedEvent).TransitionTo(TestCSucceed)
           .Publish(context => new SirketUpdateRequestEvent(context.Saga.CorrelationId)
           {
               TenantId = context.Message.TenantId,
               TaskId = context.Message.TaskId,
               UserId = context.Message.UserId,
               Token = context.Message.Token,
               UserRoleNames = context.Message.UserRoleNames,
               PreviousData = context.Message.PreviousData,
               UpdatedData = context.Message.UpdatedData

           }).Then(context =>
           {
               Console.WriteLine($"Muhasebe State after SirketUpdateRequestEvent sent : {context.Saga.CurrentState}");
           })
           , When(TestCFailedEvent).TransitionTo(TestCFailed)
           .Send(new Uri($"queue:{EventQueueConsts.TestBRollBackRequestMessageQueue}")
                , context => new TestBRollBackRequestMessage() //TestA nin rollback için dinleyeceği mesaj
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    Token = context.Message.Token,
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Message = context.Message.Message,
                    Data = context.Message.Data
                })
         .Then(context =>
         {
             Console.WriteLine($"Muhasebe State after TestBRollBackRequestMessage sent : {context.Saga.CurrentState}");
         }));

            During(TestCSucceed,
            Ignore(TestCSucceedEvent),
          When(SirketUpdateSucceedEvent).TransitionTo(SirketUpdateSucceed)
          .Publish(context => new IsletmeUpdateSucceedRequestEvent(context.Saga.CorrelationId)
          {
              TenantId = context.Message.TenantId,
              TaskId = context.Message.TaskId,
              Token = context.Message.Token

          }).Finalize()
          , When(SirketUpdateFailedEvent).TransitionTo(SirketUpdateFailed)
            .Send(new Uri($"queue:{EventQueueConsts.TestCRollBackRequestMessageQueue}"),context => new TestCRollBackRequestMessage() //TestB nin rollback için dinleyeceği mesaj
               {
                   TenantId = context.Message.TenantId,
                   TaskId = context.Message.TaskId,
                   Token = context.Message.Token,
                   UserId = context.Message.UserId,
                   UserRoleNames = context.Message.UserRoleNames,
                   Message = context.Message.Message,
                   Data = context.Message.Data
               })
        .Then(context =>
        {
            Console.WriteLine($"Muhasebe State after TestCRollBackRequestMessage sent : {context.Saga.CurrentState}");
        }));

            SetCompletedWhenFinalized();
        }
        private void TestEventStateMachineFromFinsat()
        {

            InstanceState(x => x.CurrentState);

            //taskId göre sorgulama yapıcak eğer yoksa veritabanında Instance correlationId oluştur.
            Event(() => SirketUpdatedEvent, y => y.CorrelateBy<Guid>(x => x.TaskId, z => z.Message.TaskId).SelectId(context => Guid.NewGuid()));

            Event(() => IsletmeUpdateSucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => IsletmeUpdateFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestASucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestAFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestBSucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestBFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestCSucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => TestCFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));


            Initially(When(SirketUpdatedEvent).Then(context =>
            {
                context.Saga.TaskId = context.Message.TaskId;
                context.Saga.UserRoleNames.AddRange(context.Message.UserRoleNames.Select(roleName =>
                new UserRoleName { Name = roleName, EventStateInstanceCorrelationId = context.Saga.CorrelationId, EventStateInstance = context.Saga }));
                context.Saga.UserId = context.Message.UserId;
                context.Saga.TenantId = context.Message.TenantId;
                context.Saga.Token = context.Message.Token;
                context.Saga.PreviousData = context.Message.PreviousData;
                context.Saga.UpdatedData = context.Message.UpdatedData;
                context.Saga.CreatedDate = DateTime.Now;

            }).Then(context =>
            {
                Console.WriteLine($"Finsat State before TestARequestEvent sent : {context.Saga.CurrentState}");

            }).TransitionTo(SirketUpdated)
            .Publish(context => new TestARequestEvent(context.Saga.CorrelationId) //işletmedeki update işlemi için yolladık.
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                UserId = context.Message.UserId,
                Token = context.Message.Token,
                UserRoleNames = context.Message.UserRoleNames,
                PreviousData = context.Message.PreviousData,
                UpdatedData = context.Message.UpdatedData
            })
            .Then(context =>
            {
                Console.WriteLine($"Finsat State after TestARequestEvent sent : {context.Saga.CurrentState}");
            }));

            //-----------------------
            During(SirketUpdated,
            When(TestASucceedEvent).TransitionTo(TestASucceed)
            .Publish(context => new TestBRequestEvent(context.Saga.CorrelationId)
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                UserId = context.Message.UserId,
                Token = context.Message.Token,
                UserRoleNames = context.Message.UserRoleNames,
                PreviousData = context.Message.PreviousData,
                UpdatedData = context.Message.UpdatedData
            })
            .Then(context =>
            {
                Console.WriteLine($"Finsat State after TestBRequestEvent sent : {context.Saga.CurrentState}");
            }),
            When(TestAFailedEvent).TransitionTo(TestAFailed)
            .Send(new Uri($"queue:{EventQueueConsts.IsletmeUpdateRollBackRequestMessageQueue}"), context => new SirketUpdateRollBackRequestMessage() //TestB nin rollback için dinleyeceği mesaj
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                Message = context.Message.Message,
                Data = context.Message.Data
            })
            .Then(context =>
            {
                Console.WriteLine($"Finsat State after SirketUpdateRollBackRequestMessage sent : {context.Saga.CurrentState}");
            })
            );

            //--------------------
            During(TestASucceed,
            Ignore(TestASucceedEvent),
            When(TestBSucceedEvent).TransitionTo(TestBSucceed)

            .Publish(context => new TestCRequestEvent(context.Saga.CorrelationId)
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                PreviousData = context.Message.PreviousData,
                UpdatedData = context.Message.UpdatedData,

            }).Then(context =>
            {
                Console.WriteLine($"Finsat State after TestCRequestEvent sent : {context.Saga.CurrentState}");
            })
            , When(TestBFailedEvent).TransitionTo(TestBFailed)
            .Send(new Uri($"queue:{EventQueueConsts.TestARollBackRequestMessageQueue}"), context => new TestARollBackRequestMessage() //TestA servisinden bir önceki servis şirkete rollback yolla.
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                Message = context.Message.Message,
                Data = context.Message.Data
            })
          .Then(context =>
          {
              Console.WriteLine($"Finsat State after TestARollBackRequestMessage sent : {context.Saga.CurrentState}");
          }));

            //-----------------------
            During(TestBSucceed,
            Ignore(TestBSucceedEvent),
           When(TestCSucceedEvent).TransitionTo(TestCSucceed)
           .Publish(context => new IsletmeUpdateRequestEvent(context.Saga.CorrelationId)
           {
               TenantId = context.Message.TenantId,
               TaskId = context.Message.TaskId,
               UserId = context.Message.UserId,
               Token = context.Message.Token,
               UserRoleNames = context.Message.UserRoleNames,
               PreviousData = context.Message.PreviousData,
               UpdatedData = context.Message.UpdatedData

           }).Then(context =>
           {
               Console.WriteLine($"Finsat State after IsletmeUpdateRequestEvent sent : {context.Saga.CurrentState}");
           })
           , When(TestCFailedEvent).TransitionTo(TestCFailed)
           .Send(new Uri($"queue:{EventQueueConsts.TestBRollBackRequestMessageQueue}")
                , context => new TestBRollBackRequestMessage() //TestB nin rollback için dinleyeceği mesaj
                {
                    TenantId = context.Message.TenantId,
                    TaskId = context.Message.TaskId,
                    Token = context.Message.Token,
                    UserId = context.Message.UserId,
                    UserRoleNames = context.Message.UserRoleNames,
                    Message = context.Message.Message,
                    Data = context.Message.Data
                })
         .Then(context =>
         {
             Console.WriteLine($"Finsat State after TestBRollBackRequestMessage sent : {context.Saga.CurrentState}");
         }));

            During(TestCSucceed,
            Ignore(TestCSucceedEvent),
          When(IsletmeUpdateSucceedEvent).TransitionTo(IsletmeUpdateSucceed)
          .Publish(context => new SirketUpdateSucceedRequestEvent(context.Saga.CorrelationId)
          {
              TenantId = context.Message.TenantId,
              TaskId = context.Message.TaskId,
              Token = context.Message.Token

          }).Finalize()
          , When(IsletmeUpdateFailedEvent).TransitionTo(IsletmeUpdateFailed)
            .Send(new Uri($"queue:{EventQueueConsts.TestCRollBackRequestMessageQueue}"), context => new TestCRollBackRequestMessage() //TestB nin rollback için dinleyeceği mesaj
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,
                UserId = context.Message.UserId,
                UserRoleNames = context.Message.UserRoleNames,
                Message = context.Message.Message,
                Data = context.Message.Data
            })
        .Then(context =>
        {
            Console.WriteLine($"Finsat State after TestCRollBackRequestMessage sent : {context.Saga.CurrentState}");
        }));

            SetCompletedWhenFinalized();
        }
        private void InitializeEventStateMachineFromMuhasebe()
        {
            InstanceState(x => x.CurrentState);

            //taskId göre sorgulama yapıcak eğer yoksa veritabanında Instance correlationId oluştur.
            //Muhasebeden başlarsa
            Event(() => IsletmeUpdatedEvent, y => y.CorrelateBy<Guid>(x => x.TaskId, z => z.Message.TaskId).SelectId(context => Guid.NewGuid()));
            Event(() => SirketUpdateSucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => SirketUpdateFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));


            Initially(When(IsletmeUpdatedEvent).Then(context =>
            {
                context.Saga.TaskId = context.Message.TaskId;
                context.Saga.UserRoleNames.AddRange(context.Message.UserRoleNames.Select(roleName =>
                new UserRoleName { Name = roleName, EventStateInstanceCorrelationId = context.Saga.CorrelationId, EventStateInstance = context.Saga }));
                context.Saga.UserId = context.Message.UserId;
                context.Saga.TenantId = context.Message.TenantId;
                context.Saga.Token = context.Message.Token;
                context.Saga.PreviousData = context.Message.PreviousData;
                context.Saga.UpdatedData = context.Message.UpdatedData;
                context.Saga.CreatedDate = DateTime.Now;

            }).Then(context =>
            {
                Console.WriteLine($"SirketUpdateRequestEvent before : {context.Saga.CurrentState}");

            }).Publish(context => new SirketUpdateRequestEvent(context.Saga.CorrelationId) //şirketteki update işlemi için yolladık.
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                UserId = context.Message.UserId,
                Token = context.Message.Token,
                UserRoleNames = context.Message.UserRoleNames,
                PreviousData = context.Message.PreviousData,
                UpdatedData = context.Message.UpdatedData
            })
            .TransitionTo(IsletmeUpdated)
            .Then(context =>
            {
                Console.WriteLine($"SirketUpdateRequestEvent after : {context.Saga.CurrentState}");
            }));


            During(IsletmeUpdated,
            When(SirketUpdateSucceedEvent) //eğer şirket de succeed ise.
            .Publish(context => new IsletmeUpdateSucceedRequestEvent(context.Saga.CorrelationId) //başlatıcı servis muhasebeye gönderilcek. tüm update işlemleri tamam şirket de update edildi yani.
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,

            })
            .Then(context =>
            {
                Console.WriteLine($"SirketUpdateSucceedEvent after : {context.Saga.CurrentState}");
            }).TransitionTo(IsletmeUpdateSucceed)
            .Finalize(),

           When(SirketUpdateFailedEvent)
          .TransitionTo(IsletmeUpdateFailed)
          .Publish(context => new IsletmeUpdateFailedRequestEvent() //muhasebe dinlicek bunu
          {
              TenantId = context.Message.TenantId,
              TaskId = context.Message.TaskId,
              Token = context.Message.Token,
              UserId = context.Message.UserId,
              UserRoleNames = context.Message.UserRoleNames,
              Message = context.Message.Message,
              Data = context.Message.Data

          })
          .Then(context =>
          {
              Console.WriteLine($"SirketUpdateFailedEvent after : {context.Saga.CurrentState}");
          }));

            SetCompletedWhenFinalized();
        }
        private void InitializeEventStateMachineFromFinsat()
        {
            InstanceState(x => x.CurrentState);

            //taskId göre sorgulama yapıcak eğer yoksa veritabanında Instance correlationId oluştur.
            //Muhasebeden başlarsa
            Event(() => SirketUpdatedEvent, y => y.CorrelateBy<Guid>(x => x.TaskId, z => z.Message.TaskId).SelectId(context => Guid.NewGuid()));
            Event(() => IsletmeUpdateSucceedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));
            Event(() => IsletmeUpdateFailedEvent, y => y.CorrelateById(x => x.Message.CorrelationId));


            Initially(When(SirketUpdatedEvent).Then(context =>
            {
                context.Saga.TaskId = context.Message.TaskId;
                context.Saga.UserRoleNames.AddRange(context.Message.UserRoleNames.Select(roleName =>
                new UserRoleName { Name = roleName, EventStateInstanceCorrelationId = context.Saga.CorrelationId, EventStateInstance = context.Saga }));
                context.Saga.UserId = context.Message.UserId;
                context.Saga.TenantId = context.Message.TenantId;
                context.Saga.Token = context.Message.Token;
                context.Saga.PreviousData = context.Message.PreviousData;
                context.Saga.UpdatedData = context.Message.UpdatedData;
                context.Saga.CreatedDate = DateTime.Now;

            }).Then(context =>
            {
                Console.WriteLine($"IsletmeUpdateRequestEvent before : {context.Saga.CurrentState}");

            }).Publish(context => new IsletmeUpdateRequestEvent(context.Saga.CorrelationId) //işletmedeki update işlemi için yolladık.
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                UserId = context.Message.UserId,
                Token = context.Message.Token,
                UserRoleNames = context.Message.UserRoleNames,
                PreviousData = context.Message.PreviousData,
                UpdatedData = context.Message.UpdatedData
            })
            .TransitionTo(SirketUpdated)
            .Then(context =>
            {
                Console.WriteLine($"IsletmeUpdateRequestEvent after : {context.Saga.CurrentState}");
            }));


            During(SirketUpdated,
            When(IsletmeUpdateSucceedEvent) //eğer işletme de succeed ise.
            .Publish(context => new SirketUpdateSucceedRequestEvent(context.Saga.CorrelationId) //başlatıcı servis finsata gönderilcek. tüm update işlemleri tamam muhasebe de update edildi yani.
            {
                TenantId = context.Message.TenantId,
                TaskId = context.Message.TaskId,
                Token = context.Message.Token,

            })
            .Then(context =>
            {
                Console.WriteLine($"IsletmeUpdateSucceedEvent after : {context.Saga.CurrentState}");
            }).TransitionTo(SirketUpdateSucceed)
            .Finalize(),

           When(IsletmeUpdateFailedEvent)
          .TransitionTo(SirketUpdateFailed)
          .Publish(context => new SirketUpdateFailedRequestEvent() //muhasebe dinlicek bunu
          {
              TenantId = context.Message.TenantId,
              TaskId = context.Message.TaskId,
              Token = context.Message.Token,
              UserId = context.Message.UserId,
              UserRoleNames = context.Message.UserRoleNames,
              Message = context.Message.Message,
              Data = context.Message.Data

          })
          .Then(context =>
          {
              Console.WriteLine($"IsletmeUpdateFailedEvent after : {context.Saga.CurrentState}");
          }));

            SetCompletedWhenFinalized();
        }

    }
}
