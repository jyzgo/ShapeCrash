using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using MTUnity.Utils;
using MTUnity;
using System;
using System.Linq;




public class FacebookHelper : MonoBehaviour
{
    public bool debugInfo = false;
    public bool active = true;

    // request call back
    public delegate void RequstFriendsCallBack(bool res);

    // facebook app id
    public string appId = "1712266368984819";

    // facebook namespace
    public string nameSpaceName = "gummywonders";

    // app url link
    public string appUrlLink = "https://fb.me/1728901217321334";

    // facebook login permission
    private List<string> perms = new List<string>(){ "public_profile", "email", "user_friends" };

    // recode the time scale before the hiding,defalut value is 1.0f
    private float timeScale = 1.0f;
    // because the unity platform the ui is create at OnGUI
    private bool dirtyLogin = false;
    private bool _showLoginResult = false;

    // picture
    private string _currDownloadId = "";
    private HashSet<string> _pictureDownloadIds = new HashSet<string>();
    private Dictionary<string, Sprite> _pictures = new Dictionary<string, Sprite>();

    // invite friends
    private List<string> _inviteFriends = new List<string>();

    // init facebook id
    private string _currFacebookId = "";

    public string CurrFacebookId
    {
        get
        { 
            return _currFacebookId;
        }
    }

    // curr facebook connect status
    private bool _currLogin = false;

    private static FacebookHelper _facebookHelper = null;

    public static FacebookHelper Instance
    {
        get { return _facebookHelper; }
    }

    public  void FacebookHelperDebug(string info)
    {
        Debug.Log("[FacebookHelper] " + info);
    }

    public void FacebookHelperDebug(string formatName, params object[] args)
    {
        FacebookHelperDebug(string.Format(formatName, args));
    }

    public void TrackPay(float price, string currency)
    {
        FB.LogPurchase(price, currency);
    }

    public bool IsLoggedIn
    {
        get { return active && FB.IsLoggedIn; }
    }

    public void LogIn()
    {
        if(!active)
        {
            return;
        }
        if(!IsLoggedIn && !dirtyLogin)
        {
          //  GameManager.Instance.connectFacebookAwardStatus = FacebookConnectAwardStatus.Check;
            _showLoginResult = true;
            // 弹出等待登录结果的弹窗

            #if UNITY_EDITOR
            dirtyLogin = true;
            #else
			FB.LogInWithReadPermissions(perms, AuthCallback);
            #endif
        }
    }

    public void InitUserInfo()
    {
        if(IsLoggedIn)
        {
            string facebookId = Facebook.Unity.AccessToken.CurrentAccessToken.UserId;
          //  UserModel user = GameManager.Instance.User;
            //if(user != null && _currFacebookId != facebookId)
            //{
            //    _currFacebookId = facebookId;
            //    Tracker.Instance.FacebookLogin(_currFacebookId);
            //    // 获得facebook的信息
            //    FB.API("/me?fields=id,name,first_name,last_name,age_range,link,gender,locale,timezone,updated_time,verified,email", HttpMethod.GET, GetUserInfoCallback);

            //    GameServerHelper gameServerHelper = GameServerHelper.Instance;
            //    if(user.uid != facebookId)
            //    {
            //        gameServerHelper.AddOfflineSyncUser(user);
            //        if(user.LoginPlatform != PlatformType.None)
            //        {
            //            if(debugInfo)
            //                FacebookHelperDebug("InitUserInfo user.uid = " + user.uid + ", facebookId = " + facebookId + ", user.LoginPlatform = " + user.LoginPlatform);
            //            user.Reset();
            //            ResetPictures();
            //        }
            //    }
            //    user.uid = facebookId;
            //    user.LoginPlatform = PlatformType.Facebook;
            //    GetGameFriends();
            //    GetInviteFriends();
            //    AddDownloadPictureId(facebookId);

            //    gameServerHelper.SyncCurrUserData();
            //    gameServerHelper.UpdateCurrUserFriendsInfo();
            //    gameServerHelper.GetCurrUserInboxMessages();
            //}
            ClearRequests();
        }
        InvokeFacebookStatusChanged();
    }

    public void GetGameFriends()
    {
        // 获得游戏好友信息
        if(IsLoggedIn)
        {
            FB.API("/me/friends", HttpMethod.GET, GetFriendsCallback);
        }
    }

    public void GetInviteFriends()
    {
        if(IsLoggedIn)
        {
            FB.API("/me/invitable_friends", HttpMethod.GET, GetInviteFriendsCallback);
        }
    }

    public void ClearRequests()
    {
        if(IsLoggedIn)
        {
            FB.API("me/apprequests", HttpMethod.GET, delegate(IGraphResult result)
                {
                    if(debugInfo)
                        FacebookHelperDebug("ClearRequests");
                    if(result == null)
                    {
                        if(debugInfo)
                            FacebookHelperDebug("ClearRequests Null Response\n");
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(result.Error))
                        {
                            if(debugInfo)
                                FacebookHelperDebug("ClearRequests Error Response:\n" + result.Error);
                        }
                        else if(result.Cancelled)
                        {
                            if(debugInfo)
                                FacebookHelperDebug("ClearRequests Cancelled Response:\n" + result.RawResult);
                        }
                        else if(!string.IsNullOrEmpty(result.RawResult))
                        {
                            if(debugInfo)
                                FacebookHelperDebug("ClearRequests Success Response:\n" + result.RawResult);
                            MTJSONObject mtJson = MTJSON.Deserialize(result.RawResult);
                            List<MTJSONObject> friends = mtJson.Get("data").list;
                            for(int i = 0; i < friends.Count; i++)
                            {
                                string id = friends[i].GetString("id", "");
                                if(id.Length > 0)
                                {
                                    FB.API(id, HttpMethod.DELETE, delegate(IGraphResult deleteResult)
                                        {
                                            if(debugInfo)
                                                FacebookHelperDebug("DeleteRequests" + id);
                                            if(deleteResult == null)
                                            {
                                                if(debugInfo)
                                                    FacebookHelperDebug("DeleteRequests Null Response\n" + id);
                                            }
                                            else
                                            {
                                                if(!string.IsNullOrEmpty(deleteResult.Error))
                                                {
                                                    if(debugInfo)
                                                        FacebookHelperDebug("DeleteRequests Error Response:\n" + deleteResult.Error + ", id = " + id);
                                                }
                                                else if(deleteResult.Cancelled)
                                                {
                                                    if(debugInfo)
                                                        FacebookHelperDebug("DeleteRequests Cancelled Response:\n" + deleteResult.RawResult + ", id = " + id);
                                                }
                                                else if(!string.IsNullOrEmpty(deleteResult.RawResult))
                                                {
                                                    if(debugInfo)
                                                        FacebookHelperDebug("DeleteRequests Success Response:\n" + deleteResult.RawResult + ", id = " + id);
                                                }
                                                else
                                                {
                                                    if(debugInfo)
                                                        FacebookHelperDebug("DeleteRequests Empty Response\n" + id);
                                                }
                                            }
                                        });
                                }
                            }
                        }
                        else
                        {
                            if(debugInfo)
                                FacebookHelperDebug("ClearRequests Empty Response\n");
                        }
                    }
                });
        }
    }

    //public void SharePassLevel(LevelSuccInfo info, bool ask)
    //{
    //    if(IsLoggedIn)
    //    {
    //        if(AccessToken.CurrentAccessToken.Permissions.Contains("publish_actions"))
    //        {
    //            if(debugInfo)
    //                FacebookHelperDebug("SharePassLevel have publish actions");
    //            var level = new Dictionary<string, string>();
    //            level.Add("og:url", appUrlLink);
    //            level.Add("og:title", "Level " + info.level);
    //            level.Add("og:type", nameSpaceName + ":level");
    //            level.Add("og:image", "http://gummywonders.static.magictavern.com/facebook/GummyWonders_PassLevel.png");
    //            level.Add("og:description", "I completed Level " + info.level + " with a score of " + info.score + "! Can you do better?");
    //            level.Add("og:site_name", GameConfig.APP_NAME);
    //            level.Add("fb:app_id", appId);
    //            var data = new Dictionary<string, string>();
    //            data.Add("level", new MTJSONObject(level).ToString());
    //            data.Add("fb:explicitly_shared", "true");

    //            FB.API("me/" + nameSpaceName + ":complete", HttpMethod.POST, delegate(IGraphResult result)
    //                {
    //                    if(debugInfo)
    //                        FacebookHelperDebug("SharePassLevel");
    //                    if(result == null)
    //                    {
    //                        if(debugInfo)
    //                            FacebookHelperDebug("SharePassLevel Null Response\n");
    //                    }
    //                    else
    //                    {
    //                        if(!string.IsNullOrEmpty(result.Error))
    //                        {
    //                            if(debugInfo)
    //                                FacebookHelperDebug("SharePassLevel Error Response:\n" + result.Error);
    //                        }
    //                        else if(result.Cancelled)
    //                        {
    //                            if(debugInfo)
    //                                FacebookHelperDebug("SharePassLevel Cancelled Response:\n" + result.RawResult);
    //                        }
    //                        else if(!string.IsNullOrEmpty(result.RawResult))
    //                        {
    //                            if(debugInfo)
    //                                FacebookHelperDebug("SharePassLevel Success Response:\n" + result.RawResult);
    //                        }
    //                        else
    //                        {
    //                            if(debugInfo)
    //                                FacebookHelperDebug("SharePassLevel Empty Response\n");
    //                        }
    //                    }
    //                }, data);
    //        }
    //        else if(ask)
    //        {
    //            if(debugInfo)
    //                FacebookHelperDebug("SharePassLevel no publish actions");
    //            FB.LogInWithPublishPermissions(
    //                new List<string>(){ "publish_actions" },
    //                delegate(ILoginResult result)
    //                {
    //                    if(debugInfo)
    //                        FacebookHelperDebug("SharePassLevel ask publish actions   result=" + result);
    //                    SharePassLevel(info, false);
    //                }
    //            );
    //        }
    //    }
    //}

    public void BeatFriends(string friend, bool ask)
    {
        if(IsLoggedIn)
        {
            if(AccessToken.CurrentAccessToken.Permissions.Contains("publish_actions"))
            {
                if(debugInfo)
                    FacebookHelperDebug("BeatFriends have publish actions");
                string title = friend;
                string content = friend;
                if(friend.Length == 0)
                {
                    title = "Awesome";
                    content = "a person";
                }
                var person = new Dictionary<string, string>();
                person.Add("og:url", appUrlLink);
                person.Add("og:title", title);
                person.Add("og:type", nameSpaceName + ":person");
                person.Add("og:image", "http://gummywonders.static.magictavern.com/facebook/GummyWonders_SurpassPerson.png");
              //  person.Add("og:description", "I just beat " + content + " in " + GameConfig.APP_NAME + ". Come play and catch me if you can.");
                //person.Add("og:site_name", GameConfig.APP_NAME);
                person.Add("fb:app_id", appId);
                var data = new Dictionary<string, string>();
                data.Add("person", new MTJSONObject(person).ToString());
                data.Add("fb:explicitly_shared", "true");

                FB.API("me/" + nameSpaceName + ":beat", HttpMethod.POST, delegate(IGraphResult result)
                    {
                        if(debugInfo)
                            FacebookHelperDebug("BeatFriends");
                        if(result == null)
                        {
                            if(debugInfo)
                                FacebookHelperDebug("BeatFriends Null Response\n");
                        }
                        else
                        {
                            if(!string.IsNullOrEmpty(result.Error))
                            {
                                if(debugInfo)
                                    FacebookHelperDebug("BeatFriends Error Response:\n" + result.Error);
                            }
                            else if(result.Cancelled)
                            {
                                if(debugInfo)
                                    FacebookHelperDebug("BeatFriends Cancelled Response:\n" + result.RawResult);
                            }
                            else if(!string.IsNullOrEmpty(result.RawResult))
                            {
                                if(debugInfo)
                                    FacebookHelperDebug("BeatFriends Success Response:\n" + result.RawResult);
                            }
                            else
                            {
                                if(debugInfo)
                                    FacebookHelperDebug("BeatFriends Empty Response\n");
                            }
                        }
                    }, data);
            }
            else if(ask)
            {
                if(debugInfo)
                    FacebookHelperDebug("BeatFriends no publish actions");
                FB.LogInWithPublishPermissions(
                    new List<string>(){ "publish_actions" },
                    delegate(ILoginResult result)
                    {
                        if(debugInfo)
                            FacebookHelperDebug("BeatFriends ask publish actions   result=" + result);
                        BeatFriends(friend, false);
                    }
                );
            }
        }
    }

    public void SurpassFriends(string friend, bool ask)
    {
        if(IsLoggedIn)
        {
            if(AccessToken.CurrentAccessToken.Permissions.Contains("publish_actions"))
            {
                if(debugInfo)
                    FacebookHelperDebug("SurpassFriends have publish actions");
                string title = friend;
                string content = friend;
                if(friend.Length == 0)
                {
                    title = "Great Job";
                    content = "a person";
                }
                var person = new Dictionary<string, string>();
                person.Add("og:url", appUrlLink);
                person.Add("og:title", title);
                person.Add("og:type", nameSpaceName + ":person");
                person.Add("og:image", "http://gummywonders.static.magictavern.com/facebook/GummyWonders_BeatPerson.png");
               // person.Add("og:description", "I've passed " + content + " in " + GameConfig.APP_NAME + ". Come play and catch me if you can.");
               // person.Add("og:site_name", GameConfig.APP_NAME);
                person.Add("fb:app_id", appId);
                var data = new Dictionary<string, string>();
                data.Add("person", new MTJSONObject(person).ToString());
                data.Add("fb:explicitly_shared", "true");

                FB.API("me/" + nameSpaceName + ":surpass", HttpMethod.POST, delegate(IGraphResult result)
                    {
                        if(debugInfo)
                            FacebookHelperDebug("SurpassFriends");
                        if(result == null)
                        {
                            if(debugInfo)
                                FacebookHelperDebug("SurpassFriends Null Response\n");
                        }
                        else
                        {
                            if(!string.IsNullOrEmpty(result.Error))
                            {
                                if(debugInfo)
                                    FacebookHelperDebug("SurpassFriends Error Response:\n" + result.Error);
                            }
                            else if(result.Cancelled)
                            {
                                if(debugInfo)
                                    FacebookHelperDebug("SurpassFriends Cancelled Response:\n" + result.RawResult);
                            }
                            else if(!string.IsNullOrEmpty(result.RawResult))
                            {
                                if(debugInfo)
                                    FacebookHelperDebug("SurpassFriends Success Response:\n" + result.RawResult);
                            }
                            else
                            {
                                if(debugInfo)
                                    FacebookHelperDebug("SurpassFriends Empty Response\n");
                            }
                        }
                    }, data);
            }
            else if(ask)
            {
                if(debugInfo)
                    FacebookHelperDebug("SurpassFriends no publish actions");
                FB.LogInWithPublishPermissions(
                    new List<string>(){ "publish_actions" },
                    delegate(ILoginResult result)
                    {
                        if(debugInfo)
                            FacebookHelperDebug("SurpassFriends ask publish actions   result=" + result);
                        SurpassFriends(friend, false);
                    }
                );
            }
        }
    }

    public bool ShareByOrder()
    {
        return false;
    }

    //public void FeedShare(string title, string des, string imageUrl)
    //{
    //    if(IsLoggedIn)
    //    {
    //        FB.FeedShare(null,
    //            new Uri(appUrlLink), 
    //            title, 
    //            LangConfig.Current.GetLangByKey("app.name"), 
    //            des, 
    //            new Uri(imageUrl), 
    //            "", 
    //            delegate(IShareResult result)
    //            {
    //                if(debugInfo)
    //                    FacebookHelperDebug("ShareLinks");
    //                if(result == null)
    //                {
    //                    if(debugInfo)
    //                        FacebookHelperDebug("ShareLinks Null Response\n");
    //                }
    //                else
    //                {
    //                    if(!string.IsNullOrEmpty(result.Error))
    //                    {
    //                        if(debugInfo)
    //                            FacebookHelperDebug("ShareLinks Error Response:\n" + result.Error);
    //                    }
    //                    else if(result.Cancelled)
    //                    {
    //                        if(debugInfo)
    //                            FacebookHelperDebug("ShareLinks Cancelled Response:\n" + result.RawResult);
    //                    }
    //                    else if(!string.IsNullOrEmpty(result.RawResult))
    //                    {
    //                        if(debugInfo)
    //                            FacebookHelperDebug("ShareLinks Success Response:\n" + result.RawResult);
    //                    }
    //                    else
    //                    {
    //                        if(debugInfo)
    //                            FacebookHelperDebug("ShareLinks Empty Response\n");
    //                    }
    //                }
    //            });
    //    }
    //}

    public void InviteFriends(string source)
    {
        if(IsLoggedIn)
        {
//#if !UNITY_EDITOR
			FB.Mobile.AppInvite (new Uri (appUrlLink), null, delegate (IAppInviteResult result) {
				if (debugInfo) FacebookHelperDebug ("InviteFriendsCallback");
				if (result == null) {
					if (debugInfo) FacebookHelperDebug ("InviteFriendsCallback Null Response\n");
				} else {
					if (!string.IsNullOrEmpty(result.Error)) {
						if (debugInfo) FacebookHelperDebug ("InviteFriendsCallback Error Response:\n" + result.Error);
					} else if (result.Cancelled) {
						if (debugInfo) FacebookHelperDebug ("InviteFriendsCallback Cancelled Response:\n" + result.RawResult);
					} else if (!string.IsNullOrEmpty(result.RawResult)) {
						if (debugInfo) FacebookHelperDebug ("InviteFriendsCallback Success Response:\n" + result.RawResult);
						//Tracker.Instance.FacebookInvite(_currFacebookId, source, "");
					} else {
						if (debugInfo) FacebookHelperDebug ("InviteFriendsCallback Empty Response\n");
					}
				}
			});
//#endif
        }
    }

    public void InviteByRequest(RequstFriendsCallBack callBack)
    {
        if(debugInfo)
            FacebookHelperDebug("InviteByRequest");
        if(IsLoggedIn)
        {
            //#if !UNITY_EDITOR
            //LangConfig lang = LangConfig.Current;
            FB.AppRequest(
               "invite",// lang.GetLangByKey("social.facebook.message.invite"),//message
                _inviteFriends,// to
                null,//filters
                null,//excludeIds
                null,//maxRecipients
                "",//data
                "invite title",//lang.GetLangByKey("social.facebook.title.invite"),// title
                delegate (IAppRequestResult result)
                {
                    bool res = false;
                    if (result != null && string.IsNullOrEmpty(result.Error) && !result.Cancelled)
                    {
                        res = true;
                    }
                    if (callBack != null)
                    {
                        callBack.Invoke(res);
                    }
                }
            );
            //#endif
        }
    }

    public void RequestFriends(List<string> ids, string langKey, RequstFriendsCallBack callBack)
    {
        if(debugInfo)
            FacebookHelperDebug("RequestFriends ids = ", ids);
        if(IsLoggedIn)
        {    
            #if !UNITY_EDITOR
			//LangConfig lang = LangConfig.Current; 
			//FB.AppRequest(
			//	lang.GetLangByKey("social.facebook.message." + langKey),//message
			//	ids,// to
			//	null,//filters
			//	null,//excludeIds
			//	null,//maxRecipients
			//	"",//data
			//	lang.GetLangByKey("social.facebook.title." + langKey),// title
			//	delegate (IAppRequestResult result) {
			//		bool res = false;
			//		if (result != null && string.IsNullOrEmpty(result.Error) && !result.Cancelled) {
			//			res = true;
			//		}
			//		if (callBack != null) {
			//			callBack.Invoke(res);
			//		}
			//	}
			//);
            #endif
        }
    }

    void InvokeFacebookStatusChanged()
    {
      //  ModelBinder.OnPropChanged(ModelPropKey.FacebookLoginLogout.ToString(), IsLoggedIn);
    }

    public void LogOut()
    {
        if(IsLoggedIn)
        {
           // Tracker.Instance.FacebookLogout(_currFacebookId);
            FB.LogOut();
            _currFacebookId = "";
            //GameManager.Instance.connectFacebookAwardStatus = FacebookConnectAwardStatus.None;
        }
        InvokeFacebookStatusChanged();
    }

    void Awake()
    {
        if(_facebookHelper != null && _facebookHelper != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _facebookHelper = this;
        DontDestroyOnLoad(this.gameObject);

        if(debugInfo)
            FacebookHelperDebug("Awake FB.IsInitialized = {0}", FB.IsInitialized);
        if(!FB.IsInitialized)
        {
            FB.Init(InitCallback);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    void Update()
    {
        bool connected = IsLoggedIn;
        if(_currLogin != connected)
        {
            _currLogin = connected;
            InvokeFacebookStatusChanged();
        }
    }

    protected void OnGUI()
    {
        if(dirtyLogin)
        {
            dirtyLogin = false;
            FB.LogInWithReadPermissions(perms, AuthCallback);
        }
    }

    // Initialize the Facebook SDK callback
    private void InitCallback()
    {
        if(FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
        }
        else
        {
            Debug.LogWarning("FacebookHelper::InitCallback: Failed to Initialize the Facebook SDK");
        }
    }

    // A delegate to invoke when unity is hidden
    private void OnFacebookHideUnity(bool isGameShown)
    {
        if(debugInfo)
            FacebookHelperDebug("OnHideUnity timeScale = {0}, isGameShown = {1}", timeScale, isGameShown);
        if(!isGameShown)
        {
            // Pause the game - we will need to hide
            timeScale = Time.timeScale;
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = timeScale;
        }
    }

    private void AuthCallback(ILoginResult result)
    {
        // 弹出登录成功与否框
        if(_showLoginResult)
        {
            _showLoginResult = false;
           // PopupPanelManager.Current.ShowFacebookLoginLogoutBox(IsLoggedIn ? FacebookLoginLogoutType.loginSucc : FacebookLoginLogoutType.loginFailed);
        }

        InitUserInfo();
    }

    private void GetFriendsCallback(IResult result)
    {
        if(debugInfo)
            FacebookHelperDebug("GetFriendsCallback");
        if(result == null)
        {
            if(debugInfo)
                FacebookHelperDebug("GetFriendsCallback Null Response\n");
            return;
        }

        if(!string.IsNullOrEmpty(result.Error))
        {
            if(debugInfo)
                FacebookHelperDebug("GetFriendsCallback Error Response:\n" + result.Error);
        }
        else if(result.Cancelled)
        {
            if(debugInfo)
                FacebookHelperDebug("GetFriendsCallback Cancelled Response:\n" + result.RawResult);
        }
        else if(!string.IsNullOrEmpty(result.RawResult))
        {
            if(debugInfo)
                FacebookHelperDebug("GetFriendsCallback Success Response:\n" + result.RawResult);
            MTJSONObject mtJson = MTJSON.Deserialize(result.RawResult);
            {// friend
                List<MTJSONObject> friends = mtJson.Get("data").list;
                //UserModel user = GameManager.Instance.User;
                //for(int i = 0; i < friends.Count; i++)
                //{
                //    MTJSONObject friendJson = friends[i];
                //    FriendModel friendModel = user.GetFriendModel(friendJson.GetString("id", ""), true);
                //    friendModel.Name = friendJson.GetString("name", "");

                //    AddDownloadPictureId(friendModel.Id);
                //}
                //Tracker.Instance.FacebookGameFriendsCount(_currFacebookId, friends.Count);
            }
        }
        else
        {
            if(debugInfo)
                FacebookHelperDebug("GetFriendsCallback Empty Response\n");
        }
    }

    private void GetInviteFriendsCallback(IResult result)
    {
        if(debugInfo)
            FacebookHelperDebug("GetInviteFriendsCallback");
        if(result == null)
        {
            if(debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Null Response\n");
            return;
        }

        if(!string.IsNullOrEmpty(result.Error))
        {
            if(debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Error Response:\n" + result.Error);
        }
        else if(result.Cancelled)
        {
            if(debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Cancelled Response:\n" + result.RawResult);
        }
        else if(!string.IsNullOrEmpty(result.RawResult))
        {
            if(debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Success Response:\n" + result.RawResult);
            MTJSONObject mtJson = MTJSON.Deserialize(result.RawResult);
            {// friend
                List<MTJSONObject> friends = mtJson.Get("data").list;
                _inviteFriends.Clear();
                for(int i = 0; i < friends.Count; i++)
                {
                    string id = friends[i].GetString("id", "");
                    if(id.Length > 0)
                    {
                        _inviteFriends.Add(id);
                    }
                }
            }
        }
        else
        {
            if(debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Empty Response\n");
        }
    }

    private void GetUserInfoCallback(IResult result)
    {
        if(debugInfo)
            FacebookHelperDebug("GetUserInfoCallback");
        if(result == null)
        {
            if(debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Null Response\n");
            return;
        }

        if(!string.IsNullOrEmpty(result.Error))
        {
            if(debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Error Response:\n" + result.Error);
        }
        else if(result.Cancelled)
        {
            if(debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Cancelled Response:\n" + result.RawResult);
        }
        else if(!string.IsNullOrEmpty(result.RawResult))
        {
            if(debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Success Response:\n" + result.RawResult);
           // GameServerHelper.Instance.UploadCurrFacebookInfo(result.RawResult);
        }
        else
        {
            if(debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Empty Response\n");
        }
    }

    #region pictures

    void AddDownloadPictureId(string id)
    {
        if(!_pictures.ContainsKey(id) && !_pictureDownloadIds.Contains(id))
        {
            _pictureDownloadIds.Add(id);
            CheckDownloadPicture();
        }
    }

    void CheckDownloadPicture()
    {
        if(_currDownloadId.Length == 0 && _pictureDownloadIds.Count > 0)
        {
            var fristEnum = _pictureDownloadIds.GetEnumerator();
            fristEnum.MoveNext();
            _currDownloadId = fristEnum.Current;
            FB.API(string.Format("/{0}/picture?height=128&width=128", _currDownloadId), HttpMethod.GET, delegate(IGraphResult result)
                {
                    if(debugInfo)
                        FacebookHelperDebug("CheckDownloadPicture");
                    if(result == null)
                    {
                        if(debugInfo)
                            FacebookHelperDebug("CheckDownloadPicture Null Response\n");
                        return;
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(result.Error))
                        {
                            if(debugInfo)
                                FacebookHelperDebug("CheckDownloadPicture Error Response:\n" + result.Error);
                        }
                        else if(result.Cancelled)
                        {
                            if(debugInfo)
                                FacebookHelperDebug("CheckDownloadPicture Cancelled Response:\n" + result.RawResult);
                        }
                        else if(!string.IsNullOrEmpty(result.RawResult))
                        {
                            if(debugInfo)
                                FacebookHelperDebug("CheckDownloadPicture Success Response:\n");
                            if(_pictureDownloadIds.Contains(_currDownloadId))
                            {
                                Sprite pic = Sprite.Create(result.Texture, new Rect(0, 0, result.Texture.width, result.Texture.height), Vector2.one * 0.5f);
                                if(_pictures.ContainsKey(_currDownloadId))
                                {
                                    _pictures[_currDownloadId] = pic;
                                }
                                else
                                {
                                    _pictures.Add(_currDownloadId, pic);
                                }
                                _pictureDownloadIds.Remove(_currDownloadId);
                                ModelBinder.OnPropChanged(ModelPropKey.Picture.ToString(), _currDownloadId);
                                _currDownloadId = "";
                                CheckDownloadPicture();
                            }
                        }
                        else
                        {
                            if(debugInfo)
                                FacebookHelperDebug("CheckDownloadPicture Empty Response\n");
                        }
                    }
                });
        }
    }

    void ResetPictures()
    {
        _pictures.Clear();
        _pictureDownloadIds.Clear();
        _currDownloadId = "";
    }

    public Sprite GetPictureById(string id)
    {
        if(_pictures.ContainsKey(id))
        {
            return _pictures[id];
        }

        return null;
    }

    #endregion
}
