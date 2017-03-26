using Facebook.Unity;
using MTUnity;
using MTUnity.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModelPropKey
{
    Picture
}

public class FacebookMgr : MonoBehaviour
{
    public bool active = true;
    public static FacebookMgr current;

    // invite friends
    private List<string> _inviteFriends = new List<string>();

    void Awake()
    {
        current = this;
     
    }

    UserModel _user;

    public void InitFacebook()
    {
        FacebookHelperDebug("ininnnn");
        _user = SettingMgr.current._user;
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }
    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    public void FacebookLogin()
    {

        var perms = new List<string>() { "public_profile", "email", "user_friends" };
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }
    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            //Debug.Log(aToken.UserId);
            FacebookHelperDebug(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                FacebookHelperDebug(perm); // Debug.Log(perm);
            }
            InitUserInfo();
        }
        else
        {
            FacebookHelperDebug("cancel login");
            //Debug.Log("User cancelled login");
        }
    }

    public void InitUserInfo()
    {
        FacebookHelperDebug("init user in fo");
        if (IsLoggedIn)
        {
            string facebookId = Facebook.Unity.AccessToken.CurrentAccessToken.UserId;
            _user.uid = facebookId;
            //  UserModel user = GameManager.Instance.User;
            //if(user != null && _currFacebookId != facebookId)
            //{
            //    _currFacebookId = facebookId;
            //    Tracker.Instance.FacebookLogin(_currFacebookId);
            //    // 获得facebook的信息
                FB.API("/me?fields=id,name,first_name,last_name,age_range,link,gender,locale,timezone,updated_time,verified,email", HttpMethod.GET, GetUserInfoCallback);

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
               // GetGameFriends();
                GetInviteFriends();
               // AddDownloadPictureId(facebookId);

            //    gameServerHelper.SyncCurrUserData();
            //    gameServerHelper.UpdateCurrUserFriendsInfo();
            //    gameServerHelper.GetCurrUserInboxMessages();
            //}
            //ClearRequests();
        }
        //InvokeFacebookStatusChanged();
    }


    public void FacebookShare()
    {
        FB.ShareLink(new Uri("https://developers.facebook.com/"), callback: ShareCallback);
    }

    private void ShareCallback(IShareResult result)
    {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
        {
            Debug.Log("ShareLink Error: " + result.Error);
        }
        else if (!String.IsNullOrEmpty(result.PostId))
        {
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
        }
        else
        {
            // Share succeeded without postID
            Debug.Log("ShareLink success!");
        }

    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
            FacebookHelperDebug("init suc active app");
        }
        else
        {
            FacebookHelperDebug("fail load sdk");
            //Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    public GameObject FacebookInvite;

    public void GetGameFriends()
    {
        // 获得游戏好友信息
        if (IsLoggedIn)
        {
            FB.API("/me/friends", HttpMethod.GET, GetFriendsCallback);
        }
    }

    public void GetInviteFriends()
    {
        if (IsLoggedIn)
        {
            FB.API("/me/invitable_friends", HttpMethod.GET, GetInviteFriendsCallback);
        }

    }

    public bool IsLoggedIn
    {
        get { return active && FB.IsLoggedIn; }
    }

    private void GetFriendsCallback(IResult result)
    {

            FacebookHelperDebug("GetFriendsCallback");
        if (result == null)
        {

                FacebookHelperDebug("GetFriendsCallback Null Response\n");
            return;
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
           
                FacebookHelperDebug("GetFriendsCallback Error Response:\n" + result.Error);
        }
        else if (result.Cancelled)
        {
           
                FacebookHelperDebug("GetFriendsCallback Cancelled Response:\n" + result.RawResult);
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            ShowInvite();
            FacebookHelperDebug("GetFriendsCallback Success Response:\n" + result.RawResult);

            MTJSONObject mtJson = MTJSON.Deserialize(result.RawResult);
            {// friend
                List<MTJSONObject> friends = mtJson.Get("data").list;

                for (int i = 0; i < friends.Count; i++)
                {
                    MTJSONObject friendJson = friends[i];
                    FriendModel friendModel = _user.GetFriendModel(friendJson.GetString("id", ""), true);
                    friendModel.Name = friendJson.GetString("name", "");
                    FacebookInviteMgr.current.AddIcon(friendModel);

                    AddDownloadPictureId(friendModel.Id);
                }
               // Tracker.Instance.FacebookGameFriendsCount(_currFacebookId, friends.Count);
            }
        }
        else
        {
          
                FacebookHelperDebug("GetFriendsCallback Empty Response\n");
        }
    }


    public void HideInvite()
    {
        FacebookInvite.SetActive(false);
    }

    public void ShowInvite()
    {
        FacebookInvite.SetActive(true);
    }

    public static void FacebookHelperDebug(string info)
    {
      //  return;
        Debug.Log("[FacebookHelper] " + info);
    }

    private void GetInviteFriendsCallback(IResult result)
    {
        if (debugInfo)
            FacebookHelperDebug("GetInviteFriendsCallback");
        if (result == null)
        {
            if (debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Null Response\n");
            return;
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
            if (debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Error Response:\n" + result.Error);
        }
        else if (result.Cancelled)
        {
            if (debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Cancelled Response:\n" + result.RawResult);
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            if (debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Success Response:\n" + result.RawResult);
            MTJSONObject mtJson = MTJSON.Deserialize(result.RawResult);
            {// friend
                List<MTJSONObject> friends = mtJson.Get("data").list;
                _inviteFriends.Clear();
                for (int i = 0; i < friends.Count; i++)
                {

                    MTJSONObject friendJson = friends[i];
                    FriendModel friendModel = _user.GetFriendModel(friendJson.GetString("id", ""), true);
                    friendModel.Name = friendJson.GetString("name", "");
                    FacebookInviteMgr.current.AddIcon(friendModel);

                    AddDownloadPictureId(friendModel.Id);

                    //string id = friends[i].GetString("id", "");
                    //if (id.Length > 0)
                    //{
                    //    _inviteFriends.Add(id);
                    //}
                }
            }
        }
        else
        {
            if (debugInfo)
                FacebookHelperDebug("GetInviteFriendsCallback Empty Response\n");
        }
    }


    private void GetUserInfoCallback(IResult result)
    {
        if (debugInfo)
            FacebookHelperDebug("GetUserInfoCallback");
        if (result == null)
        {
            if (debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Null Response\n");
            return;
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
            if (debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Error Response:\n" + result.Error);
        }
        else if (result.Cancelled)
        {
            if (debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Cancelled Response:\n" + result.RawResult);
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            if (debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Success Response:\n" + result.RawResult);
            // GameServerHelper.Instance.UploadCurrFacebookInfo(result.RawResult);
        }
        else
        {
            if (debugInfo)
                FacebookHelperDebug("GetUserInfoCallback Empty Response\n");
        }
    }

    #region pictures

    // picture
    private string _currDownloadId = "";
    private HashSet<string> _pictureDownloadIds = new HashSet<string>();
    private Dictionary<string, Sprite> _pictures = new Dictionary<string, Sprite>();


    

    void AddDownloadPictureId(string id)
    {
        if (!_pictures.ContainsKey(id) && !_pictureDownloadIds.Contains(id))
        {
            _pictureDownloadIds.Add(id);
            CheckDownloadPicture();
        }
    }

    public bool debugInfo = true;
    void CheckDownloadPicture()
    {
        if (_currDownloadId.Length == 0 && _pictureDownloadIds.Count > 0)
        {
            var fristEnum = _pictureDownloadIds.GetEnumerator();
            fristEnum.MoveNext();
            _currDownloadId = fristEnum.Current;
            Debug.LogWarning("cccc " + _currDownloadId);
            FB.API(string.Format("/{0}/picture?height=128&width=128", _currDownloadId), HttpMethod.GET, delegate (IGraphResult result)
            {
                if (debugInfo)
                    FacebookHelperDebug("CheckDownloadPicture");
                if (result == null)
                {
                    if (debugInfo)
                        FacebookHelperDebug("CheckDownloadPicture Null Response\n");
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(result.Error))
                    {
                        if (debugInfo)
                            FacebookHelperDebug("CheckDownloadPicture Error Response:\n" + result.Error);
                    }
                    else if (result.Cancelled)
                    {
                        if (debugInfo)
                            FacebookHelperDebug("CheckDownloadPicture Cancelled Response:\n" + result.RawResult);
                    }
                    else if (!string.IsNullOrEmpty(result.RawResult))
                    {
                        if (debugInfo)
                            FacebookHelperDebug("CheckDownloadPicture Success Response:\n");
                        if (_pictureDownloadIds.Contains(_currDownloadId))
                        {
                            Sprite pic = Sprite.Create(result.Texture, new Rect(0, 0, result.Texture.width, result.Texture.height), Vector2.one * 0.5f);
                            if (_pictures.ContainsKey(_currDownloadId))
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
                            Debug.Log("id dddd  " + _currDownloadId);
                            CheckDownloadPicture();
                        }
                    }
                    else
                    {
                        if (debugInfo)
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
        if (_pictures.ContainsKey(id))
        {
            return _pictures[id];
        }

        return null;
    }

    #endregion
}
