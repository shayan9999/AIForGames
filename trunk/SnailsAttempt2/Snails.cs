using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridWorld;

namespace GridWorld
{
    public class mainAI : BasePlayer
    {
        PlayerWorldState currentGridState; // the board

        public mainAI()
            : base()
        {
            this.Name = "Snails AI Second attempt";

        }

        /// <summary>
        /// Using an internal representation of the board, generate all the board positions that result from 
        /// making a single move, evaluating them all using Board.GetEstimatedScore and choosing the move that
        /// results in the board of highest value.
        /// </summary>
        public override ICommand GetTurnCommands(IPlayerWorldState igrid)
        {
        #if MINIMAX  // We need to implement these methods and use nested structure to evaluate different grid states
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

        #else
            currentGridState = (PlayerWorldState)igrid;
            Command turnCommand;

            // initial positions of the player snail
            int myX = 0; int myY = 0;

            // loop through the world grid and get the current players position
            // Probably improve this to always have current position for this player on the GRID?
            for (int x = 0; x < currentGridState.GridWidthInSquares; x++)
                for (int y = 0; y < currentGridState.GridHeightInSquares; y++)
                    if (currentGridState[x, y].Contents == GridSquare.ContentType.Snail && currentGridState[x, y].Player == this.ID)
                    {
                        myX = x;
                        myY = y;
                    }

            List<GridSquare> emptySquares = GetAdjacentEmptySquares(myX, myY);
            WriteTrace("Size: " + emptySquares.Count);

            if (emptySquares.Count > 0)
            {
                Random r = new Random();
                int index = r.Next(emptySquares.Count);
                GridSquare gs = (GridSquare)emptySquares[index];

                if (gs.X == myX + 1)
                    turnCommand = new Command(myX, myY, Command.Direction.Right);
                else if (gs.X == myX - 1)
                    turnCommand = new Command(myX, myY, Command.Direction.Left);
                else if (gs.Y == myY + 1)
                    turnCommand = new Command(myX, myY, Command.Direction.Up);
                else if (gs.Y == myY - 1)
                    turnCommand = new Command(myX, myY, Command.Direction.Down);
                else
                {
                    WriteTrace(" NULL gs.X: " + gs.X + " myX:" + myX);
                    WriteTrace(" NULL gs.Y: " + gs.Y + " myY:" + myY);
                    turnCommand = null;
                }
            }
            else
            {
                WriteTrace(" NULL");
                turnCommand = null;
            }

            // The command for snail includes current position and a direction
            return turnCommand;
        #endif
        }


        private List<GridSquare> GetAdjacentEmptySquares(int initialX, int initialY)
        {
            List<GridSquare> AdjacentEmptySpaces = new List<GridSquare>();

            int x1, y1;

            for (x1 = initialX - 1; x1 <= initialX + 1; x1++)
            {

                for (y1 = initialY - 1; y1 <= initialY + 1; y1++)
                {

                    if (y1 >= 0 && y1 <= currentGridState.GridHeightInSquares // checkin if y value is within bounds of Grid
                       && x1 >= 0 && x1 <= currentGridState.GridWidthInSquares  // checking if x value is within bounds of Grid
                       && Math.Abs(x1 - y1) == 1)
                    { // checking if we are looking only at right and left and up and down not diagonal ) {

                        if (  currentGridState[x1, y1].Contents == GridSquare.ContentType.Empty ||
                             (currentGridState[x1, y1].Contents == GridSquare.ContentType.Trail &&
                              currentGridState[x1, y1].Player == this.ID))
                        {

                            AdjacentEmptySpaces.Add(currentGridState[x1, y1]);
                        }//if
                    }//if       
                }//for
            }//for


            return AdjacentEmptySpaces;

        }
    }
}
