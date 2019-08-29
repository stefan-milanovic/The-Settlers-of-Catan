using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditAnimator : MonoBehaviour
{
    // Start is called before the first frame update

    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        
        animator.SetBool("Walk", false);
        animator.SetBool("SprintJump", false);
        animator.SetBool("SprintSlide", false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
