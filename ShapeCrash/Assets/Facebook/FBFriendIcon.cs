using MTUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBFriendIcon : MonoBehaviour {

    public void Init(FriendModel fmodel)
    {
           
        _uid = fmodel.Id;
        _Name.text = fmodel.Name;
        UpdateIcon();
    }

    string _uid  = "";

    public void OnBtnPressed()
    { }

    public GameObject _CheckMark;
    public Text _Name;
    public Image headIcon;


    protected void Awake()
    {
        ModelBinder.BindProp(ModelPropKey.Picture.ToString(), OnPictureChanged);
        
    }
    protected void OnDestroy()
    {
        ModelBinder.UnbindProp(ModelPropKey.Picture.ToString(), OnPictureChanged);
    }

    void UpdateIcon()
    {
        var headSprite = FacebookMgr.current.GetPictureById(_uid);
        if (headSprite != null)
        {
            headIcon.sprite = headSprite;
        }
    }
    public void OnPictureChanged(object changeValue)
    {
        Debug.Log("pp");
        UpdateIcon(); 

        //if (gameObject.activeSelf && _info != null)
        //{
        //    image.UpdateHeadImage(GameManager.Instance.User.GetPicture(_info.id));
        //}
    }
}
