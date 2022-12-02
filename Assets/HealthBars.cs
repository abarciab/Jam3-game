using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Data.Common;
using System.Xml.Serialization;

public class HealthBars : MonoBehaviour
{
    public static HealthBars instance;

    private void Awake()
    {
        instance = this;
    }

    public Gradient HPGradient;
    public int score;

    [Header("References")]
    public Slider triSlider;
    public Image triFill;
    public Slider roundSlider;
    public Image roundSprite;
    public Sprite roundColorSprite;
    public Image roundFill;
    public Image SquareSprite;
    public Sprite ColorSquare;
    public Slider squareSlider;
    public Image squareFill;
    public TextMeshProUGUI scoreText;

    public PartyMemberStats triStats;
    public PartyMemberStats roundStats;
    public PartyMemberStats SquareStats;

    bool roundActivated;
    bool squareActivated;

    void Update()
    {
        triSlider.value = triStats.currentHealth / triStats.maxHealth;
        triFill.color = HPGradient.Evaluate(triSlider.value);
        if (roundActivated) {
            roundSlider.value = roundStats.currentHealth / roundStats.maxHealth;
            roundFill.color = HPGradient.Evaluate(roundSlider.value);
        }
        if (squareActivated) {
            squareSlider.value = SquareStats.currentHealth / SquareStats.maxHealth;
            squareFill.color = HPGradient.Evaluate(squareSlider.value);
        }   
    }

    public void IncreaseScore (int _score)
    {
        score += _score;
        scoreText.text = "Score: " + score;
    }

    public void ActiveRound()
    {
        roundSprite.sprite = roundColorSprite;
        roundActivated = true;
    }

    public void ActivateSqaure()
    {
        SquareSprite.sprite = ColorSquare;
        squareActivated = true;
    }
}
