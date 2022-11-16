using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script exists to allow an animation event to turn off the gameobject. (specifically, the damage indicator animation, for enemies and characters)
public class damageIndicatorCoordinator : MonoBehaviour
{
    public void TurnOff() {
        gameObject.SetActive(false);
    }
}
