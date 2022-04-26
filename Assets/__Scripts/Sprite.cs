using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprite : MonoBehaviour
{
    public string currentAnimation;
    public SpriteRenderer SR;
    public Animator animator;
    public BoxActor actor;


   
    private void Awake()
    {
        actor = transform.parent.GetComponent<BoxActor>();
        SR = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    public void Play(string animationName,bool canInterruptSelf=false)
    {
        //for animations that can interrupt themselves like taking damage
        if (canInterruptSelf)
        {
            animator.Play(animationName, -1, 0);
        }
        else if (currentAnimation != animationName )
        {
            currentAnimation = animationName;
            animator.Play(animationName);
        }
        
    }
    //public int CurrentFrame
    //{
    //    get
    //    {
    //        var cAnimation = animator.GetCurrentAnimatorClipInfo(0)[0];
    //        return Mathf.RoundToInt(cAnimation.weight * cAnimation.clip.frameRate * cAnimation.clip.length);
    //    }
    //}
    public bool IsPlaying(string name)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    public void Flip(int flip)
    {
        var scale = transform.localScale;

        if (flip == 1)
        {
            transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);
        }
       
    }
    public Vector3 Scale
    {
        get
        {
            return transform.localScale;
        }
        set
        {
            value.x = value.x * actor.facing;
            transform.localScale = value;
        }
    }
    public void SetSpriteColor(Color color)
    {
        SR.color = color;
    }
}
