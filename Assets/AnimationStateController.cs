using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.J))
        {
            animator.SetBool("isWalking", false);
        }
    }

    public void startWalking()
    {
        animator.SetBool("isWalking", true);
    }

    public void stopWalking()
    {
        animator.SetBool("isWalking", false);
    }
}
