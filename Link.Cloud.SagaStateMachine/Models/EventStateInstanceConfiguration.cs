using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Link.Cloud.SagaStateMachine.Models
{
    public class EventStateInstanceConfiguration:SagaClassMap<EventStateInstance>
    {
        protected override void Configure(EntityTypeBuilder<EventStateInstance> entity, ModelBuilder model)
        {
            //entity.Property(x => x.TaskId).IsRequired().HasColumnName("TaskId");
            entity.HasKey("CorrelationId");
            entity.HasMany(x => x.UserRoleNames).WithOne(y => y.EventStateInstance).HasForeignKey(z => z.EventStateInstanceCorrelationId);
        }
    }
}
