using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirage : MonoBehaviour
{
    SpriteRenderer SR;
    private bool Active;
    private float lifeSpan;
    private const float LifeSpan = 0.25f;
    [SerializeField]
    private Color color;
    // Start is called before the first frame update
    private void Awake()
    {
        Active = false;
        SR = GetComponent<SpriteRenderer>();
        
        transform.position = Vector3.back * 20;
        transform.localScale = Vector3.one;
        

    }
    void Start()
    {
        LevelHandler.s.availableMirage.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            lifeSpan -= Time.deltaTime;
            if (lifeSpan < 0)
            {
                RecycleMirage();
            }
            SR.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0,color.a ,lifeSpan / LifeSpan));
        }
    }
    public void ActivateMirage(Vector2 position,Vector2 scale,SpriteRenderer sr,float rotation=0)
    {
        Active = true;
        LevelHandler.s.availableMirage.Remove(this);
        SR.sprite = sr.sprite;
        transform.localScale = scale;
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation * Vector3.forward);
        lifeSpan = LifeSpan;
    }
    public void RecycleMirage()
    {
        Active = false;
        LevelHandler.s.availableMirage.Add(this);
        transform.position = Vector3.back * 20;
        transform.localScale = Vector3.one;
        
    }
}
