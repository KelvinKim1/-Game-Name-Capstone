using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLord : Enemy
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
        stateManager.AddState("shootState", ShootUpdate, ShootStart);
        stateManager.currentState = "normalState";
        //stateManager.AddState
    }
    //variables
    private float changeDirectionTimer;
    private Vector2 idleFloatDirection;
    private float chargeTime = 0;
    
    //constants
    private const float FloatAccel = 100f;
    private const float FloatSpeed = 40f;
    private const float ShootRange = 150f;
    private const float TargetRange = 250f;
    private const float ChargeTime = 4f;
    private const float PeriodTime = 0.75f;
    private const float MaxHoverSpeed = 10f;
    private const float Recoil = 200f;
    

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
        if (InRangeOfPlayer(ShootRange))
        {
            return "shootState";

        }
        //targetRange>ShootRange
        else if (InRangeOfPlayer(TargetRange))
        {
            idleFloatDirection = Player.s.transform.position - transform.position;
        }

        //normal state
        else if (changeDirectionTimer < 0)
        {
            idleFloatDirection = ((Vector2)Random.onUnitSphere).normalized * FloatSpeed;
            changeDirectionTimer = Random.Range(4, 7);
        }

        speed.x = Approach(speed.x, idleFloatDirection.x, FloatAccel * Time.deltaTime);
        speed.y = Approach(speed.y, idleFloatDirection.y, FloatAccel * Time.deltaTime);
        return "normalState";
    }
    private void NormalStart()
    {
        idleFloatDirection = ((Vector2)Random.onUnitSphere).normalized * FloatSpeed;
    }
    private void NormalEnd()
    {

    }


    #endregion
    #region ShootState
    private string ShootUpdate()
    {
        //if out of range return to normalState
        chargeTime += Time.deltaTime;
        if (chargeTime > ChargeTime)
        {
            Fire();
            chargeTime = 0;
        }
        else
        {
            //move
        }
        return "shootState";
    }
    private void ShootStart()
    {
        speed = Vector2.zero;
        chargeTime = 0;
    }
    private void ShootEnd()
    {

    }
    private void Hover()
    {
        speed.x = Approach(speed.x, 0, Time.deltaTime * 30);
        speed.y = Mathf.Sin(Time.time * 360 / PeriodTime) * MaxHoverSpeed;
    }
    private void Fire()
    {
        Vector2 fireDirection =  Player.s.Position- Position;
        //LevelManagerSpawnProjectile 
        //Recoil
        speed = fireDirection.normalized * Recoil;
    }
    #endregion
    private bool InRangeOfPlayer(float range)
    {
        return ((transform.position - Player.s.transform.position).magnitude < range);
    }
    // Start is called before the first frame update


}
