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

    private void Update()
    {
        if (!target) { return; }
        if (Vector3.Distance(transform.position, target.transform.position + (Vector3.up * .7f)) >= 0.05f)
        {

            CalculateHeading(target.transform.position + (Vector3.up * .7f));
            SetHorizontalVelocity();

            transform.forward = heading;
            transform.position += velocity * Time.deltaTime;
        } else
        {
            Transform t = transform;
            InstantiateHit(t.position);
            Destroy(gameObject);
            if (target.GetComponent<TacticsAttributes>())
            {
                target.GetComponent<TacticsAttributes>().TakeDamage(damage);
            }
        }
    }

    public float speed = 15f;
    public float hitOffset = 0f;
    public bool UseFirePointRotation;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public GameObject hit;
    public GameObject flash;
    public GameObject[] Detached;

    void Start()
    {
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

    void InstantiateHit(Vector3 posn)
    {
        
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, posn);
        Vector3 pos = posn;
        if (hit != null)
        {
            var hitInstance = Instantiate(hit, pos, rot);
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
        velocity = heading * 6;
    }
}
