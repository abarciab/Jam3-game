using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayScreenCoordinator : MonoBehaviour
{
    public List<GameObject> texts = new List<GameObject>();

    public void DisplayText(int num)
    {
        print("display");
        gameObject.SetActive(true);
        texts[num - 1].SetActive(true);
    }
}
