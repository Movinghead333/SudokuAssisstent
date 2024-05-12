using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public delegate void PotentialNumbersUpdatedDelegate(HashSet<int>[,] potentialNumbers);
    public PotentialNumbersUpdatedDelegate OnPotentialNumbersUpdated;

    private SudokuGame _sudokuGame;

    // Start is called before the first frame update
    void Start()
    {
        _sudokuGame = new SudokuGame();
        _sudokuGame.SetFields(new int?[,]
        {
            {null,null,8,null,null,null,null,null,null, },
            {null,1,null,null,2,null,null,3,9, },
            {5,null,null,null,4,null,null,null,8, },
            {null,null,2,null,9,null,null,null,null, },
            {null,9,1,5,null,2,null,null,null, },
            {null,null,null,null,null,1,null,6,null, },
            {null,null,null,9,null,null,null,null,4, },
            {8,null,3,null,null,null,5,null,null, },
            {null,4,null,7,null,null,6,null,null, },
        });

        InitializeSudokuGUI();


        UpdatePotentialFields();
    }

    public void OnFieldChanged(int x, int y, int oldNumber, int newNumber)
    {
        if (newNumber == SudokoField.EMPTY_FIELD)
        {
            if (oldNumber != SudokoField.EMPTY_FIELD)
            {
                _sudokuGame.SetNumberField(x, y, null);
                UpdatePotentialFields();
            }
            return;
        }

        Debug.Log("Processing field change in game-manager");

        bool validChange = _sudokuGame.SetNumberField(x, y, newNumber);
        // check for any collisions on the row,col and square of the changed field
        if (validChange)
        {
            UpdatePotentialFields();
        }
        else
        {
            Debug.LogWarning("Collision detected!");
            GetSodukoField(x, y).SetNumberField(oldNumber == SudokoField.EMPTY_FIELD ? null : oldNumber);
        }
    }

    private void UpdatePotentialFields()
    {
        (HashSet<int>[,] potentialNumbersForFields, Dictionary<(int, int), int> nextMoveDict) = _sudokuGame.GeneratePotentialNumbersForFields();

        for (int fx = 0; fx < 9; fx++)
        {
            for (int fy = 0; fy < 9; fy++)
            {
                SudokoField field = GetSodukoField(fx, fy);
                field.UpdatePotentialNumbers(potentialNumbersForFields[fx, fy]);

                if (nextMoveDict.ContainsKey((fx, fy)))
                {
                    field.SetFieldColor(new Color(0.2f, 0.8f, 0.2f, 1.0f));
                }
            }
        }
    }

    public SudokoField GetSodukoField(int x, int y)
    {
        GameObject g = fields[y * 9 + x];
        return g.GetComponent<SudokoField>();
    }

    private void InitializeSudokuGUI()
    {
        sodukoFieldPrototype = Resources.Load("SodukoField") as GameObject;
        Instantiate(
            sodukoFieldPrototype,
            new Vector3(100, 0, 0),
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

                int? number = _sudokuGame.Fields()[x, y];
                if (number.HasValue)
                {
                    newField.SetNumberField(number.Value);
                }

                /* get sqaure associated with x,y coordinates
                 * squares are layouted as followed:
                 * 6 7 8
                 * 3 4 5
                 * 0 1 2
                 */
                int square = x / 3 + 3 * (y / 3);

                // color fields form every other square
                if (square % 2 == 0)
                {
                    newFieldObject.GetComponent<Image>().color = new Color(0.8f, 0.8f, 1f, 1f);
                }

                fields.Add(newFieldObject);
            }
        }
    }
}
