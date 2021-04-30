using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobblyMotion : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        Vector3 randVec = new Vector3(Random.Range(0f, 0.05f), Random.Range(0f, 0.05f), Random.Range(0f, 0.05f));
        transform.position += randVec;
        Quaternion randQ = new Quaternion(Random.Range(0f, 0.05f), Random.Range(0f, 0.05f), Random.Range(0f, 0.05f), 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, randQ, Time.deltaTime);
    }
}
