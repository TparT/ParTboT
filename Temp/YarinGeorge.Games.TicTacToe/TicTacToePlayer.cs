namespace YarinGeorge.Games.TicTacToe
{
    public class TicTacToePlayer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TicTacToeSide Side { get; set; }

        ///// <summary>
        ///// Gets whether the two YarinGeorge.Games.TicTacToe.TicTacToePlayer objects are equal.
        ///// </summary>
        ///// <param name="player1">First player to compare.</param>
        ///// <param name="player2">Second player to compare.</param>
        ///// <returns>
        ///// Whether the two players are equal.
        ///// </returns>
        //public static bool operator ==(TicTacToePlayer player1, TicTacToePlayer player2)
        //    => player1.Id == player2.Id;

        ///// <summary>
        ///// Gets whether the two YarinGeorge.Games.TicTacToe.TicTacToePlayer objects are not equal.
        ///// </summary>
        ///// <param name="player1">First player to compare.</param>
        ///// <param name="player2">Second player to compare.</param>
        ///// <returns>
        ///// Whether the two players are not equal.
        ///// </returns>
        //public static bool operator !=(TicTacToePlayer player1, TicTacToePlayer player2)
        //    => player1.Id != player2.Id;
    }
}