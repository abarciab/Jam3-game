using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [SerializeField] private string nextLevelName;
    [SerializeField] private RectTransform fade;
    private BoxCollider2D box;

    private void Awake() {
        box = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player") {
            //SceneManager.LoadScene(nextLevelName);
            StartCoroutine("startTransition");
        }
    }

    private IEnumerator startTransition() {
        //fade.gameObject.SetActive(true);
        fade.GetComponent<Animator>().Play("FadeOut");
        yield return new WaitUntil(() => fade.GetComponent<FadeTransition>().transitionOver());
        //fade.gameObject.SetActive(false);
        SceneManager.LoadScene(nextLevelName);
    }
}
