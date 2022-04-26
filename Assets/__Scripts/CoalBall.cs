using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalBall : Projectile
{
    Sprite sprite;
    LayerMask enemyMask;
    private const float MaxGravSpeed = -400;
    private const float Gravity=350;
    private const float BounceTreshold = 70f;
    protected override void Awake()
    {
        //sprite = transform.GetChild(0).GetComponent<Sprite>();
        base.Awake();
        enemyMask = LayerMask.GetMask("Enemy");
        //set hitbox
        Hitbox = new HitBox(16, 16, 0, 0);
        physicsHitbox = hitbox.GetPhysicsBox();
        active = false;
        transform.position = Vector3.back * 20;

    }
    // Start is called before the first frame update
    protected override void Start()
    {
        LevelHandler.s.playerItems.Add(this);
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!active)
        {
            return;
        }
        //triggerExplosion
        //if (Physics2D.OverlapCircle(Position, TriggerRadius * pixToWorld))
        //{
        //    foreach (Collider2D coll in Physics2D.OverlapCircleAll(Position, BlastRadius * pixToWorld))
        //    {
        //        Enemy enemy = coll.GetComponent<Enemy>();
        //        enemy.Hit(1, Time.time, (enemy.Position - Position).normalized, 1);
        //    }
        //}
        //gravity
        speed.y = Approach(speed.y, MaxGravSpeed, Gravity * Time.deltaTime);

        PixMoveX(speed.x * Time.deltaTime, groundMask,HorizontalCollision);
        PixMoveY(speed.y * Time.deltaTime, groundMask,VerticalCollision);
        
    }

    //physics
    void HorizontalCollision()
    {
        if (true)
        {
            //bounce
            if (Mathf.Abs(speed.x) < BounceTreshold)
            {
                Explode();
            }
            else
            {
                speed.x = -speed.x * 0.7f;
            }

        }
    }
    void VerticalCollision()
    {
        if (true)
        {
            //bounce
            if (Mathf.Abs(speed.y) < BounceTreshold)
            {
                Explode();
            }
            else
            {
                speed.y = -speed.y * 0.7f;
            }
        }
    }
    void Explode()
    {
        RecycleCoalBall();
    }
    public void ActivateCoalBall(Vector2 position,Vector2 speed)
    {
        LevelHandler.s.playerItems.Remove(this);
        this.speed = speed;
        transform.position = position;
        active = true;
    }
    public void RecycleCoalBall()
    {
        active = false;
        LevelHandler.s.playerItems.Add(this);
        transform.position = Vector3.back * 20;
    }
    

}
