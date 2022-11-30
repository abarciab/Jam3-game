using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float range;
    [SerializeField] private float idleTime;
    [SerializeField] private Transform player;
    private Rigidbody2D body;
    private Rigidbody2D playerBody;
    private Vector2 movement;
    private Vector2 originalPos;
    private bool movingBack = false;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        playerBody = player.GetComponent<Rigidbody2D>();
        originalPos = body.position;
    }

    private void FixedUpdate() {
        // if player is in range
        if(Vector2.Distance(body.position, playerBody.position) < range) {
            // stop move back efforts
            movingBack = false;
            StopCoroutine("idleTimer");

            // move enemy towards player
            movement = (playerBody.position - body.position) * speed;
            body.MovePosition(body.position + (movement * Time.fixedDeltaTime));
        }
        // if enemy is moving back to original pos
        else if(movingBack) {
            movement = (originalPos - body.position) * speed;
            body.MovePosition(body.position + (movement * Time.fixedDeltaTime));
        }
        else {
            // if enemy is idle
            movement = Vector2.zero;
            movingBack = false;

            // begin moving back after given time
            if(body.position != originalPos)
                StartCoroutine("idleTimer");
        }
    }

    private IEnumerator idleTimer() {
        yield return new WaitForSeconds(idleTime);
        movingBack = true;
    }
}
