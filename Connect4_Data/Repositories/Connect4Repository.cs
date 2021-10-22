using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Connect4_Data.Config;
using Connect4_Data.Models;
using Dapper;

namespace Connect4_Data.Repositories
{
    public class Connect4Repository
    {
        private readonly string _connString;

        public Connect4Repository(Connect4Config configMonitor)
        {
            _connString = configMonitor.DbConnectionString;
        }

        public async Task<IEnumerable<Position>> GetPositionsAsync(IEnumerable<ulong> positionIds)
        {
            using (var conn = new SqlConnection(connectionString: _connString))
            {
                conn.Open();
                return await GetPositionsAsync(
                    positionIds: positionIds,
                    conn: conn,
                    transaction: null);
            }
        }

        public async Task<IEnumerable<Position>> GetPositionsAsync(
            IEnumerable<ulong> positionIds,
            IDbConnection conn,
            IDbTransaction transaction)
            => await conn.QueryAsync<Position>(
                sql: "[O365].[GetPositions]",
                param: new
                {
                    @PositionIdList = GetPositionIdListTvp(positionIds: positionIds)
                },
                commandType: CommandType.StoredProcedure,
                transaction: transaction);

        public async Task PutPositionsAsync(IEnumerable<Position> positions)
        {
            using (var conn = new SqlConnection(connectionString: _connString))
            {
                conn.Open();
                await PutPositionsAsync(
                    positions: positions,
                    conn: conn,
                    transaction: null);
            }
        }

        public async Task PutPositionsAsync(
            IEnumerable<Position> positions,
            IDbConnection conn,
            IDbTransaction transaction)
            => await conn.QueryAsync(
                sql: "[O365].[PutPositions]",
                param: new
                {
                    @PositionList = GetPositionsListTvp(positions: positions)
                },
                commandType: CommandType.StoredProcedure,
                transaction: transaction);

        private SqlMapper.ICustomQueryParameter GetPositionsListTvp(IEnumerable<Position> positions)
        {
            var positionTable = new DataTable(tableName: "Connect4.PositionList");
            positionTable.Columns.Add(
                columnName: "Id",
                type: typeof(ulong));
            positionTable.Columns.Add(
                columnName: "DepthToWin",
                type: typeof(byte));
            foreach (var position in positions)
            {
                positionTable.Rows.Add(
                    position.Id,
                    position.DepthToWin);
            }

            return positionTable.AsTableValuedParameter(typeName: "Connect4.PositionList");
        }

        private SqlMapper.ICustomQueryParameter GetPositionIdListTvp(IEnumerable<ulong> positionIds)
        {
            var positionTable = new DataTable(tableName: "Connect4.PositionIdList");
            positionTable.Columns.Add(
                columnName: "Id",
                type: typeof(ulong));
            foreach (var positionId in positionIds)
            {
                positionTable.Rows.Add(positionId);
            }

            return positionTable.AsTableValuedParameter(typeName: "Connect4.PositionIdList");
        }
    }
}
