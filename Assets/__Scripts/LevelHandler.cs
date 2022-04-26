using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelHandler : MonoBehaviour
{
    protected Vector2 yPixel = new Vector2(0, 0.0625f);
    public static LevelHandler s;
    
    public string stageEntrance;
    private List<string> StagesWithAlts = new List<string> { };
    private Dictionary<string, int> stageIndex = new Dictionary<string, int> { };
    [SerializeField]
    GameObject PlayerPrefab;
    GameObject player;
    [SerializeField]
    public int pixelsFromGround;
    public List<FXScript> availableFX = new List<FXScript>();
    public List<Mirage> availableMirage = new List<Mirage>();
    [SerializeField]
    public List<MultiFXScript> availableMultiFX = new List<MultiFXScript>();
    public List<RodScript> availableRods = new List<RodScript>();
    public List<MonoBehaviour> playerItems = new List<MonoBehaviour>();
    float bgmTime=0;
    AudioSource bgm;
    bool inMenu = false;
    private GameObject menu;
    Menu menuMenu;
    float menuNavTime;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (s != null)
        {
            bgmTime = s.GetComponent<AudioSource>().time;
            stageEntrance = s.stageEntrance;
            player = GameObject.Find("Player(Clone)");
            pixelsFromGround = s.pixelsFromGround;
            StartCoroutine(OnEnter());
            //SceneManager.MoveGameObjectToScene(s.gameObject, SceneManager.GetActiveScene());
            Destroy(s.gameObject) ;

            s = this;
        }
        else
        {
            s = this;
            //instantiate player
            player =Instantiate(PlayerPrefab, new Vector3(1, -7, 0), Quaternion.identity);
        }
        //moves player and camera to entrance instantiate player from previous scene and turns on transition
        //find gameObject with name of entrance and spawns player there, moves camera to child of game object position
        
    }
    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = Player.s.gameObject;
        }
        GetComponent<AudioSource>().time = bgmTime;
        
  
    }

    // Update is called once per frame
    void Update()
    {
        if (inMenu)
        {
            if (menu != null)
            {
                if (menuMenu == null)
                {
                    menuMenu = menu.GetComponent<Menu>();
                }
                if(menuNavTime<=0)
                {
                    if((Controller.S.leftStick.y != 0))
                    {
                        menuMenu.Selected -= (int)Controller.S.leftStick.y;
                        menuNavTime = 0.4f;
                    }
                }
                else
                {
                    menuNavTime -= Time.unscaledDeltaTime;
                }
                
               
            }
        }
    }
    public void CreateFx(string fxName, Vector2 position,Vector2 scale,float rotation=0)
    {
        availableFX[0].PlayFX(fxName, position, scale, rotation);
    }
    public RodScript CreateRod(Vector2 position)
    {
        return availableRods[0].Setup(position);
    }
    private int  GetStageIndex(string stageName)
    {
        if (StagesWithAlts.Contains(stageName))
        {
            //checkInMemory for stage currentState
            stageName=stageName + "";

        }
        return (stageIndex[stageName]);
    }
    public void SetAlt(string stageName,string state)
    {
        //update state of stage in memory
    }

    //fix this
    public void EnterStage(string stageName,string entrance)
    {
        stageEntrance = entrance;
        StartCoroutine(TransitionEnter(stageName));
        

    }
    
    IEnumerator TransitionEnter(string stageName)
    {
        Time.timeScale = 0;
        pixelsFromGround = Player.s.GetPixelsFromGround();
        GameObject.Find("TransitionObject").transform.GetChild(0).GetChild(3).GetComponent<Animator>().Play("Enter");
        yield return new WaitForSecondsRealtime(0.35f);
        SceneManager.LoadScene(stageName);
        Time.timeScale = 1;

    }
    IEnumerator OnEnter()
    {
        Time.timeScale = 0;

        Vector2 newCamPos = GameObject.Find(stageEntrance).transform.GetChild(0).transform.position;
        GameObject.Find("Main Camera").transform.position = new Vector3(newCamPos.x, newCamPos.y, -10) ;

        player.GetComponent<Player>().ReAwake();
        player.transform.position = (Vector2)GameObject.Find(stageEntrance).transform.position+yPixel*pixelsFromGround;
        //GameObject.Find("TransitionObject").transform.GetChild(0).GetChild(0).GetComponent<Animator>().Play("Leave", -1, 0);
        yield return new WaitForSecondsRealtime(0.35f);
        Time.timeScale = 1;

    }
    public void MakeMirage(Vector2 position,Vector2 scale,SpriteRenderer sr,float rotation=0)
    {
        availableMirage[0].ActivateMirage(position, scale, sr,rotation);
    }
    public void MakeCoalBall(Vector2 position,Vector2 speed)
    {
        foreach(MonoBehaviour mb in playerItems)
        {
            if (mb.GetType().ToString() == "CoalBall")
            {
                ((CoalBall)mb).ActivateCoalBall(position,speed);
                return;
            }

        }
    }
    public void ActivateFade(bool activate)
    {

        GameObject.Find("TransitionObject").transform.GetChild(0).GetChild(4).gameObject.SetActive(activate);
    }
    public void MenuButton()
    {
        if (inMenu)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }
     void OpenMenu()
    {
        ActivateFade(true);
        menuNavTime = 0;
        menu = GameObject.Find("InGameMenu");
        menu.GetComponent<Menu>().Selected = 1;
        
        menu.GetComponent<Animator>().Play("Enter1");
        StartCoroutine(OpenMenuCoroutine());

    }
     void CloseMenu()
    {
        ActivateFade(false);
        menu.GetComponent<Animator>().Play("Leave1");
        StartCoroutine(CloseMenuCoroutine());
    }
    IEnumerator OpenMenuCoroutine()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.1f);
        inMenu = true;
    }
    IEnumerator CloseMenuCoroutine()
    {
        inMenu = false;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
    }

}

//stagename in scene, bool if it has alts
//gameMemory
