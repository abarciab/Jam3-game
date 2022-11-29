using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCam;
    [SerializeField] private float speed;
    [SerializeField] private List<Vector2> panPositions;
    private Camera panCam;
    private BoxCollider2D box;

    private void Awake() {
        panCam = GetComponentInChildren<Camera>();
        box = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player") {

        }
    }

    private void moveCam() {
        
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
