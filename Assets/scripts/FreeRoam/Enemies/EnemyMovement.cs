using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float range;
    [SerializeField] private float idleTime;
    [SerializeField] private Vector2 originalPos;
    [SerializeField] private Transform player;
    private Rigidbody2D body;
    private Rigidbody2D playerBody;
    private Vector2 movement;
    private bool movingBack = false;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        playerBody = player.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        // if player is in range
        if(distanceToPlayer() < range) {
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

    private float distanceToPlayer() {
        float xEquation = Mathf.Pow((player.position.x - body.position.x), 2);
        float yEquation = Mathf.Pow((player.position.y - body.position.y), 2);
        return Mathf.Sqrt(xEquation + yEquation);
    }

    private IEnumerator idleTimer() {
        yield return new WaitForSeconds(idleTime);
        movingBack = true;
    }
}
