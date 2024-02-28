using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Cloud.SagaStateMachine.Models
{
    public class EventStateDbContext:SagaDbContext
    {
        public EventStateDbContext(DbContextOptions<EventStateDbContext> options) : base(options)
        {
        }
        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new EventStateInstanceConfiguration(); }  //dbcontext map configurasyon dosyasını veridk
        }
    }
}
