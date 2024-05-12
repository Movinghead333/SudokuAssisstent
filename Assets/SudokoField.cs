using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SudokoField : MonoBehaviour
{
    public int xpos;
    public int ypos;

    public InputField numberInput;
    public GameObject potentialNumbersParent;
    private Text[] potentialNumbers = new Text[9];
    public static int EMPTY_FIELD = -1;

    public bool[] potentialNumbersData = new bool[9];

    public int number = EMPTY_FIELD;

    // Start is called before the first frame update
    void Awake()
    {
        potentialNumbers = potentialNumbersParent.GetComponentsInChildren<Text>();
        for (int i = 0; i < potentialNumbersData.Length; i++)
        {
            potentialNumbersData[i] = true;
        }
        //numberInput.onEndEdit.AddListener(OnNumberFieldChangeFinished);
    }

    public void OnNumberFieldChanged(string enteredText)
    {
        //Debug.Log("Numberfield changed! text: " + enteredText);
    }

    public void OnNumberFieldChangeFinished(string enteredText)
    {
        int oldNumber = number;
        //Debug.Log("Numberfield change finished! text: " + enteredText);
        if (enteredText.Length == 1)
        {
            if (int.TryParse(enteredText, out number) && number != 0)
            {
                Debug.Log("Entered a valid number: " + number);
                SetPotentialNumbersVisible(false);
            }
            // invaled char or 0 as number so reset the text field
            else
            {
                Debug.LogWarning("invaled char or 0 as number so reset the text field");
                numberInput.text = "";
                number = EMPTY_FIELD;
                SetPotentialNumbersVisible(true);
            }
        }
        // text is longer or shorter than one character so reset the text field
        else
        {
            Debug.LogWarning("text is longer or shorter than one character so reset the text field");
            numberInput.text = "";
            number = EMPTY_FIELD;
            SetPotentialNumbersVisible(true);
        }

        SudokoGameManager.instance.OnFieldChanged(xpos, ypos, oldNumber, number);
    }

    public void SetNumberField(int? newNumber)
    {
        numberInput.text = newNumber == null ? "" : newNumber.ToString();
        number = newNumber ?? EMPTY_FIELD;
    }

    public void SetFieldColor(Color color)
    {
        gameObject.GetComponent<Image>().color = color;
    }

    private void SetPotentialNumbersVisible(bool visible)
    {
        if (visible)
        {
            UpdatePotentialNumbers();
        }
        else
        {
            foreach (Text potentialNumberText in potentialNumbers)
            {
                potentialNumberText.enabled = false;
            }
        }
    }
    

    public void UpdatePotentialNumbers()
    {
        if (number != -1) return;

        for (int i = 0; i < potentialNumbers.Length; i++)
        {
            potentialNumbers[i].enabled = potentialNumbersData[i];
        }
    }

    public void UpdatePotentialNumbers(HashSet<int> newPotentialNumbers)
    {
        for (int i = 0; i < potentialNumbers.Length; i++)
        {
            potentialNumbers[i].enabled = newPotentialNumbers.Contains(i+1);
        }
    }
}
