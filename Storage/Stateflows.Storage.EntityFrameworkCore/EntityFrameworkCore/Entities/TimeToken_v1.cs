using System.ComponentModel.DataAnnotations.Schema;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities
{
    [Table("StateflowsTimeTokens_v1")]
    internal class TimeToken_v1
    {
        public int Id { get; set; }

        public string BehaviorClass { get; set; }

        public string Data { get; set; }

        public TimeToken_v1(string behaviorClass, string data)
        {
            BehaviorClass = behaviorClass;
            Data = data;
        }
    }
}
