using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extra;

namespace YarinGeorge.Games.TicTacToe
{
    public class TicTacToe
    {
        public ConcurrentDictionary<int, TicTacToePlayer> Board { get; private set; }
        public ConcurrentDictionary<string, TicTacToePlayer> Players { get; private set; }
        public TicTacToePlayer Player1 { get; set; }
        public TicTacToePlayer Player2 { get; set; }
        public int Moves { get; set; } = 0;

        public TicTacToe(TicTacToePlayer FirstPlayer = null, TicTacToePlayer SecondPlayer = null)
        {
            Board = new();
            //PopulateBoardDict();
            Players = new();

            Player1 = FirstPlayer;
            Player2 = SecondPlayer;

            Players[Player1.Id] = Player1;
            Players[Player2.Id] = Player2;
        }

        public readonly static TicTacToePlayer DummyPlayer = new TicTacToePlayer
        {
            Id = "0",
            Name = "Dummy",
            Side = TicTacToeSide.None
        };

        public TicTacToePlayer[,] playField = new TicTacToePlayer[,]
        {
            { DummyPlayer, DummyPlayer, DummyPlayer },
            { DummyPlayer, DummyPlayer, DummyPlayer },
            { DummyPlayer, DummyPlayer, DummyPlayer }
        };

        //public TicTacToe CreateBoard(TicTacToePlayer FirstPlayer = null, TicTacToePlayer SecondPlayer = null)
        //{
        //    Board = new();

        //    if (Player1 != FirstPlayer && Player1 is not null)
        //        Player1 = FirstPlayer;

        //    if (Player2 != SecondPlayer && Player2 is not null)
        //        Player2 = SecondPlayer;

        //    return this;
        //}

        public void ResetBoard()
        {
            playField = new TicTacToePlayer[,]
            {
                { DummyPlayer, DummyPlayer, DummyPlayer },
                { DummyPlayer, DummyPlayer, DummyPlayer },
                { DummyPlayer, DummyPlayer, DummyPlayer }
            };
        }

        public TicTacToeGameResponse Play(TicTacToePlayer Player, int Position)
        {
            Player = Players[Player.Id];
            //bool ValidField = false;

            if ((Position == 1) && (playField[0, 0] == DummyPlayer))
                playField[0, 0] = Player;
            else if ((Position == 2) && (playField[0, 1] == DummyPlayer))
                playField[0, 1] = Player;
            else if ((Position == 3) && (playField[0, 2] == DummyPlayer))
                playField[0, 2] = Player;
            else if ((Position == 4) && (playField[1, 0] == DummyPlayer))
                playField[1, 0] = Player;
            else if ((Position == 5) && (playField[1, 1] == DummyPlayer))
                playField[1, 1] = Player;
            else if ((Position == 6) && (playField[1, 2] == DummyPlayer))
                playField[1, 2] = Player;
            else if ((Position == 7) && (playField[2, 0] == DummyPlayer))
                playField[2, 0] = Player;
            else if ((Position == 8) && (playField[2, 1] == DummyPlayer))
                playField[2, 1] = Player;
            else if ((Position == 9) && (playField[2, 2] == DummyPlayer))
                playField[2, 2] = Player;
            else
            {
                //ValidField = false;
                return TicTacToeGameResponse.PlaceTaken;
            }


            if (Board.TryAdd(Position, Player))
            {
                Moves++;

                if (PlayerWon(Player))
                    return Player.Side == TicTacToeSide.X ? TicTacToeGameResponse.Player_X_Won : TicTacToeGameResponse.Player_O_Won;
                else
                {
                    if (Moves >= 9)
                    {
                        return TicTacToeGameResponse.Tie;
                    }
                    else
                        return TicTacToeGameResponse.NothingSpecial;
                }
                //}
            }

            else
                return TicTacToeGameResponse.PlaceTaken;
        }

        private void PopulateBoardDict()
        {
            for (int i = 1; i < 10; i++)
                Board.TryAdd(i, DummyPlayer);
        }

        private bool PlayerWon(TicTacToePlayer Player)
        {
            if ( ((playField[0, 0] == Player) && (playField[0, 1] == Player) && (playField[0, 2] == Player)) ||
                 ((playField[1, 0] == Player) && (playField[1, 1] == Player) && (playField[1, 2] == Player)) ||
                 ((playField[2, 0] == Player) && (playField[2, 1] == Player) && (playField[2, 2] == Player)) ||
                 ((playField[0, 0] == Player) && (playField[1, 0] == Player) && (playField[2, 0] == Player)) ||
                 ((playField[0, 1] == Player) && (playField[1, 1] == Player) && (playField[2, 1] == Player)) ||
                 ((playField[0, 2] == Player) && (playField[1, 2] == Player) && (playField[2, 2] == Player)) ||
                 ((playField[0, 0] == Player) && (playField[1, 1] == Player) && (playField[2, 2] == Player)) ||
                 ((playField[0, 2] == Player) && (playField[1, 1] == Player) && (playField[2, 0] == Player))
               )
            {
                return true;
            }
            else
            {
                return false;
            }


            //#region Try Get VALUES

            //bool OneExists = Board.TryGetValue(1, out TicTacToePlayer one);
            //bool TwoExists = Board.TryGetValue(2, out TicTacToePlayer two);
            //bool ThreeExists = Board.TryGetValue(3, out TicTacToePlayer three);
            //bool FourExists = Board.TryGetValue(4, out TicTacToePlayer four);
            //bool FiveExists = Board.TryGetValue(5, out TicTacToePlayer five);
            //bool SixExists = Board.TryGetValue(6, out TicTacToePlayer six);
            //bool SevenExists = Board.TryGetValue(7, out TicTacToePlayer seven);
            //bool EightExists = Board.TryGetValue(8, out TicTacToePlayer eight);
            //bool NineExists = Board.TryGetValue(9, out TicTacToePlayer nine);

            //#endregion Try Get VALUES


            //#region Check for full horizontal lines.
            //bool H1 = OneExists && TwoExists && ThreeExists; // Check if 1st Horizontal line is full.
            //bool H2 = FourExists && FiveExists && SixExists; // Check if 2nd Horizontal line is full.
            //bool H3 = SevenExists && EightExists && NineExists; // Check if 3rd Horizontal line is full. 
            //#endregion Check for full horizontal lines.

            //#region Check for full vertical lines.
            //bool V1 = OneExists && FourExists && SevenExists;
            //bool V2 = TwoExists && FiveExists && EightExists;
            //bool V3 = ThreeExists && SixExists && NineExists;
            //#endregion Check for full vertical lines.

            //#region Check for full slanting lines.
            //bool S1 = OneExists && FiveExists && NineExists; // Check if 1st Slanting line is full.
            //bool S2 = ThreeExists && FiveExists && SevenExists; // Check if 2nd Slanting line is full.
            //#endregion Check for full slanting lines.

            ////Player = Players[Player.Id];

            //if (H1 || H2 || H3) // If one of the horizontal lines are complete, check if its all completed by the same player.
            //{
            //    if (H1) { if (Player.Id.EqualsAll(one.Id, two.Id, three.Id)) return true; } // Check if the 1st horizontal line is owned by owned by player.
            //    else if (H2) { if (Player.Id.EqualsAll(four.Id, five.Id, six.Id)) return true; } // Check if the 2nd horizontal line is owned by owned by player.
            //    else if (H3) { if (Player.Id.EqualsAll(seven.Id, eight.Id, nine.Id)) return true; } // Check if the 3rd horizontal line is owned by owned by player.
            //}

            //if (V1 || V2 || V3)
            //{
            //    if (V1) { if (Player.Id.EqualsAll(one.Id, four.Id, seven.Id)) return true; }
            //    else if (V2) { if (Player.Id.EqualsAll(two.Id, five.Id, eight.Id)) return true; }
            //    else if (V3) { if (Player.Id.EqualsAll(three.Id, six.Id, nine.Id)) return true; }
            //}

            //if (S1 || S2) // If one of the slanting lines are complete, check if its all completed by the same player.
            //{
            //    if (S1) { if (Player.Id.EqualsAll(one.Id, five.Id, nine.Id)) return true; } // Check if the 1st slanting line is owned by owned by player.
            //    else if (S2) { if (Player.Id.EqualsAll(three.Id, five.Id, seven.Id)) return true; } // Check if the 2nd slanting line is owned by owned by player.
            //}

            //return false;
        }
    }
}
