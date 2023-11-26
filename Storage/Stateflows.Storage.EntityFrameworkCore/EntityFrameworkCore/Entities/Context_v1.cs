using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities
{
    [Table("StateflowsContexts_v1")]
    [Index(nameof(BehaviorId), IsUnique = true)]
    public class Context_v1
    {
        public int Id { get; set; }

        [StringLength(1024)]
        public string BehaviorClass { get; set; }

        [StringLength(1024)]
        public string BehaviorId { get; set; }

        public string Data { get; set; }

        public Context_v1(string behaviorClass, string behaviorId, string data)
        {
            BehaviorClass = behaviorClass;
            BehaviorId = behaviorId;
            Data = data;
        }
    }
}
