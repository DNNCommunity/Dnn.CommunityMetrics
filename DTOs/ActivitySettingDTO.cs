
using System;

namespace Dnn.CommunityMetrics
{
    public class ActivitySettingDTO
    {
        // initialization
        public ActivitySettingDTO()
        {
        }

        // public properties
        public Nullable<int> id { get; set; }
        public Nullable<int> activity_id { get; set; }
        public string name { get; set; }
        public string help_text { get; set; }
        public string value { get; set; }
    }

}