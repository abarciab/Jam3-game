using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboGagueCoordinator : MonoBehaviour
{
    public TextMeshProUGUI multiplierDisplay;
    public RectTransform meterTransform;
    public float timeMod;
    public float comboExponent;
    float originalHeight;
    float width;
    [SerializeField] float comboPercent;

    private void Start() {
        width = meterTransform.rect.width;
        originalHeight = meterTransform.rect.height;
        //GameManager.onEnemyTurnStart += AddToCombo;
        GameManager.instance.comboScript = this;
    }

    public float GetComboValue() {
        return Mathf.Pow(1 + comboPercent, comboExponent);
    }

    void Update()
    {
        if (GameManager.instance.playerTurn){
            comboPercent -= Time.deltaTime * timeMod;
            comboPercent = Mathf.Max(comboPercent, 0);
        }
        multiplierDisplay.text = (Mathf.Round((GetComboValue()) * 10) / 10) + "x";
        meterTransform.sizeDelta = new Vector2(width, originalHeight * comboPercent);
    }

    //this function is called when an attack is sucssessfully performed, and the value passed is the portion of the combo meter that is granted by this attack (max value of 1)
    public void AddCombo(float comboValue) {
        comboPercent += comboValue;
        comboPercent = Mathf.Min(comboPercent, 1);
    }
}
