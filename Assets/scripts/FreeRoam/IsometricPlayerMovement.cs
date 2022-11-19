using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricPlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D body;
    private IsometricAnimtor animator;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<IsometricAnimtor>();
    }

    private void FixedUpdate() {
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
