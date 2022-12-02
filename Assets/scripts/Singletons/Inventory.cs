using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    private List<string> inventoryItems = new List<string>();

    public List<GameObject> Sprites;
    public List<string> requiredItems;

    private void Awake() {
        instance = this;
    }

    public void addItem(string dropName) {
        print("collected: " + dropName);
        for (int i = 0; i < requiredItems.Count; i++) {
            if (requiredItems[i] == dropName) {
                Sprites[i].SetActive(true);
                Sprites[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
        }
        if(requiredItems.Contains(dropName))
            requiredItems.Remove(dropName);

        if (requiredItems.Count == 0) {
            BattleManager.instance.winGame();
        }
    }

    public void activate(bool vines)
    {
        if (vines) {
            for (int i = 0; i < 3; i++) {
                Sprites[i].SetActive(true);
            }
        }
        else {
            Sprites[3].SetActive(true);
        }
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.I)) {
            foreach(string drop in inventoryItems)
                print(drop);
        }
    }
}
