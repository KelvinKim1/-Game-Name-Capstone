using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashFlame : MonoBehaviour
{
    public static DashFlame s;
    Animator flameBack;
    Animator flameFront;
    Animator smallFlame1Animator;
    Animator smallFlame2Animator;
    Animator smallFlame3Animator;
    GameObject smallFlame1;
    GameObject smallFlame2;
    GameObject smallFlame3;


    SpriteRenderer front;
    SpriteRenderer flame1;
    SpriteRenderer flame2;
    SpriteRenderer flame3;

    SpriteMask frontMask;
    SpriteMask flame1Mask;
    SpriteMask flame2Mask;
    SpriteMask flame3Mask;
    private bool Active = false;
    private Vector2 offset;
    // Start is called before the first frame update
    private void Awake()
    {
        offset = Vector2.zero;
        transform.position = Vector3.back * 20;
        s= this;
        flameBack = transform.GetChild(0).GetComponent<Animator>();
        flameFront = transform.GetChild(1).GetComponent<Animator>();
        smallFlame1 = transform.GetChild(2).gameObject;
        smallFlame2 = transform.GetChild(3).gameObject;
        smallFlame3 = transform.GetChild(4).gameObject;
        smallFlame1Animator = smallFlame1.GetComponent<Animator>();
        smallFlame2Animator = smallFlame2.GetComponent<Animator>();
        smallFlame3Animator = smallFlame3.GetComponent<Animator>();

        front = transform.GetChild(1).GetComponent<SpriteRenderer>();
        frontMask = transform.GetChild(1).GetComponent<SpriteMask>();

        flame1 = smallFlame1.GetComponent<SpriteRenderer>();
        flame1Mask = smallFlame1.GetComponent<SpriteMask>();

        flame2 = smallFlame2.GetComponent<SpriteRenderer>();
        flame2Mask = smallFlame2.GetComponent<SpriteMask>();

        flame3 = smallFlame3.GetComponent<SpriteRenderer>();
        flame3Mask = smallFlame3.GetComponent<SpriteMask>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            transform.position = (Vector2)Player.s.transform.position + Vector2.up ;
            SetSpriteMasks();
        }
    }
    public void ActivateFlame(float angle, Vector2 offset)
    {
        Active = true;
        SetSmallFlame(smallFlame1);
        SetSmallFlame(smallFlame2);
        SetSmallFlame(smallFlame3);
        transform.rotation = Quaternion.Euler(Vector3.forward * angle) ;
        flameBack.Play("Flame", -1, 0.6f);
        flameFront.Play("Flame", -1, 0.2f);
        smallFlame1Animator.Play("Flame", -1, 0.8f);
        smallFlame2Animator.Play("Flame", -1, 0.8f);
        smallFlame3Animator.Play("Flame", -1, 0.8f);
        

    }
    private void SetSmallFlame(GameObject sFlame)
    {
        
        sFlame.transform.localPosition = new Vector2(Random.Range(0.4f, 0.9f)*(Random.value>0.5f?-1:1),Random.Range(0,0.3f)* (Random.value > 0.5f ? -1 : 1));
        sFlame.transform.localScale = new Vector3(Random.Range(0.2f, 0.5f), Random.Range(0.2f, 0.5f), 1);
    }
    public void RecycleFlame()
    {
        Active = false;
        transform.position = Vector3.back * 20;
    }
    public void SetSpriteMasks()
    {
        frontMask.sprite = front.sprite;
        flame1Mask.sprite = flame1.sprite;
        flame2Mask.sprite = flame2.sprite;
        flame3Mask.sprite = flame3.sprite;

    }
}
