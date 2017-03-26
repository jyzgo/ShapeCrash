using System.Collections.Generic;
using System;
using UnityEngine;
using MTUnity.Utils;
using MTUnity.Actions;
using System.Runtime.Serialization;
using MTUnity;


public class UserModel
{
    string _uid = "";
    // 用户id
    public string uid
    {
        get
        {
            return _uid;
        }
        set
        {
            _uid = value;
        }
    }

    bool _currUser = false;

    Dictionary<string, FriendModel> _friendModels = new Dictionary<string, FriendModel>();

    public FriendModel GetFriendModel(string id, bool create)
    {
        FriendModel res = null;
        if (_friendModels.ContainsKey(id))
        {
            res = _friendModels[id];
        }
        else if (create)
        {
            res = new FriendModel(_currUser);
            res.Id = id;
            _friendModels.Add(id, res);

        }
        return res;
    }

    public int FriendCount
    {
        get { return _friendModels.Count; }
    }
}
[Serializable]
public class FriendModel
{
    //**********************************
    // id 从V1开始: 好友的uid
    //**********************************
    private string _id = "";
    //**********************************
    // name 从V1开始: 好友的名字
    //**********************************
    private string _name = "";
    //**********************************
    // maxLevel 从V1开始: 好友的名字
    //**********************************
    private int _maxLevel = 0;
    //**********************************
    // levels 从V1开始: 好友对应关卡的分数
    //**********************************
    private Dictionary<int, int> _levels = new Dictionary<int, int>();

    private bool _currUser = false;

    public FriendModel(bool currUser)
    {
        _currUser = currUser;
    }

    public string Id
    {
        set
        { 
            _id = value;
            if (_currUser)
            {
             //   GameManager.Instance.MarkUserDataIsDirty();
               // ModelBinder.OnPropChanged(ModelPropKey.UserFriends.ToString(), Id);
            }
        }
        get
        { 
            return _id;
        }
    }

    public string Name
    {
        set
        { 
            _name = value;
            if (_currUser)
            {
             //   GameManager.Instance.MarkUserDataIsDirty();
            }
        }
        get
        { 
            return _name;
        }
    }

    public int MaxLevel
    {
        set
        { 
            _maxLevel = value;
            if (_currUser)
            {
              //  GameManager.Instance.MarkUserDataIsDirty();
                //ModelBinder.OnPropChanged(ModelPropKey.FriendMaxLevel.ToString(), Id);
            }
        }
        get
        { 
            return _maxLevel;
        }
    }

    public int GetLevelScore(int level)
    {
        if (_levels.ContainsKey(level))
        {
            return _levels[level];
        } 

        return 0;
    }

    public void UpdateLevelScore(int level, int score)
    {
        if (_levels.ContainsKey(level))
        {
            _levels[level] = score;
        }
        else
        {
            _levels.Add(level, score);
        }
        if (_currUser)
        {
          //  GameManager.Instance.MarkUserDataIsDirty();
        }
    }

    public void DeserializeFromJson(MTJSONObject curJson, bool sync)
    {
        Id = curJson.GetString("id", Id);
        Name = curJson.GetString("name", Name);
        MaxLevel = curJson.GetInt("maxLevel", MaxLevel);

        //  levels
        MTJSONObject levelsJson = curJson.Get("levels");
        if (levelsJson != null)
        {
            if (!sync)
            {
                _levels.Clear();
            }

            if (levelsJson.count > 0)
            {
                var itemEnum = levelsJson.dict.GetEnumerator();
                while (itemEnum.MoveNext())
                {
                    string curKey = itemEnum.Current.Key;
                    int curValue = itemEnum.Current.Value.i;
                    int curKeyInt = Convert.ToInt32(curKey);

                    if (!_levels.ContainsKey(curKeyInt))
                    {
                        _levels.Add(curKeyInt, curValue);
                    }
                    else
                    {
                        _levels[curKeyInt] = curValue;
                    }
                }
            }
        }
    }

    public MTJSONObject SerializeToJson(bool justSyncData)
    {
        MTJSONObject res = MTJSONObject.CreateDict();

        res.Set("id", _id);
        res.Set("name", _name);
        res.Set("maxLevel", _maxLevel);

        // levels
        var levelsEnum = _levels.GetEnumerator();
        MTJSONObject levelsJson = MTJSONObject.CreateDict();
        while (levelsEnum.MoveNext())
        {
            var current = levelsEnum.Current;
            levelsJson.Set(current.Key.ToString(), current.Value);
        }
        res.Set("levels", levelsJson);

        return res;
    }
}

