using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCam;
    [SerializeField] private float speed;
    [SerializeField] private List<Vector3> panPositions;

    [SerializeField]private Transform panCam;
    private BoxCollider2D box;
    private Vector3 mainCamPos;
    private bool activated;
    private int listIndex;

    private void Awake() {
        //panCam = GetComponentInChildren<Camera>();
        //panCam = transform.GetChild(0);
        box = GetComponent<BoxCollider2D>();
        activated = false;
        listIndex = 0;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player") {
            player.GetComponent<IsometricPlayerMovement>().toggleMovement(false);
            mainCamPos = mainCam.transform.position;

            panCam.gameObject.SetActive(true);
            mainCam.gameObject.SetActive(false);

            panCam.position = mainCamPos;
            panPositions.Insert(0, panCam.position);
            activated = true;
        }
    }

    private void getNext() {
        print(listIndex);
        if(listIndex < panPositions.Count - 1)
            listIndex++;
        else
            listIndex--;
    }

    private void Update() {
        if(activated) {
            //Vector3 newPos = getNext();
            //panCam.transform.position = Vector3.Lerp(panCam.transform.position, newPos, speed * Time.deltaTime);
            panCam.position = Vector3.Lerp(panCam.position, panPositions[listIndex], speed * Time.deltaTime);
            if(listIndex >= 0 && getDistance(panCam.position, panPositions[listIndex]) < 0.1) {//panCam.position == panPositions[listIndex]) {
                print("yo");
                getNext();
            }
            if(getDistance(panCam.position, panPositions[0]) < 0.1 && listIndex == 0) {
                player.GetComponent<IsometricPlayerMovement>().toggleMovement(true);
                panCam.gameObject.SetActive(false);
                mainCam.gameObject.SetActive(true);
                activated = false;
                gameObject.SetActive(false);
            }
        }
    }

    private float getDistance(Vector3 vec1, Vector3 vec2) {
        float x = Mathf.Pow(vec2.x - vec1.x, 2);
        float y = Mathf.Pow(vec2.y - vec1.y, 2);
        float z = Mathf.Pow(vec2.z - vec1.z, 2);
        return Mathf.Sqrt(x + y + z);
    }

    // disable player movement
    // enable pan cam
    // disable main cam
    // lerp pan cam to each vector in list
    // lerp pan cam through list backwards
    // enable main cam
    // disable pan cam
    // enable player movement
    // disable game object
}
