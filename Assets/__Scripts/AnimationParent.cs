using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationParent : MonoBehaviour
{
    private BoxActor parent;
    private void Awake()
    {
        parent = transform.parent.GetComponent<BoxActor>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CallEvent()
    {
        parent.AnimationEvent1();
    }
}
