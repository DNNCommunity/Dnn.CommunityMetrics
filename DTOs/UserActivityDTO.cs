using System;
using System.Collections.Generic;
using System.Data.Linq;

namespace Dnn.CommunityMetrics
{
    public class UserActivityDTO
    {
        public UserActivityDTO()
        {
            links = new List<Link>();
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

        public List<Link> links { get; set; }

        public class Link
        {
            public long user_activity_id { get; set; }
            public string text { get; set; }
            public string href { get; set; }
        }
    }
}