using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect4
{
    class GamePosition
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
        private int[] _columnHeights;
        private long _bitBoard;
        private int _numMoves;
        private Boolean _player1MovesFirst;
        public int BoardWidth { get; }
        public int BoardHeight { get; }
        public long BitBoard { get { return _bitBoard; } }
        public int NumMoves { get { return _numMoves; } }
        public Boolean Player1ToMove { get { return _player1MovesFirst & ((_numMoves & 1) == 0); } }
        #endregion

        #region Constructors
        /**
         * Instance must be fully formed at construction.
         * The consumer is responsible for ensuring the validity of the position being created.
         **/
        public GamePosition(int width, int height, Boolean p1ToMove, long board = 0, byte[] columnHeights = null)
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
            else if (columnHeights != null)
                if (board == 0) 
                    throw new ArgumentException("Column heights must be provided if the board is not empty.");
            else if (columnHeights == null)
                if (board > 0)
                    throw new ArgumentException("Column heights must be provided if the board is not empty.");
            //Setting our member variables
            BoardWidth = width;
            BoardHeight = height;
            _bitBoard = board;
            _player1MovesFirst = p1ToMove;
            //Initializing our _columnHeights array which will track the height of each stack of checkers, which will make adding/removing checkers faster.

        }
        #endregion

        #region Public Methods
        /**
         * 
         **/
        public void playMove(int columnIndex)
        {
            if (columnIndex >= BoardWidth)
                throw new ArgumentException("Invalid Move: Column index of " + columnIndex.ToString() + " exceeds the board width (" + BoardWidth.ToString() + ").");
            else if (columnIndex < 0)
                throw new IndexOutOfRangeException("Column Index must be >= 0.");
            _numMoves++;
        }
        #endregion
    }
}
