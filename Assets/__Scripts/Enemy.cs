using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BoxActor
{
    protected Sprite sprite;
    protected Player player;
    protected Vector2 launchVector;
    protected LayerMask playerMask;
    protected List<RodScript> attatchedRods=new List<RodScript>();
    protected virtual void Awake()
    {
        //sprite = transform.GetChild(0).GetComponent<Sprite>();
        
        
        playerMask = LayerMask.GetMask("Player");
        groundMask = LayerMask.GetMask("Ground");
        boxCollider = GetComponent<BoxCollider2D>();
        stateManager = new StateManager();
        //Hitbox = new HitBox(24, 32, 0, 16);
        //physicsHitbox = Hitbox.GetPhysicsBox();

    }
    #region variables
    protected int lastHitBy = 0;
    [SerializeField]
    public bool onGround;
    public bool wasOnGround;
    [SerializeField]
    protected Vector2 speed = Vector2.zero;
    protected bool launched=false;
    protected const float BounceTreshold = 100;
    #endregion
    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = Player.s;

    }

    // Update is called once per frame
    public override void Update()
    {
        onGround = CheckOnGround();
        
        base.Update();
        PixMoveX(speed.x * Time.deltaTime, groundMask,HorizontalCollision);
        PixMoveY(speed.y * Time.deltaTime, groundMask,VerticalCollision);
        
        wasOnGround = onGround;
    }
    void HorizontalCollision()
    {
        if (launched)
        {
            //bounce
            if (Mathf.Abs(speed.x) < BounceTreshold)
            {
                speed.x = 0;
            }
            else
            {
                speed.x = -speed.x * 0.50f;
            }
            
        }
    }
    void VerticalCollision()
    {
        if (launched)
        {
            //bounce
            if (Mathf.Abs(speed.y) < BounceTreshold)
            {
                speed.y = 0;
            }
            else
            {
                speed.y = -speed.y * 0.50f;
            }
        }
        else
        {
            if (speed.y < 0)
            {
                speed.y = 0;
            }
        }
    }
    public virtual void Hit(int damage,float attackId,Vector2 launchVector,float staggerTime=0)
    {
        if (hitBy.Contains(attackId))
        {
            return;
        }
        hitBy.Add(attackId);
        if (launchVector.magnitude>1.5)
        {
            speed = launchVector;
        }
        else
        {
            Stagger(launchVector);
        }
        
    }
    //return true if succefully hit
    public virtual bool SkeweredHit(int damage, float attackID, Vector2 launchVector, bool strong = false)
    {
        return true;
    }
    protected float DistanceFromPlayer()
    {
        return (player.Position - Position).magnitude*wsRatio;
    }
    protected bool CheckForWall(Vector2 offsetWS)
    {
        return (Physics2D.OverlapBox(Position + offsetWS, physicsHitbox.wsSize, 0, groundMask));
    }
    protected bool CheckIfOnLedge(bool movingRight)
    {
        if (movingRight)
        {
            //Debug.Log(Physics2D.OverlapBox(new Vector2(RightWS, BottomWS) + pixToWorld * new Vector2(1, -1), pixToWorld * Vector2.one, 0, groundMask));
            return (!Physics2D.OverlapBox(new Vector2(RightWS,BottomWS) + pixToWorld * new Vector2(2, -2), 5*pixToWorld * Vector2.one, 0, groundMask));
        }
        else
        {
            return (!Physics2D.OverlapBox(new Vector2(LeftWS,BottomWS) - pixToWorld * Vector2.one*2,5* pixToWorld * Vector2.one, 0, groundMask));
        }
         

    }
    protected virtual void Stagger(Vector2 launchVector)
    {
        if(onGround && (launchVector==Vector2.right|| launchVector == Vector2.left))
        {
            speed = new Vector2(launchVector.x * 30, 100);
        }
        else
        {
            speed = launchVector.normalized * 50;
        }
    }
    //function call when hitting wall adjust later
    private void yZero()
    {
        speed.y = 0;
    }
    //only for testing remove later
    private void Gravity()
    {
        if (onGround)
        {
            speed.x = Approach(speed.x, 0, 200* Time.deltaTime);
        }
        else
        {
            speed.y = Approach(speed.y, -250, 1000 * Time.deltaTime);
        }
    }
    protected void ActivateAttatchedRods()
    {
        foreach(RodScript rs in attatchedRods)
        {
            rs.Activate();
        }
        attatchedRods.Clear();
    }
}
