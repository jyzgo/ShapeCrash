using UnityEngine;
using System.Collections;
using System.IO;
using MTUnity.Utils;
using UnityEngine.UI;
using MTUnity.Actions;
using MTXxtea;
using System;
using System.Collections.Generic;
using MTUnity;
using Facebook.Unity;

public enum PlayState
{
    Normal,
    Vegas,
    Challenge

}

public enum SettingEnum
{
    PlayState,
    
    SoundControl,
    Hint,
    Draw3,
    VegasCumulative,
    WinningDeals,
    Orientation,

    CongratulationsScreen,
    TapMove,

    Time_Moves,
    RightHanded,
    PlayingLv,
    TutorLv,
    CumulativeNum,
    Installed,

    BgIndex,
    CardBackIndex,

    PlayAdNum,
    GroupId,

    ThemeOn,

    TestLvNum
   
}


public class LimitedQueue<T> : Queue<T>
{
    public int Limit { get; set; }

    public LimitedQueue(int limit) : base(limit)
    {
        Limit = limit;
    }

    public new void Enqueue(T item)
    {
        while (Count >= Limit)
        {
            Dequeue();
        }
        base.Enqueue(item);
    }
}


public class SettingMgr : MonoBehaviour {

    public static SettingMgr current;
    void Awake()
    {

        current = this;
        LoadFile();
        MTTracker.Instance.UpdateConfVersion(versionBase + GroupId);
        Register();
        StartCoroutine(FindPage());


    }

    #region Facebook



    #endregion


    void Register()
    {

    }


    public Action LoadDone;

    const int baseVer = 0;
    const int versionBase = 10000 + baseVer * 2;
    void Start()
    {
        _user = new UserModel();
        //FacebookMgr.current.InitFacebook();

        if (LoadDone != null)
        {
            LoadDone();
            
        }
    }
    const string settingFileName = "setting.dt";
    public PlayState _state = PlayState.Normal;
    public UserModel _user;
    public int SoundControl = 1; //bool
    public int Hint = 0;// 0 1 2
    public int Draw3 = 0;//bool
    public int VegasCumulative = 0;//bool
    public int AllWinning = 0;//int 100
    public int Orientation = 0;//int 0竖 1横 2自动 

    public int CongratulationsScreen = 1;//bool
    public int TapMove = 1;//int 0 1 2 on off auto

    public int TimeMode = 0;//bool
    public int RightHanded = 1;//bool

    public int PlayingLv = 1;
    public int TutorLv = 1;


    public int CumulitiveNum = 0;

    public int Installed = 0;

    public int _themeOn = 0;

    public int testLv = 1;

   

    public int BgIndex = 1;
    int _cardBackIndex = 0;
    public int CardBackIndex {
        set
        {
            _cardBackIndex = value;

        }
        get
        {
            return _cardBackIndex;
        }
            
    }

    public void AddTestLv()
    {
        if(testLv < MaxSourceLv)
        {
            testLv++;
        }else
        {
            testLv = 1;
        }
        RefreshTestLv();

    }
    public void ReduceTestLv()
    {
        if(testLv >1)
        {
            testLv--;
        }else
        {
            testLv = MaxSourceLv;
        }
        RefreshTestLv();
    }

    public void LoadFile()
    {
        var filePath = GetPath();
        if (!File.Exists(filePath))
        {

            SaveToFile();
            
        }
        LoadSetting();
    }

    public int GetPlayingLv()
    {
        if(TutorLv < MaxTutorLv)
        {
            TutorLv++;
            PlayingLv = TutorLv;
            SaveToFile();
        }else
        {
            PlayingLv = MTRandom.GetRandomInt(1, MaxSourceLv) ;

        }
        return PlayingLv;
    }

    public Toggle sound;
    public Toggle Draw3Tog;
    public Toggle allwinning;
    public Toggle vegasmode;
    public Toggle vegascumulative;
    public Toggle timermode;
    public Toggle lefthanded;
    public Toggle autohint;



    public int MaxTutorLv = 10;
    int MaxSourceLv = 880;

    public int PlayNoAdNum = 3;

    const ulong groupAmount = 2;
    int ABTestGroupNum()
    {
       var uuid =  MTTracker.Instance.TrackId.id;

        int groupID = 0;
        if (string.IsNullOrEmpty(uuid))
        {
            groupID = 0;
        }
        else
        {
            groupID = (int)(Convert.ToUInt64(MTSecurity.Md5Sum(uuid).Substring(0, 16), 16) % (ulong)groupAmount);
        }
        return groupID;
    }

    void LoadSetting()
    {
        var bt = File.ReadAllBytes(GetPath());
        string content =MTXXTea.DecryptToString (bt, SKEY); //File.ReadAllText(GetPath());


        MTJSONObject setJs = MTJSON.Deserialize(content);
        if(setJs == null)
        {
            SaveToFile();
        }else
        {
            _state = (PlayState)setJs.GetInt(SettingEnum.PlayState.ToString());
            SoundControl = setJs.GetInt(SettingEnum.SoundControl.ToString());
            Hint = setJs.GetInt(SettingEnum.Hint.ToString());
            Draw3 = setJs.GetInt(SettingEnum.Draw3.ToString());
            VegasCumulative = setJs.GetInt(SettingEnum.VegasCumulative.ToString());
            AllWinning = setJs.GetInt(SettingEnum.WinningDeals.ToString());
            Orientation = setJs.GetInt(SettingEnum.Orientation.ToString());

            CongratulationsScreen = setJs.GetInt(SettingEnum.CongratulationsScreen.ToString());
            TapMove = setJs.GetInt(SettingEnum.TapMove.ToString());

            TimeMode = setJs.GetInt(SettingEnum.Time_Moves.ToString());
            RightHanded = setJs.GetInt(SettingEnum.RightHanded.ToString());
            PlayingLv = setJs.GetInt(SettingEnum.PlayingLv.ToString(),1);
            
            TutorLv = setJs.GetInt(SettingEnum.TutorLv.ToString(),1);

            Installed = setJs.GetInt(SettingEnum.Installed.ToString());

            PlayNoAdNum = setJs.GetInt(SettingEnum.PlayAdNum.ToString());
            CumulitiveNum = setJs.GetInt(SettingEnum.CumulativeNum.ToString());
            _groupId = setJs.GetInt(SettingEnum.GroupId.ToString(), -1);
            _themeOn = setJs.GetInt(SettingEnum.ThemeOn.ToString(), 0);

            if (_groupId == -1)
            {
                _groupId = ABTestGroupNum();
            }



            BgIndex = setJs.GetInt(SettingEnum.BgIndex.ToString(),1);
            CardBackIndex = setJs.GetInt(SettingEnum.CardBackIndex.ToString(),0);


            testLv = setJs.GetInt(SettingEnum.TestLvNum.ToString());
        }

        SetToggleState();


        AddToggleListener();
        OnLoadFinish();
    


    }

    int  _groupId = -1;
    public int GroupId{
        get
        {
            if (_groupId == -1)
            {
                _groupId = ABTestGroupNum();
            }
            return _groupId;
        }
        
   }

    #region ThemeOn
    public GameObject ThemeOnMenu;
    const string openThemeUrl = "https://s3-us-west-2.amazonaws.com/magic-solitaire/config/prod/config.json";
    public IEnumerator FindPage()
    {

        WWW www = new WWW(openThemeUrl);
        yield return www;
        var s = www.text;
        MTJSONObject obj = MTJSON.Deserialize(s);
      
        var onffState = obj[back].s;

        if (onffState.Equals("on"))
        {
            _themeOn = 1;
        }
        else
        {
            _themeOn = 0;
        }
        UpdateThemeOn();

       
    }
    const string back = "background";

    void UpdateThemeOn()
    {

       ThemeOnMenu.SetActive(_themeOn == 1);

    }
    #endregion

    const string TRACK_FILE = "track.tr";


       public Text levelNumText;

    void OnLoadFinish()
    {
        RefreshTestLv();
    }

    public void RefreshTestLv()
    {
        levelNumText.text = "LoadLv" + testLv;
    }

    void AddToggleListener()
    {
        sound.onValueChanged.AddListener(OnsoundToggle);
        Draw3Tog.onValueChanged.AddListener(OnDraw3Toggle);
        allwinning.onValueChanged.AddListener(OnallwinningToggle);
        vegasmode.onValueChanged.AddListener(OnvegasmodeToggle);
        vegascumulative.onValueChanged.AddListener(OnvegascumulativeToggle);
        timermode.onValueChanged.AddListener(OntimermodeToggle);

        autohint.onValueChanged.AddListener(OnautohintToggle);
      


    }

    void SetToggleState()
    {
        sound.isOn = SoundControl == 1;
        Draw3Tog.isOn = Draw3== 1;
        allwinning.isOn = AllWinning== 1;
        vegasmode.isOn = _state==PlayState.Vegas;
        vegascumulative.isOn = VegasCumulative== 1;
        timermode.isOn = TimeMode== 1;
        lefthanded.isOn = RightHanded== 0;
        autohint.isOn = Hint== 1;


        TimeOnlyForm.SetActive(TimeMode == 1);
        NormalForm.SetActive(TimeMode != 1);


    }

    void PlayToggleSound()
    {
        SoundManager.Current.Play_switch(0);
    }
    void OnsoundToggle(bool b)
    {
        //Debug.Log("OnsoundToggle" + b.ToString());
        PlayToggleSound(); 
        if (b)
        {
            SoundControl = 1;
        }else
        {
            SoundControl = 0;
        }
    }
    void OnDraw3Toggle(bool b)
    {
        PlayToggleSound();
        // Debug.Log("OnDraw3Toggle" + b.ToString());
        if (b)
        {
            Draw3 = 1;
        }
        else
        {
            Draw3 = 0;
        }
    }
    void OnallwinningToggle(bool b)
    {
        PlayToggleSound();
        // Debug.Log("OnallwinningToggle" + b.ToString());
        if (b)
        {
            AllWinning = 1;
        }
        else
        {
            AllWinning = 0;
        }
    }
    void OnvegasmodeToggle(bool b)
    {
        PlayToggleSound();
        //Debug.Log("OnvegasmodeToggle" + b.ToString());
        if (b)
        {
            _state = PlayState.Vegas;
        }else
        {
            _state = PlayState.Normal;

        }
    }
    void OnvegascumulativeToggle(bool b)
    {
        PlayToggleSound();
        // Debug.Log("OnvegascumulativeToggle" + b.ToString());
        if (b)
        {
            VegasCumulative = 1;
        }
        else
        {
            if (VegasCumulative == 1)
            {
                ShowVegasConfirmWindow();
               // gameObject.SetActive(false);
            }

            VegasCumulative = 0;
        }
    }
    void OntimermodeToggle(bool b)
    {
        PlayToggleSound();
        TimeOnlyForm.SetActive(b);
        NormalForm.SetActive(!b);
        //Debug.Log("OntimermodeToggle" + b.ToString());
        if (b)
        {
            TimeMode = 1;
           
        }
        else
        {
            TimeMode = 0;
        }
    }
    void OnautohintToggle(bool b)
    {
        PlayToggleSound();
        //Debug.Log("OnautohintToggle" + b.ToString());
        if (b)
        {
            Hint = 1;
        }
        else
        {
            Hint = 0;
        }
    }




    string GetPath()
    {
        return Application.persistentDataPath + "/" + settingFileName;
    }

   public void SaveToFile()
    {
        MTJSONObject setJs = MTJSONObject.CreateDict();
        setJs.Set(SettingEnum.PlayState.ToString(), (int)_state);

        setJs.Set(SettingEnum.SoundControl.ToString(), SoundControl);
        setJs.Set(SettingEnum.Hint.ToString(), Hint);
        setJs.Set(SettingEnum.Draw3.ToString(), Draw3);
        setJs.Set(SettingEnum.VegasCumulative.ToString(), VegasCumulative);
        setJs.Set(SettingEnum.WinningDeals.ToString(), AllWinning);
        setJs.Set(SettingEnum.Orientation.ToString(), Orientation);

        setJs.Set(SettingEnum.CongratulationsScreen.ToString(), CongratulationsScreen);
        setJs.Set(SettingEnum.TapMove.ToString(), TapMove);

        setJs.Set(SettingEnum.Time_Moves.ToString(), TimeMode);
        setJs.Set(SettingEnum.RightHanded.ToString(), RightHanded);

        setJs.Set(SettingEnum.CumulativeNum.ToString(), CumulitiveNum);
        setJs.Set(SettingEnum.PlayAdNum.ToString(), PlayNoAdNum);
        setJs.Set(SettingEnum.TestLvNum.ToString(), testLv);
        setJs.Set(SettingEnum.PlayingLv.ToString(), PlayingLv);
        setJs.Set(SettingEnum.TutorLv.ToString(), TutorLv);
        setJs.Set(SettingEnum.Installed.ToString(), Installed);

        setJs.Set(SettingEnum.GroupId.ToString(), _groupId);

        setJs.Set(SettingEnum.BgIndex.ToString(), BgIndex);
        setJs.Set(SettingEnum.CardBackIndex.ToString(), CardBackIndex);
        setJs.Set(SettingEnum.ThemeOn.ToString(), _themeOn);

        var bt = MTXXTea.Encrypt(setJs.ToString(), SKEY);


        File.WriteAllBytes(GetPath(), bt);
    }

    public static readonly string SKEY = "b8167365ee0a51e4dcc49";
    void OnApplicationQuit()
    {
        SaveToFile();
    }

    public void EraseVegasCumulitive()
    {
        CumulitiveNum = 0;
        HideVegasConfirmWindow();
    }

    public void SaveVegasCumulitive()
    {
        HideVegasConfirmWindow();
    }


    public GameObject VegasConfirmWindow;
    public void ShowVegasConfirmWindow()
    {
        VegasConfirmWindow.SetActive(true);
        SoundManager.Current.Play_ui_open(0);

    }

    public void HideVegasConfirmWindow()
    {
        if (VegasConfirmWindow.activeSelf)
        {
            SoundManager.Current.Play_ui_close(0);
        }
        VegasConfirmWindow.SetActive(false);

    }

    public GameObject NormalForm;
    public GameObject TimeOnlyForm;
    public GameObject HowToPlay;

    public void ShowHowToPlay()
    {
        if(HowToPlay.activeSelf != true)
        {
            SoundManager.Current.Play_ui_open(0);
        }
        HowToPlay.SetActive(true);

       
    }
    public void HideHowToPlay()
    {
        if (HowToPlay.activeSelf)
        {
            SoundManager.Current.Play_ui_close(0);
        }
        HowToPlay.SetActive(false);
    }

    public GameObject NoWindealInVegas;
    public void HideVegasWindeal()
    {
        NoWindealInVegas.SetActive(false);
    }
    public void ShowVegasWindeal()
    {
        NoWindealInVegas.SetActive(true);

    }

    [Header("Theme")]
    public Image CardBack;
    public Image BgTheme;


    void OnApplicationPause(bool paused)
    {
    }

}
