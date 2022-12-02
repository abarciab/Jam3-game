using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyMemberStats : MonoBehaviour
{
    public string characterName;
    public int maxHealth;
    public float currentHealth;
    public string pattern;
    public Sprite portrait;
    public Ability ability;
    public bool round;
    public bool square;

    public void setCurrentHealth(float health) {
        currentHealth = health;
    }

    private void Start()
    {
        if (round) {
            HealthBars.instance.roundStats = this;
            HealthBars.instance.ActiveRound();
        }
        if (square) {
           
            HealthBars.instance.SquareStats = this;
            HealthBars.instance.ActivateSqaure();
        }
        
    }
}
