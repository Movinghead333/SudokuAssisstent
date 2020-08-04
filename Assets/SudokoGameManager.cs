using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SudokoGameManager : MonoBehaviour
{
    #region Singleton
    public static SudokoGameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    #endregion

    private GameObject sodukoFieldPrototype;
    public GameObject parentCanvas;
    private List<GameObject> fields = new List<GameObject>();



    // Start is called before the first frame update
    void Start()
    {
        sodukoFieldPrototype = Resources.Load("SodukoField") as GameObject;
        Instantiate(
            sodukoFieldPrototype,
            new Vector3(100,0,0),
            Quaternion.identity,
            parentCanvas.transform);
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                GameObject newFieldObject = Instantiate(
                    sodukoFieldPrototype,
                    new Vector3(x * 100, y * 100, 0),
                    Quaternion.identity, parentCanvas.transform);

                SudokoField newField = newFieldObject.GetComponent<SudokoField>();
                newField.xpos = x;
                newField.ypos = y;

                // color fields form every other square
                if (GetSquare(x, y) % 2 == 0)
                {
                    newFieldObject.GetComponent<Image>().color = new Color(0.8f,0.8f,1f,1f);
                }

                fields.Add(newFieldObject);
            }
        }
    }

    public void OnFieldChanged(int x, int y, int oldNumber, int newNumber)
    {
        if (newNumber == SudokoField.EMPTY_FIELD)
        {
            if (oldNumber != SudokoField.EMPTY_FIELD)
            {
                ResetPotentialFields(x, y, oldNumber);
            }
            return;
        }
        Debug.Log("processing field change in game-manager");
        // check for any collisions on the row,col and square of the changed field
        if (CheckForCollision(x,y,newNumber))
        {
            Debug.LogWarning("Collision detected!");
        }
        else
        {
            UpdatePotentialFields(x, y, newNumber, false);
        }
    }

    private void ResetPotentialFields(int x, int y, int number)
    {
        for (int i = 0; i < 9; i++)
        {
            if (!CheckForCollision(x, i, number))
            {
                GetSodukoField(x, i).potentialNumbersData[number - 1] = true;
                GetSodukoField(x, i).UpdatePotentialNumbers();
            }
            if (!CheckForCollision(i, y, number))
            {
                GetSodukoField(i, y).potentialNumbersData[number - 1] = true;
                GetSodukoField(i, y).UpdatePotentialNumbers();
            }
        }

        int squareX = (x / 3) * 3;
        int squareY = (y / 3) * 3;
        for (int yi = squareY; yi < squareY + 3; yi++)
            for (int xi = squareX; xi < squareX + 3; xi++)
            {
                if (!CheckForCollision(xi, yi, number))
                {
                    GetSodukoField(xi, yi).potentialNumbersData[number - 1] = true;
                    GetSodukoField(xi, yi).UpdatePotentialNumbers();
                }
            }
    }

    private bool CheckForCollision(int x, int y, int number)
    {
        return CheckRow(x, y, number) || CheckCol(x, y, number) || CheckSquare(x, y, number);
    }

    private void UpdatePotentialFields(int x, int y, int number, bool possible)
    {
        for (int i = 0; i < 9; i++)
        {
            GetSodukoField(x, i).potentialNumbersData[number - 1] = possible;
            GetSodukoField(x, i).UpdatePotentialNumbers();
            GetSodukoField(i, y).potentialNumbersData[number - 1] = possible;
            GetSodukoField(i, y).UpdatePotentialNumbers();
        }

        int squareX = (x / 3) * 3;
        int squareY = (y / 3) * 3;
        for (int yi = squareY; yi < squareY + 3; yi++)
            for (int xi = squareX; xi < squareX + 3; xi++)
            {
                GetSodukoField(xi, yi).potentialNumbersData[number - 1] = possible;
                GetSodukoField(xi, yi).UpdatePotentialNumbers();
            }
    }

    private bool CheckRow(int x, int y, int number)
    {
        for (int i = 0; i < 9; i++)
        {
            if (i == x) continue;
            if (GetSodukoField(i, y).number == number)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckCol(int x, int y, int number)
    {
        for (int i = 0; i < 9; i++)
        {
            if (i == y) continue;
            if(GetSodukoField(x,i).number == number)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckSquare(int x, int y, int number)
    {
        int squareX = (x / 3) * 3;
        int squareY = (y / 3) * 3;
        for (int yi = squareY; yi < squareY + 3; yi++)
            for (int xi = squareX; xi < squareX + 3; xi++)
            {
                if (xi == x && yi == y) continue;
                if (GetSodukoField(xi, yi).number == number)
                {
                    return true;
                }
            }

        return false;
    }

    /* get sqaure associated with x,y coordinates
     * squares are layouted as followed:
     * 6 7 8
     * 3 4 5
     * 0 1 2
     */
    public static int GetSquare(int x, int y)
    {
        return x / 3 + 3 * (y / 3);
    }

    public SudokoField GetSodukoField(int x, int y)
    {
        GameObject g = fields[y * 9 + x];
        return g.GetComponent<SudokoField>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
