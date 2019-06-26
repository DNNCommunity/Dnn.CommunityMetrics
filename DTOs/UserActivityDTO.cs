using System;

namespace Dnn.CommunityMetrics
{
    public class UserActivityDTO
    {

        // initialization
        public UserActivityDTO()
        {
        }

        public long id { get; set; }

        public int activity_id { get; set; }

        public int user_id { get; set; }

        public DateTime date { get; set; }

        public int count { get; set; }

        public string notes { get; set; }

        public DateTime created_on_date { get; set; }

        public string user_name { get; set; }
        public string activity_name { get; set; }

    }
}