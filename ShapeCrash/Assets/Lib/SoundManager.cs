using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour {

    public static SoundManager Current;

    public SettingMgr _settingMgr;

    public AudioClip _btnClip;
    public AudioClip _ui_open;
    public AudioClip _ui_close;
    public AudioClip _switch;
    public AudioClip _put_success;
    public AudioClip _shake_card;
    public AudioClip _new_game;

    public AudioClip _flip_pile;
    public AudioClip _put;



    public AudioSource _winSource;
    public AudioSource _commonEff;

    void Awake()
    {
        Current = this;
    }

    public void PlayWinMusic()
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        _winSource.Play();
    }
    public void Play_btnClip(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_btnClip, t);
    }
    public void Play_ui_open(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_ui_open, t);
    }
    public void Play_ui_close(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_ui_close, t);
    }
    public void Play_switch(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_switch, t);
    }
    public void Play_put_success(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_put_success, t);
    }
    public void Play_shake_card(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_shake_card, t);
    }
    public void Play_new_game(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_new_game, t);
    }

    public void Play_flip_pile(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_flip_pile, t);
    }
    public void Play_put(float t)
    {
        if (_settingMgr.SoundControl != 1)
        {
            return;
        }
        PlayClip(_put, t);
    }

    void PlayClip(AudioClip clip,float t)
    {
        StartCoroutine(DelayPlaySound(clip,t));
    }
    IEnumerator DelayPlaySound(AudioClip clip, float t)
    {
        yield return new WaitForSeconds(t);
        _commonEff.clip = clip;
        _commonEff.Stop();
        _commonEff.Play();
    }
}
