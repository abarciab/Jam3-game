using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class LevelUpCoordinator : MonoBehaviour
{
    public enum RobotType { triangle, round, square}

    Color normalColor;
    public Color highlightedColor;
    XPSystem xpScript;
    //public Vector3 HPLevels;        //levels are the amount of times that that robot has been upgraded in that track. so if HPlevels reads 3, 0, 0, that means that tri has gotten bonus HP 3 times
    //public Vector3 STRLevels;
    Vector3 HPValues;        //values are the amount that is displayed on the buttons. so it'll be like 9,3,3 (tri will get +9 maxHP, round and square will only get + 3 maxHP)
    Vector3 STRValues;
    public float HPmultiplier = 0.1f;      //multipliers are how the values are calculated - these numbers are multiplied by the current health of the robots. so HPmult of 0.5 means they get +50% max health each time
    public float dmgPerTurnMult = 0.1f;      //multipliers are how the values are calculated - these numbers are multiplied by the current health of the robots. so HPmult of 0.5 means they get +50% max health each time
    public float HealMult = 0.6f;      //multipliers are how the values are calculated - these numbers are multiplied by the current health of the robots. so HPmult of 0.5 means they get +50% max health each time
    public float dmgMult = 0.5f;
    public Vector2 choices;         //each int (1,2,3) marks which robot was selected - so 1,3 means that tri gets meor health and square gets more healing

    [Header("References")]
    public GameObject tri;
    public TextMeshProUGUI triHPButton;
    public TextMeshProUGUI triSTRButton;
    public TextMeshProUGUI triHP;
    public TextMeshProUGUI triAbility;
    public Slider triHPBar;
    public GameObject round;
    public TextMeshProUGUI roundHPButton;
    public TextMeshProUGUI roundSTRButton;
    public TextMeshProUGUI roundHP;
    public TextMeshProUGUI roundAbility;
    public Slider roundHPBar;
    public GameObject square;
    public TextMeshProUGUI squareHPButton;
    public TextMeshProUGUI squareSTRButton;
    public TextMeshProUGUI squareHP;
    public TextMeshProUGUI squareAbility;
    public Slider squareHPBar;
    [Space(20)]
    public GameObject confirmButton;
    bool HpSelected;
    bool strSelected;

    private void Start()
    {
        xpScript = BattleManager.instance.xpSystem;
    }

    private void OnEnable()
    {
        BattleManager.instance.PauseCharacterAndEnemies();
        if (xpScript == null) {
            xpScript = BattleManager.instance.xpSystem;
        }
        confirmButton.SetActive(false);
        round.SetActive(HealthBars.instance.roundActivated);
        square.SetActive(HealthBars.instance.squareActivated);
        HealthBars.instance.gameObject.SetActive(false);
        InitializeButtonValues();
    }

    public void CloseLevelUpScreen()
    {
        switch (choices.x) {
            case 1:
                HealthBars.instance.triStats.maxHealth += Mathf.RoundToInt(HPValues.x);
                break;
            case 2:
                HealthBars.instance.roundStats.maxHealth += Mathf.RoundToInt(HPValues.y);
                break;
            case 3:
                HealthBars.instance.SquareStats.maxHealth += Mathf.RoundToInt(HPValues.z);
                break;
        }
        switch (choices.y) {
            case 1:
                HealthBars.instance.triStats.ability.damage += Mathf.RoundToInt(STRValues.x);
                break;
            case 2:
                HealthBars.instance.roundStats.ability.damagePerTurn+= Mathf.RoundToInt(STRValues.y);
                break;
            case 3:
                HealthBars.instance.SquareStats.ability.damage -= Mathf.RoundToInt(STRValues.z);
                break;
        }
        HealthBars.instance.gameObject.SetActive(true);
        gameObject.SetActive(false);
        BattleManager.instance.ResumeCharacterAndEnemies();
        BattleManager.instance.xpSystem.GainXP(0);
    }

    void InitializeButtonValues()
    {
        PartyMemberStats tri = HealthBars.instance.triStats;

        HPValues.x = Mathf.RoundToInt(tri.maxHealth * HPmultiplier);
        STRValues.x = Mathf.RoundToInt(tri.ability.damage * dmgMult);
        triHPButton.text = "+ " + HPValues.x + " max HP";
        triSTRButton.text = "+ " + STRValues.x + " dmg";
        triHP.text = tri.currentHealth + "/" + tri.maxHealth;
        triHPBar.value = tri.currentHealth / tri.maxHealth;
        string triDescrip = tri.ability.description.Replace("DMG", tri.ability.damage.ToString());
        triAbility.text = triDescrip;
        ToggleButtonVisuals(triHPButton.transform.parent.gameObject, 1);
        ToggleButtonVisuals(triSTRButton.transform.parent.gameObject, 1);

        if (round.activeInHierarchy) {
            PartyMemberStats round = HealthBars.instance.roundStats;
            HPValues.y = Mathf.RoundToInt(round.maxHealth * HPmultiplier);
            STRValues.y = Mathf.Max(Mathf.RoundToInt(round.ability.damagePerTurn * dmgPerTurnMult), 1);
            roundHPButton.text = "+ " + HPValues.y + " max HP";
            roundSTRButton.text = "+ " + STRValues.y + " dmg/turn";
            roundHP.text = round.currentHealth + "/" + round.maxHealth;
            roundHPBar.value = round.currentHealth / round.maxHealth;
            string roundDescrip = round.ability.description.Replace("DMG", round.ability.damagePerTurn.ToString());
            roundAbility.text = roundDescrip;
            ToggleButtonVisuals(roundHPButton.transform.parent.gameObject, 1);
            ToggleButtonVisuals(roundSTRButton.transform.parent.gameObject, 1);
        }

        if (square.activeInHierarchy) {
            PartyMemberStats square = HealthBars.instance.SquareStats;
            HPValues.z = Mathf.RoundToInt(square.maxHealth * HPmultiplier);
            STRValues.z = Mathf.RoundToInt(-square.ability.damage * HealMult);
            squareHPButton.text = "+ " + HPValues.z + " max HP";
            squareSTRButton.text = "+ " + STRValues.z + " heal";
            squareHP.text = square.currentHealth + "/" + square.maxHealth;
            squareHPBar.value = square.currentHealth / square.maxHealth;
            string sqaureDescrip = square.ability.description.Replace("DMG", (-square.ability.damage).ToString());
            squareAbility.text = sqaureDescrip;
            ToggleButtonVisuals(squareHPButton.transform.parent.gameObject, 1);
            ToggleButtonVisuals(squareSTRButton.transform.parent.gameObject, 1);
        }
    }

    //mode 0: no ovveride, just flips state of button
    //mode 1: force turns off button
    //mode 2: force turns on button
    void ToggleButtonVisuals(GameObject button, int mode = 0)
    {
        var img = button.GetComponent<Image>();
        if ((img.color.a < 1 && mode != 1) || mode == 2) {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1);
        }
        else if ((img.color.a == 1 && mode != 2) || mode == 1){
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.2f);
        }
    }

    public void LevelUpHP(int type)
    {
        choices.x = type;
        HpSelected = true;
        print("levelupHP: " + type);
        switch (type) {
            case 1:
                ToggleButtonVisuals(roundHPButton.transform.parent.gameObject, 1);
                ToggleButtonVisuals(triHPButton.transform.parent.gameObject, 2);
                ToggleButtonVisuals(squareHPButton.transform.parent.gameObject, 1);
                break;
            case 2:
                ToggleButtonVisuals(roundHPButton.transform.parent.gameObject, 2);
                ToggleButtonVisuals(triHPButton.transform.parent.gameObject, 1);
                ToggleButtonVisuals(squareHPButton.transform.parent.gameObject, 1);
                break;
            case 3:
                ToggleButtonVisuals(roundHPButton.transform.parent.gameObject, 1);
                ToggleButtonVisuals(squareHPButton.transform.parent.gameObject, 2);
                ToggleButtonVisuals(triHPButton.transform.parent.gameObject, 1);
                break;
        }
        if (strSelected) {
            confirmButton.SetActive(true);
        }
    }

    public void LevelUpSTR(int type)
    {
        print("str");
        choices.y = type;
        strSelected = true;

        switch (type) {
            case 1:
                ToggleButtonVisuals(roundSTRButton.transform.parent.gameObject, 1);
                ToggleButtonVisuals(triSTRButton.transform.parent.gameObject, 2);
                ToggleButtonVisuals(squareSTRButton.transform.parent.gameObject, 1);
                break;
            case 2:
                ToggleButtonVisuals(roundSTRButton.transform.parent.gameObject, 2);
                ToggleButtonVisuals(triSTRButton.transform.parent.gameObject, 1);
                ToggleButtonVisuals(squareSTRButton.transform.parent.gameObject, 1);
                break;
            case 3:
                ToggleButtonVisuals(roundSTRButton.transform.parent.gameObject, 1);
                ToggleButtonVisuals(squareSTRButton.transform.parent.gameObject, 2);
                ToggleButtonVisuals(triSTRButton.transform.parent.gameObject, 1);
                break;
        }
        if (HpSelected) {
            confirmButton.SetActive(true);
        }
    }

    //animation functions
    


}
