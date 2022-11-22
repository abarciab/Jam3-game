using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementLog : MonoBehaviour
{
    [SerializeField] private int logLength;
    private Rigidbody2D body;
    public List<Vector2> log { get; private set; }

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        log = new List<Vector2>();
    }

    private void FixedUpdate() {
        // log position only if character isn't standing still
        if(log.Count == 0 || (log.Count > 0 && body.position != log[log.Count - 1])) {
            log.Add(body.position);

            // remove oldest position when positions have been logged
            if(log.Count > logLength)
                log.RemoveAt(0);
        }
    }
}
