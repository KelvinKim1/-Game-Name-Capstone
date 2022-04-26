using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dino : Enemy
{
    // Start is called before the first frame update

    protected override void Awake()
    {
        //sprite stuff later
        attackBoxSize = Vector2.one * 1.5f;
        attackBoxOffset = new Vector2(0.75f, 0.75f);
        sprite = transform.GetChild(0).GetComponent<Sprite>();
        base.Awake();
        Hitbox = new HitBox(24, 32, 0, 16);
        physicsHitbox = Hitbox.GetPhysicsBox();
        stateManager.AddState("normalState", NormalUpdate, NormalStart);
        stateManager.AddState("chaseState", ChaseUpdate, ChaseStart);
        stateManager.AddState("staggeredState", StaggeredUpdate, StaggeredStart,StaggeredEnd);
        stateManager.AddState("attackState",AttackUpdate,AttackStart);
        stateManager.AddState("skeweredState", SkeweredUpdate, SkeweredStart, SkeweredEnd);
        stateManager.currentState = "normalState";
        //stateManager.AddState
    }
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        //timers
        if (skeweredTimer > 0)
        {
            skeweredTimer -= Time.deltaTime;
        }
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        if (staggerTimer > 0)
        {
            staggerTimer -= Time.deltaTime;
        }
        if (changeDirectionTimer > 0)
        {
            changeDirectionTimer -= Time.deltaTime;
        }
        //gravity moved to other states
        if (speed.x > 0)
        {
            facing = 1;
        }
        else if (speed.x < 0)
        {
            facing = -1;
        }
        base.Update();
        SpriteUpdate();


    }
    public void Gravity()
    {
        if (!onGround)
        {
            speed.y = Approach(speed.y, -250, 1000 * Time.deltaTime);

        }
    }

    private void SpriteUpdate()
    {
        sprite.Scale = new Vector3(Approach(Mathf.Abs(sprite.Scale.x), 1, 1.75f * Time.deltaTime), Approach(Mathf.Abs(sprite.Scale.y), 1, 1.75f * Time.deltaTime), 1);
        sprite.Flip(facing);
        //logic for animations here
        //names Idle,Hurt,Run.Walk
        if (stateManager.currentState == "staggeredState" || stateManager.currentState == "attackState")
        {
            return;
        }

        if (Mathf.Abs(speed.x) > 80)
        {
            sprite.Play("Run");
        }
        else if (Mathf.Abs(speed.x) > 10)
        {
            sprite.Play("Walk");
        }
        else
        {
            sprite.Play("Idle");
        }
    }
    //normal is walk here
    #region normalState
    public void NormalStart()
    {
        changeDirectionTimer = ChangeTime;
        walkDirection = Random.value > 0.5 ? 1 : -1;
    }
    private string NormalUpdate()
    {


        if (CanAttackPlayer())
        {
            return ("chaseState");
        }
        if (walkDirection == 0)
        {
            //idle
            if (changeDirectionTimer < 0)
            {
                walkDirection = Random.value > 0.5 ? 1 : -1;
                changeDirectionTimer = 5f;
            }
        }
        else
        {
            //walk
            if (changeDirectionTimer < 0 && Random.value < 0.01)
            {
                //can change direction or enter idle state
                if (Random.value > 0.6f)
                {
                    walkDirection = 0;
                    changeDirectionTimer = 7f;
                }
                else
                {
                    changeDirectionTimer = 5f;
                    walkDirection *= -1;
                }
            }

            if (CheckForWall(xPixel * Mathf.Sign(speed.x)) || CheckIfOnLedge(speed.x > 0))
            {
                speed.x = 0;
                walkDirection *= -1;
            }



        }
        if (Mathf.Abs(WalkSpeed) > WalkSpeed)
        {
            speed.x = Approach(speed.x, walkDirection * WalkSpeed, 1000 * Time.deltaTime);
        }
        else
        {
            speed.x = Approach(speed.x, walkDirection * WalkSpeed, WalkAccel * Time.deltaTime);
        }
        
        
        Gravity();
        //if (CheckForWall(xPixel * Mathf.Sign(speed.x)) || CheckIfOnLedge(speed.x>0))
        //{
        //    speed = new Vector2(-speed.x, speed.y);
        //}



        return "normalState";
    }
    #endregion
    #region VC
    private int walkDirection = 1;
    private float changeDirectionTimer = 0;
    private float staggerTimer = 0;

    private const float MaxFall= -440;
    private const float ChangeTime = 4f;
    private const float WalkSpeed = 36f;
    private const float RunSpeed = 100f;
    private const float RunAccel = 240f;
    private const float WalkAccel = 80f;
    private const float AttackHeight = 15f;
    private const float LaunchedCheck = 500;
    private const float SkewerTime = 1.75f;
    private float skeweredTimer = 0;
    private int skewerCount=0;
    private const int MaxSkewer = 4;
    

    #endregion
    #region ChaseState
    private void ChaseStart()
    {

    }
    private string ChaseUpdate()
    {
        if (!onGround)
        {
            speed.y = Approach(speed.y, -250, 1000 * Time.deltaTime);

        }
        //exit chase state
        if (CanStopChasing())
        {
            return ("normalState");
        }
        //enter attack state
        if (Physics2D.OverlapBox((Vector2)transform.position + new Vector2(facing * attackBoxOffset.x, attackBoxSize.y), attackBoxSize, 0, playerMask))
        {
            if (attackTimer <= 0)
            {
                return "attackState";
            }
            else
            {
                speed.x = 0;
            }
        }
        else
        {
            walkDirection = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
            speed.x = Approach(speed.x, walkDirection * RunSpeed, RunAccel * Time.deltaTime);
        }


        //walkDirection = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
        //speed.x = Approach(speed.x, walkDirection * RunSpeed, RunAccel * Time.deltaTime);

        return "chaseState";
    }
    #endregion
    #region SkeweredState
    private void SkeweredStart()
    {
        speed = Vector2.zero;
        //idk?
    }
    private string SkeweredUpdate()
    {

        if (skeweredTimer < 0 || skewerCount >= MaxSkewer)
        {
            return "staggeredState";
            //transition into staggeredState
        }
        return "skeweredState";
    }
    public override bool SkeweredHit(int damage, float attackId, Vector2 launch, bool strong =false)
    {
        
        if (hitBy.Contains(attackId))
        {
            return false;
        }
        hitBy.Add(attackId);
        skewerCount += 1;

        attatchedRods.Add(LevelHandler.s.CreateRod(Position));

        if (launch.magnitude > 1.5)
        {
            launchVector = launch;
        }
        else
        {
            if (onGround && (launch == Vector2.right || launch == Vector2.left))
            {
                launchVector = new Vector2(launch.x * 30, 100);
            }
            else
            {
                launchVector = launch.normalized * 50;
            }
        }
        sprite.Play("Hurt", true);
        if (strong)
        {
            //add current speed to launch speed if in same direction
            //if in skewerd state speed =0 so redundant
            if (stateManager.currentState != "skeweredState")
            {
                if (Mathf.Sign(speed.y) == Mathf.Sign(launchVector.y))
                {
                    launchVector.y += speed.y;
                }
                if (Mathf.Sign(speed.x) == Mathf.Sign(launchVector.x))
                {
                    launchVector.x += speed.x;
                }
            }
            if (stateManager.currentState == "staggeredState")
            {
                stateManager.ResetState();
            }
            stateManager.currentState = "staggeredState";
            ActivateAttatchedRods();
            skewerCount = 0;

            return true;
        }
        if (stateManager.currentState == "skeweredState")
        {
            stateManager.ResetState();
        }
        stateManager.currentState = "skeweredState";
        skeweredTimer = SkewerTime;
        return true;
    }
    void SkeweredEnd()
    {
        if (launchVector.normalized == Vector2.up)
        {
            launchVector *= 0.7f;
        }
        if (skewerCount == 2)
        {
            launchVector *= 1.35f;
        }
        else if (skewerCount == 3)
        {
            launchVector *=1.5f;
        }
        else if (skewerCount == 4)
        {
            launchVector *= 1.65f;
        }
        
        ActivateAttatchedRods();
        skewerCount = 0;
    }

    #endregion
    #region StaggeredState
    public override void Hit(int damage, float attackId, Vector2 launch, float staggerTime = 0)
    {
        if (hitBy.Contains(attackId))
        {
            return;
        }
        hitBy.Add(attackId);
        LevelHandler.s.CreateFx("HitEffect", Position, Vector2.one);
        if (launch.magnitude > 1.5)
        {
            launchVector = launch;
        }
        else
        {
            if (onGround && (launch == Vector2.right || launch == Vector2.left))
            {
                launchVector = new Vector2(launch.x * 30, 100);
            }
            else
            {
                launchVector = launch.normalized * 50;
            }
        }
        if (stateManager.currentState == "staggeredState")
        {
            stateManager.ResetState();
        }
        stateManager.currentState = "staggeredState";
        sprite.Play("Hurt", true);
        staggerTimer = staggerTime;
    }
    public string StaggeredUpdate()
    {
        if (launched && speed.magnitude < LaunchedCheck)
        {
            launched = false;
        }
        if (!onGround)
        {
            if (speed.y < MaxFall)
            {
                speed.y = Approach(speed.y, MaxFall, 1000 * Time.deltaTime);
            }
            else
            {
                speed.y = Approach(speed.y, MaxFall, 1000 * Time.deltaTime);
            }
        }
        //exit Staggered
        if (onGround && staggerTimer <= 0 && !launched)
        {
            return "normalState";
        }
        //Do contact Damage if launched on change hitid on interval

        //friction
        if (onGround)
        {
            speed.x = Approach(speed.x, 0,500 * Time.deltaTime);
        }
        return "staggeredState";
    }
    public void StaggeredStart()
    {
        launched = launchVector.magnitude > LaunchedCheck;
        speed = launchVector;
    }
    public void StaggeredEnd()
    {
        launched = false;
    }
    #endregion
    #region AttackState
    private float attackId;
    private Vector2 attackBoxSize;
    private Vector2 attackBoxOffset;
    //So that they dont Attack
    private float attackTimer;
    private const float AttackTime = 1f;

    private bool CanAttackPlayer()
    {
        if (DistanceFromPlayer() < 120 && CanReachPlayer())
        {
            return true;
            //moved to CanReachPlayer lol
            //check if there's floor to player
            //Vector2 playerToEnemy = transform.position + player.transform.position;
            //Physics2D.OverlapBox(playerToEnemy / 2 - yPixel, new Vector2(Mathf.Abs(transform.position.x - player.transform.position.x), pixToWorld),0,groundMask);
            //check if player is in attacking range vertically

        }
        return false;
    }
    private bool CanReachPlayer()
    {
        //check if player is to high and if they can walk to player
        if (!(player.transform.position.y - transform.position.y < AttackHeight * pixToWorld)) {

            return false;
        }
        if (Physics2D.OverlapBox(new Vector2((player.transform.position.x + transform.position.x) / 2, transform.position.y - 2 * pixToWorld), new Vector2(Mathf.Abs(transform.position.x - player.transform.position.x), 5 * pixToWorld), 0, groundMask))
        {
            return true;
        }


        return false;
    }
    private bool CanStopChasing()
    {

        if (DistanceFromPlayer() > 200)
        {
            return true;
        }
        else if (!CanReachPlayer())
        {
            return true;
        }
        return false;
    }
    private void AttackStart()
    {
        attackId = Time.time;
        attackTimer = AttackTime;
        sprite.Play("Attack");
        StartCoroutine(AttackCoroutine());
    }
    private IEnumerator AttackCoroutine()
    {
        //exit attack state after time when not interrupted
        yield return new WaitForSeconds(0.1f);
        if (stateManager.currentState == "attackState")
        {
            stateManager.currentState = "normalState";
        }
    }
    //cast attack in inherited virtual box actor method 
    public override void AnimationEvent1()
    {
        Attack();
    }
    //TriggerAnimationEvent

    public void Attack()
    {

        Physics2D.OverlapBox((Vector2)transform.position + new Vector2(facing * attackBoxOffset.x, attackBoxSize.y), attackBoxSize, 0, playerMask)?.GetComponent<Player>().Hit(1,attackId, new Vector2(facing, 0));
    }
    private string AttackUpdate()
    {

        if (!onGround)
        {
            speed.y = Approach(speed.y, -250, 1000 * Time.deltaTime);

        }
        return "attackState";
    }
    #endregion
    private void OnDrawGizmos()
    {

        //Gizmos.DrawWireCube(new Vector2((player.transform.position.x + transform.position.x) / 2, transform.position.y - pixToWorld), new Vector2(Mathf.Abs(transform.position.x - player.transform.position.x), pixToWorld));
    }
}
