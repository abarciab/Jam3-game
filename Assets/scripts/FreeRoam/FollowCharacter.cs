using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCharacter : MonoBehaviour
{
    [SerializeField] private Transform characterToFollow;
    [SerializeField] private float speed;
    [SerializeField] private float minDistanceToCharacter;

    private MovementLog characterMovementLog;
    private IsometricAnimator anim;
    private Rigidbody2D body;
    private Vector2 movement;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<IsometricAnimator>();
    }

    private void Start() {
        characterMovementLog = characterToFollow.GetComponent<MovementLog>();
        anim = GetComponent<IsometricAnimator>();

        Physics2D.IgnoreLayerCollision(gameObject.layer, characterToFollow.gameObject.layer);
    }

    private void FixedUpdate() {
        // if there is a position to move to and the character to follow is not too close
        if(characterMovementLog.log.Count > 0 && distanceToCharacter() > minDistanceToCharacter) {
            // move to character to follow
            movement = (characterMovementLog.log[0] - body.position) * speed;
            body.MovePosition(body.position + (movement * Time.fixedDeltaTime));
        }
        else
            movement = Vector2.zero;

        anim.setDirection(movement);
    }

    private float distanceToCharacter() {
        float xEquation = Mathf.Pow((characterToFollow.position.x - body.position.x), 2);
        float yEquation = Mathf.Pow((characterToFollow.position.y - body.position.y), 2);
        return Mathf.Sqrt(xEquation + yEquation);
    }

    // FOR DEBUGGING
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            print(gameObject.name + " " + movement);
        }
    }
}
