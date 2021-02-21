using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCamera : MonoBehaviour
{
    public float speed;
    public void RotateLeft()
    {
        transform.Rotate(Vector3.up, 90, Space.Self);
    }
    
    public void RotateRight()
    {
        transform.Rotate(Vector3.up, -90, Space.Self);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z + Time.deltaTime * speed);
            transform.position = pos;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z - Time.deltaTime * speed);
            transform.position = pos;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 pos = new Vector3(transform.position.x - Time.deltaTime * speed, transform.position.y, transform.position.z);
            transform.position = pos;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 pos = new Vector3(transform.position.x + Time.deltaTime * speed, transform.position.y, transform.position.z);
            transform.position = pos;
        }

    }
}
