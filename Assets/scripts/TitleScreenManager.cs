using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private string lvl1SceneName;
    [SerializeField] private Transform creditsScreen;
    [SerializeField] private Transform fade;

    public void playGame() {
        StartCoroutine("fadeToGame");
    }

    public void toggleCredits(bool active) {
        creditsScreen.gameObject.SetActive(active);
    }

    private IEnumerator fadeToGame() {
        fade.gameObject.SetActive(true);
        fade.GetComponent<Animator>().Play("FadeOut");
        yield return new WaitUntil(() => fade.GetComponent<FadeTransition>().transitionOver());
        SceneManager.LoadScene(lvl1SceneName);
    }
}
