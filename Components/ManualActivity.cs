using System;
using System.Collections;
using System.Collections.Generic;

namespace Dnn.CommunityMetrics
{
    public class ManualActivity : IActivity
    {
        public MetricTypeEnum MetricType
        {
            get { return MetricTypeEnum.Once; }
        }

        public List<ActivitySettingDTO> GetSettings()
        {
            List<ActivitySettingDTO> settings = new List<ActivitySettingDTO>();

            return settings;
        }

        public List<UserActivityDTO> GetUserActivity(ActivityDTO objActivity)
        {
            return new List<UserActivityDTO>();
        }
    }
}