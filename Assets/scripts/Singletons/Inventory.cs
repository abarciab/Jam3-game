using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    private List<string> inventoryItems = new List<string>();

    public List<GameObject> Sprites;
    public GameObject background;
    public List<string> requiredItems;
    int numCollected;

    //testing
    public bool testActivate;

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
        numCollected++;

        if (numCollected == 4) {
            BattleManager.instance.winGame();
        }
    }

    public void activate()
    {
        background.SetActive(true);
        for (int i = 0; i < 4; i++) {
            Sprites[i].SetActive(true);
        }
    }

    private void Update() {
        if (testActivate) {
            activate();
        }
    }

    
}
