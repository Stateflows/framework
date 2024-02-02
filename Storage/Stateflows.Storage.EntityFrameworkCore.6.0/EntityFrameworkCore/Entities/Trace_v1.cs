using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities
{
    [Table("StateflowsTraces_v1")]
    [Index(nameof(BehaviorId))]
#pragma warning disable S101 // Types should be named in PascalCase
    public class Trace_v1
#pragma warning restore S101 // Types should be named in PascalCase
    {
        public int Id { get; set; }

        [StringLength(1024)]
        public string BehaviorId { get; set; }

        public DateTime? ExecutionTime { get; set; }

        public string Data { get; set; }

        public Trace_v1(string behaviorId, DateTime? executionTime, string data)
        {
            BehaviorId = behaviorId;
            ExecutionTime = executionTime;
            Data = data;
        }
    }
}
