SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommunityMetrics_UserActivityLinks]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CommunityMetrics_UserActivityLinks](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[user_activity_id] [bigint] NOT NULL,
	[text] [nvarchar](255) NULL,
	[href] [nvarchar](255) NULL,
 CONSTRAINT [PK_CommunityMetrics_UserActivityLinks] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CommunityMetrics_UserActivityLinks_CommunityMetrics_UserActivity]') AND parent_object_id = OBJECT_ID(N'[dbo].[CommunityMetrics_UserActivityLinks]'))
ALTER TABLE [dbo].[CommunityMetrics_UserActivityLinks]  WITH CHECK ADD  CONSTRAINT [FK_CommunityMetrics_UserActivityLinks_CommunityMetrics_UserActivity] FOREIGN KEY([user_activity_id])
REFERENCES [dbo].[CommunityMetrics_UserActivity] ([id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CommunityMetrics_UserActivityLinks_CommunityMetrics_UserActivity]') AND parent_object_id = OBJECT_ID(N'[dbo].[CommunityMetrics_UserActivityLinks]'))
ALTER TABLE [dbo].[CommunityMetrics_UserActivityLinks] CHECK CONSTRAINT [FK_CommunityMetrics_UserActivityLinks_CommunityMetrics_UserActivity]
GO
