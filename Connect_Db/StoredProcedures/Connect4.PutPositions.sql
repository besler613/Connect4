CREATE PROCEDURE [Connect4].[PutPositions]
	@PositionList [Connect4].[PositionList] readonly
AS
	with cte as (
		select
			p.Id as Id,
			p.MirrorId as MirrorId,
			p.DepthToWin as DepthToWin
		from @PositionList p)
	merge [Connect4].[Position] targ using cte as src
		on src.Id = targ.Id
		or src.MirrorId = targ.Id
	when not matched by target then
		insert
			(Id,
			DepthToWin)
		values
			(src.Id,
			src.DepthToWin)
	when matched then
		update set
			DepthToWin = src.DepthToWin;
			