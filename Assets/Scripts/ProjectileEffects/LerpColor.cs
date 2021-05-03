using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpColor : MonoBehaviour
{
    ParticleSystem ps;
    [SerializeField] [Range(0f, 1f)] float lerpTime;

    [SerializeField] Color myColor;

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.startColor = Color.cyan;
    }

    // Update is called once per frame
    void Update()
    {
        //renderer.material.color = Color.Lerp(renderer.material.color, Color.cyan, lerpTime);
    }
}
