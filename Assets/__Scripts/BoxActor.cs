using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxActor : MonoBehaviour
{
    protected List<float> hitBy = new List<float>();
    protected const int wsRatio = 16;
    protected const float pixToWorld = 0.0625f;
    protected Vector2 xPixel = new Vector2(0.0625f, 0);
    protected Vector2 yPixel = new Vector2(0, 0.0625f);
    [SerializeField]
    public HitBox hitbox;
    //hitbox-1 pixel
    public BoxCollider2D boxCollider;
    [SerializeField]
    public HitBox physicsHitbox;

    public float remainderX = 0;
    public float remainderY = 0;
    public int facing=1;

    public StateManager stateManager;
    public LayerMask groundMask;

    public Collider2D CheckHorizontalPixel(int offset, LayerMask lm)
    {
        return Physics2D.OverlapBox(Position + offset * xPixel, physicsHitbox.wsSize, 0, lm);
    }
    public Collider2D CheckVerticalPixel(int offset, LayerMask lm)
    {
        return Physics2D.OverlapBox(Position + offset * yPixel, physicsHitbox.wsSize, 0, lm);
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
            if (CheckHorizontalPixel(sign,lm)==null)
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
            if (CheckVerticalPixel(sign,lm)==null)
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

    public virtual void MoveExactX(int x, LayerMask lm, System.Action method = null)
    {
        int sign = (int)Mathf.Sign(x);
        x *= sign;
        while (x > 0)
        {
            if (!CheckHorizontalPixel(sign,lm))
            {
                transform.position += sign * (Vector3)xPixel;
            }
            else
            {
                method?.Invoke();
                return;
            }
            x--;
        }


    }
    public virtual void MoveExactY(int y,LayerMask lm, System.Action method = null)
    {
        int sign = (int)Mathf.Sign(y);
        y *= sign;
        while (y > 0)
        {
            if (!CheckVerticalPixel(sign,lm))
            {
                transform.position += sign * (Vector3)yPixel;
            }
            else
            {
                method?.Invoke();
                return;
            }
            y--;
        }
    }
    //public virtual void PushExactX(int x, Platform platform, int move)
    //{
    //    int sign = (int)Mathf.Sign(x);
    //    x *= sign;
    //    Vector2 finalPos = (Vector2)transform.position + sign * x * xPixel;
    //    while (x > 0)
    //    {
    //        if (!Physics2D.OverlapBox(Position + sign * xPixel, Collider.size * pixToWorld, 0, groundMask))
    //        {
    //            transform.position += sign * (Vector3)xPixel;
    //        }
    //        else
    //        {
    //            OnSquish(platform, finalPos);
    //            return;
    //        }
    //        x--;
    //    }


    //}
    //public virtual void PushExactY(int y, Platform platform, int move)
    //{
    //    int sign = (int)Mathf.Sign(y);
    //    y *= sign;
    //    Vector2 finalPos = (Vector2)transform.position + sign * y * yPixel;
    //    while (y > 0)
    //    {
    //        if (!Physics2D.OverlapBox(Position + sign * yPixel, Collider.size * pixToWorld, 0, groundMask))
    //        {
    //            transform.position += sign * (Vector3)yPixel;
    //        }
    //        else
    //        {
    //            OnSquish(platform, finalPos);
    //            return;
    //        }
    //        y--;
    //    }
    //}

    //public virtual void OnSquish(Platform platform, Vector2 finalPos)
    //{

    //}

    public Vector2 MoveToClosestPixel(Transform tf)
    {


        Vector2 initialPosition = tf.position;
        tf.position = new Vector3(Mathf.Round(tf.position.x * wsRatio) * pixToWorld, Mathf.Round(tf.position.y * wsRatio) * pixToWorld, tf.position.z);
        return initialPosition - (Vector2)tf.position;
    }






    //utillities

    //returns true or false depending on set interval
    public bool OnInterval(float time)
    {
        return Time.time % (time * 2) > time;
    }
    //returns true once every interval
    public bool OnceFloatInterval(float time)
    {
        return (OnInterval(time/2) && !((Time.time - Time.deltaTime) % (time) > time/2));
    }
    public float Approach(float val, float target, float increase)
    {
        return val > target ? Mathf.Max(val - increase, target) : Mathf.Min(val + increase, target);
    }
    public float DistanceFrom(Vector2 v)
    {
        return Mathf.Pow(Mathf.Pow(transform.position.x - v.x, 2f) + Mathf.Pow(transform.position.y - v.y, 2f), 1 / 2f);
    }
    public float DistanceFrom(Vector3 v)
    {
        return Mathf.Pow(Mathf.Pow(transform.position.x - v.x, 2f) + Mathf.Pow(transform.position.y - v.y, 2f), 1 / 2f);

    }
    public Vector3 LerpVector3(Vector3 origin, Vector3 destination, float lerp)
    {
        return new Vector3(Mathf.Lerp(origin.x, destination.x, lerp), Mathf.Lerp(origin.y, destination.y, lerp), Mathf.Lerp(origin.z, destination.z, lerp));
    }
    public virtual void Update()
    {

        if (stateManager.currentState == null || stateManager==null) return;
        //if (prevState == 0)
        //{
        //    prevState = currentState;
        //}
        if (stateManager.prevState != stateManager.currentState)
        {
            

            stateManager.states[stateManager.currentState].coroutine?.Invoke();
            stateManager.states[stateManager.currentState].start?.Invoke();



        }

        stateManager.prevState = stateManager.currentState;
        stateManager.currentState = stateManager.states[stateManager.currentState].update.Invoke();


        // on same frame as prevState endState of that state
        if (stateManager.prevState != stateManager.currentState)
        {
            if (stateManager.prevState != null)
            {
                stateManager.states[stateManager.prevState].end?.Invoke();
            }
        }



    }
    #region hitbox et collisions
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
    public virtual float TopWS
    {
        get
        {
            return (Position.y + hitbox.wsSize.y / 2);
        }
    }
    public virtual float BottomWS
    {
        get
        {
            return (Position.y - hitbox.wsSize.y / 2);
        }
    }
    public virtual float RightWS
    {
        get
        {
            return (Position.x + hitbox.wsSize.x / 2);
        }
    }
    public virtual float LeftWS
    {
        get
        {
            return (Position.x - hitbox.wsSize.x / 2);
        }
    }

    public virtual int Top
    {
        get
        {
            return (int)(Position.y * wsRatio + hitbox.size.y / 2);
        }
    }
    public virtual int Bottom
    {
        get
        {
            return (int)(Position.y * wsRatio - hitbox.size.y / 2);
        }

    }
    public virtual int Left
    {
        get
        {
            return (int)(Position.x * wsRatio - hitbox.size.x / 2);
        }
    }
    public virtual int Right
    {
        get
        {
            return (int)(Position.x * wsRatio + hitbox.size.x / 2);
        }
    }
    public bool CheckOnGround()
    {
        return (CheckVerticalPixel(-1,groundMask));
    }

    #endregion

    //empty virtual functions for animationEvents has to be void since animationEvents cannot give arguments
    public virtual void AnimationEvent1()
    {

    }
}


public class State
{
    public string name;
    public System.Action start;
    public System.Action end;
    public System.Func<string> update;
    //cast coroutine as function and pass into here
    public System.Action coroutine;

    public State(string name, System.Func<string> update = null, System.Action start = null, System.Action end = null, System.Action coroutine = null)
    {
        this.name = name;
        this.start = start;
        this.end = end;
        this.update = update;
        this.coroutine = coroutine;
    }
}

[System.Serializable]
public class StateManager
{
    public string currentState;
    public string prevState;

    public Dictionary<string, State> states = new Dictionary<string, State>();
    public void AddState(string name, System.Func<string> update = null, System.Action start = null, System.Action end = null, System.Action coroutine = null)
    {
        var state = new State(name, update, start, end, coroutine);
        states.Add(name, state);
    }
    public void ResetState()
    {
        //sets previousState to null treating the same
        prevState = null;
    }
}

//hitbox size will be in pixels, basically world space * wsratio
public struct HitBox
{
    public Vector2 size;
    public Vector2 wsSize;
    public Vector2 offset;
    public HitBox(float x, float y, float xOffset, float yOffset)
    {
        size = new Vector2(x, y);
        offset = new Vector2(xOffset, yOffset);
        wsSize = size * 1 / 16;
    }
    public HitBox GetPhysicsBox()
    {
        //0.6-1 works
        return new HitBox(size.x - 1f, size.y - 1f, offset.x, offset.y);
    }
}