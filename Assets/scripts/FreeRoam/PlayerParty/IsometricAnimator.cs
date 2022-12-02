using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricAnimator : MonoBehaviour
{
    public bool round;
    public bool square;

    public string[] stillAnims = {"Still_N", "Still_W", "Still_W", "Still_W", 
                                   "Still_S", "Still_E", "Still_E", "Still_E"};
    public string[] moveAnims = {"Move_N", "Move_W", "Move_W", "Move_W",
                                  "Move_S", "Move_E", "Move_E", "Move_E"};

    private Animator anim;
    private int lastDir;

    private void Awake() {
        anim = GetComponent<Animator>();
        updateShape();
    }

    public void updateShape()
    {
        if (round) {
            for (int i = 0; i < stillAnims.Length; i++) {
                stillAnims[i] += "_round";
            }
            for (int i = 0; i < moveAnims.Length; i++) {
                moveAnims[i] += "_round";
            }
        }
        if (square) {
            for (int i = 0; i < stillAnims.Length; i++) {
                stillAnims[i] += "_square";
            }
            for (int i = 0; i < moveAnims.Length; i++) {
                moveAnims[i] += "_square";
            }
        }

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
