SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CommunityMetrics_UserActivityLinks](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[user_activity_id] [bigint] NOT NULL,
	[text] [nvarchar](255) NULL,
	[href] [nvarchar](255) NULL,
 CONSTRAINT [PK_CommunityMetrics_UserActivityLinks] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CommunityMetrics_UserActivityLinks]  WITH CHECK ADD  CONSTRAINT [FK_CommunityMetrics_UserActivityLinks_CommunityMetrics_UserActivity] FOREIGN KEY([user_activity_id])
REFERENCES [dbo].[CommunityMetrics_UserActivity] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[CommunityMetrics_UserActivityLinks] CHECK CONSTRAINT [FK_CommunityMetrics_UserActivityLinks_CommunityMetrics_UserActivity]
GO


