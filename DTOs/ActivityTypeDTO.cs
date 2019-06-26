using DotNetNuke.Entities;

namespace Dnn.CommunityMetrics
{

    public class ActivityTypeDTO : BaseEntityInfo
    {
        // initialization
        public ActivityTypeDTO()
        {
        }

        // public properties
        public string name { get; set; }
        public string full_name { get; set; }
    }

}