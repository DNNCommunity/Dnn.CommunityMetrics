

CREATE TABLE [dbo].[CommunityMetrics_Activity](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NOT NULL,
	[description] [nvarchar](255) NULL,
	[type_name] [nvarchar](255) NULL,
	[factor] [float] NOT NULL,
	[active] [bit] NOT NULL,
	[metric_type] [int] NOT NULL,
	[user_filter] [nvarchar](500) NOT NULL,
	[min_daily] [int] NOT NULL,
	[max_daily] [int] NOT NULL,
	[created_by_user_id] [int] NOT NULL,
	[created_on_date] [datetime] NOT NULL,
	[last_modified_by_user_id] [int] NOT NULL,
	[last_modified_on_date] [datetime] NOT NULL,
 CONSTRAINT [PK_CommunityMetrics_Activity] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CommunityMetrics_ActivitySetting](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[activity_id] [int] NOT NULL,
	[name] [nvarchar](50) NOT NULL,
	[value] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_CommunityMetrics_ActivitySetting] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CommunityMetrics_UserActivity](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[activity_id] [int] NOT NULL,
	[user_id] [int] NOT NULL,
	[date] [datetime] NOT NULL,
	[count] [int] NOT NULL,
	[notes] [nvarchar](255) NULL,
	[created_on_date] [datetime] NOT NULL,
 CONSTRAINT [PK_CommunityMetrics_UserActivity] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CommunityMetrics_ActivitySetting]  WITH NOCHECK ADD  CONSTRAINT [FK_CommunityMetrics_ActivitySetting_CommunityMetrics_Activity] FOREIGN KEY([activity_id])
REFERENCES [dbo].[CommunityMetrics_Activity] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
NOT FOR REPLICATION 
GO
ALTER TABLE [dbo].[CommunityMetrics_ActivitySetting] CHECK CONSTRAINT [FK_CommunityMetrics_ActivitySetting_CommunityMetrics_Activity]
GO
ALTER TABLE [dbo].[CommunityMetrics_UserActivity]  WITH NOCHECK ADD  CONSTRAINT [FK_CommunityMetrics_UserActivity_CommunityMetrics_Activity] FOREIGN KEY([activity_id])
REFERENCES [dbo].[CommunityMetrics_Activity] ([id])
ON DELETE CASCADE
NOT FOR REPLICATION 
GO
ALTER TABLE [dbo].[CommunityMetrics_UserActivity] CHECK CONSTRAINT [FK_CommunityMetrics_UserActivity_CommunityMetrics_Activity]
GO
ALTER TABLE [dbo].[CommunityMetrics_UserActivity]  WITH CHECK ADD  CONSTRAINT [FK_CommunityMetrics_UserActivity_Users] FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([UserID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunityMetrics_UserActivity] CHECK CONSTRAINT [FK_CommunityMetrics_UserActivity_Users]
GO





INSERT INTO dbo.Schedule
	( TypeFullName, [TimeLapse], [TimeLapseMeasurement], [RetryTimeLapse], [RetryTimeLapseMeasurement], [RetainHistoryNum], [AttachToEvent], [CatchUpEnabled], [Enabled], [ObjectDependencies], [Servers], [FriendlyName])
VALUES ( 'Dnn.CommunityMetrics.ActivityJob, Dnn.CommunityMetrics', 1, 'd', 0, 's', 10, '', 0, 1, '', null, 'Community Metrics Job' )
GO