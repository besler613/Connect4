using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect4
{
    public class GamePosition
    {
        /**
         * This class represents a connect 4 position during a game.
         * Note that this class is "dumb" and has no knowledge of the rules of connect 4.  The exception is the board dimensions, which are enforced.
         * It is up to the consumer to verify the validity of the position prior to the creation of an instance.
         * 
         * NOTE: This class uses a binary representation of the position which involves encoding the position of each players' checkers into a seperate bitboard, and then representing each bitboard as a long.
         * This highly efficient representation was originally authored by computer scientist John Tromp in the 1990's.
         **/

        #region Member Variables
        public const int MaxBoardWidth = 9;
        private Boolean _redPlayerStarts = true;    // May be set to false if the initial loaded position is part way through the game
        private uint _columnHeights;
        private ulong _bitBoard;
        private Stack<int> _moves = new Stack<int>();
        private CheckerStateEnum _Winner = CheckerStateEnum.Empty;
        public int BoardWidth { get; }
        public int BoardHeight { get; }
        public ulong BitBoard { get { return _bitBoard; } }
        public uint ColumnHeights { get { return _columnHeights; } }
        public Stack<int> Moves { get { return _moves; } }
        public CheckerStateEnum PlayerToMove
        {
            get
            {
                if (_redPlayerStarts & ((_moves.Count & 1) == 0))
                    return CheckerStateEnum.Red; // Red player started and we've had an even number of moves, so it's yellows turn
                else
                    return CheckerStateEnum.Yellow;    // Otherwise it must be reds turn.
            }
        }
        public CheckerStateEnum GameWinner { get { return _Winner; } }
        #endregion

        #region Constructors
        public GamePosition(int width, int height)
        {
            //Checking argument validity
            if (width > MaxBoardWidth)
                throw new ArgumentException("Provided board width of " + width.ToString() + " exceeds maximum allowed value of " + MaxBoardWidth.ToString() + ".");
            else if (width < 1)
                throw new ArgumentException("Provided board width of " + width.ToString() + " is invalid: Board width must larger than 0.");
            else if (height < 1)
                throw new ArgumentException("Provided board height of " + height.ToString() + " is invalid: Board height must larger than 0.");
            else if (width * (height + 1) > 64)
                throw new ArgumentException("Board dimensions of " + width.ToString() + "x" + height.ToString() + " exceed size of 64 bit bitboards.");
            //Setting our member variables
            BoardWidth = width;
            BoardHeight = height;
            //Populating our seach directoins, which will be used to check for wins every every move.
            //Note: Since each move lands on top of a column, we can ommit the "upwards" direction (1, 0)
            _SearchDirections.Add(new int[] { 0, -1});
            _SearchDirections.Add(new int[] { -1, -1 });
            _SearchDirections.Add(new int[] { -1, 0 });
            _SearchDirections.Add(new int[] { -1, 1 });
            _SearchDirections.Add(new int[] { 0, 1 });
            _SearchDirections.Add(new int[] { 1, 1 });
            _SearchDirections.Add(new int[] { 1, -1 });
        }
        #endregion

        #region Events
        public event Action<int, int, CheckerStateEnum> MoveMade;
        public event Action<int, int> MoveTakeBack;
        private void RaiseMoveMade(int row, int column, CheckerStateEnum checker)
        {
            if (MoveMade != null)
                MoveMade(row, column, checker);
        }
        private void RaiseMoveTakeBack(int row, int column)
        {
            if (MoveTakeBack != null)
                MoveTakeBack(row, column);
        }
        #endregion

        #region Public Methods
        /**
         * 
         **/
        public void PlayMove(int columnIndex)
        {
            if (columnIndex >= BoardWidth)
                throw new ArgumentException("Invalid Move: Column index of " + columnIndex.ToString() + " exceeds the board width (" + BoardWidth.ToString() + ").");
            else if (columnIndex < 0)
                throw new IndexOutOfRangeException("Invalid Move: Column Index must be >= 0.");
            CheckerStateEnum toMove = PlayerToMove;
            if (toMove == CheckerStateEnum.Yellow)
                //If Yellow moved, we need to flip the bit that is one decimal place larger than the current column height
                //But remember: Red is represented as 0's in our bitboard, so we don't need to modify our bitboard if they moved!
                _bitBoard += (ulong)Math.Pow(2, (8 * columnIndex + ((byte)((int)_columnHeights >> 4 * columnIndex))));
            _columnHeights += (uint)Math.Pow(2, (4 * columnIndex));
            int rowIndex = Convert.ToInt32((_columnHeights >> 4 * columnIndex) - 1 & 0b1111);
            _moves.Push(columnIndex);
            _Winner = checkForFour(rowIndex, columnIndex);
            RaiseMoveMade(rowIndex, columnIndex, toMove);
        }
        public void TakebackMoves(int numMoves)
        {
            if (numMoves > _moves.Count)
                throw new ArgumentException("Invalid takeback request: Number of takebacks (" + numMoves.ToString() + ") exceeds the number of moves made (" + _moves.Count.ToString() + ").");
            for (int i = 0; i < numMoves; i++)
            {
                int nextMove = _moves.Pop();
                if (PlayerToMove == CheckerStateEnum.Yellow)
                    _bitBoard -= (ulong)Math.Pow(2, (8 * nextMove + ((byte)((int)_columnHeights >> 4 * nextMove))) - 1);
                _columnHeights -= (uint)Math.Pow(2, 4 * nextMove);
                int rowIndex = Convert.ToInt32((_columnHeights >> 4 * nextMove) & 0b1111);
                _Winner = CheckerStateEnum.Empty;    // NOTE: This assumes that a game did not continue after someone won, which is up to the consumer to enforce! 
                RaiseMoveTakeBack(rowIndex, nextMove);
            }
        }
        public CheckerStateEnum GetPositionState(int row, int column)
        {
            if (row >= BoardHeight)
                throw new ArgumentException("Invalid Request: Row index of " + row.ToString() + " exceeds the board height (" + BoardHeight.ToString() + ").");
            else if (row < 0)
                throw new IndexOutOfRangeException("Invalid Request: Row Index must be >= 0.");
            else if (column >= BoardWidth)
                throw new ArgumentException("Invalid Request: Column index of " + column.ToString() + " exceeds the board height (" + BoardWidth.ToString() + ").");
            else if (column < 0)
                throw new IndexOutOfRangeException("Invalid Request: Column Index must be >= 0.");
            if (row >= ((_columnHeights >> 4 * column) & 0b1111))
                return CheckerStateEnum.Empty;   //Requested position exceeds column height, so it must be empty
            return (CheckerStateEnum)Enum.ToObject(typeof(CheckerStateEnum), ((_bitBoard >> (8 * column + 4 * row)) & 1));
        }
        #endregion

        #region Private Methods
        private List<int[]> _SearchDirections = new List<int[]> ();    //We search for wins from top down, so we only need to search west, southwest, south, southeast, and     //We search for wins from top down, so we only need to search west, southwest, south, southeast, and east
        private CheckerStateEnum checkForFour(int startRow, int startColumn)
        {
            CheckerStateEnum playerWhoMoved = GetPositionState(startRow, startColumn);
            foreach (int[] dir in _SearchDirections)
            {
                int nextRow = startRow;
                int nextColumn = startColumn;
                int counter = 0;
                while (nextRow >= 0 && nextRow < BoardHeight && nextColumn >= 0 && nextColumn < BoardWidth
                    && GetPositionState(nextRow, nextColumn) == playerWhoMoved)
                {
                    counter++;
                    if (counter == 4)
                        // We have ourselves a winner!
                        return playerWhoMoved;
                    nextRow += dir[0];
                    nextColumn += dir[1];
                }
            }
            return CheckerStateEnum.Empty;  //Use empty to indicate that there is no winner
        }
        
        #endregion

        #region Classes and Enums
        public enum CheckerStateEnum
        {
            Empty = -1,
            Red = 0,
            Yellow = 1
        }
        #endregion
    }
}