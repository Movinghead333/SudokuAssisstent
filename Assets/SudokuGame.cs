using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class SudokuGame
{
    private int?[,] _fields;

    /// <summary>
    /// Creates an empty Sudoku game, where all number fields are set to null.
    /// </summary>
    public SudokuGame()
    {
        _fields = new int?[9, 9];

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                _fields[x, y] = null;
            }
        }
    }

    /// <summary>
    /// Get the current state of the number fields.
    /// </summary>
    /// <returns></returns>
    public int?[,] Fields() { return _fields; }

    /// <summary>
    /// Overrides the contents of the number fields. Expects a int?[9, 9] sized array.
    /// </summary>
    /// <param name="newFields"></param>
    public void SetFields(int?[,] newFields)
    {
        if (newFields.Rank != 2 || newFields.GetLength(0) != 9 || newFields.GetLength(1) != 9)
        {
            throw new ArgumentException("The format of the passed fields array is invalid.");
        }

        int?[,] oldFiels = _fields;
        _fields = newFields;

        bool newFieldsValid = true;

        for (int fx = 0; fx < 9; fx+=3)
        {
            for (int fy = 0; fy < 9; fy+=3)
            {
                newFieldsValid &= IsNumberSquareValid(fx, fy);
            }
        }

        for (int y = 0; y < 9; y++)
        {
            newFieldsValid &= IsNumberColumnValid(y);
        }

        for (int x = 0;x < 9; x++)
        {
            newFieldsValid &= IsNumberColumnValid(x);
        }

        if (!newFieldsValid)
        {
            _fields = oldFiels;
            throw new ArgumentException("The input newFields does not represent a valid Sudoku game.");
        }
    }

    /// <summary>
    /// Tries to set a number field if the modification is valid and returns true if the operations was successful. 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public bool SetNumberField(int x, int y, int? number)
    {
        bool inputNumberValid = number == null || (number >= 1 && number <= 9);

        if (inputNumberValid)
        {
            int? oldValue = _fields[x, y];
            _fields[x, y] = number;

            bool modificationValid = IsNumberSquareValid(x, y) && IsNumberRowValid(y) && IsNumberColumnValid(x);

            // If the modification was not valid, revert the changes and return false
            if (!modificationValid)
            {
                _fields[x, y] = oldValue;
                return false;
            }
        }

        return inputNumberValid;
    }

    /// <summary>
    /// Returns true if all number fields have a non-null valiu.
    /// </summary>
    /// <returns></returns>
    public bool IsFinished()
    {
        return _fields.Cast<int?>().All(val => val.HasValue);
    }

    public (HashSet<int>[,], Dictionary<(int, int), int>) GeneratePotentialNumbersForFields()
    {
        HashSet<int>[,] potentialNumbersForFields = new HashSet<int>[9, 9];
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                potentialNumbersForFields[x, y] = Enumerable.Range(1, 9).ToHashSet();
            }
        }

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                if (_fields[x, y] == null)
                {
                    continue;
                }

                int number = _fields[x, y].Value;

                // Since this field is already occupied, there are no potential numbers left
                potentialNumbersForFields[x, y] = new HashSet<int>();

                for (int i = 0; i < 9; i++)
                {
                    potentialNumbersForFields[i, y].Remove(number);
                    potentialNumbersForFields[x, i].Remove(number);
                }

                int squareXOffset = (x / 3) * 3;
                int squareYOffset = (y / 3) * 3;

                for (int fx = squareXOffset; fx < squareXOffset + 3; fx++)
                {
                    for (int fy = squareYOffset; fy < squareYOffset + 3; fy++)
                    {
                        potentialNumbersForFields[fx, fy].Remove(number);
                    }
                }
            }
        }

        // Testing

        // For each column find all numbers that only occur in one potential numbers set
        // Then find those number fields and set the potential numbers set to the only remaining option
        Dictionary<(int, int), int> nextMovesDict = new Dictionary<(int, int), int>();

        List<List<(int, int)>> indexLists = new List<List<(int, int)>>();

        for (int i = 0; i < 9; i++)
        {
            indexLists.Add(GetSquareIndexSet(i));
            indexLists.Add(GetRowIndexList(i));
            indexLists.Add(GetColumnIndexList(i));
        }

        foreach (var indexList in indexLists)
        {
            int[] occurences = new int[9];
            Array.Fill(occurences, 0);

            foreach (var coordinatePair in indexList)
            {
                foreach (int number in potentialNumbersForFields[coordinatePair.Item1, coordinatePair.Item2])
                {
                    occurences[number - 1]++;
                }
            }

            HashSet<int> nextMoveNumbers = new HashSet<int>();
            for (int i = 0; i < occurences.Length; i++)
            {
                if (occurences[i] == 1)
                {
                    nextMoveNumbers.Add(i + 1);
                }
            }

            foreach (var coordinatePair in indexList)
            {
                HashSet<int> intersection = potentialNumbersForFields[coordinatePair.Item1, coordinatePair.Item2].Intersect(nextMoveNumbers).ToHashSet();

                if (intersection.Count == 1)
                {
                    nextMovesDict[coordinatePair] = intersection.ToList()[0];
                }
            }
        }

        foreach (var nextMoveEntry in nextMovesDict)
        {
            Console.WriteLine($"{nextMoveEntry.Key.Item1}, {nextMoveEntry.Key.Item1}: {nextMoveEntry.Value}");
            potentialNumbersForFields[nextMoveEntry.Key.Item1, nextMoveEntry.Key.Item2] = new HashSet<int> { nextMoveEntry.Value };
        }

        // Testing end

        return (potentialNumbersForFields, nextMovesDict);
}

    /// <summary>
    /// Check if the square where the number field x,y lies in is valid.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsNumberSquareValid(int x, int y)
    {
        List<int> numbersList = new List<int>();

        int squareXOffset = (x / 3) * 3;
        int squareYOffset = (y / 3) * 3;

        for (int fx = squareXOffset; fx < squareXOffset + 3; fx++)
        {
            for (int fy = squareYOffset; fy < squareYOffset + 3; fy++)
            {
                if (_fields[fx, fy] == null)
                {
                    continue;
                }

                numbersList.Add(_fields[fx, fy].Value);
            }
        }

        return numbersList.Count == numbersList.ToHashSet().Count;
    }

    /// <summary>
    /// Check if all number fields have unique numbers within row y.
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsNumberRowValid(int y)
    {
        List<int> numbersList = new List<int>();

        for (int fx = 0; fx < 9; fx++)
        {
            if (_fields[fx, y] == null)
            {
                continue;
            }

            numbersList.Add(_fields[fx, y].Value);
        }

        return numbersList.Count == numbersList.ToHashSet().Count;
    }

    /// <summary>
    /// Check if all number fields have unique numbers within column x.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private bool IsNumberColumnValid(int x)
    {
        List<int> numbersList = new List<int>();

        for (int fy = 0; fy < 9; fy++)
        {
            if (_fields[x, fy] == null)
            {
                continue;
            }

            numbersList.Add(_fields[x, fy].Value);
        }

        return numbersList.Count == numbersList.ToHashSet().Count;
    }

    /// <summary>
    /// 6 7 8
    /// 3 4 5
    /// 0 1 2
    /// </summary>
    /// <param name="squareIndex"></param>
    /// <returns></returns>
    private List<(int, int)> GetSquareIndexSet(int squareIndex)
    {
        List<(int, int)> squareIndexList = new List<(int, int)>(squareIndex);
        int squareXOffset = (squareIndex % 3) * 3;
        int squareYOffset = (squareIndex / 3) * 3;

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                squareIndexList.Add((squareXOffset + x, squareYOffset + y));
            }
        }

        return squareIndexList;
    }

    private List<(int, int)> GetRowIndexList(int rowIndex)
    {
        List<(int, int)> rowIndexList = new List<(int, int)>();

        for (int x = 0; x < 9; x++)
        {
            rowIndexList.Add((x, rowIndex));
        }

        return rowIndexList;
    }

    private List<(int, int)> GetColumnIndexList(int columnIndex)
    {
        List<(int, int)> columnIndexList = new List<(int, int)>();

        for (int y = 0; y < 9; y++)
        {
            columnIndexList.Add((columnIndex, y));
        }

        return columnIndexList;
    }
}
