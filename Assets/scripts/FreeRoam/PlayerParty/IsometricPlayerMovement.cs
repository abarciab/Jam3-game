using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricPlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int collisionLayerNumber;
    private Rigidbody2D body;
    private IsometricAnimator animator;
    private Vector2 originalPos;
    private bool movementEnabled;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<IsometricAnimator>();
        originalPos = body.position;
        movementEnabled = true;

        // ignore collisions for EnemyDetector
        Physics2D.IgnoreLayerCollision(transform.GetChild(0).gameObject.layer, collisionLayerNumber);
        Physics2D.IgnoreLayerCollision(transform.GetChild(0).gameObject.layer, gameObject.layer);
    }

    private void FixedUpdate() {
        if(movementEnabled) {
            // get input and store in a vector
            float horizInput = Input.GetAxis("Horizontal");
            float vertInput = Input.GetAxis("Vertical");

            // combine inputs into vector and clamp to avoid faster speed when moving diagonally
            Vector2 totalInput = new Vector2(horizInput, vertInput);
            totalInput = Vector2.ClampMagnitude(totalInput, 1);

            // move player
            Vector2 movement = totalInput * speed;
            body.MovePosition(body.position + (movement * Time.fixedDeltaTime));
            animator.setDirection(totalInput);
        }
    }

    public void toggleMovement(bool on) {
        movementEnabled = on;
    }

    public void resetPosition() {
        body.position = originalPos;
    }
}
