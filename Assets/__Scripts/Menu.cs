using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Menu : MonoBehaviour
{
    public static Menu s;
    private Animator anim;
    int selected=1;
    
    private void Awake()
    {
        s= this;
        anim = GetComponent<Animator>();
        
    }
    public int Selected
    {
        set
        {
            selected = value;

            if (selected > 3)
            {
                selected = 1;
            }

                if (selected < 1)
                {
                    selected = 3;
                }
            HighlightButton();

        }
        get
        {
            return selected;
        }
    }
    public void Enter()
    {
        anim.Play("Enter1");
    }
    public void Exit()
    {
        anim.Play("Leave1");
    }
    void HighlightButton()
    {
        int x = selected - 1;
        foreach(Text text in transform.GetComponentsInChildren<Text>())
        {
            text.color = Color.white;
        }
        transform.GetChild(x).GetComponent<Text>().color = Color.yellow;
    }

}
