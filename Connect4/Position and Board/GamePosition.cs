using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;

namespace Connect4
{
	/// <summary>
	/// A class designed to represent a connect 4 game.
	/// <para>The current position state at a given moment is represented by two <b>BitBoards</b>, which represent the placement of each player's pieces.
	/// The use of the specialized <b>BitBoard</b> DataStructure provides the following advantages:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>Efficient Computations</term>
	/// <description><b>Make Move</b>, <b>Takeback Move</b>, and <b>Check for Win</b> operations can be performed by ultra fast bitwise operations.  
	/// <para>See: <see cref="GamePosition.checkForWin(ulong)"/>/></para>
	/// <para>See Also: <seealso cref="GamePosition.playMove_Internal(int, GamePosition.CheckerStateEnum)"/>/></para></description>
	/// </item>
	/// <item>
	/// <term>Efficient Storage</term>
	/// <description>The two <b>BitBoards</b> can be combined into one BitBoard which uniquely represents a position in an ultra space-efficient manner.</description>
	/// </item>
	/// </list>
	/// </summary>
	public class GamePosition
	{
		#region Member Variables
		/// <summary>
		/// The maximum board width representable using this class.
		/// <para><b>Note:</b> Choice of board width implicitely limits the board height: The inequality <code>(width * (height + 1) <= 64)</code> cannot be violated.</para>
		/// </summary>
		public const int MaxBoardWidth = 9;
		protected Boolean _RedPlayerStarts = true;
		protected Stack<int> _Moves = new Stack<int>();
		protected ulong bitBoardRed;
		protected ulong bitBoardYellow;
		protected int[] nextAvailableRow;
		private readonly int[] bitmapShiftValues;    //Setting up our bitmap shifting distances.  These will be used to shift our bitmaps along the four different directions in which one can achieve a line (vertical, diagonal down, horizontal, diagonal up).
		protected readonly ulong bottomRow;  // This is used for the transformation to a unique ID
		protected readonly ulong topRow;  // This is used for the transformation FROM our unique ID
		private int numberToWin;
		/// <summary>
		/// The width of the current board.
		/// </summary>
		public int BoardWidth { get; }
		/// <summary>
		/// The height of the current board.
		/// </summary>
		public int BoardHeight { get; }
		/// <summary>
		/// Returns a copy of the stack containing the moves made.
		/// </summary>
		/// <value>A stack containing column indices representing the moves made, with the most recent move on top.</value>
		public Stack<int> Moves { get { return new Stack<int>(new Stack<int>(_Moves)); } }
		/// <summary>
		/// Returns an integer representing the number of moves made since the game was created.
		/// </summary>
		public int NumberOfMoves { get {return _Moves.Count; } }
		/// <summary>
		/// Indicates the player whose turn it is to move.
		/// </summary>
		/// <value>A <see cref="CheckerStateEnum"/> representing the whose turn it is to move./></value>
		public CheckerStateEnum PlayerToMove
		{
			get
			{
				if (_RedPlayerStarts & ((_Moves.Count & 1) == 0))
					return CheckerStateEnum.Red; // Red player started and we've had an even number of moves, so it's yellows turn
				else
					return CheckerStateEnum.Yellow;    // Otherwise it must be reds turn.
			}
		}
        /// <summary>
        /// Indicates the current winner of the game.
        /// </summary>
        /// <value>A <see cref="CheckerStateEnum"/> representing the winner given the position on the board, with <b>Empty</b> representing an ongoing or drawn game./></value>
        public CheckerStateEnum GameWinner { get; private set; } = CheckerStateEnum.None;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new game of Connect 4 with an empty board.
		/// </summary>
		/// <param name="width">The width of the board.  (See <see cref="MaxBoardWidth"/>)</param>
		/// <param name="height">The width of the board.  (See <see cref="MaxBoardWidth"/>)</param>
		/// <param name="numberToWin">The number of consecutive checkers required to win the game.  Must no exceed the maximum of the board width and height.</param>
		public GamePosition(int width = 7, int height = 6, int numberToWin = 4)
		{
			//Checking argument validity
			if (numberToWin > width & numberToWin > height)
				throw new ArgumentException("Provided Number to Win value of " + numberToWin.ToString() + " is invalid: This value cannot be greater than the width AND the height.");
			else if (width > MaxBoardWidth)
				throw new ArgumentException("Provided board width of " + width.ToString() + " exceeds maximum allowed value of " + MaxBoardWidth.ToString() + ".");
			else if (width < 1)
				throw new ArgumentException("Provided board width of " + width.ToString() + " is invalid: Board width must larger than 0.");
			else if (height < 1)
				throw new ArgumentException("Provided board height of " + height.ToString() + " is invalid: Board height must larger than 0.");
			else if (width * (height + 1) > 64)
				throw new ArgumentException("Board dimensions of " + width.ToString() + "x" + height.ToString() + " exceed size of 64 bit bitboards.");
			//Setting our member variables
			this.numberToWin = numberToWin;
			BoardWidth = width;
			BoardHeight = height;
			nextAvailableRow = new int[BoardWidth];
			for (int i = 0; i < BoardWidth; i++)
				nextAvailableRow[i] = i * (BoardHeight + 1);
			bitmapShiftValues = new int[4] { 1, BoardHeight, BoardHeight + 1, BoardHeight + 2 };
			ulong topPositionInColumn = ((ulong)1 << BoardHeight);
			bottomRow = 1;
			topRow = topPositionInColumn;
			for (int i = 1; i < BoardWidth; i++)
			{
				bottomRow <<= BoardHeight + 1;
				topRow <<= BoardHeight + 1;
				bottomRow += 1;
				topRow += topPositionInColumn;
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Event meant to notify that a move has been made.
		/// </summary>
		public event Action<int, int, CheckerStateEnum> MoveMade;
		/// <summary>
		/// Event meant to notify that a move has been retracted (taken back).
		/// </summary>
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
		/// <summary>
		/// Method used to play a move.
		/// </summary>
		/// <param name="columnIndex">The column index of the move to be made</param>
		/// <exception cref="ArgumentException">Indicates that an invalid or illegal move has been attempted.  When in doubt, use <see cref="IsValidMove"/> to check if a move is valid./></exception>
		/// <remarks>Invalid or illegal moves will cause an <see cref="ArgumentException"/>./>/></remarks>
		public void PlayMove(int columnIndex)
		{
			// Error checking our input
			int rowIndex = nextAvailableRow[columnIndex] - (BoardHeight + 1) * columnIndex;
			if (columnIndex >= BoardWidth)
				throw new ArgumentException("Invalid Move: Column index of " + columnIndex.ToString() + " exceeds the board width (" + BoardWidth.ToString() + ").");
			else if (columnIndex < 0)
				throw new IndexOutOfRangeException("Invalid Move: Column Index must be >= 0.");
			else if (rowIndex >= BoardHeight)
				throw new ArgumentException("Invalid Move: Column index of " + columnIndex.ToString() + " is full.");
			CheckerStateEnum toMove = PlayerToMove;
			playMove_Internal(columnIndex, toMove);
			if (checkForWin((toMove == CheckerStateEnum.Red ? ref bitBoardRed : ref bitBoardYellow)))
				GameWinner = toMove;
			else
				GameWinner = CheckerStateEnum.None;
			_Moves.Push(columnIndex);
			RaiseMoveMade(rowIndex, columnIndex, toMove);
		}
		/// <summary>
		/// Method used to take back one or more moves.
		/// </summary>
		/// <param name="numMoves">The number of moves to take back.</param>
		/// <exception cref="ArgumentException">Indicates that the number of takebacks requested is either negative, or greater than the number of moves made.  When in doubt, use <see cref="NumberOfMoves"/> to check the number of moves made./></exception>
		public void TakebackMoves(int numMoves)
		{
			if (numMoves > _Moves.Count)
				throw new ArgumentException("Invalid takeback request: Number of takebacks (" + numMoves.ToString() + ") exceeds the number of moves made (" + _Moves.Count.ToString() + ").");
			else if (numMoves < 0)
                throw new ArgumentException("Invalid takeback request: Number of takebacks (" + numMoves.ToString() + ") cannot be negative.");
            for (int i = 0; i < numMoves; i++)
            {
                int columnIndex = _Moves.Pop();
				int rowIndex = --nextAvailableRow[columnIndex] - (BoardHeight + 1) * columnIndex;
				(PlayerToMove == CheckerStateEnum.Red ? ref bitBoardRed : ref bitBoardYellow) ^= ((ulong)1 << nextAvailableRow[columnIndex]);
				GameWinner = CheckerStateEnum.None;    // NOTE: This assumes that a game did not continue after someone won, which is up to the consumer to enforce! 
				RaiseMoveTakeBack(rowIndex, columnIndex);
			}
		}
		/// <summary>
		/// Function used to check if a move is valid and legal given the board position and size.
		/// </summary>
		/// <param name="columnIndex">The index of the column representing the move.</param>
		/// <returns><see cref="Boolean"/> value indicating whether the move is legal and valid.</returns>
		public Boolean IsValidMove(int columnIndex)
		{
			return (!(columnIndex >= BoardWidth || columnIndex < 0 || nextAvailableRow[columnIndex] - 7 * columnIndex >= BoardHeight));
		}
		/// <summary>
		/// Returns a representation of the current board position./>
		/// </summary>
		/// <returns>A <see cref="BoardWidth"/> by <see cref="BoardHeight"/> array of <see cref="CheckerStateEnum"/> representing the current board position./></returns>
		public CheckerStateEnum[,] GetBoardColumns()
		{
			CheckerStateEnum[,] board = new CheckerStateEnum[BoardHeight, BoardWidth];
			for (int i = 0; i < BoardHeight; i++)
			{
				for (int j = 0; j < BoardWidth; j++)
				{
					if (i < nextAvailableRow[j] - (BoardHeight + 1) * j)
					{
						if ((bitBoardRed & ((ulong)1 << (BoardHeight + 1) * j + i)) != 0)
							board[i, j] = CheckerStateEnum.Red; // Red checker found.
						else
							// NOTE: We're lower than the uppermost checker, and a red one wasn't found here, so we must have a yellow.
							board[i, j] = CheckerStateEnum.Yellow;  // Yellow checker found.
					}
					else
						//We're over our uppermost checker in this column.
						board[i, j] = CheckerStateEnum.None;
				}
			}
			return board;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// <b>Internal</b> method used to make a move.
		/// </summary>
		/// <param name="columnIndex">The column index of the move to be made.</param>
		/// <param name="toMove">The player who is making the move.</param>
		/// <remarks>
		/// The purpose of separating the actual mechanics of "making a move" into this function is to avoid the computational overhead of error checking, event raising,
		/// checking whose move it is, checking for wins, etc., with the objective of optimizing for speed.  This API should therefore be used instead of the public <see cref="PlayMove(int)"/>
		/// by a derived class when performing searches.  However, it is up to the consumer of this API to send valid arguments, and to call "checkForWin" following the move (or as appropriate).
		/// </remarks>
		protected void playMove_Internal(int columnIndex, CheckerStateEnum toMove)
		{
			ulong encodedMove = (ulong)1 << nextAvailableRow[columnIndex]++;    // Doing two things: Encoding our move into its own bitboard "encodedMove", and incrementing our column height.
			(toMove == CheckerStateEnum.Red ? ref bitBoardRed : ref bitBoardYellow) ^= encodedMove;
		}
		/// <summary>
		/// Function used to check if the current position on the board is a win for one player.
		/// </summary>
		/// <param name="bitBoard">The bitboard representing the board position for one player.</param>
		/// <returns>A <see cref="Boolean"/> indicating whether the position is won for the player represented by <paramref name="bitBoard"/>.</returns>
		protected Boolean checkForWin(ulong bitBoard)
		{
			foreach (int shiftDir in bitmapShiftValues)
			{
				ulong bitBoard_Copy = bitBoard;
				for (int i = 1; i < numberToWin; i++)
					bitBoard_Copy = bitBoard_Copy & (bitBoard >> i * shiftDir);
				if (bitBoard_Copy != 0)
					return true;
			}
			return false;
		}
		#endregion

		#region Classes and Enums
		/// <summary>
		/// An enum representing one of two players (or neither).
		/// </summary>
		public enum CheckerStateEnum
		{
			/// <summary>No player (aka "empty space", "game in progress", "game drawn", etc.).</summary>
			None = -1,
			/// <summary>Red player (aka "red checker", "red is winner", etc.)</summary>
			Red = 0,
			/// <summary>Yellow player (aka "yellow checker", "yellow is winner", etc.)</summary>
			Yellow = 1
		}
		#endregion
	}
	/// <summary>
	/// An extension of the <see cref="GamePosition"/> class which serves as a datamodel for storage in a MongoDB database.
	/// <para>The ID attribute is generated by encoding the two <b>BitBoards</b> representing the game state into one <b>BitBoard</b> which serves as a unique key for the position while also directly representing the position.</para>
	public class GamePosition_DataModel : GamePosition
	{
		[BsonIdAttribute]
		public ulong ID
		{
			get
			{
				// Step-by-Step Explanation:
				// 1) "bitBoardYellow ^ bitBoardRed..." - This gives us a mask identifying all occupied spaces, 
				// 2) "...+ bitBoardRed + bottomRow" - This results in a bitboard identical to bitBoardRed but with an extra 1 at the top of every column.  This is used to indicate the first unnoccupied square and has the function of making the ID unique.
				return (bitBoardYellow ^ bitBoardRed) + bottomRow + bitBoardRed;
			}
			set
			{
				_Moves.Clear();
				bitBoardRed = value;
				bitBoardYellow = ~value & (ulong)(Math.Pow(2, (BoardHeight + 1) * BoardWidth) - 1);
				ulong searchMask = topRow;
				int numberOfMoves = 0;
				//Step 1: Our searchMask will "rain down" from the top row and every time we see a non-zero "&" operation with our bitBoardRed, we'll know we've hit the first non-empty row of a column.
				while (searchMask > 0)
				{
					ulong columnTopBits = bitBoardRed & searchMask;
					searchMask -= columnTopBits;
					bitBoardYellow -= searchMask;
					while (columnTopBits > 0)
					{
						// We have at least one "first non-empty row" identifier.  Put these into our nextRowForColumn array and remove from our bitBoardRed.
						// First we find the least significant bit in our set.
						ulong leastSignificantBit = columnTopBits & (~columnTopBits + 1);
						// Next, we remove that bit from our bitMapRed and our firstSetBits
						columnTopBits -= leastSignificantBit;
						bitBoardRed -= leastSignificantBit;
						int lsbLog = (int)Math.Log(leastSignificantBit, 2);
						int columnIndex = lsbLog / (int)(BoardHeight + 1);
						nextAvailableRow[(int)(columnIndex)] = lsbLog;
						numberOfMoves += lsbLog - (BoardHeight + 1) * columnIndex;
					}
					searchMask >>= 1;
				}
				// Now we use the total number of moves to determine whose move it is
				_RedPlayerStarts = (numberOfMoves & 1) == 0;
			}
		}
	}
}
