using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAttributes : MonoBehaviour
{
    public bool willMiss;
    public Transform target;
    public int damage;
    public Vector3 velocity = new Vector3();
    public Vector3 heading = new Vector3();
    public GameObject impactPrefab;
    public Status possibleStatusEffect;
    [SerializeField] [Range(0f, 1f)] int statusEffectChance;

    public float speed;
    public float hitOffset = 0f;
    public bool UseFirePointRotation;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public GameObject hit;
    public GameObject flash;
    public GameObject[] Detached;
    public bool isBouncePorjectile;
    public float bounceHeight;
    public float hitDistance;
    Queue<Vector3> bouncePosns = new Queue<Vector3>();

    private void Update()
    {
        if (!target) { return; }
        if (Vector3.Distance(transform.position, target.transform.position + (Vector3.up * .7f)) >= hitDistance)
        {
            if (!isBouncePorjectile)
            {
                CalculateHeading(target.transform.position + (Vector3.up * .7f));
                SetHorizontalVelocity();

                transform.forward = heading;
                transform.position += velocity * Time.deltaTime;
            } else
            {
                if (bouncePosns.Count == 0)
                {
                    bouncePosns = new Queue<Vector3>();
                    List<Vector3> bezPoints = BezierCurveLineRenderer.GetCurvePoints(transform.position, target.position + (Vector3.up * .6f), bounceHeight);
                    foreach (Vector3 vec in bezPoints)
                    {
                        bouncePosns.Enqueue(vec);
                    }
                }
                transform.position = Vector3.MoveTowards(transform.position, bouncePosns.Peek(), Time.deltaTime * speed);
                if (Vector3.Distance(bouncePosns.Peek(), transform.position) <= .05)
                {
                    bouncePosns.Dequeue();
                }
            }
            
        }
        else
        {
            Transform t = transform;
            Destroy(gameObject, 0.1f);
            InstantiateHit(t.position);
            TacticsAttributes targetTA = target.GetComponent<TacticsAttributes>();
            if (GameObject.FindGameObjectsWithTag("Projectile").Length == 1)
            {
                GameStateManager.isAnyoneAttacking = false;
            }
            if (targetTA)
            {
                targetTA.TakeDamage(damage);
                if (possibleStatusEffect != Status.None && targetTA.status == Status.None)
                {
                    if (Random.Range(0f, 1f) < statusEffectChance)
                    {
                        targetTA.status = possibleStatusEffect;
                    }
                }
            }
        }
    }

    void Start()
    {
        if (hitDistance == 0) {
            hitDistance = 0.05f;
        }
        if (bounceHeight == 0) {
            bounceHeight = 3;
        }
        if (flash != null)
        {
            var flashInstance = Instantiate(flash, transform.position, Quaternion.identity);
            flashInstance.transform.forward = gameObject.transform.forward;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(flashInstance, flashPsParts.main.duration);
            }
        }
        Destroy(gameObject, 5);
    }

    private void OnTriggerEnter(Collider other)
    {
        print(333);
    }

    void InstantiateHit(Vector3 posn)
    {
        
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, posn);
        rot.y = Random.Range(0, 360);
        Vector3 pos = posn;
        if (hit != null)
        {
            var hitInstance = Instantiate(hit, pos, rot);
            hitInstance.transform.rotation = rot;
            Destroy(hitInstance, 2);
        }
        Destroy(gameObject);
    }


    void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
        heading.Normalize();
    }

    void SetHorizontalVelocity()
    {
        velocity = heading * speed;
    }
}
