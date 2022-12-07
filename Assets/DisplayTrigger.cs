using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTrigger : MonoBehaviour
{
    public bool unlockGrappleInfo;
    public DisplayScreenCoordinator coordinator;
    public int num;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.ToLower().Contains("play")) {
            BattleManager.instance.PauseCharacterAndEnemies();
            coordinator.DisplayText(num);
            Destroy(gameObject);
            if (unlockGrappleInfo) {
                Inventory.instance.activate();
            }
        }
    }
}
