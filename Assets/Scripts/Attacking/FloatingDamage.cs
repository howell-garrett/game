using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{

    public float targetScale;
    public float timeToLerpScale = 1f;
    public float timeToLerpPos = 1f;
    float scaleModifier = 1;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LerpFunction(targetScale, timeToLerpScale));
        StartCoroutine(LerpPosition(transform.position + Vector3.up*1.5f, timeToLerpPos));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator LerpFunction(float endValue, float duration)
    {
        float time = 0;
        float startValue = scaleModifier;
        Vector3 startScale = transform.localScale;

        while (time < duration)
        {
            scaleModifier = Mathf.Lerp(startValue, endValue, time / duration);
            transform.localScale = startScale * scaleModifier;
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = startScale * targetScale;
        scaleModifier = targetScale;
    }

    IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }
}
