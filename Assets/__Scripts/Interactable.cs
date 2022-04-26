using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{

    public float interactTime;

    public virtual void InteractionStart()
    {
        
    }
    public virtual void EndInteraction()
    {

    }
    public virtual void Activate()
    {
        interactTime = 0.1f;
    }
    public virtual void WhileActive()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
     public virtual void Update()
    {
        if (interactTime>0)
        {
            WhileActive();
            interactTime -= Time.deltaTime;
        }
    }
}
