using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

[System.Serializable]
public class EnemyStats {
    public enum EnemyType { rock, vines, crab}

    public string name;
    public int pointValue;
    public EnemyType type;
    public Color tint = Color.black;        //black is the default value, and if it's full black, tint is ignored
    public int maxHealth;
    public float attackDamage;
    public float attackTime;
    public List<Ability> usableAbilities;
    public bool isSpeaker = false;
    [Tooltip("NOTE: First line is intro line said at beginning of battle." +
             "Last line is a generic line that is repeated when all other lines are said.")]
    public List<string> dialogueLines;
    public bool revealVines;
    public bool revealHook;
}
public class FreeRoamEnemy : MonoBehaviour
{
    [SerializeField] private Transform fade;

    [Header("Drop Settings")]
    [Tooltip("Place the drop prefab here")]
    [SerializeField] private GameObject drop;
    [Tooltip("Name of the drop stored in the inventory")]
    public string dropName;
    [Tooltip("Place the sprite of the drop here. Make sure it matches with the Image used in the UI")]
    [SerializeField] private Sprite dropSprite;
    [Tooltip("This is a reference to the Image in the UI")]
    [SerializeField] private Transform uiImage;

    private Collider2D enemyTrigger;
    private bool defeated;

    public List<EnemyStats> enemiesInEncounter;
    //public List<Enemy> enemyList = new List<Enemy>();

    private void Awake() {
        enemyTrigger = GetComponent<Collider2D>();
        defeated = false;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        // disable player movement and load stats into battle manager
        if(other.collider.tag == "Player") {
            other.transform.GetComponent<IsometricPlayerMovement>().toggleMovement(false);
            BattleManager.instance.toggleAllEnemyMovement(false);
            StartCoroutine("startTransition");
        }
    }
    

    private void OnDisable() {
        // create a drop
        if(defeated && dropName != "") {
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
            if(enemy.name.ToLower() == name.ToLower())
                return enemy;
        }
        return null;
    }

    public void setDefeated(bool isDefeated) {
        defeated = isDefeated;
    }
}
