using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Computationele_Intelligentie_Pi
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to a sudokuSolver by: Bram Kreuger (5990653) & Pieter Barkema (...)");
            Sudoku s = new Sudoku(false);                 
            Console.ReadKey();
        }
    }

    class Number
    {
        public int value;
        public int x;
        public int y;
        public bool anchored;

        public Number(int value, int x, int y, bool anchored)
        {
            this.value = value;
            this.x = x;
            this.y = y;
            this.anchored = anchored;
        }
    }

    class Sudoku
    {
        public int[,] board; //The 2D array representing the board/field with all the numbers
        public Number[,] boardExtra; // The same 2D array, but now with tuples. Int meaning the number, The bool meaning wether the number is fixed = TRUE or not-fixed = FALSE
        int n; // The size of the sudoku, we will use: 9, 16 or 25
        int nSqrd; // The square root of n: 3, 4 or 5
        Random random;

        /// <summary>
        /// Constructor for the sudoku. Use the bool input to override the test sudoku. True = override, False = keep the old one (just for testing)
        /// </summary>
        public Sudoku(bool overrideBoard)
        {
            if (overrideBoard)
            {
                Initialize();
            }
            else
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
            }

            n = board.GetLength(0);

            boardExtra = new Number[n, n];

            nSqrd = (int)Math.Sqrt(n);

            random = new Random();

            //PrintBoard(true);

            FillBoard();

            PrintBoard(false);

            //Console.WriteLine("Press any key to start HillClimbing...");

            //Console.ReadKey();

            //Console.WriteLine(" ");

            HillClimb();

            PrintBoard(false);
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
                                boardExtra[xCor, yCor] = new Number(number, xCor, yCor, false); // Is the number "new"? Than fixed = FALSE
                            }
                            else
                            {
                                boardExtra[xCor, yCor] = new Number(board[xCor, yCor], xCor, yCor, false); // Is the number "old"? Than fixed = TRUE
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method for printing the whole board. Choose bool true to print original board (DEBUG ONLY)   
        /// </summary>
        /// /// <param name="printOriginal">Choose True to print original board!</param>
        public void PrintBoard(bool printOriginal)
        {
            if (printOriginal)
            {
                for (int x = 0; x < n; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        Console.Write(board[x, y] + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("- - - - - - - - -");
            }
            else
            {
                for (int x = 0; x < n; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        Console.Write(boardExtra[x, y].value + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("- - - - - - - - -");
            }
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
                if (numbers.Contains(boardExtra[rowNumber, y].value))
                {
                    numbers.Remove(boardExtra[rowNumber, y].value);
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
                if (numbers.Contains(boardExtra[x, columnNumber].value))
                {
                    numbers.Remove(boardExtra[x, columnNumber].value);
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
        public Tuple<int, Number, Number> TrySwap(int x1, int y1, int x2, int y2)
        {
            Number number1 = boardExtra[y1, x1];
            Number number2 = boardExtra[y2, x2];


            if (number1.anchored == false && number2.anchored == false && (x1 != x2 || y1 != y2)) // If both numbers are not "Fixed", or they are the same location
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

                //Console.WriteLine("Initial eval: " + eval);

                boardExtra[y1, x1] = number2; //Swap
                boardExtra[y2, x2] = number1;

                if (y1 == y2) // If the column is the same
                {
                    eval += Evaluatecolumn(y1);
                }
                else
                {
                    eval += Evaluatecolumn(y1);
                    eval += Evaluatecolumn(y2);
                }
                if (x1 == x2) // If the row is the same
                {
                    eval += EvaluateRow(x1);
                }
                else
                {
                    eval += EvaluateRow(x1);
                    eval += EvaluateRow(x2);
                }

                //Console.WriteLine("Total eval difference: " + eval);

                boardExtra[y1, x1] = number1; //Swap back
                boardExtra[y2, x2] = number2;

                return new Tuple<int, Number, Number>(eval, number1, number2);
            }

            return null;
        }

        /// <summary>
        /// Runs the Hillclimbing algorithm
        /// </summary>
        public void HillClimb()
        {
            Console.WriteLine("Initial evaluation: " + FullEvaluation());

            //Run the algorithm x times
            for (int i = 0; i < 1; i++)
            {
                // Choose a random (Nsqrd x Nsqrd) Square
                int ranSquareX = random.Next(0, nSqrd);
                int ranSquareY = random.Next(0, nSqrd);

                // Obscure way to store data: Item1 = evaluationValue, Item2 = <x1, y2>, Item3 = <x2, y2>
                Tuple<int, Number, Number> bestResult = new Tuple<int, Number, Number>(0, null, null);
                Tuple<int, Number, Number> result = new Tuple<int, Number, Number>(0, null, null);
                //Tuple<int, Tuple<int, int>, Tuple<int, int>> bestResult = new Tuple<int, Tuple<int, int>, Tuple<int, int>>(0, new Tuple<int, int>(0, 0), new Tuple<int, int>(0, 0));
                //Tuple<int, Tuple<int, int>, Tuple<int, int>> result = new Tuple<int, Tuple<int, int>, Tuple<int, int>>(0, new Tuple<int, int>(0, 0), new Tuple<int, int>(0, 0));

                Console.WriteLine("Choosen square: " + ranSquareX + " : " + ranSquareY);

                //Loop through each number in the square
                for (int x1 = 0; x1 < nSqrd; x1++)
                {
                    for (int y1 = 0; y1 < nSqrd; y1++)
                    {
                        int xCor1 = ranSquareX + x1;
                        int yCor1 = ranSquareY + y1;

                        //Do it again

                        for (int x2 = 0; x2 < nSqrd; x2++)
                        {
                            for (int y2 = 0; y2 < nSqrd; y2++)
                            {
                                int xCor2 = ranSquareX + x2;
                                int yCor2 = ranSquareY + y2;

                                result = TrySwap(xCor1, yCor1, xCor2, yCor2);

                                if (result != null)
                                {
                                    if (bestResult.Item1 < result.Item1)
                                    {
                                        bestResult = result;
                                    }
                                }
                            }
                        }
                    }
                }

                if (bestResult.Item2 != null)
                {
                    Console.WriteLine("Best swap: " + bestResult.Item2.x + " : " + bestResult.Item2.y + " and " + bestResult.Item3.x + " : " + bestResult.Item3.y + " with score: " + bestResult.Item1);

                    var pos1 = bestResult.Item2;
                    var pos2 = bestResult.Item3;
                    var num1 = boardExtra[pos1.x, pos1.y];
                    var num2 = boardExtra[pos2.x, pos2.y];

                    boardExtra[pos2.x, pos2.y] = num1; //Swap
                    boardExtra[pos1.x, pos1.y] = num2;

                    //PrintBoard(false);

                    int eval = FullEvaluation();

                    //Console.WriteLine("Iteration: " + i + " eval: " + eval);
                }
                else
                {
                    Console.WriteLine("Iteration: " + i + ". No swap.");
                }
            }
        }

        /// <summary>
        /// Loads the file containing the sudokus and take the one matching the sudokuNumber. Make sure the text file is in:
        /// \Computationele Intelligentie Pi\Computationele Intelligentie Pi\bin\Debug\netcoreapp2.0
        /// Or somthing similair.
        /// </summary>        
        public void Initialize()
        {
            string textFile = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "sudoku_puzzels.txt"));

            //Each "Seperator" is the number of the line where there is text, not numbers, these lines seperate the sudoku's.
            List<int> seperators = new List<int>();

            using (StringReader reader = new StringReader(textFile))
            {
                string line = string.Empty;
                int counter = 0;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (Regex.IsMatch(line, @"[a-zA-z]"))
                        {
                            seperators.Add(counter);
                        }
                    }

                    counter++;

                } while (line != null);
                seperators.Add(counter - 1); //Add last line for processing 
            }
            // So now you've got a list of numbers containing the beginnings and ends of the sudoku's and the loaded textfile
            AskInput(seperators, textFile);

        }

        /// <summary>
        /// Ask for input from the user. This is an self-contained method so it can be called recursively.
        /// </summary>
        public void AskInput(List<int> seperators, string textFile)
        {            
            Console.WriteLine("Choose a sudoku-puzzel from: 0 to: " + (seperators.Count - 1) + " . By typing the corrosponding number");
            string sudokuInput = Console.ReadLine();
            if (int.TryParse(sudokuInput, out int res))
            {
                int sudokuNumber = int.Parse(sudokuInput);

                if (sudokuNumber < seperators.Count && sudokuNumber > -1)
                {
                    LoadSudoku(sudokuNumber, seperators, textFile);
                }
                else
                {
                    Console.WriteLine("Given sudoku-number is out of range.");
                    AskInput(seperators, textFile);
                }
            }
            else
            {
                Console.WriteLine("Please insert a number, not an string...");
                AskInput(seperators, textFile);
            }
        }

        /// <summary>
        /// Converts textfile to 2D array.
        /// </summary>
        public void LoadSudoku(int sudokuNumber, List<int> seperators, string textFile)
        {
            List<int[]> charlist = new List<int[]>();
            int[,] sudoku = null;

            List<string> allLines = (from l in textFile.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                            select l).ToList(); // Splits the lines on the newlines

            int size = seperators[sudokuNumber + 1] - (seperators[sudokuNumber] + 1);
            sudoku = new int[size, size];
            for (int i = seperators[sudokuNumber] + 1; i < seperators[sudokuNumber + 1]; i++)
            {
                charlist.Add(Array.ConvertAll(allLines[i].ToCharArray(), c => (int)Char.GetNumericValue(c)));
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    sudoku[y, x] = (charlist[y])[x];
                }
            }
            board = sudoku;
        }

        public int FullEvaluation()
        {
            int eval = n * n;

            for (int r = 0; r < n; r++)
            {
                eval -= EvaluateRow(r);
            }
            for (int c = 0; c < n; c++)
            {
                eval -= Evaluatecolumn(c);
            }

            return eval;
        }
    }
}
