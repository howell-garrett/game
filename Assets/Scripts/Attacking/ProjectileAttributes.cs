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

            Instantiate(impactPrefab, t.position, t.rotation);
            Destroy(gameObject);
            if (target.GetComponent<TacticsAttributes>())
            {
                target.GetComponent<TacticsAttributes>().TakeDamage(damage);
            }
        }
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
