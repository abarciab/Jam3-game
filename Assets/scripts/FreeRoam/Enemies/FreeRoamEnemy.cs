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
    [SerializeField] private Transform fade;
    private Collider2D enemyTrigger;

    public List<EnemyStats> enemiesInEncounter;

    private void Awake() {
        enemyTrigger = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // load stats into battle manager
        if(other.tag == "Player")
            //BattleManager.instance.startBattleScene(this);
            StartCoroutine("startTransition");
    }

    private IEnumerator startTransition() {
        fade.gameObject.SetActive(true);
        fade.GetComponent<Animator>().Play("FadeOut");
        yield return new WaitUntil(() => fade.GetComponent<FadeTransition>().transitionOver());
        fade.gameObject.SetActive(false);
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
