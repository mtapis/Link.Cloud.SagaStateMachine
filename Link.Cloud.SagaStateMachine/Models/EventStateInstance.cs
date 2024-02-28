using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Cloud.SagaStateMachine.Models
{

    public class EventStateInstance:SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public Guid TenantId { get; set; }
        public Guid TaskId { get; set; }
        public string UserId { get; set; }
        public List<UserRoleName> UserRoleNames { get; set; } = new List<UserRoleName>();
        public string Token { get; set; }
        public string PreviousData { get; set; }
        public string UpdatedData { get; set; }
        public string CurrentState { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
