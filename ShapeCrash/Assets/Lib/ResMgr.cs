using UnityEngine;
using System.Collections;


public enum ShapeColor
{
    Blue1,
    Green2,
    DarkBlue3,
    Pink4,
    Orange5,
    Cyan6,
    MaxColorNum

}

public enum ShapeType
{
}

public class ResMgr : MonoBehaviour {

   
    public static ResMgr current;
    // Use this for initialization

    public GameObject ShapePrefab;




    void Awake()
    {

        current = this;
    }

    public Sprite[] ShapeColors;

    public Sprite GetShapeColor(ShapeColor col)
    {

        return ShapeColors[(int)col];
    }



    [Header("State Play Menus")]
    public GameObject ChallengePlayMenu;
    public GameObject NormalPlayMenu;
}
