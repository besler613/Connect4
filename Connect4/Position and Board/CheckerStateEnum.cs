namespace Connect4
{
    public enum CheckerStateEnum
    {
		/// <summary>No player (aka "empty space", "game in progress", "game drawn", etc.).</summary>
        None = -1,
        /// <summary>Red player (aka "red checker", "red is winner", etc.)</summary>
        Red = 0,
        /// <summary>Yellow player (aka "yellow checker", "yellow is winner", etc.)</summary>
        Yellow = 1
	}
}