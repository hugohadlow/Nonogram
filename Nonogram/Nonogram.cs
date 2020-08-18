using System;
using System.Collections.Generic;
using System.Linq;

namespace Nonogram
{
    public class Nonogram
    {
        public byte[,] Solve(byte[,] input, int[][] xClues, int[][] yClues) //0=white, 1=black
        {
            int xDimension = input.GetLength(0);
            int yDimension = input.GetLength(1);
            if (xClues.Length != xDimension) throw new ArgumentException("Wrong number of X clues");
            if (yClues.Length != yDimension) throw new ArgumentException("Wrong number of Y clues");

            //Generate possibilities
            List<List<byte[]>> xPossibilities = Possibilities(xClues, yDimension);
            List < List<byte[]>> yPossibilities = Possibilities(yClues, xDimension);

            var output = (byte[,])input.Clone();
            for (int i = 0; i < xDimension; i++)
                for (int j = 0; j < yDimension; j++)
                    if (output[i, j] == 0) output[i, j] = 2; //2 = unknown

            bool[] xSolved = new bool[xDimension];
            bool[] ySolved = new bool[yDimension];

            //Loop
            while (xSolved.Contains(false) || ySolved.Contains(false))
            {
                //For each column
                for(int i = 0; i < xDimension; i++)
                {
                    var column = GetColumn(output, i);
                    (var result, bool solved) = Prune(column, xPossibilities[i]);
                    xSolved[i] = solved;
                    InsertColumn(output, result, i);
                }

                //For each row
                for (int j = 0; j < yDimension; j++)
                {
                    var row = GetRow(output, j);
                    (var result, bool solved) = Prune(row, yPossibilities[j]);
                    ySolved[j] = solved;
                    InsertRow(output, result, j);
                }
            }

            return output;
        }

        byte[] GetColumn(byte[,] array, int x)
        {
            var length = array.GetLength(1);
            var result = new byte[length];
            for (int i = 0; i < length; i++)
                result[i] = array[x, i];
            return result;
        }

        byte[] GetRow(byte[,] array, int y)
        {
            var length = array.GetLength(0);
            var result = new byte[length];
            for (int i = 0; i < length; i++)
                result[i] = array[i, y];
            return result;
        }

        private void InsertColumn(byte[,] output, byte[] input, int x)
        {
            var length = output.GetLength(1);
            for (int i = 0; i < length; i++)
                output[x, i] = input[i];
        }

        private void InsertRow(byte[,] output, byte[] input, int y)
        {
            var length = output.GetLength(0);
            for (int i = 0; i < length; i++)
                output[i, y] = input[i];
        }


        public List<List<byte[]>> Possibilities(int[][] clues, int dimension)
        {
            var result = new List<List<byte[]>>();
            for (int i = 0; i < clues.Length; i++)
            {
                result.Add(Possibilities(clues[i], dimension));
            }
            return result;
        }

        public List<byte[]> Possibilities(int[] clues, int dimension)
        {
            var cluesCount = clues.Length;
            var blacks = clues.Sum();
            var buckets = cluesCount + 1;
            var whites = dimension - blacks;
            whites = whites - (buckets - 2); //Middle buckets must contain at least 1 white.

            var combinations = Combinations(buckets, whites);

            var possibilities = new List<byte[]>();
            foreach (var combination in combinations)
            {
                for (int i = 1; i < buckets - 1; i++) combination[i]++; //Middle buckets must contain at least 1 white.
                var possibility = new byte[dimension];
                int index = 0;
                for (int b = 0; b < cluesCount; b++)
                {
                    index += combination[b];
                    var stop = index + clues[b];
                    for (; index < stop; index++)
                        possibility[index] = 1;
                }
                //Final bucket is zeros

                possibilities.Add(possibility);
            }
            return possibilities;
        }

        public (byte[], bool) Prune(byte[] input, List<byte[]> possibilities)
        {
            byte[] output = (byte[])input.Clone();
            var inputLength = input.Length;

            //Remove possibilities incompatible with input
            for (int p = possibilities.Count - 1; p >= 0; p--)
            {
                var possibility = possibilities[p];
                bool compatible = true;
                for(int i = 0; i < inputLength; i++)
                {
                    if (input[i] == 0 && possibility[i] != 0) 
                    {
                        compatible = false; 
                        break;
                    }
                    if (input[i] == 1 && possibility[i] != 1)
                    {
                        compatible = false;
                        break;
                    }
                }
                if (!compatible) possibilities.RemoveAt(p);
            }

            //Unify remaining possibilities
            int[] unified = new int[inputLength];
            for (int i = 0; i < inputLength; i++)
            {
                bool black = false;
                bool white = false;
                foreach (var possibility in possibilities)
                {
                    if (possibility[i] == 1) black = true;
                    if (possibility[i] == 0) white = true;
                    if (black && white) break;
                }

                //Merge into output
                if (black && white) continue;
                else if (black) output[i] = 1;
                else if (white) output[i] = 0;
            }

            //Return
            bool solved = !output.Any(x => x == 2);
            return (output, solved);
        }

        public List<int[]> Combinations(int buckets, int pebbles)
        {
            return Combinations(new List<int[]>() { new int[0] }, buckets, pebbles);
        }

        private List<int[]> Combinations(List<int[]> combinations, 
            int buckets, int pebbles)
        {
            var newCombinations = new List<int[]>();

            if (buckets == 1) 
            {
                foreach (var combination in combinations)
                {
                    var newCombination = new int[combination.Length + 1];
                    for (int i = 0; i < combination.Length; i++) newCombination[i] = combination[i];
                    newCombination[combination.Length] = pebbles;
                    newCombinations.Add(newCombination);
                }
                return newCombinations;
            }
            else 
            {
                for (int p = 0; p <= pebbles; p++)
                {
                    var tempCombinations = new List<int[]>();

                    foreach (var combination in combinations)
                    {
                        
                        var newCombination = new int[combination.Length + 1];
                        for (int i = 0; i < combination.Length; i++) newCombination[i] = combination[i];

                        newCombination[combination.Length] = p;
                        tempCombinations.Add(newCombination);
                    }
                    newCombinations.AddRange(Combinations(tempCombinations, buckets - 1, pebbles - p));
                }
                return newCombinations;
            }
        }
    }
}
