using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridWorld;

namespace GridWorld
{
    public enum MiniMaxPlayer{
        Maximiser = 0,
        Minimiser = 1
    }

    public class mainAI : BasePlayer
    {
        PlayerWorldState currentGridState; // the board

        public mainAI()
            : base()
        {
            this.Name = "Snails AI Second attempt";

        }

        private MiniMaxPlayer switchCurrentPlayer(MiniMaxPlayer playerType)
        {
            return (playerType == MiniMaxPlayer.Maximiser) ? MiniMaxPlayer.Minimiser : MiniMaxPlayer.Maximiser;
        }

        private double MiniMax(Board currentBoard, MiniMaxPlayer playerType, int depth)
        {
            List<Command> movesAtThisStage = null;

            if (playerType == MiniMaxPlayer.Maximiser)
                movesAtThisStage = currentBoard.GetMoves();
            else
                movesAtThisStage = currentBoard.GetOpponentsMoves();

            // If there are very few moves left, I assume that to be the end nodes 
            // TODO: needs improvements to make it work for actual end nodes where there are no possible moves left
            if (movesAtThisStage.Count <= 0 || depth >= 100)
            {
                double scoreFinal =  currentBoard.GetBoardScoreForPlayer();
                WriteTrace(currentBoard);
                WriteTrace("Player = " + playerType);
                WriteTrace("Depth = " + depth);
                WriteTrace("Score = " + scoreFinal);
                return scoreFinal;
            }
            else
            {

                double BestScoreThisLayer = (playerType == MiniMaxPlayer.Maximiser ? double.MinValue : double.MaxValue);

                foreach (Command command in movesAtThisStage)
                {

                    Board boardClone = new Board(currentBoard);

                    if (playerType == MiniMaxPlayer.Maximiser)
                    {
                        boardClone.DoMove(command);
                        double scoreThisMove = MiniMax(boardClone, switchCurrentPlayer(playerType), depth + 1);
                        if (scoreThisMove > BestScoreThisLayer){
                            BestScoreThisLayer = scoreThisMove;
                        }
                    }
                    else
                    {
                        boardClone.DoOpponentMove(command);
                        double scoreThisMove = MiniMax(boardClone, switchCurrentPlayer(playerType), depth + 1);
                        if (scoreThisMove < BestScoreThisLayer)
                        {
                            BestScoreThisLayer = scoreThisMove;
                        }
                    }
                }

                return BestScoreThisLayer;
            }

        }

        /// <summary>
        /// Using an internal representation of the board, generate all the board positions that result from 
        /// making a single move, evaluating them all using Board.GetEstimatedScore and choosing the move that
        /// results in the board of highest value.
        /// </summary>
        public override ICommand GetTurnCommands(IPlayerWorldState igrid)
        {
            #if true  // We need to implement these methods and use nested structure to evaluate different grid states

                currentGridState        = (PlayerWorldState) igrid;
                Board boardAtStartOfTurn     = new Board(currentGridState);
                //WriteTrace(internalBoard);
                
                // Get all moves possible at this stage
                List<Command> movesAtThisStage = boardAtStartOfTurn.GetMoves();

                Command BestMove = movesAtThisStage[0];
                double BestScore = double.MinValue; // start out at a value worse than any value we could get

                // Evaluate all those moves one by one using Minimax algorithm to find the best move at this stage
                foreach (Command c in movesAtThisStage)
                {
                    Board boardFromThisMove = new Board(boardAtStartOfTurn);

                    double scoreBeforeMove = boardAtStartOfTurn.GetBoardScoreForPlayer();
                    boardFromThisMove.DoMove(c); // the board resulting from doing move c
                    double scoreAfterMove = boardFromThisMove.GetBoardScoreForPlayer();

                    //Dont consider a turn that does not cover any new squares
                    //if (scoreAfterMove <= scoreBeforeMove)
                        //continue;

                    //WriteTrace("Considering Move:" + c);

                    double scoreFromMove = MiniMax(boardFromThisMove, MiniMaxPlayer.Minimiser, 1); // an estimate of who is winning in this position
                    //WriteTrace("Score = " + bestScore);

                    if (scoreFromMove > BestScore)
                    {
                        BestScore = scoreFromMove;
                        BestMove = c;
                    }
                }

                return BestMove; 


            /*
                currentGridState = (PlayerWorldState)igrid;

                // create a new board with current state of the grid
                Board internalBoard = new Board(currentGridState);
                //WriteTrace(internalBoard);

                double scoreFirstLayer; // temporary estmated result for a board
                Command BestMove = null;
                double BestScore = double.MinValue; // start out at a value worse than any value we could get

                List<Command> moves = internalBoard.GetMoves();

                foreach (Command c in moves)
                {
                    Board tempBoard = new Board(currentGridState);
                    tempBoard.DoMove(c); // the board resulting from doing move c
                    //WriteTrace(tempBoard);

                    scoreFirstLayer = tempBoard.GetBoardScoreForPlayer(); // an estimate of who is winning in this position

                    int turnNumber              = 0;
                    bool gotMoves               = true;
                    ContenType[,] currentBoard  = tempBoard.GetCurrentBoard;

                    while (gotMoves)
                    {
                        turnNumber = (turnNumber + 1) % currentGridState.PlayerCount;
                        
                        //Board bestBoardThisLayer    = null;
                        double bestScoreThisLayer   = 0;

                        switch (turnNumber)
                        {

                            case 0:
                                {
                                    Board tempBoard2 = new Board(currentGridState, currentBoard);
                                    List<Command> moves2 = tempBoard2.GetOpponentsMoves();
                                    if (moves2.Count <= 2) { gotMoves = false; break; }

                                    bestScoreThisLayer = BestScore;

                                    foreach (Command c2 in moves2)
                                    {
                                        tempBoard2.DoOpponentMove(c2);
                                        double scoreThisMove = tempBoard2.GetBoardScoreForPlayer();

                                        if (bestScoreThisLayer + scoreThisMove > bestScoreThisLayer)
                                        {
                                            bestScoreThisLayer = bestScoreThisLayer + scoreThisMove;
                                            BestMove = c;
                                            //bestBoardThisLayer = tempBoard2;
                                        }


                                    }
                                    break;
                                }

                            case 1:
                                {
                                    Board tempBoard2 = new Board(currentGridState, currentBoard);
                                    List<Command> moves2 = tempBoard2.GetOpponentsMoves();
                                    if (moves2.Count <= 2) { gotMoves = false; break; }
                                    
                                    bestScoreThisLayer = BestScore;

                                    foreach (Command c2 in moves2)
                                    {
                                        tempBoard2.DoOpponentMove(c2);
                                        double scoreThisMove = tempBoard2.GetEstimatedResult(this.ID);

                                        if (bestScoreThisLayer + scoreThisMove > bestScoreThisLayer)
                                        {
                                            bestScoreThisLayer = bestScoreThisLayer + scoreThisMove;
                                            BestMove = c;
                                            //bestBoardThisLayer = tempBoard2;
                                        }
                                    }
                                    break;
                                }


                            default:
                                {

                                    break;
                                }

                        } // switch

                        BestScore = BestScore + bestScoreThisLayer;

                    }



                    //ContenType[,] boardFromThisMove = tempBoard.GetCurrentBoard;
                    //tempBoard.MyID = ((currentGridState.ID) % (currentGridState.PlayerCount + 1)) + 1;

                }

                return BestMove; */

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
                       && Math.Abs(x1 - y1) == 1)// checking if we are looking only at right and left and up and down not diagonal )
                    { 

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
