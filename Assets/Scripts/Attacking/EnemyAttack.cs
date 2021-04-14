using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : TacticsAttack
{
    // Start is called before the first frame update
    void Start()
    {
        attributes = GetComponent<TacticsAttributes>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
