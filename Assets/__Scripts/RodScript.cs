using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodScript : MonoBehaviour
{
    Color green;
    Material matWhite;
    Vector3 angleOffset;
    int direction;
    bool y;
    bool active = false;
    float angle;
    float rotationSpeed = 0;
    const float RotationAcceleration = 10000;
    const float  MaxRotationSpeed= 1400;
    const float ShakeTime = 0.3f;

    float timer = 0;
    private void Awake()
    {
        transform.position = Vector3.back * 30;
        green=GetComponent<MeshRenderer>().material.color;

    }
    // Start is called before the first frame update
    void Start()
    {
        LevelHandler.s.availableRods.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                active = false;
                Recycle();
                return;

            }
            rotationSpeed = Approach(rotationSpeed, MaxRotationSpeed, 10000 * Time.deltaTime);
            angle += rotationSpeed * Time.deltaTime;
            if (y)
            {
                transform.rotation = Quaternion.Euler(angleOffset + angle * Vector3.forward);

            }
            else
            {
                transform.rotation = Quaternion.Euler(angleOffset + angle * Vector3.right);
                
            }
            transform.localScale = new Vector3(Approach(transform.localScale.x, 0.05f, Time.deltaTime), Approach(transform.localScale.y, 5, 50 * Time.deltaTime), Approach(transform.localScale.z, 0.05f, Time.deltaTime));
            GetComponent<MeshRenderer>().material.color = ColorLerpWhite(green,1-timer/ShakeTime);
        }
    }

    public void Activate()
    {
        active = true;
    }
    public void Recycle()
    {
        transform.position = Vector3.back * 30;
        LevelHandler.s.availableRods.Add(this);
    }
    public RodScript Setup(Vector2 position)
    {
        LevelHandler.s.availableRods.Remove(this);
        GetComponent<MeshRenderer>().material.color = green;
        timer = ShakeTime;
        rotationSpeed = 0;
        transform.localScale = new Vector3(0.3f, Random.Range(1f, 1.25f), 0.3f);

        //active = true;
        angle = 0;
        direction = Random.value > 0.5 ? 1 : -1;
        y = Random.value > 0.5;
        transform.position = position;
        angleOffset=  new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        transform.rotation = Quaternion.Euler(angleOffset);

        return this;


    }
    public float Approach(float val, float target, float increase)
    {
        return val > target ? Mathf.Max(val - increase, target) : Mathf.Min(val + increase, target);
    }
    //lerp color with white
    public Color ColorLerpWhite(Color color,float lerp)
    {
        return new Color(Mathf.Lerp(color.r,1,lerp), Mathf.Lerp(color.g, 1, lerp), Mathf.Lerp(color.b, 1, lerp),1) ;
    }
}
