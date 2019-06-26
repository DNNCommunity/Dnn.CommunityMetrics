using System.Collections;
using DotNetNuke.Entities;

namespace Dnn.CommunityMetrics
{
    public enum MetricTypeEnum : int
    {
        Undefined = -1,
        Daily = 0,
        Cumulative = 1,
        Once = 2
    }

    public class ActivityDTO : BaseEntityInfo
    {
        // initialization
        public ActivityDTO()
        {
        }

        // public properties
        public int id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string type_name { get; set; }

        public double factor { get; set; }

        public bool active { get; set; }

        public MetricTypeEnum metric_type { get; set; }

        public string user_filter { get; set; }

        public int min_daily { get; set; }

        public int max_daily { get; set; }

        public Hashtable settings { get; set; }
    }

}