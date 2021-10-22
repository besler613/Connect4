using System;
using System.Collections.Generic;

namespace Connect4_Data.Models
{
    public class Position
    {
        public Position(
            ulong id,
            byte depthToWin,
            int boardHeight)
        {
            Id = id;
            DepthToWin = depthToWin;
            MirrorId = getMirrorId(
                id: id,
                boardHeight: boardHeight);
        }

        public ulong Id { get; }

        public ulong MirrorId { get; }

        public byte DepthToWin { get; }

        private ulong getMirrorId(
            ulong id,
            int boardHeight)
        {
            ulong mirrorId = 0;
            byte nRows = (byte)(boardHeight + 1);
            ulong chunk = (ulong)(Math.Pow(x: 2, y: nRows) - 1);

            while (id > 0)
            {
                mirrorId <<= nRows;
                mirrorId |= id & chunk;
                id >>= nRows;
            }

            return mirrorId;
        }
    }
}