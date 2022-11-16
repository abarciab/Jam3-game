using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("stats")]
    public string enemyName;
    public int maxHealth;
    public float health;
    public float testDamage;
    public float attackTime = 0.5f;

    [Header("references")]
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI healthLabel;
    public TextMeshProUGUI damageIndicator;

    [Header("misc")]
    public int deathSoundID = 5;
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();

    private void Start() {
        nameLabel.text = enemyName;
        GameManager.instance.AddEnemyToFight(this);
        health = maxHealth;
        healthLabel.text = health + "/" + maxHealth;
        GameManager.onEnemyTurnStart += AddToAttackQueue;
    }

    void AddToAttackQueue() {
        if (!GameManager.instance.enemyActionQueue.Contains(this)) {
            GameManager.instance.enemyActionQueue.Add(this);
        }
    }

    public void StartAttack() {
        StartCoroutine("ReadyAttack");        
    }

    void Attack() {
        if (GameManager.instance.charactersInFight.Count == 0) { return; }
        Character target = GameManager.instance.charactersInFight[Random.Range(0, GameManager.instance.charactersInFight.Count)];
        GameManager.instance.Log(enemyName + " attacks " + target.characterName + " for " + testDamage + " dmg");
        target.Damage(testDamage);
        AudioManager.instance.PlayGlobal(6, _priority: 1, restart: true);
        GameManager.instance.CompleteAttack(this);
    }

    IEnumerator ReadyAttack() {
        yield return new WaitForSeconds(attackTime);
        Attack();
    }

    public void Damage(float damageAmount, int duration = 1, float damagePerTurn = 1) {
        damageIndicator.gameObject.SetActive(false);
        health = Mathf.Max(0, health - damageAmount);
        health = Mathf.Min(health, maxHealth);
        healthLabel.text = (Mathf.Round(health * 10) / 10) + "/" + maxHealth;
        damageIndicator.text = Mathf.Abs(Mathf.Round(damageAmount * 10) / 10).ToString();
        damageIndicator.color = damageAmount < 0 ? Color.green : Color.white;
        damageIndicator.gameObject.SetActive(true);

        if (health == 0) {
            Die();
        }
    }


    void Die() {
        print(enemyName + " died!!!");
        GameManager.instance.enemiesInFight.Remove(this);
        GameManager.onEnemyTurnStart -= AddToAttackQueue;
        AudioManager.instance.PlayGlobal(deathSoundID);
        Destroy(gameObject);
    }
}
