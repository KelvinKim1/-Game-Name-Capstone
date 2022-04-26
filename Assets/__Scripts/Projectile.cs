using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected Vector2 speed;
    public float remainderX = 0;
    public float remainderY = 0;
    protected const int wsRatio = 16;
    protected const float pixToWorld = 0.0625f;
    protected Vector2 xPixel = new Vector2(0.0625f, 0);
    protected Vector2 yPixel = new Vector2(0, 0.0625f);
    protected LayerMask groundMask;
    protected HitBox hitbox;
    protected HitBox physicsHitbox;
    private BoxCollider2D boxCollider;
    float attackId;
    protected bool active;
    public HitBox Hitbox
    {
        get
        {
            return hitbox;
        }
        set
        {
            hitbox = value;
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider2D>();
            }
            //boxCollider.size = hitbox.size * pixToWorld;
            //boxCollider.offset = hitbox.offset * pixToWorld;
        }
    }
    public Vector2 Position
    {
        get
        {
            return (Vector2)transform.position + hitbox.offset * pixToWorld;
        }
        set
        {
            transform.position = value - hitbox.offset * pixToWorld;
        }
    }
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        //get box collider and ground
        GetComponent<BoxCollider2D>();
        groundMask = LayerMask.GetMask("Ground");
        
        
    }
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
    public virtual void PixMoveX(float amount, LayerMask lm, System.Action method = null)
    {
        amount += remainderX;
        int move = Mathf.RoundToInt(amount);
        remainderX = amount - move;
        int sign = (int)Mathf.Sign(move);
        move *= sign;
        while (move != 0)
        {
            if (CheckHorizontalPixel(sign, lm) == null)
            {

                transform.position = (Vector2)transform.position + sign * xPixel;
                move--;
            }
            else
            {
                method?.Invoke();
                //Debug.Log(Physics2D.OverlapBox((Vector2)transform.position + sign * xPixel + physicsHitbox.offset*pixToWorld, physicsHitbox.size * pixToWorld, 0, lm));
                return;
            }
        }

    }
    public virtual void PixMoveY(float amount, LayerMask lm, System.Action method = null)
    {
        amount += remainderY;
        int move = Mathf.RoundToInt(amount);
        remainderY = amount - move;
        int sign = (int)Mathf.Sign(move);
        move *= sign;
        while (move != 0)
        {
            if (CheckVerticalPixel(sign, lm) == null)
            {
                transform.position = (Vector2)transform.position + sign * yPixel;
                move--;

            }
            else
            {
                method?.Invoke();
                return;
            }
        }

    }
    public Collider2D CheckHorizontalPixel(int offset, LayerMask lm)
    {
        return Physics2D.OverlapBox(Position + offset * xPixel, physicsHitbox.wsSize, 0, lm);
    }
    public Collider2D CheckVerticalPixel(int offset, LayerMask lm)
    {
        return Physics2D.OverlapBox(Position + offset * yPixel, physicsHitbox.wsSize, 0, lm);
    }
    protected float Approach(float val, float target, float increase)
    {
        return val > target ? Mathf.Max(val - increase, target) : Mathf.Min(val + increase, target);
    }

}
