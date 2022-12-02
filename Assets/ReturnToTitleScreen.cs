using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ReturnToTitleScreen : MonoBehaviour
{
    public void Return()
    {
        SceneManager.LoadScene(0);
    }
}
