using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [Header("Throwing")]
    public float throwForce = 9;
    public float spinForce = 1, spinPush = .1f, spinWeightExponent = 3;
    [Space(10)]
    [Header("Movement")]
    public float friction;
    public float radialFriction, stopThreshold, slowDownThreshold,
        slowDownLerp, bounce = 1.5f;

    [Space(10)]
    public float resultViewThreshold = 75;

    [Header("Sounds")]
    public AudioClip[] sounds;

    [HideInInspector]
    public Skipper skip;

    private Rigidbody rb;
    private AudioSource sfx;

    private float spin = 0;
    private bool thrown, turnEnded, resultsViewed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sfx = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        rb.AddForce(rb.angularVelocity.y * Vector3.right * spinPush);

        rb.velocity *= (1 - friction);
        rb.angularVelocity *= (1 - radialFriction);

        if(rb.velocity.magnitude < slowDownThreshold)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, slowDownLerp);
        }
        if (rb.velocity.magnitude < stopThreshold)
        {
            rb.velocity = Vector3.zero;
            if (!turnEnded && rb.position.z > 25)
            {
                turnEnded = true;
                skip.StartTurn();
            }
        }
        if(rb.position.y < -4 && ! turnEnded)
        {
            turnEnded = true;
            skip.StartTurn();
        }
        if(!turnEnded && !resultsViewed && rb.position.z > resultViewThreshold)
        {
            resultsViewed = true;
            CameraPositions.OnResult();
        }
    }

    public void Throw(float spin, float ratio = 1)
    {
        if (thrown)
            return;
        thrown = true;
        this.spin = spin;

        float rad = spin * Mathf.Deg2Rad;
        Vector3 dir = Vector3.forward * Mathf.Cos(rad) +
            Vector3.left * Mathf.Sin(rad);
        
        rb.AddForce(dir.normalized * throwForce, ForceMode.Impulse);

        float r = Mathf.Abs(Mathf.Pow(ratio, 3));
        rb.AddTorque(Vector3.up * spin * spinForce * r, ForceMode.Impulse);
    }

    public float Sigmoid(float value)
    {
        float k = Mathf.Exp(value);
        return k / (1.0f + k);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (sfx && !sfx.isPlaying)
        {
            sfx.clip = sounds[Random.Range(0, sounds.Length)];
            sfx.Play();
        }
        //TODO make custom collission stuff
        if (collision.collider.CompareTag("Rock"))
            rb.velocity *= bounce;
    }
}