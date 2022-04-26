using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BoxActor
{
    Controller controller;
    Sprite sprite;
    GameObject kid;
    LayerMask enemyMask;
    LayerMask eventMask;
    LayerMask interactLayer;
    ParticleSystem softLandPS;
    GameObject softLand;
    GameObject dashParticles;
    ParticleSystem dashParticlesPS;
    //singleton player
    public static Player s;
    AudioSource JumpSFX;
    AudioSource LandSFX;
    AudioSource DashSFX;
    AudioSource FootstepSFX;

    int health = 100;
    int totalHealth = 100;
    int Health
    {
        set
        {
            health = value;
            SetHealthBar(health);
        }
    }
    void SetHealthBar(int health )
    {
        GameObject.Find("HealthBar").GetComponent<RectTransform>().localScale = new Vector3(health / totalHealth * 3.4f, 0.3f, 1);
    }
    //constructor
    
    private void Awake()
    {
        DontDestroyOnLoad(this);
        s = this;
        //particle awake change with own class later
        softLand = GameObject.Find("SoftLand");
        softLandPS = softLand.GetComponent<ParticleSystem>();

        dashParticles = GameObject.Find("DashParticles");
        dashParticlesPS = dashParticles.GetComponent<ParticleSystem>();
        dashParticlesPS.Stop();
        //
        kid = transform.GetChild(0).gameObject;
        sprite = transform.GetChild(0).GetComponent<Sprite>();
        groundMask = LayerMask.GetMask("Ground");
        enemyMask = LayerMask.GetMask("Enemy");
        eventMask = LayerMask.GetMask("Event");
        interactLayer = LayerMask.GetMask("Interactable");

        boxCollider = GetComponent<BoxCollider2D>();
        stateManager = new StateManager();
        stateManager.AddState("normalState", NormalUpdate, NormalStart, NormalEnd);
        stateManager.AddState("attackState", AttackUpdate, AttackStart, AttackEnd);
        stateManager.AddState("dashState", DashUpdate, DashStart, DashEnd, StartDashCoroutine);
        stateManager.AddState("staggeredState", StaggeredUpdate, StaggeredStart);
        Hitbox = new HitBox(24, 32, 0, 16);
        physicsHitbox = Hitbox.GetPhysicsBox();

        JumpSFX = transform.GetChild(2).GetComponent<AudioSource>();
        DashSFX = transform.GetChild(3).GetComponent<AudioSource>();
        LandSFX = transform.GetChild(4).GetComponent<AudioSource>();
        FootstepSFX = transform.GetChild(5).GetComponent<AudioSource>();


    }
    // Start is called before the first frame update
    void Start()
    {
        Health = totalHealth;
        controller = Controller.S;
        stateManager.currentState = "normalState";
    }
    public void ReAwake()
    {
        controller = GameObject.Find("LevelHandler").GetComponent<Controller>();
        softLand = GameObject.Find("SoftLand");
        softLandPS = softLand.GetComponent<ParticleSystem>();

        dashParticles = GameObject.Find("DashParticles");
        dashParticlesPS = dashParticles.GetComponent<ParticleSystem>();
        dashParticlesPS.Stop();
        StartCoroutine(ReStart());
    }
    IEnumerator ReStart()
    {
        yield return null;
        SetHealthBar(health);
        GameObject.Find("TransitionObject").transform.GetChild(0).GetChild(3).GetComponent<Animator>().Play("Leave", -1, 0);
        controller = Controller.S;
    }
    // Update is called once per frame
    public override void Update()
    {
        
        
        //timers
        {
            if (pauseGravTimer > 0)
            {
                pauseGravTimer -= Time.deltaTime;
            }
            if (staggerTimer > 0)
            {
                staggerTimer -= Time.deltaTime;
            }
            if (chargeAttackTimer > ChargeAttackTime)
            {
                charged = true;
                charging = true;
            }
            else if (chargeAttackTimer > 0)
            {
                charging = true;
                charged = false;
            }
            else
            {
                charging = false;
                charged = false;
            }
            if (attackDelay > 0)
            {
                attackDelay -= Time.deltaTime;
            }
            if (jumpGraceTimer > 0) jumpGraceTimer -= Time.deltaTime;
            //get Ground

            if (CheckOnGround())
            {
                onGround = true;
                canDash = true;
                canDoubleJump = true;
                jumpGraceTimer = JumpGraceTime;
            }
            else
            {
                onGround = false;
            }
            if (varJumpTimer > 0)
            {
                varJumpTimer -= Time.deltaTime;
            }
            //if (liftBoostTimer > 0)
            //{
            //    liftBoostTimer -= Time.deltaTime;
            //}
            //else
            //{
            //    liftBoost = Vector2.zero;
            //}
            //facing
            if (speed.x < 0)
            {
                facing = -1;
            }
            else if (speed.x > 0)
            {
                facing = 1;
            }

            // wall slide timer
            //if (wallSlideDir != 0)
            //{
            //    wallSlideTimer -= Time.deltaTime;
            //    wallSlideDir = 0;
            //}
            ////force move x
            //if (forceMoveXTimer > 0)
            //{
            //    forceMoveXTimer -= Time.deltaTime;
            //    input.joy.x = forceMoveX;
            //}
            //else
            //{

            //}

        }

        //find transition zone
        foreach (Collider2D coll in Physics2D.OverlapBoxAll(Position, physicsHitbox.wsSize, 0, eventMask))
        {
            coll.GetComponent<GEvent>().PlayEvent();
        }
        //can interact
        Interactable interactable = null;
        if(onGround && stateManager.currentState == "normalState")
        {
            
            foreach (Collider2D coll in Physics2D.OverlapBoxAll(Position, physicsHitbox.wsSize, 0, interactLayer))
            {
                
                interactable = coll.GetComponent<Interactable>();
                interactable.Activate();

            }
        }
        if (interactable != null)
        {
            //prompt to use

            if (controller.leftStick == Vector2.up)
            {
                interactable.InteractionStart();
            }
        }

        LastAim = controller.leftStick;
        base.Update();
        UpdateSprite();
        PixMoveX(speed.x * Time.deltaTime, groundMask);
        PixMoveY(speed.y * Time.deltaTime, groundMask, OnCollideV);
        wasOnGround = onGround;

    }
    #region physics
    //put these in the pixMoveX, these activate when they collide
    private void OnCollideV()
    {
        //landing
        if (speed.y < 0)
        {
            //hard land
            if (speed.y < -100)
            {
                sprite.Scale = new Vector3(1.25f, 0.8f, 1);
                softLand.transform.position = transform.position;
                softLandPS.Play();
                LandSFX.Play();
            }
            //soft land
            else if (speed.y < -40)
            {
                sprite.Scale = new Vector3(1.1f, 0.9f, 1);
                softLand.transform.position = transform.position;
                softLandPS.Play();
                LandSFX.Play();

            }
            speed.y = 0;
        }
    }
    #endregion
    #region particles
    private void SetDashParticles(Vector2 dir)
    {

        if (dir.magnitude == 1)
        {
            if (dir == Vector2.up)
            {
                dashParticles.transform.localRotation = Quaternion.Euler(Vector3.right * -45);
            }
            else if (dir == Vector2.down)
            {
                dashParticles.transform.localRotation = Quaternion.Euler(Vector3.right * 45);
            }
            else if (dir == Vector2.left)
            {
                dashParticles.transform.localRotation = Quaternion.Euler(Vector3.up * -45);
            }
            else
            {
                dashParticles.transform.localRotation = Quaternion.Euler(Vector3.up * 45);
            }
        }
        //diagonals
        else
        {
            if (dir.y == -1)
            {
                if (dir.x == -1)
                {
                    dashParticles.transform.localRotation = Quaternion.Euler(new Vector2(135, 135));

                }
                else
                {
                    dashParticles.transform.localRotation = Quaternion.Euler(new Vector2(45, 135));
                }
            }
            else
            {
                if (dir.x == -1)
                {
                    dashParticles.transform.localRotation = Quaternion.Euler(new Vector2(-135, 45));
                }
                else
                {
                    dashParticles.transform.localRotation = Quaternion.Euler(new Vector2(-45, 45));

                }
            }
        }
        dashParticlesPS.Play();
    }
    #endregion
    #region Sprite
    public void UpdateSprite()
    {
        sprite.SetSpriteColor(Color.white);
        sprite.Scale = new Vector3(Approach(Mathf.Abs(sprite.Scale.x), 1, 1.75f * Time.deltaTime), Approach(Mathf.Abs(sprite.Scale.y), 1, 1.75f * Time.deltaTime), 1);
        sprite.Flip(facing);
        if (stateManager.currentState == "attackState" || stateManager.currentState == "dashState")
        {
            return;
        }
        if (charged)
        {
            if (OnInterval(0.25f))
            {
                sprite.SetSpriteColor(Color.red);
            }
        }
        else if (charging)
        {
            if (OnInterval(0.1f)) {
                sprite.SetSpriteColor(Color.red);
            }
        }
        // state stuff

        //if (stateManager.currentState == 3)
        //{
        //    sprite.Play(6);
        //}
        //else if (stateManager.currentState == 2)
        //{
        //    sprite.Play(7);
        //}

        //else if (Ducking && stateManager.currentState == 1)
        //{
        //    sprite.Play(3);
        //}
        if (onGround)
        {
            //push anim
            //idle
            if (Mathf.Abs(speed.x) <= RunAccel / 40f && controller.leftStick.x == 0)
            {
                //idlecarry
                //edge
                sprite.Play("Idle");

            }
            //run carry here
            //normal run
            //skid flip
            else
            {
                sprite.Play("Run");
            }
        }
        //wallslide
        //else if (wallSlideDir != 0)
        //{
        //    sprite.Play(5);
        //}
        else if (speed.y > 0)
        {
            //animation that plays depends on which up event is set by jump lmao if already in same animation return so 
            //upEvent setting not implemented yet, put them in the jump functions
            if (upEvent == "jump")
            {
                sprite.Play("jump");
            }
            else if (upEvent == "doubleJump")
            {
                sprite.Play("jump");
            }
            else
            {
                sprite.Play("jump");
            }
        }
        else if (speed.y < 0)
        {
            if (sprite.currentAnimation == "jump")
            {
                sprite.Play("jump2fall");
            }

            else if (sprite.currentAnimation == "jump2fall" && sprite.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
            {
                sprite.Play("fall");
            }
            else
            {
                sprite.Play("fall");
            }
        }
        //else
        //{
        //    sprite.Play(2);
        //}






    }

    #endregion
    #region constants




    ////public static ParticleType P_DashA;
    ////public static ParticleType P_DashB;
    ////public static ParticleType P_CassetteFly;
    ////public static ParticleType P_Split;
    ////public static ParticleType P_SummitLandA;
    ////public static ParticleType P_SummitLandB;
    ////public static ParticleType P_SummitLandC;

    //public const float MaxFall = 480f;
    //private const float Gravity = 2700f;
    //private const float HalfGravThreshold = 100f;

    //private const float FastMaxFall = 720f;
    //private const float FastMaxAccel = 900f;

    //public const float MaxRun = 270f;
    //public const float RunAccel = 2600f;
    //private const float RunReduce = 1000f;
    //private const float AirMult = .65f;

    //private const float HoldingMaxRun = 70f;
    //private const float HoldMinTime = .35f;

    //private const float BounceAutoJumpTime = .1f;

    //private const float DuckFriction = 500f;
    //private const int DuckCorrectCheck = 4;
    //private const float DuckCorrectSlide = 50f;

    //private const float DodgeSlideSpeedMult = 1.2f;
    //private const float DuckSuperJumpXMult = 1.25f;
    //private const float DuckSuperJumpYMult = .5f;

    //private const float JumpGraceTime = 0.1f;
    //private const float JumpSpeed = -315f;
    //private const float JumpHBoost = 50f;
    //private const float VarJumpTime = .2f;
    //private const float CeilingVarJumpGrace = .05f;
    //private const int UpwardCornerCorrection = 4;
    //private const float WallSpeedRetentionTime = .06f;

    //private const int WallJumpCheckDist = 3;
    //private const float WallJumpForceTime = .13f;
    //private const float WallJumpHSpeed = MaxRun;

    //public const float WallSlideStartMax = -60f;
    //private const float WallSlideTime = 1.2f;

    //private const float BounceVarJumpTime = .2f;
    //private const float BounceSpeed = -140f;
    //private const float SuperBounceVarJumpTime = .2f;
    //private const float SuperBounceSpeed = -185f;

    //private const float SuperJumpSpeed = JumpSpeed;
    //private const float SuperJumpH = 260f;
    //private const float SuperWallJumpSpeed = -160f;
    //private const float SuperWallJumpVarTime = .25f;
    //private const float SuperWallJumpForceTime = .2f;
    //private const float SuperWallJumpH = MaxRun + JumpHBoost * 2;

    //private const float DashSpeed = 720f;
    //private const float EndDashSpeed = 400f;
    //private const float EndDashUpMult = .75f;
    //private const float DashTime = .15f;
    //private const float DashCooldown = .2f;
    //private const float DashRefillCooldown = .1f;
    //private const int DashHJumpThruNudge = 6;
    //private const int DashCornerCorrection = 4;
    //private const int DashVFloorSnapDist = 3;
    //private const float DashAttackTime = .3f;

    //private const float BoostMoveSpeed = 80f;
    //public const float BoostTime = .25f;

    //private const float DuckWindMult = 0f;
    //private const int WindWallDistance = 3;

    //private const float ReboundSpeedX = 120f;
    //private const float ReboundSpeedY = -120f;
    //private const float ReboundVarJumpTime = .15f;

    //private const float ReflectBoundSpeed = 220f;

    //private const float DreamDashSpeed = DashSpeed;
    //private const int DreamDashEndWiggle = 5;
    //private const float DreamDashMinTime = .1f;

    //public const float ClimbMaxStamina = 110;
    //private const float ClimbUpCost = 100 / 2.2f;
    //private const float ClimbStillCost = 100 / 10f;
    //private const float ClimbJumpCost = 110 / 4f;
    //private const int ClimbCheckDist = 2;
    //private const int ClimbUpCheckDist = 2;
    //private const float ClimbNoMoveTime = .1f;
    //public const float ClimbTiredThreshold = 20f;
    //private const float ClimbUpSpeed = -45f;
    //private const float ClimbDownSpeed = 80f;
    //private const float ClimbSlipSpeed = 30f;
    //private const float ClimbAccel = 900f;
    //private const float ClimbGrabYMult = .2f;
    //private const float ClimbHopY = -120f;
    //private const float ClimbHopX = 100f;
    //private const float ClimbHopForceTime = .2f;
    //private const float ClimbJumpBoostTime = .2f;
    //private const float ClimbHopNoWindTime = .3f;

    //private const float LaunchSpeed = 280f;
    //private const float LaunchCancelThreshold = 220f;

    //private const float LiftYCap = -130f;
    //private const float LiftXCap = 250f;

    //private const float JumpThruAssistSpeed = -40f;

    //private const float InfiniteDashesTime = 2f;
    //private const float InfiniteDashesFirstTime = .5f;
    //private const float FlyPowerFlashTime = .5f;

    //private const float ThrowRecoil = 80f;
    //private static readonly Vector2 CarryOffsetTarget = new Vector2(0, -12);

    //private const float ChaserStateMaxTime = 4f;

    //public const float WalkSpeed = 64f;

    //public const int StNormal = 0;
    //public const int StClimb = 1;
    //public const int StDash = 2;
    //public const int StSwim = 3;
    //public const int StBoost = 4;
    //public const int StRedDash = 5;
    //public const int StHitSquash = 6;
    //public const int StLaunch = 7;
    //public const int StPickup = 8;
    //public const int StDreamDash = 9;
    //public const int StSummitLaunch = 10;
    //public const int StDummy = 11;
    //public const int StIntroWalk = 12;
    //public const int StIntroJump = 13;
    //public const int StIntroRespawn = 14;
    //public const int StIntroWakeUp = 15;
    //public const int StBirdDashTutorial = 16;
    //public const int StFrozen = 17;
    //public const int StReflectionFall = 18;
    //public const int StStarFly = 19;
    //public const int StTempleFall = 20;
    //public const int StCassetteFly = 21;
    //public const int StAttract = 22;

    //public const string TalkSfx = "player_talk";


    //public const float MaxFall = 480f;
    //private const float Gravity = 2700f;
    //private const float HalfGravThreshold = 100f;

    //private const float FastMaxFall = 720f;
    //private const float FastMaxAccel = 900f;

    //public const float MaxRun = 270f;
    //public const float RunAccel = 2600f;
    //private const float RunReduce = 1000f;
    //private const float AirMult = .65f;

    //private const float HoldingMaxRun = 70f;
    //private const float HoldMinTime = .35f;

    //private const float BounceAutoJumpTime = .1f;

    //private const float DuckFriction = 500f;
    //private const int DuckCorrectCheck = 4;
    //private const float DuckCorrectSlide = 50f;

    //private const float DodgeSlideSpeedMult = 1.2f;
    //private const float DuckSuperJumpXMult = 1.25f;
    //private const float DuckSuperJumpYMult = .5f;

    //private const float JumpGraceTime = 0.1f;
    //private const float JumpSpeed = -315f;
    //private const float JumpHBoost = 50f;
    //private const float VarJumpTime = .2f;
    //private const float CeilingVarJumpGrace = .05f;
    //private const int UpwardCornerCorrection = 4;
    //private const float WallSpeedRetentionTime = .06f;

    private const float AirMult = 0.65f;
    private const float RunAccel = 2600f;
    private const float RunReduce = 1000f;
    private const float MaxRun = 270;
    private const float MaxFall = -480;
    private const float FastMaxFall = -720;
    private const float FastMaxAccel = 900f;
    private const float HalfGravThreshold = 100f;
    private const float Gravity = 2700;
    private const float JumpSpeed = 315;
    private const float VarJumpTime = 0.2f;
    private const float JumpHBoost = 50f;
    private const float JumpGraceTime = 0.1f;
    private const float DashSpeed = 720f;
    private const float EndDashSpeed = 400f;
    private const float EndDashUpMult = .75f;
    private const float DashTime = .20f;
    private const float DashCooldown = .2f;
    private const float AttackDelay = .5f;

    private const float ChargeAttackTime = 0.5f;
    private const float LaunchedCheck = 500f;
    private const float PauseGravTime = 0.2f;
    
    #endregion
    #region variables

    [SerializeField]
    private Vector2 speed = Vector2.zero;
    [SerializeField]
    private bool onGround = false;
    [SerializeField]
    private bool wasOnGround = false;
    [SerializeField]
    private float maxFall;
    [SerializeField]
    private float varJumpTimer;
    private float varJumpSpeed;
    private float jumpGraceTimer;
    private Vector2 dashDir;
    private Vector2 lastAim;
    private float attackDelay;

    private float chargeAttackTimer = 0;
    private float staggerTimer = 0;
    private bool charging = false;
    private bool charged = false;

    private bool launched;
    private bool canInteract;
    private Vector2 launchVector;

    private float pauseGravTimer=0;

    private bool canDash = false;
    private bool canDoubleJump = false;
    private string upEvent;
    #endregion
    #region normalState

    private void NormalStart()
    {
        maxFall = MaxFall;

    }
    private void NormalEnd()
    {

    }
    private string NormalUpdate()
    {
        //if (LiftBoost.y < 0 && wasOnGround && !onGround && Speed.y <= 0)
        //    Speed.y = LiftBoost.y;

        //for testing 
        //if (CanDash)
        //{
        //    input.ConsumeGrabBuffer();
        //    Die();
        //}
        //if (CanGrab)
        //{
        //    input.ConsumeGrabBuffer();
        //    Pickup();
        //}
        //FreeBallCheck();
        //if (CanThrow)
        //{
        //    input.ConsumeGrabBuffer();

        //    Throw(-LastAim);
        //    if (onGround && LastAim.y < 0)
        //    {

        //    }
        //    else
        //    {
        //        return StartDash();
        //    }

        //}

        //if (!Ducking && onGround && input.joy.y == -1 && Speed.y <= 0)
        //{
        //    Ducking = true;
        //    sprite.Scale = new Vector3(.8f, 1.2f, 1);

        //}
        //if (Ducking && input.joy.y != -1)
        //{
        //    if (CanUnduck)
        //    {
        //        Ducking = false;
        //    }
        //}
        if (controller.dashButtonDown && controller.dashButtonTimer > 0 && canDash) 
        {
            
            return BeginDash();

        }

        //running friction
        //if (Ducking && onGround)
        //    Speed.x = Approach(Speed.x, 0, DuckFriction * 3 * Time.deltaTime);
        //else



        //attack logic
        //charge
        if (controller.attackButtonDown)
        {
            ChargeAttack();
        }
        else if (chargeAttackTimer >= ChargeAttackTime)
        {
            chargeAttackTimer = 0;

            //chargeAttack
            if (controller.leftStick == Vector2.up)
            {
                attackType = "upperCut";
                return "attackState";
            }
            else
            {
                attackType = "strike";
                return "attackState";
            }
        }
        else if (chargeAttackTimer > 0)
        {
            chargeAttackTimer = 0;
            attackType = "landCombo1";
            return "attackState";
        }
        //if (controller.attackButtonDown)
        //{
        //    ChargeAttack();
        //}
        //else if (chargeAttackTimer >= ChargeAttackTime)
        //{
        //    chargeAttackTimer = 0;

        //    //chargeAttack
        //    LaunchCoalBall(Position, 500 * controller.leftStick);
        //}
        //else if (chargeAttackTimer > 0)
        //{
        //    chargeAttackTimer = 0;
        //    float ballSpeed = chargeAttackTimer / ChargeAttackTime * 500;
        //    ballSpeed = ballSpeed < 200 ? 200 : ballSpeed;
        //    LaunchCoalBall(Position, ballSpeed*controller.leftStick);


        //}


        //x velocity
        {
            float mult = onGround ? 1 : AirMult;
            //if (onGround && level.CoreMode == Session.CoreModes.Cold)
            //    mult *= .3f;

            float max = MaxRun;
            //float max = holding == null ? MaxRun : MaxRun;
            //if (level.InSpace)
            //    max *= SpacePhysicsMult;
            if (Mathf.Abs(speed.x) > max && Mathf.Sign(speed.x) == controller.leftStick.x)
                speed.x = Approach(speed.x, max * controller.leftStick.x, RunReduce * mult * Time.deltaTime);  //Reduce back from beyond the max speed
            else
            {
                speed.x = Approach(speed.x, max * controller.leftStick.x, RunAccel * mult * Time.deltaTime);   //Approach the max speed
            }
            if (sprite.IsPlaying("Run"))
            {
                if (OnceFloatInterval(0.05f))
                {
                    FootstepSFX.Play();
                }
            }

        }
        //vertical
        //calculate maxfall
        {
            float mf = MaxFall;
            float fmf = FastMaxFall;
            //adjust mf fmf here
            if (controller.leftStick.y == -1 && speed.y <= mf)
            {
                maxFall = Approach(maxFall, fmf, FastMaxAccel * Time.deltaTime);

            }
            else
            {
                maxFall = Approach(maxFall, mf, FastMaxAccel * Time.deltaTime);
            }

        }
        //gravity
        {
            if (!onGround)
            {
                float max = maxFall;
                //wallslide here
                //Wall Slide
                if (controller.leftStick.x == facing)
                {
                    //if (speed.y <= 0 && wallSlideTimer > 0 && holding == null && Physics2D.OverlapBox(Position + xPixel * facing, Collider.size * pixToWorld, 0, groundMask) && CanUnduck)
                    //{
                    //    Ducking = false;
                    //    wallSlideDir = facing;
                    //}

                    //if (wallSlideDir != 0)
                    //{
                    //    //if (wallSlideTimer > WallSlideTime * .5f && ClimbBlocker.Check(level, this, Position + Vector2.UnitX * wallSlideDir))
                    //    //    wallSlideTimer = WallSlideTime * .5f;

                    //    max = Mathf.Lerp(-MaxFall, WallSlideStartMax, wallSlideTimer / WallSlideTime);
                    //    //if (wallSlideTimer / WallSlideTime > .65f)
                    //    //    CreateWallSlideParticles(wallSlideDir);
                    //}
                }
                float mult = (Mathf.Abs(speed.y) < HalfGravThreshold) && controller.southButtonDown ? .5f : 1f;
                if(pauseGravTimer>0 && controller.leftStick.y != -1)
                {
                    mult = 0.1f;
                }
                speed.y = Approach(speed.y, max, Gravity * mult * Time.deltaTime);
                if(canDoubleJump && controller.southButtonTimer > 0)
                {
                    //change this to double jump when implemented
                    canDoubleJump = false;
                    Jump();
                }
            }
        }
        if (varJumpTimer > 0)
        {
            if (controller.southButtonDown)
            {
                speed.y = Mathf.Max(speed.y, varJumpSpeed);

            }
            else
            {
                varJumpTimer = 0;
            }
        }
        if (controller.southButtonTimer > 0)
        {
            if (jumpGraceTimer > 0)
            {
                Jump();
            }
            //else if (CanUnduck)
            //{
            //    bool canUnDuck = CanUnduck;
            //    if (canUnDuck && WallJumpCheck(1))
            //    {
            //        //if (Facing == Facings.Right && Input.Grab.Check && Stamina > 0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * WallJumpCheckDist))
            //        //    ClimbJump();
            //        //else if (DashAttacking && DashDir.X == 0 && DashDir.Y == -1)
            //        //    SuperWallJump(-1);
            //        //else
            //        WallJump(-1);
            //    }
            //    else if (canUnDuck && WallJumpCheck(-1))
            //    {
            //        //if (Facing == Facings.Left && Input.Grab.Check && Stamina > 0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * -WallJumpCheckDist))
            //        //    ClimbJump();
            //        //else if (DashAttacking && DashDir.X == 0 && DashDir.Y == -1)
            //        //    SuperWallJump(1);
            //        //else
            //        WallJump(1);
            //    }
            //    //else if ((water = CollideFirst<Water>(Position + Vector2.UnitY * 2)) != null)
            //    //{
            //    //    Jump();
            //    //    water.TopSurface.DoRipple(Position, 1);
            //    //}
            //}
        }


        return "normalState";
    }
    #endregion
    #region Jumps
    //LiftBoost

    public void Jump(bool particles = true, bool playSfx = true)
    {
        JumpSFX.Play();
        // in input on JumpPressed
        controller.ConsumeJumpBuffer();
        jumpGraceTimer = 0;
        varJumpTimer = VarJumpTime;
        sprite.Scale = new Vector3(0.8f, 1.25f, 1);

        //AutoJump = false;
        //dashAttackTimer = 0;
        //wallSlideTimer = WallSlideTime;
        //wallBoostTimer = 0;

        speed.x += JumpHBoost * controller.leftStick.x;
        speed.y = JumpSpeed;
        //speed += LiftBoost;
        varJumpSpeed = speed.y;
        
        //LaunchedBoostCheck();

        //if (playSfx)
        //{
        //    if (launched)
        //        Play(Sfxs.char_mad_jump_assisted);

        //    if (dreamJump)
        //        Play(Sfxs.char_mad_jump_dreamblock);
        //    else
        //        Play(Sfxs.char_mad_jump);
        //}

        //sprite.Scale = new Vector2(.8f, 1.2f);
        //if (particles)
        //    Dust.Burst(BottomCenter, Calc.Up, 4);

        //SaveData.Instance.TotalJumps++;
    }

    #endregion
    #region staggeredState
    public void Hit(int damage, float attackId, Vector2 launch, float staggerTime = 0.5f)
    {
        if (hitBy.Contains(attackId))
        {
            return;
        }
        hitBy.Add(attackId);
        LevelHandler.s.CreateFx("HitEffect", Position, Vector2.one);
        if (launch.magnitude > 1.5)
        {
            launched = launchVector.magnitude > LaunchedCheck ? true : false;
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
        //exit Staggered
        if (staggerTimer <= 0)
        {
            return "normalState";
        }
        //Do contact Damage if launched on change hitid on interval

        //friction
        if (!onGround)
        {
            speed.y = Approach(speed.y, -250, 1000 * Time.deltaTime);

        }
        else
        {
            speed.x = Approach(speed.x, 0, 80 * Time.deltaTime);
        }
        return "staggeredState";
    }
    public void StaggeredStart()
    {
        speed = launchVector;

    }
    #endregion
    #region attackState

    private void ChargeAttack()
    {
        chargeAttackTimer += Time.deltaTime;
        //flickerSprite
        
    }
    //variables
    public string attackType;
    public Vector2 attackBox;
    public Vector2 attackBoxOffset;
    public int attackDamage;
    public float  attackId;
    private Vector2 attackLaunchVector = Vector2.zero;
    private bool gravityOnAttack;

    private void AttackStart()
    {
        //set launch Vector to start adjust individually on special attacks
        gravityOnAttack = true;
        attackLaunchVector = Vector2.right*facing;
        speed = Vector2.zero;
        //attackId so that attack only register once
        attackId = Time.time;
        //check attack type to set parameters for coroutine, invoke coroutine in start
        if (attackType == "landCombo1")
        {


            sprite.Play("landCombo1");
            speed.x = facing*30;
            attackBox = new Vector2(3, 1.75f);
            attackBoxOffset = new Vector2(1.5f * facing, 1);
            StartCoroutine(LandCombo1Coroutine());
        }
        else if (attackType == "landCombo2")
        {
            sprite.Play("landCombo2");
            speed.x = facing * 50;
            attackBox = new Vector2(3, 1.75f);
            attackBoxOffset = new Vector2(1.5f * facing, 1);
            StartCoroutine(LandCombo2Coroutine());
        }
        else if (attackType == "landCombo3")
        {
            sprite.Play("landCombo1");
            speed.x = facing * 50;
            attackBox = new Vector2(3, 1.75f);
            attackBoxOffset = new Vector2(1.5f * facing, 1);
            StartCoroutine(LandCombo3Coroutine());
        }
        else if (attackType == "upperCut")
        {
            gravityOnAttack = false;
            attackLaunchVector = new Vector2(0,500);
            sprite.Play("upperCut");
            speed = new Vector2(facing * 25, 300);
            attackBox = new Vector2(2.5f, 3);
            attackBoxOffset = new Vector2(0.75f * facing, 1.3f);
            StartCoroutine(UpperCutCoroutine());
        }
        else if (attackType == "strike")
        {
            attackLaunchVector = new Vector2(200*facing, 50);
            sprite.Play("landCombo1");
            speed.x = facing * 300;
            attackBox = new Vector2(3, 1.75f);
            attackBoxOffset = new Vector2(1.5f * facing, 1);
            StartCoroutine(StrikeCoroutine());
        }

    }
    private void AttackEnd()
    {
        speed = Vector2.zero;
    }
    private string AttackUpdate()
    {
        //physics check collider;
        if (gravityOnAttack)
        {
            speed.y = Approach(speed.y, MaxFall, Gravity  * Time.deltaTime);
        }
        Attack(1,attackId,attackLaunchVector,0.35f);
        return "attackState";
    }
    
    //coroutine changes attackbox and confirms follow up on attacks.
    IEnumerator LandCombo1Coroutine()
    {
        
        yield return new WaitForSeconds(0.20f);
        //follow up
        CheckForFollowUp("landCombo2", 0.5f) ;
        
    }
    IEnumerator LandCombo2Coroutine()
    {

        yield return new WaitForSeconds(0.20f);
        if (stateManager.currentState == "attackState")
        {
            
            if (controller.attackButtonTimer > 0)
            {
                if (controller.leftStick == Vector2.right * facing)
                {
                    attackType = "strike";
                    controller.ConsumeAttackBuffer();
                    stateManager.ResetState();
                    stateManager.currentState = "attackState";
                }
                else if (controller.leftStick == Vector2.up)
                {
                    attackType = "upperCut";
                    controller.ConsumeAttackBuffer();
                    stateManager.ResetState();
                    stateManager.currentState = "attackState";
                }
                else
                {
                    attackType = "landCombo3";
                    controller.ConsumeAttackBuffer();
                    stateManager.ResetState();
                    stateManager.currentState = "attackState";
                }
            }
            else
            {
                attackDelay = 0.5f;
                stateManager.currentState = "normalState";
            }
        }

    }
    IEnumerator LandCombo3Coroutine()
    {
        //speed = Vector2.right * facing * 200;
        //yield return new WaitForSeconds(0.02f);

        yield return new WaitForSeconds(0.11f);
        speed = Vector2.zero;
        yield return new WaitForSeconds(0.10f);

        stateManager.currentState = "normalState";
        attackDelay = 1f;
    }
    IEnumerator UpperCutCoroutine()
    {
        //speed = Vector2.right * facing * 200;
        //yield return new WaitForSeconds(0.02f);
        
        yield return new WaitForSeconds(0.13f);
        
        stateManager.currentState = "normalState";
        attackDelay = 1f;
    }
    IEnumerator StrikeCoroutine()
    {
        //speed = Vector2.right * facing * 200;
        //yield return new WaitForSeconds(0.02f);

        yield return new WaitForSeconds(0.15f);
        speed = Vector2.zero;
        yield return new WaitForSeconds(0.05f);

        stateManager.currentState = "normalState";
        attackDelay = 1f;
    }
    //check at the end of corouting for follow up if triggered
    private void CheckForFollowUp(string attackName,float delay)
    {
        if (stateManager.currentState == "attackState")
        {
            if (controller.attackButtonTimer > 0)
            {
                attackType = attackName;
                controller.ConsumeAttackBuffer();
                stateManager.ResetState();
                stateManager.currentState = "attackState";
            }
            else
            {
                attackDelay = delay;
                stateManager.currentState = "normalState";
            }
        }
    }
    private void Attack(int attackValue, float attackId,Vector2 launchVector,float staggerTime=0)
    {
        foreach (Collider2D hits in Physics2D.OverlapBoxAll((Vector2)transform.position + attackBoxOffset, attackBox,0,enemyMask))
        {
            hits.GetComponent<Enemy>().Hit(attackDamage, attackId, attackLaunchVector,staggerTime);
        }
        //Physics2D enemy Scan, attackId so that attacked enemy can't be hit by the same attack   
    }
    #endregion
    #region dashState
    //public bool CanDash
    //{
    //    get
    //    {
    //        return input.GrabPressed;
    //    }
    //}
    private bool StrongDash = false;
    public string BeginDash()
    {
        //dashCanceled = false;
        StrongDash = controller.attackButtonDown && controller.attackButtonTimer > 0;
        if (StrongDash)
        {
            controller.ConsumeAttackBuffer();
            chargeAttackTimer = 0;
        }
        canDash = false;
        controller.ConsumeDashBuffer();
        return "dashState";
    }
    public void DashStart()
    {
        DashSFX.Play();
        pauseGravTimer = 0;
        attackId = Time.time;
        speed = Vector2.zero;
    }
    public void DashEnd()
    {
        StrongDash = false;
        succesfulDashAttack = false;
    }
    private bool succesfulDashAttack = false;
    public string DashUpdate()
    {
        //if (dashCanceled)
        //{
        //    return 1;
        //}
        if (!StrongDash)
        {
            StrongDash = controller.attackButtonDown && controller.attackButtonTimer > 0;
        }
        else
        {
            controller.ConsumeAttackBuffer();
            chargeAttackTimer = 0;
        }

        foreach ( Collider2D coll in Physics2D.OverlapBoxAll(Position, physicsHitbox.wsSize, 0, enemyMask)){
            if (! coll.GetComponent<Enemy>().onGround)
            {
                succesfulDashAttack=coll.GetComponent<Enemy>().SkeweredHit(1,attackId,-dashDir*500,StrongDash);
                canDash = true;
                canDoubleJump = true;
                if (dashDir.y <= 0)
                {
                    pauseGravTimer = PauseGravTime;
                }
                //put changing into animations here
                if (succesfulDashAttack)
                {
                    sprite.Play("Spin");
                }
            }
        }
        return "dashState";
    }

    //cast Coroutine as method
    public void StartDashCoroutine()
    {

        StartCoroutine(DashCoroutine());
    }
    public IEnumerator DashCoroutine()
    {
        //start next frame after first frame of dashUpdate
        
        var dir = LastAim;
        //animations && scale
        
        if((dir.magnitude==1 && dir.y == 0) || dir.magnitude>1)
        {
            sprite.Play("DashHorizontal");
            sprite.Scale = new Vector3(1,0.7f,1);
        }

        float angle = 0;
        dashSpriteAngle = 0;
        //dpad Directions
        if (dir.magnitude == 1)
        {
            if (dir == Vector2.up)
            {
                angle = -180;
            }
            else if (dir == Vector2.right)
            {
                angle = 90;
            }
            else if (dir == Vector2.left)
            {
                angle = -90;
            }
        }
        //diagonals
        else
        {
            if (dir.y == -1)
            {
                if(dir.x == -1)
                {
                    angle = -45;
                    setChild(20f, Vector2.right * 0.25f);
                    dashSpriteAngle = 20f;
                }
                else
                {
                    angle = 45;
                    setChild(-20f, Vector2.left * 0.25f);
                    dashSpriteAngle = -20;
                }
            }
            else
            {
                if (dir.x == -1)
                {
                    angle = -135;
                    setChild(-20f, Vector2.left * 0.25f);
                    dashSpriteAngle = -20;
                }
                else
                {
                    angle = 135;
                    setChild(20f, Vector2.right * 0.25f);
                    dashSpriteAngle = 20;
                }
            }
        }
        LevelHandler.s.CreateFx("DashLine", Position, new Vector2(1.5f, 1.5f), angle-90);
        SetDashParticles(dir);
        Camera.s.SetCameraShake(dir,6,0.3f);
        //LevelHandler.s.CreateRod(transform.position);


        //trail.gameObject.transform.position = Position + dir * pixToWorld * 10;
        //trail.emitting = true;
        yield return null;
        Invoke("CreateMirage", 0.01f);
        Invoke("CreateMirage", 0.10f);
        Invoke("CreateMirage", 0.18f);



        //setShake = new Vector3(dir.x, dir.y, 5f);
        //camShakeTime = 0.20f;
        //input.Rumble(2, 0.25f);

        //DashFlame.s.ActivateFlame(angle,Vector2.zero);


        if (dir.magnitude != 1)
        {
            dir *= 0.72f;
        }
        dashDir = dir;
        var newSpeed = dir * DashSpeed;
        speed = newSpeed;

        //set scale again
        if (dir.magnitude == 1 && dir.y == 0)
        {
            sprite.Scale = new Vector3(1.3f, sprite.Scale.y, 1);
        }

        yield return new WaitForSeconds(DashTime);
        //if (!dashCanceled)
        //{
        //    trail.emitting = false;
        //    if (DashDir.y >= 0)
        //    {
        //        Speed = DashDir * EndDashSpeed;
        //    }
        //    if (Speed.y > 0)
        //        Speed.y *= EndDashUpMult;

        //    stateManager.currentState = 1;
        //}

        //trail.emitting = false;
        //DashFlame.s.RecycleFlame();

        setChild(0, Vector2.zero);

        if (dashDir.y >= 0)
        {
            speed = dashDir * EndDashSpeed;
        }
        if (speed.y > 0)
            speed.y *= EndDashUpMult;
        stateManager.currentState = "normalState";
        //else
        //{
        //    yield return new WaitForSeconds(0.1f);
        //    trail.emitting = false;
        //}

    }

    //public void ActivateMirage()
    //{
    //    foreach (Mirage m in mirages)
    //    {
    //        if (!m.gameObject.activeInHierarchy)
    //        {
    //            m.Set(transform.position);
    //            return;
    //        }
    //    }
    //}
    public Vector2 LastAim
    {
        set
        {
            lastAim = value;
        }
        get
        {
            if (lastAim == Vector2.zero)
            {
                return Vector2.right * facing;
            }
            else
            {
                return lastAim;
            }

        }
    }
    float dashSpriteAngle;
    private void CreateMirage()
    {
        //levelHandler makeMirage
        Debug.Log(sprite.transform.localRotation.z);
        LevelHandler.s.MakeMirage(sprite.transform.position, sprite.transform.localScale, sprite.SR,dashSpriteAngle);

    }
    public void setChild(float angle, Vector2 offset)
    {
        kid.transform.rotation = Quaternion.Euler(Vector3.forward * angle);
        kid.transform.localPosition = offset;
    }
    #endregion
    #region ActiveItems
    private void LaunchCoalBall(Vector2 position,Vector2 speed)
    {
        LevelHandler.s.MakeCoalBall(position, speed);
    }
    #endregion
    //utilities mov around later
    public int GetPixelsFromGround()
    {
        Vector2 iPos = transform.position;
        if (onGround)
        {
            return (0);
        }
        else
        {
            int k=0;
            foreach(int i in new int[]{4,8,12,16,20,24}){
                k = i;
                transform.position = iPos - i * yPixel;
                if (CheckOnGround())
                    break;
            }
            foreach (int i in new int[] {1,2,3})
            {
                transform.position = iPos +(-k + i) * yPixel;
                if (!CheckOnGround())
                {
                    return (k - i+1);
                }
            }
            return (k);

        }

    }
    #region Gizmos
    private void OnDrawGizmos()
    {
        if (stateManager.currentState== "attackState")
        {
            Gizmos.DrawWireCube((Vector2)transform.position + attackBoxOffset, attackBox);
        }
        
        
    }
    //GayDicks
    
    #endregion


}
