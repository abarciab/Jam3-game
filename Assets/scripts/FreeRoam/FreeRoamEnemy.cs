using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStats {
    public string enemyName;
    public Sprite portrait;
    public int maxHealth;
    public float attackTime;
    public List<string> abilityNames;
}

public class FreeRoamEnemy : MonoBehaviour
{
    private Collider2D enemyTrigger;

    public List<EnemyStats> enemiesInEncounter;

    private void Awake() {
        enemyTrigger = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // load stats into battle manager
        if(other.tag == "Player")
            BattleManager.instance.startBattleScene(this);
    }

    public EnemyStats findEnemyByName(string name) {
        foreach(EnemyStats enemy in enemiesInEncounter) {
            if(enemy.enemyName.ToLower() == name.ToLower())
                return enemy;
        }
        return null;
    }
}
