using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickup : MonoBehaviour
{
    public int healthIncrease = 20;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent<PartyMemberStats>(out var hit)) {
            AudioManager.instance.PlayGlobal(-4);
            for (int i = 0; i < hit.transform.parent.childCount; i++) {
                var stats = hit.transform.parent.GetChild(i).GetComponent<PartyMemberStats>();
                stats.currentHealth = Mathf.Min(stats.currentHealth + healthIncrease, stats.maxHealth);
            }
            Destroy(gameObject);
        }
    }
}
