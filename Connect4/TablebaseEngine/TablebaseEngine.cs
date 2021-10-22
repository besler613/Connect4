using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Connect4.TablebaseEngine
{
    public class TablebaseEngine
    {
        private readonly Game _game;

        public TablebaseEngine(
            Game game)
        {
            _game = game;
        }

        public int Solve()
        {
            Queue<int> moves = new Queue<int>();
            for (int i = 0; i < _game.BoardWidth; i++)
            {
                moves.Enqueue(i);
            }
            var toMove = CheckerStateEnum.Red;
            var nextToMoveTransform = -2;
            var numMoves = 0;

            while (moves.Any())
            {
                Queue<int> nextMoves = new Queue<int>();
                _game.PlayMove(moves.Dequeue());
                if (_game.CheckForWin(toMove))
                {
                    return ++numMoves;
                }

                for (var column = 0; column < _game.BoardWidth; column++)
                {
                    if (_game.NextAvailableRow[column] < _game.BoardHeight)
                    {

                    }
                }

                toMove += nextToMoveTransform;
                nextToMoveTransform *= -1;
            }
        }
    }
}