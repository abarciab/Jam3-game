using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//this script handles the combo meter 
public class ComboGagueCoordinator : MonoBehaviour
{
    public bool verticalBar = true;
    public TextMeshProUGUI multiplierDisplay;
    public RectTransform meterTransform;
    public float timeMod;
    public float comboExponent;
    float originalHeight;
    float originalWidth;
    float width;
    float height;
    [SerializeField] float comboPercent;

    private void Start() {
        width = meterTransform.rect.width;
        height = meterTransform.rect.height;
        originalHeight = meterTransform.rect.height;
        originalWidth = meterTransform.rect.width;
        GameManager.instance.comboScript = this;
    }

    //the combo percent is in a range from 0-1, but you can specify a different equation here to change that range. currently, it outputs a value equal to (1+combopercent) to a certain power.
    public float GetComboValue() {
        return Mathf.Pow(1 + comboPercent, comboExponent);
    }

    void Update()
    {
        if (GameManager.instance.playerTurn){       //decrement the combo score by x amount per second. lower values of timeMod make it fall more slowly
            comboPercent -= Time.deltaTime * timeMod;
            comboPercent = Mathf.Max(comboPercent, 0);
        }
        multiplierDisplay.text = (Mathf.Round((GetComboValue()) * 10) / 10) + "x\nCOMBO";      //for display purposes, the value is rounded to 1 decimal point
        meterTransform.sizeDelta = new Vector2(width, originalHeight * comboPercent);       //this updates the actual meter
    }

    //this function is called when an attack is sucssessfully performed, and the value passed is the fraction of the combo meter that is granted by this attack (max value of 1)
    public void AddCombo(float comboValue) {
        comboPercent += comboValue;
        comboPercent = Mathf.Min(comboPercent, 1);
    }
}
