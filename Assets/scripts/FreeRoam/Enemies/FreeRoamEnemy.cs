using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStats {
    public string enemyName;
    public Sprite portrait;
    public int maxHealth;
    public float attackDamage;
    public float attackTime;
    public List<Ability> usableAbilities;
    public bool isSpeaker = false;
    [Tooltip("NOTE: First line is intro line said at beginning of battle")]
    public List<string> dialogueLines;
}
public class FreeRoamEnemy : MonoBehaviour
{
    [SerializeField] private Transform fade;
    private Collider2D enemyTrigger;

    public List<EnemyStats> enemiesInEncounter;
    public List<Enemy> enemyList = new List<Enemy>();

    private void Awake() {
        enemyTrigger = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // load stats into battle manager
        if(other.tag == "Player")
            StartCoroutine("startTransition");
    }
    

    private IEnumerator startTransition() {
        // play fade and start battle scene after fade
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
