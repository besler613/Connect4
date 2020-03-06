CREATE TABLE [dbo].[Positions]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Position] BIGINT NOT NULL DEFAULT 0, 
    [AdjascencyList] NCHAR(10) NULL
)
