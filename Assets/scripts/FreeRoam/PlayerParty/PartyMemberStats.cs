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

    public void setCurrentHealth(float health) {
        currentHealth = health;
    }
}
