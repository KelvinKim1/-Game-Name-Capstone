using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField]
    private bool Keyboard = true;
    private bool ADown = false;
    private bool WDown = false;
    private bool SDown = false;
    private bool DDown = false;
    
    private Controls controls;
    public static Controller S;
    public const float InputBuffer = 0.2f;
    public float southButtonTimer = 0;
    public float attackButtonTimer = 0;
    public float dashButtonTimer = 0;
    public bool southButtonDown = false;
    public bool attackButtonDown = false;
    public bool dashButtonDown = false;
    public Vector2 leftStick = Vector2.zero;
    // Start is called before the first frame update
    private void Awake()
    {
        
        controls = new Controls();
        S = this;

        if (Keyboard)
        {
            controls.Keyboard.Space.performed += ctx => SouthButtonDown= true;
            controls.Keyboard.Space.canceled += ctx => SouthButtonDown = false;
            controls.Keyboard.A.performed += ctx => ADown = true;
            controls.Keyboard.A.canceled += ctx => ADown = false;
            controls.Keyboard.W.performed += ctx => WDown = true;
            controls.Keyboard.W.canceled += ctx => WDown= false;
            controls.Keyboard.S.performed += ctx =>SDown = true;
            controls.Keyboard.S.canceled += ctx => SDown = false;
            controls.Keyboard.D.performed += ctx => DDown= true;
            controls.Keyboard.D.canceled += ctx => DDown = false;
            controls.Keyboard.E.performed += ctx => AttackButtonDown = true;
            controls.Keyboard.E.canceled += ctx => AttackButtonDown = false;
            controls.Keyboard.Q.performed += ctx => DashButtonDown = true;
            controls.Keyboard.Q.canceled += ctx => DashButtonDown = false;
            controls.Keyboard.Tab.performed += ctx => OpenMenu();


        }
        else
        {
            controls.Ps4.rightTrigger.performed += ctx => AttackButtonDown = true;
            controls.Ps4.rightTrigger.canceled += ctx => AttackButtonDown = false;
            controls.Ps4.buttonEast.performed += ctx => DashButtonDown = true;
            controls.Ps4.buttonEast.canceled += ctx => DashButtonDown = false;
            controls.Ps4.buttonSouth.performed += ctx => SouthButtonDown = true;
            controls.Ps4.buttonSouth.canceled += ctx => SouthButtonDown = false;
            controls.Ps4.leftStick.performed += ctx => LeftStick = ctx.ReadValue<Vector2>();
        }
    }
    private bool DashButtonDown
    {
        set
        {
            dashButtonDown = value;
            if (value)
            {
                dashButtonTimer = InputBuffer;
            }
        }
    }
    private bool SouthButtonDown
    {
        set
        {
            southButtonDown = value;
            if (value)
            {
                southButtonTimer = InputBuffer;
            }
        }
    }
    private bool AttackButtonDown
    {
        set
        {
            attackButtonDown = value;
            if (value)
            {
                attackButtonTimer = InputBuffer;
            }
        }
    }
    private Vector2 LeftStick
    {
        set
        {
            if (value.magnitude < 0.3f)
            {
                leftStick = Vector2.zero;
                return;
            }
            else
            {
                leftStick = new Vector2(Mathf.Abs(value.x) < 0.3f ? 0 : Mathf.Sign(value.x), Mathf.Abs(value.y) < 0.3f ? 0 : Mathf.Sign(value.y));
            }
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (dashButtonTimer > 0)
        {
            dashButtonTimer -= Time.unscaledDeltaTime;
        }
        if (southButtonTimer > 0)
        {
            southButtonTimer -= Time.unscaledDeltaTime;
        }
        if (attackButtonTimer > 0)
        {
            attackButtonTimer -= Time.unscaledDeltaTime;
        }
        if (Keyboard)
        {
            leftStick = new Vector2((ADown ? -1 : 0) + (DDown ? 1 : 0), (SDown ? -1 : 0) + (WDown ? 1 : 0));
        }
    }
    public void ConsumeJumpBuffer()
    {
        southButtonTimer = 0;
    }
    public void ConsumeAttackBuffer()
    {
        attackButtonTimer = 0;
    }
    public void ConsumeDashBuffer()
    {
        dashButtonTimer = 0;
    }
    public void OnEnable()
    {
        controls?.Enable();
    }
    public void OnDisable()
    {
        controls?.Disable();
    }
    private void OpenMenu()
    {
        LevelHandler.s.MenuButton();
    }
}
