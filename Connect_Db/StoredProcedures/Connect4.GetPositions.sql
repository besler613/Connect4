CREATE PROCEDURE [Connect4].[GetPositions]
	@PositionIdList [Connect4].[PositionIdList] readonly
AS
	select
		pids.Id as Id,
		p.DepthToWin as DepthToWin
	from [Connect4].[Position] p
		inner join @PositionIdList pids on pids.Id = p.Id 
			or pids.MirrorId = p.Id
