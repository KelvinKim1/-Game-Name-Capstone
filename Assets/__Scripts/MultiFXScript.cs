using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiFXScript : MonoBehaviour
{
    Animator over;
    Animator under;
    // Start is called before the first frame update
    private void Awake()
    {
        transform.position = Vector3.back * 20;
        over = GetComponent<Animator>();
        under = transform.GetChild(0).GetComponent<Animator>();

    }
    void Start()
    {
        LevelHandler.s.availableMultiFX.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayFX(string fxName, Vector2 position, Vector2 scale, Vector3 rotation)
    {
        over.Play(fxName, -1, 0);
        under.Play(fxName, -1, 0);
        transform.position = position;
        transform.localScale = scale;
        //add angle here

    }
    public void ResetFX()
    {
        //after fx complete to clean fx and return back to available fx
        LevelHandler.s.availableMultiFX.Add(this);
        transform.position = Vector3.back * 20;
        transform.localScale = Vector3.one;
        //add angle here
    }
}
