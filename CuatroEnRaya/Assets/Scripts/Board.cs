using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Assets
{
    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }

        internal Tuple(T1 first, T2 second)
        {
            Item1 = first;
            Item2 = second;
        }
    }

    class Board
    {
        public const int FOUR_MULTIPLIER = 100000000;
        public const int THREE_MULTIPLIER = 1000;
        public const int TWO_MULTIPLIER = 50;
        public const int ONE_MULTIPLIER = 20;

        public enum Player { Empty, Red, Yellow };

        public Player[][] elements = new Player[6][];

        public int lastPlayedColumn;
        
        public bool gameFinished { get; private set; }

        public Board()
        {
            reset();
        }

        public void placeToken(int column, Player value)
        {
            // Get first free space from the bottom up for the current column
            int row;
            for (row = 5; row > 0; --row)
            {
                if (elements[row][column] == Player.Empty)
                    break;
            }

            // Write the column
            elements[row][column] = value;

            // Keep track of the last played column
            lastPlayedColumn = column;

            getNextMovements();
        }

        public override string ToString()
        {
            string ret = "";

            for (int row = 0; row < 6; ++row)
            {
                for (int column = 0; column < 7; ++column)
                {
                    switch (elements[row][column])
                    {
                        case Player.Empty:
                            ret += "  ";
                            break;
                        case Player.Red:
                            ret += "# ";
                            break;
                        case Player.Yellow:
                            ret += "o ";
                            break;
                    }
                }

                ret += " \n";
            }

            for (int column = 0; column < 7; ++column)
            {
                ret += column + " ";
            }

            ret += "\n";

            return ret;
        }

        public bool isColumnFilled(int column)
        {
            return elements[0][column] != Player.Empty;
        }

        public bool isPositionFillable (int row, int column)
        {
            // A position is fillable if it's empty and it's either at the bottom of the
            // board (row = 5) OR over a filled position
            return elements[row][column] == Player.Empty &&
                ((row < 5 && elements[row + 1][column] != Player.Empty) || (row == 5));
        }

        public Tuple<int, int> getBestMovement(Player player, int maxDepth, int currentDepth = 0)
        {
            // Get the possible movements for the player
            var movements = getPossibleBoards(player);

            // First of all, check if any of the movements directly leads to victory
            try
            {
                var winningMovement = movements.First(s => s.getWinner() == player);
                return new Tuple<int, int>(winningMovement.evaluate(player), winningMovement.lastPlayedColumn);
            }
            catch (InvalidOperationException) { }
            

            // Now that we're sure there aren't any immediately "winnable" games, let's traverse the tree

            // If we can't go deeper, evaluate current movements
            if (currentDepth == maxDepth)
            {
                // For each possible movement, build a tuple with the score and the movement
                var bestMovement = movements.Select(s => new Tuple<int, int>(s.evaluate(player), s.lastPlayedColumn)).OrderByDescending(s => s.Item1).First();

                // Return the column 
                return bestMovement;
            }

            else
            {
                // Switch the player
                Player nextPlayer = player == Player.Red ? Player.Yellow : Player.Red;

                // Get the best movement of all the possible ones
                var movs = movements.Select(s => s.getBestMovement(nextPlayer, maxDepth, currentDepth + 1)).OrderByDescending(s => s.Item1);

                if (movs.Count() == 0)
                {
                    return new Tuple<int, int>(int.MinValue, -1);
                }

                else
                {
                    return movs.First();
                }
            }

        }

        public void reset ()
        {
            // Initialise the board to 0
            for (int i = 0; i < 6; ++i)
            {
                elements[i] = new Player[7];
                for (int j = 0; j < 7; ++j)
                {
                    elements[i][j] = Player.Empty;
                }
            }

            // Initialise game finished flag
            gameFinished = false;
        }

        public List<Board> getPossibleBoards(Player player)
        {
            List<Board> list = new List<Board>();

            foreach (int i in getNextMovements())
            {
                // Clone and fill the board
                Board b = clone();
                b.placeToken(i, player);

                // Add the board to the list of possible movements
                list.Add(b);
            }

            return list;
        }

        public List<int> getNextMovements()
        {
            List<int> list = new List<int>();

            // One possible movement per column
            for (int i = 0; i < 7; ++i)
            {
                if (!isColumnFilled(i))
                {
                    // Add the column number
                    list.Add(i);
                }
            }

            // If there are no possible movements, game's finished
            if (list.Count() == 0)
            {
                gameFinished = true;
            }

            return list;
        }

        public Player getWinner()
        {
            // To find the winner, we look for groups of 4 equal elements
            Player[] subgroup = new Player[4];


            // First horizontally
            for (int row = 0; row < 6; ++row)
            {
                for (int groupStart = 0; groupStart < 4; ++groupStart)
                {
                    var z = elements[row].Skip(groupStart).Take(4).Distinct().ToArray();

                    if (z.Count() == 1 && z[0] != Player.Empty)
                    {
                        gameFinished = true;
                        return z[0];
                    }
                }
            }

            // Then vertically
            for (int column = 0; column < 7; ++column)
            {
                for (int groupStart = 0; groupStart < 3; ++groupStart)
                {
                    // Skip until desired row, then within the row skip till the desired column
                    var z = elements.Skip(groupStart).Select(s => s.Skip(column).First()).Take(4).Distinct().ToArray();

                    if (z.Count() == 1 && z[0] != Player.Empty)
                    {
                        gameFinished = true;
                        return z[0];
                    }
                }
            }

            // Then diagonally
            for (int row = 0; row < 3; ++row)
            {
                for (int col = 0; col < 4; ++col)
                {
                    if (elements[row][col] != Player.Empty &&
                        elements[row][col] == elements[row + 1][col + 1] &&
                        elements[row][col] == elements[row + 2][col + 2] &&
                        elements[row][col] == elements[row + 3][col + 3])
                    {
                        gameFinished = true;
                        return elements[row][col];
                    }
                }
            }

            for (int row = 3; row < 6; ++row)
            {
                for (int col = 0; col < 4; ++col)
                {
                    if (elements[row][col] != Player.Empty &&
                        elements[row][col] == elements[row - 1][col + 1] &&
                        elements[row][col] == elements[row - 2][col + 2] &&
                        elements[row][col] == elements[row - 3][col + 3])
                    {
                        gameFinished = true;
                        return elements[row][col];
                    }
                }
            }

            return Player.Empty;
        }

        public static Player getAdversary (Player player)
        {
            if (player == Player.Yellow)
            {
                return Player.Red;
            }

            else
            {
                return Player.Yellow;
            }
        }

        public static void prAr (Player [] ar)
        {
            Console.WriteLine(string.Join(", ", ar.Select(v => v.ToString()).ToArray()));
        }

        public int evaluate(Player player)
        {
            int score = 0;

            Player adversary = getAdversary(player);
            Player winner = getWinner();

            Player[] meAndEmpty = { Player.Empty, player };
            Player[] adversaryAndEmpty = { Player.Empty, adversary };

            ///////////////////////////////////////////////////////////////////////////
            // 4-groups (that is, a finished board)
            
            //Console.WriteLine("Evaluating winning situations...");

            if (winner == player)
            {
                score += FOUR_MULTIPLIER;
            }

            else if (winner == adversary)
            {
                score -= FOUR_MULTIPLIER;
            }
                        
            /////////////////////////////////////////////////////////////////////////////
            // 3-element open groups (that's three tokens with a fourth empty one)
            
            //Console.WriteLine("Evaluating horizontal open groups...");

            // First horizontally, go over all rows
            for (int row = 0; row < 6; ++row)
            {

                // Groups of 4 elements, so there are NUM_COLS - 3
                for (int groupStart = 0; groupStart < 4; ++groupStart)
                {

                    var groupIsImmediatelyFillable = Enumerable.Range(groupStart, 4).All(x => isPositionFillable(row, x));

                    // From the current row, take 4 elements starting at groupStart
                    var localElements = elements[row].Skip(groupStart).Take(4);

                    var localDistinctElementsCount = localElements.Distinct().Count();
                    var localPlayerElementsCount = localElements.Count(x => x == player);
                    var localAdversaryElementsCount = localElements.Count(x => x == adversary);
                    var localEmptyElementsCount = localElements.Count(x => x == Player.Empty);

                    if (localDistinctElementsCount == 2 && localEmptyElementsCount > 0)
                    {
                        //Console.WriteLine("Row {0}, Col {1}", row, groupStart);                        
                        //prAr(localElements.ToArray());
                        
                        if (localPlayerElementsCount > 0)
                        {
                            if (localPlayerElementsCount == 1)
                            {
                                score += 2 * ONE_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }
                            else if (localPlayerElementsCount == 2)
                            {
                                score += 2 * TWO_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }

                            else if (localPlayerElementsCount == 3)
                            {
                                score += 2 * THREE_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }
                        }

                        else if (localAdversaryElementsCount > 0)
                        {
                            if (localAdversaryElementsCount == 1)
                            {
                                score -= 4 * ONE_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }

                            else if (localAdversaryElementsCount == 2)
                            {
                                score -= 4 * TWO_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }

                            else if (localAdversaryElementsCount == 3)
                            {
                                score -= 4 * THREE_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }
                        }
                    }                    
                }
            }

            /*

            Console.WriteLine("Evaluating vertical open groups...");

            // Then vertically, go over all columns
            for (int column = 0; column < 7; ++column)
            {
                for (int groupStart = 0; groupStart < 4; ++groupStart)
                {
                    var groupIsImmediatelyFillable = Enumerable.Range(groupStart, 4).All(x => isPositionFillable(x, column));

                    // Skip until desired row, then within the row skip till the desired column
                    var localElements = elements.Skip(groupStart).Select(s => s.Skip(column).First()).Take(4);

                    var localDistinctElementsCount = localElements.Distinct().Count();
                    var localPlayerElementsCount = localElements.Count(x => x == player);
                    var localAdversaryElementsCount = localElements.Count(x => x == adversary);
                    var localEmptyElementsCount = localElements.Count(x => x == Player.Empty);

                    if (localDistinctElementsCount == 2 && localEmptyElementsCount > 0)
                    {
                        if (localPlayerElementsCount > 0)
                        {
                            if (localPlayerElementsCount == 2)
                            {
                                score += 1 * TWO_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }

                            else if (localPlayerElementsCount == 3)
                            {
                                score += 1 * THREE_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }
                        }

                        else if (localAdversaryElementsCount > 0)
                        {
                            if (localAdversaryElementsCount == 2)
                            {
                                score -= 2 * TWO_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }

                            else if (localAdversaryElementsCount == 3)
                            {
                                score -= 2 * THREE_MULTIPLIER * (groupIsImmediatelyFillable ? 2 : 1);
                            }
                        }
                    }                   
                }
            }

            //*/

            /*

            Console.WriteLine("Evaluating diagonal open groups...");

            for (int row = 0; row < 3; ++row)
            {
                for (int col = 0; col < 4; ++col)
                {
                    Player[] subelements = { elements[row][col], elements[row + 1][col + 1], elements[row + 2][col + 2], elements[row + 3][col + 3] };

                    var z = subelements.Distinct().OrderBy(e => e).ToArray();

                    if (z.Count() == 2)
                    {
                        prAr(z);

                        if (z.SequenceEqual(meAndEmpty))
                        {
                            score += (1  * THREE_MULTIPLIER);
                        }
                        else if (z.SequenceEqual(adversaryAndEmpty))
                        {
                            score -= (1 * THREE_MULTIPLIER);
                        }
                    }
                }
            }

            for (int row = 3; row < 6; ++row)
            {
                for (int col = 0; col < 4; ++col)
                {
                    Player[] subelements = { elements[row][col], elements[row - 1][col + 1], elements[row - 2][col + 2], elements[row - 3][col + 3] };

                    var z = subelements.Distinct().OrderBy(e => e).ToArray();

                    if (z.Count() == 2)
                    {
                        prAr(z);

                        if (z.SequenceEqual(meAndEmpty))
                        {
                            score += 1 * THREE_MULTIPLIER;
                        }
                        else if (z.SequenceEqual(adversaryAndEmpty))
                        {
                            score -= 1 * THREE_MULTIPLIER;
                        }
                    }
                }
            }
            //*/

            
            return score;

            /*
            // 2-element groups
            // First horizontally
            for (int row = 0; row < 6; ++row)
            {
                for (int groupStart = 0; groupStart < 5; ++groupStart)
                {
                    var z = elements[row].Skip(groupStart).Take(2).Distinct().ToArray();

                    if (z.Count() == 1 && z[0] != Player.Empty)
                    {
                        score += (z[0] == player ? 20 : -1) * TWO_MULTIPLIER;
                    }
                }
            }

            // Then vertically
            for (int column = 0; column < 7; ++column)
            {
                for (int groupStart = 0; groupStart < 5; ++groupStart)
                {
                    // Skip until desired row, then within the row skip till the desired column
                    var z = elements.Skip(groupStart).Select(s => s.Skip(column).First()).Take(2).Distinct().ToArray();

                    if (z.Count() == 1 && z[0] != Player.Empty)
                    {
                        score += (z[0] == player ? 20 : -1) * TWO_MULTIPLIER;
                    }
                }
            }

            return score;

            //*/
        }

        public Board clone()
        {
            Board c = new Board();
            c.elements = elements.Select(s => s.ToArray()).ToArray();

            return c;
        }
    }
}
