using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    int selected = 1;
    float menuNavTime = 0;
    RectTransform image;
    private void Awake()
    {
        image = GameObject.Find("Canvas").transform.GetChild(0).GetComponent<RectTransform>();
    }
    public int Selected
    {
        set
        {
            selected = value;

            if (selected > 4)
            {
                selected = 1;
            }

            if (selected < 1)
            {
                selected = 4;
            }
            HighlightButton();

        }
        get
        {
            return selected;
        }
    }
    
    void HighlightButton()
    {
        int x = selected - 1;
        foreach (Text text in transform.GetComponentsInChildren<Text>())
        {
            text.color = Color.white;
        }
        transform.GetChild(x+2).GetComponent<Text>().color = Color.yellow;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (menuNavTime <= 0)
        {
            if ((Controller.S.leftStick.y != 0))
            {
                Selected -= (int)Controller.S.leftStick.y;
                menuNavTime = 0.4f;
            }
        }
        else
        {
            menuNavTime -= Time.unscaledDeltaTime;
        }
        if (Controller.S.attackButtonDown)
        {
            SceneManager.LoadScene("Start");
        }
        image.position+=Vector3.left*Time.deltaTime*5;
        if (image.position.x < -962)
        {
            image.position = Vector3.zero;
        }
    }
}
