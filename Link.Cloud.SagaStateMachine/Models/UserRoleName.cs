using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Cloud.SagaStateMachine.Models
{
    public class UserRoleName
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public EventStateInstance EventStateInstance { get; set; }
        public Guid EventStateInstanceCorrelationId { get; set; }
    }
}
