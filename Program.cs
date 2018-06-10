using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace Computationele_Intelligentie_Pi
{
    class Program
    {
        static void Main(string[] args)
        {
            // Without exhaustive testing, best result on testing one set of parameters:
            Console.WriteLine("Welcome to a sudokuSolver by: Bram Kreuger (5990653) & Pieter Barkema (5979412)");
            Sudoku s = new Sudoku(false, 20, 50, 25, 8);
            Console.WriteLine("Beste resultaat: " + s.simplythebest);

            //// Exhaustive testing on standard sudoku: var 1 (after overrideBoard=false): amount of random restarts; var 2: amount of hillclimbs, var 3: no improvement threshold;
            //// var 4: amount of random swaps after no improvement threshold is reached.
            //int[] rand_re = new int[3] { 1, 20, 100 };//, 200, 500 };
            //int[] hillclimbs = new int[3] { 1, 10, 20 };//, 100, 500 };
            //int[] no_improv = new int[2] { 10, 25 };//, 250, 2500 };
            //int[] rand_swap = new int[3] { 3, 5, 8 };//, 15 };
            //List<Tuple<string, int, double>> results = new List<Tuple<string, int, double>> { };
            //Stopwatch timer = new Stopwatch();

            //foreach (int re in rand_re)
            //{
            //    foreach (int hill in hillclimbs)
            //    {
            //        foreach (int noimp in no_improv)
            //        {
            //            foreach (int swap in rand_swap)
            //            {
            //                timer.Reset();
            //                timer.Start();
            //                Sudoku s = new Sudoku(false, re, hill, noimp, swap);
            //                timer.Stop();
            //                string curr_params = "Random restart: " + re.ToString() + " hill climbs: " + hill.ToString() + " no improves: " + noimp.ToString() + " random swaps: " + swap.ToString();
            //                results.Add(new Tuple<string, int, double>(curr_params, s.simplythebest, timer.ElapsedMilliseconds));
            //            }
            //        }
            //    }
            //}
            //results.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            ////foreach (Tuple<string, int, double> t in results)
            ////{

            ////}
            //Console.WriteLine("Best result is: " + results[0].Item1 + ", with result: " + results[0].Item2 + " time in ms: " + results[0].Item3);


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
        public int simplythebest; // The best evaluation during all intervals of a sudoku under given parameters
        Random random;

        /// <summary>
        /// Constructor for the sudoku. Use the bool input to override the test sudoku. True = override, False = keep the old one (just for testing)
        /// </summary>
        public Sudoku(bool overrideBoard, int rand_re, int x_climbs, int no_improv_thres, int x_randswaps)
        {   random = new Random(); // Seed a random
                                   
            // For reading in sudoku's from standard or from sudoku_puzzels.txt -- Not functional --
            //int[,] originalboard; // Provide algorithm with original sudoku for random restart

            //// If overrideBoard: read in a sudoku
            //if (overrideBoard)
            //{
            //    Initialize();
            //    originalboard = board;
            //    PrintBoard(true);
                
            //}
            //// Else: use test sudoku
            //else
            //{

            //}

            for (int i = 0; i < rand_re; i++)
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
                // Method for reading in sudoku's rather than using the standard one.
                n = board.GetLength(0); // Width/length of board    
                nSqrd = (int)Math.Sqrt(n); // Width/length of cube
                boardExtra = new Number[n, n]; // The mutable and filled in version of sudoku.

                //PrintBoard(true);

                FillBoard();

                //PrintBoard(false);

                //Console.WriteLine("Press any key to start HillClimbing..."); // not for test mode

                // Get the best evaluation from all iterations of a run for testing purposes
                simplythebest = Int32.MaxValue;
                HillClimb(x_climbs, no_improv_thres, x_randswaps);
                if (FullEvaluation() < simplythebest)
                {
                    simplythebest = FullEvaluation();
                }


            }

            //PrintBoard(false);
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

                            if (board[yCor, xCor] != 0)
                            {
                                numbers.Remove(board[yCor, xCor]);
                            }

                        }
                    }

                    // Not in use: Second time trough the square, now fill the 0's with the numbers from the list, which becomes a stack :)
                    //              Stack<int> numStack = new Stack<int>(numbers);
                    // Currently: Randomize the way the sudoku is filled in for more exploration in the search domain.
                    for (int x = 0; x < nSqrd; x++)
                    {
                        for (int y = 0; y < nSqrd; y++)
                        {
                            int xCor = xSq + x;
                            int yCor = ySq + y;

                            if (board[yCor, xCor] == 0)
                            {
                                int number = numbers[random.Next(0,numbers.Count())];
                                numbers.Remove(number);
                                board[yCor, xCor] = number;
                                 
                                boardExtra[xCor, yCor] = new Number(number, xCor, yCor, false); // Is the cell value not given? Than fixed = FALSE
                            }
                            else
                            {
                                boardExtra[xCor, yCor] = new Number(board[yCor, xCor], xCor, yCor, true); // Is the cell value given? Than fixed = TRUE
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
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        Console.Write(board[y,x] + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("- - - - - - - - -");
            }
            else
            {
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
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
            HashSet<int> numbers = new HashSet<int>();
            for (int x = 0; x < n; x++)
            {
                numbers.Add(boardExtra[x, rowNumber].value);
            }
            //Console.WriteLine("Row " + rowNumber + " evaluated: " + (n-numbers.Count));

            // Return the amount of mistakes made in a given row.
            return n - numbers.Count;
        }

        /// <summary>
        /// Prints (Later maybe return) a value which indicates how good a column is. N being the worst, 0 being the best.
        /// </summary>
        /// <param name="columnNumber">Which column to evaluate</param>
        public int Evaluatecolumn(int columnNumber)
        {
            HashSet<int> numbers = new HashSet<int>();
            for (int y = 0; y < n; y++)
            {
                numbers.Add(boardExtra[columnNumber,y].value);
            }

            //Console.WriteLine("Collumn " + columnNumber + " evaluated: " + (n-numbers.Count));
            // Return the amount of mistakes made in a given column.
            return n - numbers.Count;
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
            Number number1 = boardExtra[x1, y1];
            Number number2 = boardExtra[x2, y2];

            //Console.WriteLine("number 1: ", x1, ",", y1, " number 2: ", x2, ",", y2);
            if (number1.anchored == false && number2.anchored == false) // If both numbers are not "Fixed", or they are the same location
            {
                if (x1 != x2 && y1 != y2)
                {
                    int eval = 0;

                    //Console.WriteLine("Initial eval: " + eval);

                    boardExtra[x1, y1] = number2; //Swap
                    boardExtra[x2, y2] = number1;

                    if (y1 == y2) // If the column is the same
                    {
                        eval += Evaluatecolumn(x1);
                    }
                    else
                    {
                        eval += Evaluatecolumn(x1);
                        eval += Evaluatecolumn(x2);
                    }
                    if (x1 == x2) // If the row is the same
                    {
                        eval += EvaluateRow(y1);
                    }
                    else
                    {
                        eval += EvaluateRow(y1);
                        eval += EvaluateRow(y2);
                    }

                    //Console.WriteLine("Total eval difference: " + eval);

                    boardExtra[x1, y1] = number1; //Swap back
                    boardExtra[x2, y2] = number2;

                    // return amount of mistakes in eval for a given swap
                    return new Tuple<int, Number, Number>(eval, number1, number2);
                }
            }

            return new Tuple<int, Number, Number>(Int32.MaxValue, null, null);
        }

        /// <summary>
        /// Runs the Hillclimbing algorithm
        /// </summary>
        public void HillClimb(int x_climbs, int no_improv_thres, int x_randswaps)
        {
            //Console.WriteLine("Initial evaluation: " + FullEvaluation());

            // Threshold for implementing random walk (using x random swaps)
            int noImprovement = 0;
           
            //Run the algorithm x times
            for (int i = 0; i < x_climbs; i++)
            {
                int ranSquareX = 0;
                int ranSquareY = 0;

                // Choose a random (Nsqrd x Nsqrd) Square
                    
                    ranSquareX = random.Next(0, nSqrd) * nSqrd;
                    ranSquareY = random.Next(0, nSqrd) * nSqrd;

                Tuple<int, Number, Number> bestResult = new Tuple<int, Number, Number>(Int32.MaxValue-1, null, null);
                Tuple<int, Number, Number> result = new Tuple<int, Number, Number>(Int32.MaxValue, null, null);

                //Console.WriteLine("Chosen square: " + ranSquareX / nSqrd + " : " + ranSquareY / nSqrd);

                //Loop through each number in the square
                for (int x1 = 0; x1 < nSqrd; x1++)
                {
                    for (int y1 = 0; y1 < nSqrd; y1++)
                    {
                        int xCor1 = ranSquareX + x1;
                        int yCor1 = ranSquareY + y1;

                        //Console.WriteLine(xCor1 + " : " + yCor1);

                        //Do it again

                        for (int x2 = 0; x2 < nSqrd; x2++)
                        {
                            for (int y2 = 0; y2 < nSqrd; y2++)
                            {
                                int xCor2 = ranSquareX + x2;
                                int yCor2 = ranSquareY + y2;

                                result = TrySwap(xCor1, yCor1, xCor2, yCor2);

                                //if (result != null)
                                //{
                                if (result.Item1 < bestResult.Item1)
                                {
                                   
                                    bestResult = result;
                                    //Console.WriteLine("Best swap: " + bestResult.Item2.x + " : " + bestResult.Item2.y + " with number: " + bestResult.Item2.value + " and " + bestResult.Item3.x + " : " + bestResult.Item3.y + " with value: " + bestResult.Item3.value + " with score: " + bestResult.Item1);
                                }
                                //}
                            }
                        }
                    }
                }

                if (bestResult.Item2 != null)
                {
                    //Console.WriteLine(bestResult.Item2);
                    //Console.WriteLine("Best swap: " + bestResult.Item2.x + " : " + bestResult.Item2.y + " with number: " + bestResult.Item2.value + " and " + bestResult.Item3.x + " : " + bestResult.Item3.y + " with value: " + bestResult.Item3.value + " with score: " + bestResult.Item1);

                    // Cells used in best swap:
                    Number pos1 = bestResult.Item2;
                    Number pos2 = bestResult.Item3;

                    int x1 = pos1.x;
                    int y1 = pos1.y;
                    int x2 = pos2.x;
                    int y2 = pos2.y; //Verkeerd om vanwege opslaan

                    // Ik vermoedde dat je hier, bij boardextra, bedoelde dat de eerste plek y coordinaat is, en tweede x?
                    // Op andere plekken gebeurt dat niet, dus dit snap ik niet.
                    var n1 = boardExtra[x1, y1];
                    var n2 = boardExtra[x2, y2];

                    // Hier zit het probleem: bij een nieuw nummer aanmaken moet je weer eerst x en daarna y.
                    // Ik heb de coordinates in new Number omgewisseld, zodat x als eerste coordinaat gegeven wordt en y als tweede.
                    boardExtra[x1, y1] = new Number(n2.value, x1, y1, n2.anchored);
                    boardExtra[x2, y2] = new Number(n1.value, x2, y2, n1.anchored);

                    //PrintBoard(false);

                    //int eval = FullEvaluation();
                    //Console.WriteLine("==== Iteration: " + i + " eval: " + eval + " imp: " + bestResult.Item1 + " ====");
                }

                // Use randomwalk if no improvement within given threshold
                else
                {
                    //Console.WriteLine("Iteration: " + i + ". No swap.");
                    noImprovement++;
                    if (noImprovement > no_improv_thres)
                    {
                        noImprovement = 0;
                        RandomSwaps(x_randswaps);
                    }
                }
                //PrintBoard(false);
                //Console.ReadLine();
            }
        }

        public void RandomSwaps(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                // Pick two random unfixed numbers and swap them.
                Number n1 = new Number(0, 0, 0, true);
                Number n2 = new Number(0, 0, 0, true);

                int ranNumberX1 = 0;
                int ranNumberY1 = 0;
                int ranNumberX2 = 0;
                int ranNumberY2 = 0;

                while (n1.anchored == true || n2.anchored == true)
                {
                    // Choose a random (Nsqrd x Nsqrd) Square
                    int ranSquareX = random.Next(0, nSqrd) * nSqrd;
                    int ranSquareY = random.Next(0, nSqrd) * nSqrd;

                    ranNumberX1 = ranSquareX + random.Next(0, nSqrd);
                    ranNumberY1 = ranSquareY + random.Next(0, nSqrd);

                    ranNumberX2 = ranSquareX + random.Next(0, nSqrd);
                    ranNumberY2 = ranSquareY + random.Next(0, nSqrd);

                    //Console.WriteLine("n1: " + ranNumberX2 + " : " + ranNumberY2 + " n2: " + ranNumberX1 + " : " + ranNumberY1);

                    n1 = boardExtra[ranNumberX1, ranNumberY1];
                    n2 = boardExtra[ranNumberX2, ranNumberY2];
                }
                boardExtra[ranNumberX1, ranNumberY1] = n2;
                boardExtra[ranNumberX2, ranNumberY2] = n1;
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
                    sudoku[x, y] = (charlist[x])[y];
                }
            }
            board = sudoku;
        }

        public int FullEvaluation()
        {

            // Evaluate full board by adding all mistakes of all columns and rows;
            // One number can cause a mistake in its column and row.
            int eval = 0;

            for (int r = 0; r < n; r++)
            {
                eval += EvaluateRow(r);
            }
            for (int c = 0; c < n; c++)
            {
                eval += Evaluatecolumn(c);
            }

            //Console.WriteLine("full eval: " + eval);
            return eval;
        }
    }
}