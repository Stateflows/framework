using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stateflows.Storage.EntityFramework.EntityFramework.Entities
{
    [Table("StateflowsContexts_v1")]
    internal class Context_v1
    {
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [StringLength(1024)]
        public string BehaviorId { get; set; }

        public string Data { get; set; }
    }
}
