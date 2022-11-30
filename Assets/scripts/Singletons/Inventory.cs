using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    private List<string> inventoryItems;

    private void Awake() {
        instance = this;
        inventoryItems = new List<string>();
    }

    public void addItem(string dropName) {
        if(!inventoryItems.Contains(dropName))
            inventoryItems.Add(dropName);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.I)) {
            foreach(string drop in inventoryItems)
                print(drop);
        }
    }
}
