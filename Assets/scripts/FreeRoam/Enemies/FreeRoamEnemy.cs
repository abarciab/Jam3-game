using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

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

    [Header("Drop Settings")]
    [Tooltip("Place the drop prefab here")]
    [SerializeField] private GameObject drop;
    [Tooltip("Name of the drop stored in the inventory")]
    [SerializeField] private string dropName;
    [Tooltip("Place the sprite of the drop here. Make sure it matches with the Image used in the UI")]
    [SerializeField] private Sprite dropSprite;
    [Tooltip("This is a reference to the Image in the UI")]
    [SerializeField] private Transform uiImage;

    private Collider2D enemyTrigger;
    private bool defeated;

    public List<EnemyStats> enemiesInEncounter;
    public List<Enemy> enemyList = new List<Enemy>();

    private void Awake() {
        enemyTrigger = GetComponent<Collider2D>();
        defeated = false;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        // disable player movement and load stats into battle manager
        if(other.collider.tag == "Player") {
            other.transform.GetComponent<IsometricPlayerMovement>().toggleMovement(false);
            StartCoroutine("startTransition");
        }
    }
    

    private void OnDisable() {
        // create a drop
        if(defeated) {
            drop.GetComponent<SpriteRenderer>().sprite = dropSprite;
            drop.GetComponent<EnemyDrop>().uiImage = uiImage;
            drop.GetComponent<EnemyDrop>().dropName = dropName;
            Instantiate(drop, transform.position, transform.rotation);
        }
    }

    private IEnumerator startTransition() {
        // play fade and start battle scene after fade
        fade.gameObject.SetActive(true);
        fade.GetComponent<Animator>().Play("FadeOut");
        yield return new WaitUntil(() => fade.GetComponent<FadeTransition>().transitionOver());
        BattleManager.instance.startBattleScene(this);
    }

    public EnemyStats findEnemyByName(string name) {
        foreach(EnemyStats enemy in enemiesInEncounter) {
            if(enemy.enemyName.ToLower() == name.ToLower())
                return enemy;
        }
        return null;
    }

    public void setDefeated(bool isDefeated) {
        defeated = isDefeated;
    }
}
