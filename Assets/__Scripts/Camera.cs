using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    // Start is called before the first frame update
    public static Camera s;
    public bool enforceBounds = false;
    private Player player;
    public Vector2 CameraOffset;
    public bool camXLock;
    public bool camYLock;
    public Vector2 camBounds1;
    public Vector2 camBounds2;
    //x2 only distance from center
    public const float camHeight = 14.0625f;
    public const float camWidth = 25;
    protected const int wsRatio = 16;
    protected const float pixToWorld = 0.0625f;
    private void Awake()
    {
        s = this;
        camBounds1 = GameObject.Find("CB1").transform.position;
        camBounds2 = GameObject.Find("CB2").transform.position;
    }
    private void Start()
    {
        player = Player.s;
    }
    private void Update()
    {
        CameraUpdate();
    }
    public Vector2 CameraTarget
    {
        get
        {
            Vector2 target = (Vector2)player.Position + CameraOffset * pixToWorld;
            Vector2 camPos = transform.position;
            if (camXLock && camYLock)
            {
                return camPos;
            }
            else if (camXLock)
            {
                return new Vector2(camPos.x, target.y);
            }
            else if (camYLock)
            {
                return new Vector2(target.x, camPos.y);
            }
            else
            {
                return target;
            }

        }
    }
    public void EnforceCamBoundsX()
    {
        if (transform.position.x - camWidth < camBounds1.x)
        {
            transform.position = new Vector3(camBounds1.x + camWidth, transform.position.y, -10);
        }
        else if (transform.position.x + camWidth > camBounds2.x)
        {
            transform.position = new Vector3(camBounds2.x - camWidth, transform.position.y, -10);
        }
    }
    public void EnforceCamBoundsY()
    {
        if (transform.position.y - camHeight < camBounds1.y)
        {
            transform.position = new Vector3(transform.position.x, camBounds1.y + camHeight, -10);
        }
        else if (transform.position.y + camHeight > camBounds2.y)
        {
            transform.position = new Vector3(transform.position.x, camBounds2.y - camHeight, -10);
        }
    }

    public Vector2 current;
    public const float CamSpeed = 6f;
    public Vector2 CamRemainder;
    //v might be useless
    public float CamDepth = -10;
    public bool fixedCamera = true;
    private Vector3 camBeforeShake;
    private Vector2 setShake;
    public float camShakeTime;
    private int shakePriority;
    private float shakeIntensity;
    public void CameraUpdate()
    {
        if (camBeforeShake == Vector3.zero)
        {
            camBeforeShake = transform.position;
        }
        else
        {
            transform.position = camBeforeShake;
        }

        {
            current = (Vector2)transform.position + CamRemainder;
            //Camera.main.transform.position = new Vector3(Approach(current.x, CameraTarget.x, CamSpeed * Time.deltaTime), Approach(current.y, CameraTarget.y, CamSpeed * Time.deltaTime), CamDepth);
            // Camera (lerp by distance using delta-time)
            //if (InControl || ForceCameraUpdate)
            //{
            //    if (StateMachine.State == StReflectionFall)
            //    {
            //        level.Camera.Position = CameraTarget;
            //    }
            //    else
            //    {
            //        var from = level.Camera.Position;
            //        var target = CameraTarget;
            //        var multiplier = StateMachine.State == StTempleFall ? 8 : 1f;

            //        level.Camera.Position = from + (target - from) * (1f - (float)Math.Pow(0.01f / multiplier, Engine.DeltaTime));
            //    }
            //}
            var from = transform.position;
            var target = CameraTarget;
            if (!enforceBounds)
            {
                if (InBounds())
                {
                    enforceBounds = true;


                }
                else
                {
                    {
                        if (target.x - camWidth < camBounds1.x)
                        {
                            target.x = camBounds1.x + camWidth;
                        }
                        else if (target.x + camWidth > camBounds2.x)
                        {
                            target.x = camBounds2.x - camWidth;
                        }
                    }
                    {
                        if (target.y - camHeight < camBounds1.y)
                        {
                            target.y = camBounds1.y + camHeight;
                        }
                        else if (target.y + camHeight > camBounds2.y)
                        {
                            target.y = camBounds2.y - camHeight;
                        }
                    }
                }
                
            }
            

            // removed multiplayer
            transform.position = from + (Vector3)(target - (Vector2)from) * (1f - (float)Mathf.Pow(0.01f, Time.deltaTime));
            CamRemainder = MoveToClosestPixel(transform);
            if (enforceBounds)
            {
                if (!camXLock)
                {
                    EnforceCamBoundsX();
                }
                if (!camYLock)
                {
                    EnforceCamBoundsY();
                }
            }
            
            
            
        }
        camBeforeShake = transform.position;
        if (camShakeTime > 0)
        {
            camShakeTime -= Time.deltaTime;
            if (setShake != Vector2.zero)
            {
                CameraShake(setShake, shakeIntensity);
            }
            else
            {
                CameraShake(Random.insideUnitCircle, shakeIntensity);
            }
        }
        else
        {
            shakePriority = 0;
        }
    }
    private void CameraShake(Vector2 direction, float intensity)
    {
        transform.position += (Vector3)direction * Random.Range(1, (int)intensity) * pixToWorld;
    }
    public void SetCameraShake(Vector2 direction, float intensity,float shakeTime, int priority=1)
    {
        //if priority is equal or greater than
        if (priority >= shakePriority)
        {
            shakePriority = priority;
            camShakeTime = shakeTime;
            setShake = direction;
            shakeIntensity = intensity;
        }
    }
    //overload for random direction
   public void SetCameraShake(float intensity, float shakeTime, int priority = 1)
    {
        if (priority >= shakePriority)
        {
            shakePriority = priority;
            camShakeTime = shakeTime;
            setShake = Vector2.zero;
            shakeIntensity = intensity;
        }
    }
    public bool InBounds()
    {
        if (transform.position.y - camHeight < camBounds1.y)
        {
            return false;
        }
        else if (transform.position.y + camHeight > camBounds2.y)
        {
            return false;
        }
        else if (transform.position.x - camWidth < camBounds1.x)
        {
            return false;
        }
        else if (transform.position.x + camWidth > camBounds2.x)
        {
            return false;
        }
        return true;
    }
    public Vector2 Position
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = new Vector3(value.x, value.y, -10);
            if (!InBounds())
            {
                enforceBounds = false;
            }
        }
          
    }
    public Vector2 MoveToClosestPixel(Transform tf)
    {


        Vector2 initialPosition = tf.position;
        tf.position = new Vector3(Mathf.Round(tf.position.x * wsRatio) * pixToWorld, Mathf.Round(tf.position.y * wsRatio) * pixToWorld, tf.position.z);
        return initialPosition - (Vector2)tf.position;
    }

}
