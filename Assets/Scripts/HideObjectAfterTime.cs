using UnityEngine;

public class HideObjectAfterTime : MonoBehaviour
{
    float count;
    public float timeToKill;
    // Start is called before the first frame update
    void Start()
    {
        count = 0;
        if (timeToKill <= 0)
        {
            timeToKill = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        count += Time.deltaTime;
        if (count > timeToKill)
        {
            gameObject.SetActive(false);
            Destroy(this);
        }
    }
}
