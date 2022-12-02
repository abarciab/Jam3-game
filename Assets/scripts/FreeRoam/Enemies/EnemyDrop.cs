using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDrop : MonoBehaviour
{
    public string dropName;
    public Transform uiImage;
    /*
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player") {
            print("destroyed");
            Inventory.instance.addItem(dropName);
            uiImage.GetComponent<Image>().color = Color.white;
            Destroy(transform.gameObject);
        }
    }*/
}
