using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IsometricPlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int collisionLayerNumber;
    private Rigidbody2D body;
    AudioSource source;
    private IsometricAnimator animator;
    private Vector2 originalPos;
    private bool movementEnabled;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<IsometricAnimator>();
        source = GetComponent<AudioSource>();
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

    private void Update()
    {
        float horizInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizInput) > 0 || Mathf.Abs(vertInput) > 0) {
            AudioManager.instance.PlayHere(-2, source);
        }
        else {
            AudioManager.instance.StopSoundHere(-2, source);
        }
    }
}
