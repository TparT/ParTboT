namespace YarinGeorge.Games.TicTacToe
{
    /// <summary>
    /// Sides to choose for the Tic-Tac-Toe game.
    /// </summary>
    public enum TicTacToeSide
    {
        /// <summary>
        /// Side X of the Tic-Tac-Toe game.
        /// </summary>
        X,

        /// <summary>
        /// Side O of the Tic-Tac-Toe game.
        /// </summary>
        O,

        None
    }

    /// <summary>
    /// Responses that are being reported when players take actions.
    /// </summary>
    public enum TicTacToeGameResponse
    {
        /// <summary>
        /// Describes that nothing special (such as a win or an invalid move) have happened.
        /// </summary>
        NothingSpecial,

        /// <summary>
        /// Describes that player that used X won and player with O have lost.
        /// </summary>
        Player_X_Won,

        /// <summary>
        /// Describes that player that used O won and player with X have lost.
        /// </summary>
        Player_O_Won,

        /// <summary>
        /// Describes that neither player 1 or player 2 have won/lost (Basically a tie, mostly happens when the board is full and no one achieved anything).
        /// </summary>
        Tie,

        /// <summary>
        /// Describes that one of the players did an invalid move (such as trying to use a place that is already taken).
        /// </summary>
        PlaceTaken
    }
}
