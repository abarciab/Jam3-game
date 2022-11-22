using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricAnimator : MonoBehaviour
{
    private string[] stillAnims = {"Still_N", "Still_NW", "Still_W", "Still_SW", 
                                   "Still_S", "Still_SE", "Still_E", "Still_NE"};
    private string[] moveAnims = {"Move_N", "Move_NW", "Move_W", "Move_SW",
                                  "Move_S", "Move_SE", "Move_E", "Move_NE"};

    private Animator anim;
    private int lastDir;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    private void setLastDir(Vector2 direction) {
        // find angle, normalize it to a general direction, convert it to a number between 0-7
        float angle = Vector2.SignedAngle(Vector2.up, direction.normalized) + 22.5f;
        angle = angle < 0 ? angle + 360 : angle;
        lastDir = Mathf.FloorToInt(angle / 45);
    }

    public void setDirection(Vector2 direction) {
        string[] animStates = null;

        // if not moving
        if(direction.magnitude < 0.1f) {
            animStates = stillAnims;
        }
        // if moving
        else {
            animStates = moveAnims;
            setLastDir(direction);
        }

        anim.Play(animStates[lastDir]);
    }
}
