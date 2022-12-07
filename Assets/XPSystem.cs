using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class XPSystem : MonoBehaviour
{
    public GameObject levelUpScreen;

    public TextMeshProUGUI xpDisplay;

    public float xp;
    public int level = 0;
    float xpGoal = 100;

    private void Start()
    {
        GainXP(0);
    }

    public void GainXP(float amount)
    {
        xp += amount;
        if (xp >= xpGoal) {
            xp = xp-xpGoal;
            xpGoal = ((Mathf.RoundToInt(Mathf.Pow(level*2.5f, 2.6f))/2) + 100);
            LevelUp();
        }
        xpDisplay.text = "XP: " + xp + "/" + xpGoal;
    }

    void LevelUp()
    {
        print("levelup!");
        level += 1;
        levelUpScreen.SetActive(true);
    }
}
