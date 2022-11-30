using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCam;
    [SerializeField]private Transform panCam;
    [SerializeField] private float speed;
    [Tooltip("Note: Z must a large enough negative number to view entire tilemap")]
    [SerializeField] private List<Vector3> panPositions;

    private BoxCollider2D box;
    private Vector3 mainCamPos;
    private bool activated;
    private bool goingBack;
    private int listIndex;

    private float startTime;
    private float distance;

    private void Awake() {
        box = GetComponent<BoxCollider2D>();
        activated = false;
        listIndex = 0;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player" && panPositions.Count > 0) {
            // disable player movement and log main camera's position
            player.GetComponent<IsometricPlayerMovement>().toggleMovement(false);
            mainCamPos = mainCam.transform.position;

            // activate panning camera and deactivate main camera
            panCam.gameObject.SetActive(true);
            mainCam.gameObject.SetActive(false);

            // bring panning camera to main camera and insert it's position into the positions list
            panCam.position = mainCamPos;
            panPositions.Insert(0, panCam.position);

            // start timer and activate panning
            startTime = Time.time;
            goingBack = false;
            activated = true;
        }
    }

    private void getNextPosition() {
        // get distance to next point and update index depending on if moving forward or backwards
        if(listIndex < panPositions.Count - 1 && !goingBack) {
            distance = Vector3.Distance(panCam.position, panPositions[listIndex + 1]);
            listIndex++;
        }
        else {
            distance = Vector3.Distance(panCam.position, panPositions[listIndex - 1]);
            listIndex--;
            goingBack = true;
        }
        startTime = Time.time;
    }

    private void Update() {
        if(activated) {
            // get next position if current position reached
            if(panCam.position == panPositions[listIndex])
                getNextPosition();
            
            // lerp camera based on speed and distance covered
            float distanceCovered = (Time.time - startTime) * speed;
            float distanceFraction = distanceCovered / distance;
            panCam.position = Vector3.Lerp(panCam.position, panPositions[listIndex], distanceFraction);

            // if camera reaches original position
            if(panCam.position == panPositions[listIndex] && listIndex == 0) {
                // toggle player movement
                player.GetComponent<IsometricPlayerMovement>().toggleMovement(true);

                // reset back to main camera
                mainCam.gameObject.SetActive(true);
                panCam.gameObject.SetActive(false);

                // reset list, and deactivate game object
                panPositions.RemoveAt(0);
                activated = false;
                gameObject.SetActive(false);
            }
        }
    }
}
