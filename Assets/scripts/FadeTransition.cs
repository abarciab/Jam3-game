using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTransition : MonoBehaviour
{
    private bool over = false;

    private void startTransition() {
        over = false;
    }

    private void endTransition() {
        over = true;
    }
    
    public bool transitionOver() {
        return over;
    }
}
