using System;
using System.Collections.Generic;

namespace Computationele_Intelligentie_Pi
{
    class Program
    {
        static void Main(string[] args)
        {
            Sudoku s = new Sudoku();            
            s.PrintBoard();
            //s.HillClimb();
            s.TrySwap(0, 0, 0, 2);
            s.PrintBoard();
            Console.ReadKey();
        }
    }

    class Sudoku
    {
        public int[,] board; //The 2D array representing the board/field with all the numbers
        public Tuple<int, bool>[,] boardExtra; // The same 2D array, but now with tuples. Int meaning the number, The bool meaning wether the number is fixed = TRUE or not-fixed = FALSE
        int n; // The size of the sudoku, we will use: 9, 16 or 25
        int nSqrd; // The square root of n: 3, 4 or 5
        Random random;

        /// <summary>
        /// Constructor for the sudoku. The board variable can vary, comment it out to change input.
        /// </summary>
        public Sudoku()
        {
            board = new int[,]
            {
                {0,0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,3,0,8,5 },
                {0,0,1,0,2,0,0,0,0 },
                {0,0,0,5,0,7,0,0,0 },
                {0,0,4,0,0,0,1,0,0 },
                {0,9,0,0,0,0,0,0,0 },
                {5,0,0,0,0,0,0,7,3 },
                {0,0,2,0,1,0,0,0,0 },
                {0,0,0,0,4,0,0,0,9 }
            };           

            n = board.GetLength(0);

            boardExtra = new Tuple<int, bool>[n, n];

            nSqrd = (int)Math.Sqrt(n);

            random = new Random();

            FillBoard();
        }

        /// <summary>
        /// Replace all the 0's in the sudoko with numbers 1 to n such that each "square" has all the unique numbers.
        /// </summary>
        public void FillBoard()
        {
            // Randomize the 0's inside of the squares
            for (int x1 = 0; x1 < nSqrd; x1++)
            {
                int xSq = x1 * nSqrd;

                for (int y1 = 0; y1 < nSqrd; y1++)
                {
                    int ySq = y1 * nSqrd;

                    //First time trough the square, remove the existing numbers from the list.

                    List<int> numbers = new List<int>();
                    for (int i = 1; i < n + 1; i++)
                    {
                        numbers.Add(i);
                    }

                    for (int x = 0; x < nSqrd; x++)
                    {
                        for (int y = 0; y < nSqrd; y++)
                        {
                            int xCor = xSq + x;
                            int yCor = ySq + y;

                            if (board[xCor, yCor] != 0)
                            {
                                numbers.Remove(board[xCor, yCor]);
                            }

                        }
                    }

                    // Second time trough the square, now fill the 0's with the numbers from the list, which becomes a stack :)
                    Stack<int> numStack = new Stack<int>(numbers);

                    for (int x = 0; x < nSqrd; x++)
                    {
                        for (int y = 0; y < nSqrd; y++)
                        {
                            int xCor = xSq + x;
                            int yCor = ySq + y;

                            if (board[xCor, yCor] == 0)
                            {
                                int number = numStack.Pop();
                                board[xCor, yCor] = number;
                                boardExtra[xCor, yCor] = new Tuple<int, bool>(number, false); // Is the number "new"? Than fixed = FALSE
                            }
                            else
                            {
                                boardExtra[xCor, yCor] = new Tuple<int, bool>(board[xCor, yCor], true); // Is the number "old"? Than fixed = TRUE
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method for printing the whole board.
        /// </summary>
        public void PrintBoard()
        {
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    Console.Write(boardExtra[x, y].Item1 + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("- - - - - - - - -");
        }

        /// <summary>
        /// Prints (Later maybe return) a value which indicates how good a row is. N being the worst, 0 being the best.
        /// </summary>
        /// <param name="rowNumber">Which row to evaluate</param>
        public int EvaluateRow(int rowNumber)
        {
            List<int> numbers = new List<int>();
            for (int i = 1; i < n + 1; i++)
            {
                numbers.Add(i);
            }

            for (int y = 0; y < n; y++)
            {
                if(numbers.Contains(boardExtra[rowNumber, y].Item1))
                {
                    numbers.Remove(boardExtra[rowNumber, y].Item1);
                }
            }

            Console.WriteLine("Row " + rowNumber + " evaluated: " + numbers.Count);
            return numbers.Count;
        }

        /// <summary>
        /// Prints (Later maybe return) a value which indicates how good a column is. N being the worst, 0 being the best.
        /// </summary>
        /// <param name="columnNumber">Which column to evaluate</param>
        public int Evaluatecolumn(int columnNumber)
        {
            List<int> numbers = new List<int>();
            for (int i = 1; i < n + 1; i++)
            {
                numbers.Add(i);
            }

            for (int x = 0; x < n; x++)
            {
                if (numbers.Contains(boardExtra[x, columnNumber].Item1))
                {
                    numbers.Remove(boardExtra[x, columnNumber].Item1);
                }
            }

            Console.WriteLine("column " + columnNumber + " evaluated: " + numbers.Count);
            return numbers.Count;
        }

        /// <summary>
        /// Swaps two non-fixed numbers and then evaluate the relevant row(s) and column(s).
        /// </summary>
        /// <param name="x1">X coordinate of the first number.</param>
        /// <param name="y1">Y coordinate of the first number.</param>
        /// <param name="x2">X coordinate of the second number.</param>
        /// <param name="y2">Y coordinate of the second number.</param>
        public void TrySwap(int x1, int y1, int x2, int y2)
        {
            Tuple<int, bool> number1 = boardExtra[y1, x1]; 
            Tuple<int, bool> number2 = boardExtra[y2, x2];              

            if (number1.Item2 == false && number2.Item2 == false && (x1 != x2 || y1 !=y2) && (number1.Item1 != number2.Item1)) // If both numbers are not "Fixed", or they are the same number or location
            {
                int eval = 0;

                if (y1 == y2) // If the column is the same
                {
                    eval -= Evaluatecolumn(y1);
                }
                else
                {
                    eval -= Evaluatecolumn(y1);
                    eval -= Evaluatecolumn(y2);
                }
                if (x1 == x2) // If the row is the same
                {
                    eval -= EvaluateRow(x1);
                }
                else
                {
                    eval -= EvaluateRow(x1);
                    eval -= EvaluateRow(x2);
                }

                Console.WriteLine("Initial eval: " + eval);

                boardExtra[y1, x1] = number2; //Swap
                boardExtra[y2, x2] = number1;
                
                if(y1 == y2) // If the column is the same
                {
                    eval += Evaluatecolumn(y1);
                }
                else
                {
                    eval += Evaluatecolumn(y1);
                    eval += Evaluatecolumn(y2);
                }
                if(x1 == x2) // If the row is the same
                {
                    eval += EvaluateRow(x1);
                }
                else
                {
                    eval += EvaluateRow(x1);
                    eval += EvaluateRow(x2);
                }

                Console.WriteLine("Total eval difference: " + eval);
            }
        }

        public void HillClimb()
        {
            // Choose a random (Nsqrd x Nsqrd) Square
            int ranSquare = random.Next(0, n);

            Console.WriteLine("Choosen square: " + ranSquare);

            //Loop through each number in the square
            for (int x1 = 0; x1 < nSqrd; x1++)
            {
                for (int y1 = 0; y1 < nSqrd; y1++)
                {
                    int xCor1 = ranSquare + x1;
                    int yCor1 = ranSquare + y1;

                    //Do it again

                    for (int x2 = 0; x2 < nSqrd; x2++)
                    {
                        for (int y2 = 0; y2 < nSqrd; y2++)
                        {
                            int xCor2 = ranSquare + x2;
                            int yCor2 = ranSquare + y2;

                            TrySwap(xCor1, yCor1, xCor2, yCor2);                         

                        }
                    }
                }
            }
        }
        
    }
}
