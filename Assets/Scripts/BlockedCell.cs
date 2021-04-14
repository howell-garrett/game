using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedCell : MonoBehaviour
{
    public GameObject dustPrefab;
    public Cell cell;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Projectile"))
        {
            if (other.gameObject.GetComponent<ProjectileAttributes>().willMiss)
            {
                if (other.gameObject.GetComponent<ProjectileAttributes>().target)
                //Instantiate(other.gameObject.GetComponent<ProjectileAttributes>().impactPrefab, other.gameObject.transform.position, Quaternion.identity);
                Instantiate(dustPrefab, other.gameObject.transform.position, Quaternion.identity);
                Destroy(other.gameObject);
            }
        }
    }
}
