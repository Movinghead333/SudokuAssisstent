using System;
using System.Collections.Generic;
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

    public HashSet<int>[,] GeneratePotentialNumbersForFields()
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

        return potentialNumbersForFields;
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
}
