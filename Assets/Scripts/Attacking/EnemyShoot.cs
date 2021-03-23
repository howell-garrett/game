using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : TacticsShoot
{
    // Start is called before the first frame update
    void Start()
    {
        attributes = GetComponent<TacticsAttributes>();
        checkedSelectableCells = false;
        isShooting = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
