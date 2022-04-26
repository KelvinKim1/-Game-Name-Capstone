using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberScript : Enemy
{

    protected override void Awake()
    {
        //sprite stuff later
        //attackBoxSize = Vector2.one * 1.5f;
        //attackBoxOffset = new Vector2(0.75f, 0.75f);
        sprite = transform.GetChild(0).GetComponent<Sprite>();
        base.Awake();
        Hitbox = new HitBox(24, 32, 0, 16);
        physicsHitbox = Hitbox.GetPhysicsBox();
        stateManager.AddState("normalState", NormalUpdate, NormalStart);
        stateManager.currentState = "normalState";
        //stateManager.AddState
    }
    //variables
    private float changeDirectionTimer;
    private Vector2 idleFloatDirection;
    //constants
    private const float FloatAccel= 100f;
    private const float FloatSpeed= 40f;
    
    public override void Update()
    {
        //timers
        {
            changeDirectionTimer -= Time.deltaTime;

        }
        base.Update();
    }
    #region normalState
    private string NormalUpdate()
    {
        //logic to enter chase/attack state

        //normal state
        if (changeDirectionTimer<0)
        {
            idleFloatDirection = ((Vector2)Random.onUnitSphere).normalized *FloatSpeed;
            changeDirectionTimer = Random.Range(4, 7);
        }
        speed.x = Approach(speed.x, idleFloatDirection.x, FloatAccel * Time.deltaTime);
        speed.y = Approach(speed.y, idleFloatDirection.y, FloatAccel * Time.deltaTime);
        return "normalState";
    }
    private void NormalStart()
    {

    }
    private void NormalEnd()
    {

    }
    #endregion
    // Start is called before the first frame update
    
}
