using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTransition : GEvent
{
    //set in editor
    [SerializeField]
    private string NextStage;
    [SerializeField]
    private string StageEntrance;
    // Start is called before the first frame update
    public override  void PlayEvent()
    {
        LevelHandler.s.EnterStage(NextStage, StageEntrance);
    }
}
