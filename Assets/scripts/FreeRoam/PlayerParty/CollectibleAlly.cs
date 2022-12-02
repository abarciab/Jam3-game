using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleAlly : MonoBehaviour
{
    [Header("Prefab and Container")]
    [SerializeField] private GameObject partyMemberPrefab;
    [SerializeField] private Transform partyContainer;
    
    [Header("Follow")]
    [SerializeField] private float speed;
    [SerializeField] private float minDistanceToCharacter;
    [Tooltip("Note: This is is world space coordinates")]
    [SerializeField] private Vector2 originalPos;
    private Transform characterToFollow;

    [Header("Movement Log")]
    [SerializeField] private int logLength;

    [Header("Party Member Stats")]
    public string characterName;
    public int maxHealth;
    public float currentHealth;
    public string pattern;
    public Sprite portrait;
    public Ability ability;

    private BoxCollider2D box;

    private void Awake() {
        box = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player") {
            GetComponent<BoxCollider2D>().enabled = false;
            GameObject newAlly = Instantiate(partyMemberPrefab, transform.position, transform.rotation, partyContainer);
            setFollowSettings(newAlly);
            newAlly.transform.GetComponent<MovementLog>().logLength = logLength;
            setStats(newAlly);

            //newAlly.transform.position = other.transform.parent.transform.GetComponent<Rigidbody2D>().position;//transform.position;
            //Destroy(gameObject);
            StartCoroutine(waitForMamaDuck(newAlly));
        }
    }

    private void setFollowSettings(GameObject ally) {
        FollowCharacter follow = ally.transform.GetComponent<FollowCharacter>();
        follow.characterToFollow = partyContainer.GetChild(partyContainer.childCount - 2);
        follow.speed = speed;
        follow.minDistanceToCharacter = minDistanceToCharacter;
        follow.originalPos = originalPos;
        characterToFollow = follow.characterToFollow;
    }

    private void setStats(GameObject ally) {
        PartyMemberStats stats = ally.transform.GetComponent<PartyMemberStats>();
        stats.characterName = characterName;
        stats.maxHealth = maxHealth;
        stats.currentHealth = currentHealth;
        stats.pattern = pattern;
        stats.portrait = portrait;
        stats.ability = ability;
    }

    private IEnumerator waitForMamaDuck(GameObject newAlly) {
        // disable movement for newAlly
        newAlly.transform.GetComponent<FollowCharacter>().enabled = false;

        // wait until characterToFollow is close enough
        yield return new WaitUntil(() => Vector3.Distance(newAlly.transform.position, characterToFollow.position) < 1);

        // move newAlly to characterToFollow, reenable movement, and destroy pickup
        characterToFollow.GetComponent<MovementLog>().log.Clear();
        newAlly.transform.position = characterToFollow.transform.position;
        newAlly.transform.GetComponent<FollowCharacter>().enabled = true;
        Destroy(gameObject);
    }
}
