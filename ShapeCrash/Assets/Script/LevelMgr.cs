using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MTUnity.Actions;
using MonsterLove.StateMachine;
using UnityEngine.UI;
using System;
using MTUnity.Utils;
using MTUnity;
using System.IO;
using MTXxtea;
using admob;

[Serializable]


public class LevelMgr : MonoBehaviour
{

    public enum LevelState
    {
        Menu,
        Initing,
        Playing,
        Swaping,
        Droping,
        Matching,
        Win,
        Lose

    }

    bool isCellSwapAble(Cell c1, Cell c2)
    {
        if (c1 == c2)
        {
            return false;
        }
        if (c1 == null || c2 == null)
        {
            return false;
        }

        if (c1._row != c2._row && c1._col != c2._col)
        {
            return false;
        }

        if (c1._row == c2._row)
        {
            if (Mathf.Abs(c1._col - c2._col) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        if (c1._col == c2._col)
        {
            if (Mathf.Abs(c1._row - c2._row) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        return false;
    }

    public bool Playing = false;
    internal void CellDrag()
    {
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Board.position; ;
        
        int col = (int)((pos.x + CELL_HEIGHT/2) / CELL_HEIGHT);
        int row = (int)((pos.y + CELL_HEIGHT/2) / CELL_HEIGHT) ;

        var curCell = GetValidCell(row, col);

        
        if (_activeCell != curCell)
        {
           
            if (isCellSwapAble(_activeCell, curCell))
            {
                if (_activeCell.IsShapeSwapable() && curCell.IsShapeSwapable())
                {
                    _passiveCell = curCell;
                    fsm.ChangeState(LevelState.Swaping);
                }
            }
            
        }
    }

    Cell GetValidCell(int row,int col)
    {
        if (row >= 0 && row < MAX_ACTIVE_ROW && col >= 0 && col < MAX_ACTIVE_COL)
        {
            return Grid[row, col];
        }
        return null;
    }

    internal void CellRelease(Cell cell)
    {

       
    }

    Cell _activeCell;
    Cell _passiveCell;

    internal void CellDown(Cell cell)
    {
        if (Playing)
        {
            _activeCell = cell;
        }
    }

    

    public GameObject MenuCanvas;


    public GameObject UnableRefreshIcon;







    public Transform start;



    public void UpdateUI()
    {

    }
    
    
    
    


    StateMachine<LevelState> fsm;

    public static LevelMgr current;



   

    public SettingMgr _settingMgr;
    public SoundManager _soundMgr;
   
    void Awake()
    {
       
        AdMgr.RegisterAllAd(this);
        current = this;



    }

    void onAppLovinEventReceived(string ev)
    {
        if (ev.Contains("DISPLAYEDINTER"))
        {
            // An ad was shown.  Pause the game.
            //MTTracker.Instance.Track(SoliTrack.ads, StatisticsMgr.current.WinsCount(), "0", "applovin");
            AdMgr.TrackApplovin("0");
        }
        else if (ev.Contains("HIDDENINTER"))
        {
            // Ad ad was closed.  Resume the game.
            // If you're using PreloadInterstitial/HasPreloadedInterstitial, make a preload call here.
            AdMgr.ApplovinPreloadInterstitial();
            AdMgr.TrackApplovin("1");
            //MTTracker.Instance.Track(SoliTrack.ads, StatisticsMgr.current.WinsCount(), "1","applovin");
        }
        else if (ev.Contains("LOADEDINTER"))
        {
            // An interstitial ad was successfully loaded.
            AdMgr.TrackApplovin("3");
         //   MTTracker.Instance.Track(SoliTrack.ads, StatisticsMgr.current.WinsCount(), "3", "applovin");
        }
        else if (string.Equals(ev, "LOADINTERFAILED"))
        {
            // An interstitial ad failed to load.
            AdMgr.TrackApplovin("2");
       //     MTTracker.Instance.Track(SoliTrack.ads, StatisticsMgr.current.WinsCount(), "2", "applovin");
        
        }
    }

    void Start()
    {
        fsm = StateMachine<LevelState>.Initialize(this,LevelState.Initing); 
    }




    #region Initing
    [Header("init")]
    public Transform Board;
    public GameObject CellPrefab;
    Cell [,]Grid = new Cell[MAX_ACTIVE_ROW,MAX_ACTIVE_COL];
    IEnumerator Initing_Enter()
    {

        var shapePrefab = ResMgr.current.ShapePrefab;
        for (int row = 0; row < MAX_ACTIVE_ROW; row++)
        {
            for (int col = 0; col < MAX_ACTIVE_COL; col++)
            {
                var cell = Instantiate<GameObject>(CellPrefab);
                cell.transform.SetParent(Board);
                cell.transform.localPosition = new Vector3(CELL_WIDTH * col, CELL_HEIGHT * row, 0);
                var cellSc = cell.GetComponentInChildren<Cell>();
                cellSc.Init(row, col);
                int m = MTRandom.GetRandomInt(1, 100) % (int)ShapeColor.MaxColorNum;


                var s = Instantiate<GameObject>(shapePrefab);
                var shapeSc = s.GetComponent<Shape>();
                shapeSc.Init((ShapeColor)m);
                Grid[row, col] = cellSc;
                cellSc.SetShape(shapeSc);

            }
        }
        yield return null;
        fsm.ChangeState(LevelState.Playing);
        
    }

    const int MAX_ACTIVE_ROW = 9;
    const int MAX_ACTIVE_COL = 9;

    const float CELL_HEIGHT = 0.58f;
    const float CELL_WIDTH = 0.58f;

    const int MAX_ROW_COUNT = 9;

    #endregion Initing


    #region Playing
    void Playing_Enter()
    {
        Playing = true;
        _activeCell = null;
        _passiveCell = null;
    }

    void Playing_Exit()
    {
        Playing = false;
    }
    #endregion Playing


    #region Swaping
    const float SWAP_TIME = 0.2f;
    void Swaping_Enter()
    {
        swapCount = 2;
        var oldScale = _activeCell._shape.transform.localScale;
        _activeCell._shape.RunActions(
            new MTSpawn(
                new MTSequence(new MTScaleTo(SWAP_TIME/2, oldScale * 1.4f),new MTScaleTo(SWAP_TIME/2, oldScale)),
                new MTMoveToWorld(SWAP_TIME, _passiveCell.transform.position)),
            new MTCallFunc(()=>ReduceCount()));
        _passiveCell._shape.RunActions(
            new MTMoveToWorld(SWAP_TIME, _activeCell.transform.position),
            new MTCallFunc(()=>ReduceCount()));
        
    }
    int swapCount = 0;

    void ReduceCount()
    {
        swapCount--;
        if (swapCount == 0)
        {
            _activeCell.SwapShape(_passiveCell);
            fsm.ChangeState(LevelState.Matching);
        }
    }

    void Swaping_Exit()
    {
    }
    #endregion Swaping

    #region Matching
    void Matching_Enter()
    {
        fsm.ChangeState(LevelState.Playing);
    }
    void Matching_Exit()
    { }
    #endregion Matching

    public Transform Root;

   
    



  


    public void onInterstitialEvent(string eventName, string msg)
    {
        // Debug.Log("handler onAdmobEvent---" + eventName + "   " + msg);
        if (eventName == AdmobEvent.onAdLoaded)
        {
            //AdMgr.ShowAdmobInterstitial();
            AdMgr.TrackAdMob("3");
        }
        else if (eventName == AdmobEvent.onAdClosed)
        {
            AdMgr.PreloadAdmobInterstitial();
            AdMgr.TrackAdMob("1");

        }
        else if (eventName == AdmobEvent.onAdOpened)
        {
            AdMgr.TrackAdMob("0");
        } else if (eventName == AdmobEvent.onAdFailedToLoad)
        {
            AdMgr.TrackAdMob("2");
        }

    }
    public void onBannerEvent(string eventName, string msg)
    {
        Debug.Log("handler onAdmobBannerEvent---" + eventName + "   " + msg);
    }
    public void onRewardedVideoEvent(string eventName, string msg)
    {
        Debug.Log("handler onRewardedVideoEvent---" + eventName + "   " + msg);
    }
   public void onNativeBannerEvent(string eventName, string msg)
    {
        Debug.Log("handler onAdmobNativeBannerEvent---" + eventName + "   " + msg);
    }


    #region VungleCallback

    public void VunlgePlayAbleEvent(bool b)
    {
        if (b)
        {
            AdMgr.PlayVungleAd();
        }
    }
    #endregion



}
