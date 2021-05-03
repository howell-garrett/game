using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FadeOut : MonoBehaviour
{
    float count;
    float colorMod;
    MeshRenderer meshRenderer;
    private void Start()
    {
        count = 0;
        colorMod = 0;
        meshRenderer = GetComponent<MeshRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= 0) {
            Destroy(gameObject);
        }
        count += Time.deltaTime;
        if (count > 1)
        {
            colorMod += Time.deltaTime;
            meshRenderer.material.color = new Color(1, 1, 1, 1-colorMod);
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }
        if (count > 2)
        {
            Destroy(gameObject);
        }
    }
}
