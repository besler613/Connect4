CREATE TYPE [Connect4].[PositionList] AS TABLE 
(
	Id bigint, 
	MirrorId bigint,
	DepthToWin tinyint
)