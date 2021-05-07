using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollisionBehavior : MonoBehaviour
{
    public bool shouldDestroy = true;
    public GameObject hitEffect;
    public int projectileDamage;
    private void OnTriggerEnter(Collider other)
    {
        if (shouldDestroy)
        {
            Transform t = transform;
            if (hitEffect)
            {
                GameObject explosion = Instantiate(hitEffect, transform);
                explosion.transform.SetParent(null);
                Destroy(explosion, 2.5f);
            }
            print(other.tag);
            print(other.name);
            Destroy(gameObject, .1f);
        }

        if (other.GetComponent<TacticsAttributes>())
        {
            other.GetComponent<TacticsAttributes>().TakeDamage(projectileDamage, true);
        }
    }
}
