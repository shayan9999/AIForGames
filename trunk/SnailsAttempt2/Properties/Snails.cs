using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridWorld;

namespace GridWorld
{
    public class shadier : BasePlayer
    {
        PlayerWorldState myWorldState; // the board

        public shadier() : base()
        {
            this.Name = "SnailsLec3";
        }

        /// <summary>
        /// Using an internal representation of the board, generate all the board positions that result from 
        /// making a single move, evaluating them all using Board.GetEstimatedScore and choosing the move that
        /// results in the board of highest value.
        /// </summary>
        public override ICommand GetTurnCommands(IPlayerWorldState igrid)
        {
            myWorldState = (PlayerWorldState)igrid;
            Board internalBoard = new Board(myWorldState);
            WriteTrace(internalBoard);

            double r; // temporary estmated result for a board
            Command BestMove = null;
            double BestScore = double.MinValue; // start out at a value worse than any value we could get

            List<Command> moves = internalBoard.GetMoves();

            foreach (Command c in moves)
            {
                Board tempBoard = new Board(myWorldState);
                tempBoard.DoMove(c); // the board resulting from doing move c
                WriteTrace(tempBoard);
                r = tempBoard.GetEstimatedResult(this.ID); // an estimate of who is winning in this position
                WriteTrace("Score = " + r);
                if (r > BestScore)
                {
                    BestScore = r;
                    BestMove = c;
                }
            }

            return BestMove;
        }
    }
}
