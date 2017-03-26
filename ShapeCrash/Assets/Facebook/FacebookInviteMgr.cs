using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacebookInviteMgr : MonoBehaviour {

    public GameObject IconPrefab;

    public Transform UIRoot;

    public static FacebookInviteMgr current;
    void Awake()
    {
        current = this;    
    }
    
    public void AddIcon(FriendModel fmodel)
    {
        var icon = Instantiate<GameObject>(IconPrefab);
        icon.transform.SetParent(UIRoot);
        icon.transform.localScale = new Vector3(1, 1, 1);
        var iconSc = icon.GetComponent<FBFriendIcon>();
        iconSc.Init(fmodel);
    }
}
