using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXScript : MonoBehaviour
{

    //put name of fxs here HitEffect

    Animator animator;
    private void Awake()
    {
        //sets z to -20 behind camera where fx rests when unused
        //LevelHandler.s.availableFX.Add(this);
        transform.position = Vector3.back * 20;
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        LevelHandler.s.availableFX.Add(this);
    }

    // Update is called once per frame
    
    public void PlayFX(string fxName,Vector2 position,Vector2 scale,float rotation=0)
    {
        animator.Play(fxName, -1, 0);
        transform.position = position;
        transform.localScale = scale;
        transform.rotation = Quaternion.Euler(Vector3.forward * rotation);
        //add angle here

    }
    public void ResetFX()
    {
        //after fx complete to clean fx and return back to available fx
        LevelHandler.s.availableFX.Add(this);
        transform.position = Vector3.back * 20;
        transform.localScale = Vector3.one;
        //add angle here
    }
}
