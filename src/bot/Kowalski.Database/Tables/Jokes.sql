CREATE TABLE [dbo].[Jokes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Jokes] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))