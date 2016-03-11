using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    /// <summary>
    /// An enum to define content types of 2D Array in the Board.
    /// Each Element Category is represented by enum values from this set
    /// </summary>
    ///
    enum ContenType {
        Empty = 0,
        Impassable = int.MinValue,
        MySnail         = 1,
        MyTrail         = 2,
        Opponent        = 3,
        OpponentTrail   = 4
    };

    /// <summary>
    /// An internal representation of the snails board. Assumes 2 players (numbered 1 and 2).
    /// </summary>
    ///
    class Board
    {

        /// <summary>
        /// A representation of the Snails board
        /// </summary>
        protected ContenType[,] boardArray;

        /// <summary>
        /// The ID of the Snails player about to move from this position
        /// </summary>
        protected int MyID;

        /// <summary>
        /// Width and Height of the Board
        /// </summary>
        protected int Width, Height;

        public Board(PlayerWorldState pws)
        {
            MyID        = pws.ID; // I am just about to move
            Width       = pws.GridWidthInSquares;
            Height      = pws.GridHeightInSquares;

            boardArray = new ContenType[Width, Height];
            this.PopulateBoard(pws);
        }

        private void PopulateBoard(PlayerWorldState pws)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (pws[x, y].Contents == GridSquare.ContentType.Snail)
                        boardArray[x, y] = (pws[x, y].Player == MyID)? ContenType.MySnail : ContenType.Opponent;
                    else if (pws[x, y].Contents == GridSquare.ContentType.Trail)
                        boardArray[x, y] = (pws[x, y].Player == MyID) ? ContenType.MyTrail : ContenType.OpponentTrail;
                    else if (pws[x, y].Contents == GridSquare.ContentType.Impassable)
                        boardArray[x, y] = ContenType.Impassable; // For impassable, record int.MaxValue
                    else
                        boardArray[x, y] = ContenType.Empty; // empty
                }
            }
        }

        /// <summary>
        /// Find my snail(s). Fill an array of commands with Up, Right, Left, Down for each of my snails.
        /// Only considers moves that are not immediately blocked by opponent snail/trail or board edge.
        /// </summary>
        internal List<Command> GetMoves()
        {
            List<Command> commands = new List<Command>();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (boardArray[x, y] == ContenType.MySnail)
                    {
                        if (x >= 1 &&
                            (boardArray[x - 1, y] == ContenType.Empty || boardArray[x - 1, y] == ContenType.MyTrail))
                            commands.Add(new Command(x, y, Command.Direction.Left));
                        if (y >= 1 &&
                            (boardArray[x, y - 1] == ContenType.Empty || boardArray[x, y - 1] == ContenType.MyTrail))
                            commands.Add(new Command(x, y, Command.Direction.Down));
                        if (x <= Width - 2 &&
                            (boardArray[x + 1, y] == ContenType.Empty || boardArray[x + 1, y] == ContenType.MyTrail))
                            commands.Add(new Command(x, y, Command.Direction.Right));
                        if (y <= Height - 2 &&
                            (boardArray[x, y + 1] == ContenType.Empty || boardArray[x, y + 1] == ContenType.MyTrail))
                            commands.Add(new Command(x, y, Command.Direction.Up));
                    }

            return commands;
        }

        /// <summary>
        /// Is (x,y) on the grid?
        /// </summary>
        private bool IsOnBoard(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Empty squares - straighforward - just update the squares.
        /// Impossible moves - even more straighforward - do nothing.
        /// Take care when sliding along my trails that I get right to the end.
        /// </summary>
        internal void DoMove(Command c)
        {
            System.Diagnostics.Debug.Assert(boardArray[c.X, c.Y] == ContenType.MySnail); // better be my snail moving

            // When I move in direction dir, how do the (x,y) coordinates change.
            // E.g. When moving up I go from (x,y) to (x,y+1) so dx = 0, dy = +1
            int dx = 0, dy = 0;
            switch (c.DirectionToMove)
            {
                case Command.Direction.Up:
                    dx = 0; dy = +1; break;
                case Command.Direction.Down:
                    dx = 0; dy = -1; break;
                case Command.Direction.Right:
                    dx = +1; dy = 0; break;
                case Command.Direction.Left:
                    dx = -1; dy = 0; break;
            }

            // The adjacent square in direction dir is empty so that is the destination 
            if (boardArray[c.X + dx, c.Y + dy] == ContenType.Empty)
            {
                boardArray[c.X, c.Y] = ContenType.MyTrail;
                boardArray[c.X + dx, c.Y + dy] = ContenType.MySnail;
                return;
            }

            // slide along my trail
            int dist = 1; // distance slid so far
            while (IsOnBoard(c.X + dist * dx, c.Y + dist * dy) &&
                   boardArray[c.X + dist * dx, c.Y + dist * dy] == ContenType.MyTrail)
                dist++; // keep going until it is not my trail

            boardArray[c.X, c.Y] = ContenType.MyTrail;
            boardArray[c.X + (dist - 1) * dx, c.Y + (dist - 1) * dy] = ContenType.MySnail;
            return;
        }

        private int CountMySquares()
        {
            int mySqaures = 0;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (boardArray[x, y] == ContenType.MySnail || boardArray[x, y]== ContenType.MyTrail)
                        mySqaures++;
            return mySqaures;
        }

        private int CountOpponentSquares()
        {
            int myOpponent = 0;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (boardArray[x, y] == ContenType.Opponent || boardArray[x,y] == ContenType.OpponentTrail)//if the board contains coordinates which equal to the opponents coordinates 
                        myOpponent++;

            return myOpponent;
        }

        private int CountEmptySquaresAroundMe()
        {
            int emptySquares = 0;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (boardArray[x, y] == ContenType.MyTrail || boardArray[x, y] == ContenType.MySnail)
                    {
                        emptySquares += this.CountAdjacentEmptySquares(x, y);
                    }
                }

            return emptySquares;
        }
        
        //TODO: Count all empty squares around  MySnail / MyTrail
        private int CountEmptySquaresAroundOpponent()
        {
            int emptySquares = 0;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (boardArray[x, y] == ContenType.Opponent || boardArray[x, y] == ContenType.OpponentTrail)
                    {
                        emptySquares += this.CountAdjacentEmptySquares(x, y);
                    }
                }

            return emptySquares;
        }

        private int CountAdjacentEmptySquares(int initialX, int initialY) {
            int count = 0;
            int x1, y1;
            for (x1 = initialX - 1; x1 <= initialX + 1; x1++)
            {
                for (y1 = initialY - 1; y1 <= initialY + 1; y1++)
                {
                    if (y1 >= 0 && y1 < Height   // checkin if y value is within bounds of Grid
                       && x1 >= 0 && x1 < Width  // checking if x value is within bounds of Grid
                       && Math.Abs(x1 - y1) == 1) // checking if we are looking only at right and left and up and down not diagonal )
                    {

                        if (boardArray[x1, y1] == ContenType.Empty)
                        {
                            count++;
                        }//if
                    }//if       
                }//for
            }//for

            return count;
        }

        /// <summary>
        /// Loop through boardArray squares to calculate the score
        /// </summary>
        /// <param name="p"></param>
        /// <returns> 
        /// double value to represent score for this board that shall be used in deciding the best move 
        /// </returns>
        internal double GetEstimatedResult(int p)
        {
            double myScore      = this.CountMySquares() + this.CountEmptySquaresAroundMe();
            double enemyScore = this.CountOpponentSquares() + this.CountEmptySquaresAroundOpponent();

            return myScore - enemyScore;
        }

        /// <summary>
        /// Create a useful representation of the board here 
        /// (e.g. @ = my snail * = my trail, . = empty square, £ = opponent snail, - = opponent trail, O = impassable)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string outstr = "";
            for (int y = this.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < this.Width; x++)
                    if (boardArray[x, y] == ContenType.Empty)
                        outstr += "."; // empty square
                    else if (boardArray[x, y] == ContenType.Impassable)
                        outstr += "O"; // impassable square
                    else if (boardArray[x, y] == ContenType.MySnail)
                        outstr += "@"; // my snail
                    else if (boardArray[x, y] == ContenType.MyTrail)
                        outstr += "*"; // my trail
                    else if (boardArray[x, y] == ContenType.Opponent)
                        outstr += "£"; // opponent snail
                    else
                        outstr += "-"; // opponent trail

                outstr += "\n"; // new line
            }

            return outstr;

        }
    }
}

