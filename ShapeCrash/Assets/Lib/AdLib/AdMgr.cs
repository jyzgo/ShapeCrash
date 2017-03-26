//#define APPLOVIN_ENABLED
#define ADMOB_ENABLED
//#define VUNGLE_ENABLED
#define FBAD_ENABLED

using UnityEngine;
using System.Collections;
using MTUnity;
#if (ADMOB_ENABLED)
using admob;
#endif

public static class AdMgr  {
    #region Admob-------------------------
    const string BANNER_ID = "ca-app-pub-8151883983314364/7268920130";
    const string INTERSTITIAL_ID = "ca-app-pub-8151883983314364/4175852934";

#if (UNITY_IOS)
    const string OMG_NATIVE_BANNER_ID = "ca-app-pub-9169799985632280/7223730851";
    const string OMG_INTERSTITIAL_ID = "ca-app-pub-9169799985632280/8700464051";
#elif (UNITY_ANDROID)
    const string OMG_NATIVE_BANNER_ID = "ca-app-pub-9169799985632280/9572081659";
    const string OMG_INTERSTITIAL_ID = "ca-app-pub-9169799985632280/2048814858";
#endif
    static void RegiseterAdmob(LevelMgr lvMgr)
    {
#if (ADMOB_ENABLED)
        ad = Admob.Instance();
        ad.bannerEventHandler += lvMgr.onBannerEvent;
        //ad.setTesting(true);
        ad.interstitialEventHandler += lvMgr.onInterstitialEvent;
        ad.rewardedVideoEventHandler += lvMgr.onRewardedVideoEvent;
        ad.nativeBannerEventHandler += lvMgr.onNativeBannerEvent;
        ad.initAdmob("", OMG_INTERSTITIAL_ID);
        
        //   ad.setTesting(true);
        ad.setGender(AdmobGender.MALE);
        string[] keywords = { "game", "crash", "male game" };
        ad.setKeywords(keywords);

        PreloadAdmobInterstitial();
        //PreloadAdmobRewaredVideo();
        //Debug.Log("admob inited -------------");
#endif
    }
#if (ADMOB_ENABLED)
    static Admob ad;
#endif

    public static void ShowAdmobInterstitial()
    {
#if (ADMOB_ENABLED)
      

        if (ad.isInterstitialReady())
        {
            TrackAd("-3", "admob");
            ad.showInterstitial();
        }
        else
        {
            ad.loadInterstitial();
        }
#endif
    }

    public static void PreloadAdmobInterstitial()
    {
#if (ADMOB_ENABLED)
      //  if (!ad.isInterstitialReady())
        {
            ad.loadInterstitial();
        }
#endif

    }

    public static void TrackAdMob(string idStr)
    {
#if (ADMOB_ENABLED)

        TrackAd(idStr,"admob");
#endif
    }

    public static void PreloadAdmobRewaredVideo()
    {
#if (ADMOB_ENABLED)
        if (!ad.isRewardedVideoReady())
        {
            ad.loadRewardedVideo(ADMOB_REWARDVIDEO_ID);
        }
#endif
    }

    const string ADMOB_REWARDVIDEO_ID = "ca-app-pub-8151883983314364/7129319338";
    public static void ShowAdmobRewardVideo()
    {
#if (ADMOB_ENABLED)
        if (ad.isRewardedVideoReady())
        {
            ad.showRewardedVideo();
        }
        else
        {
            ad.loadRewardedVideo(ADMOB_REWARDVIDEO_ID);

        }
#endif
    }

    public static void ShowAdmobBanner()
    {
#if (ADMOB_ENABLED)
        ad.showBannerRelative(AdSize.SmartBanner, AdPosition.TOP_CENTER, 0);
        //ToolbarMgr.current.MoveUp();
#endif
    }

    public static bool IsAdmobInterstitialReady()
    {
#if (ADMOB_ENABLED)
        if (ad == null)
        {
            return false;
        }
        else
        {
         return ad.isInterstitialReady();
        }
#endif
        return false;
    }

    public static void ShowNativeBanner(int w,int h,int x,int y )
    {
#if (ADMOB_ENABLED)
        if (ad != null)
        {
           //ad.showNativeBannerAbsolute(new AdSize(w, h), x, y, OMG_NATIVE_BANNER_ID);
            ad.showNativeBannerRelative(new AdSize(w, h), AdPosition.TOP_CENTER, y, OMG_NATIVE_BANNER_ID);
        }
      
#endif

    }


    public static void HideNativeBanner()
    {
#if (ADMOB_ENABLED)

        ad.removeNativeBanner();
#endif
    }



#endregion Admob

#region Applovin

    public static void TrackApplovin(string idStr)
    {
#if (APPLOVIN_ENABLED)
     //MTTracker.Instance.Track(SoliTrack.ads, StatisticsMgr.current.WinsCount(), idStr, "applovin");
        TrackAd(idStr,"applovin");
#endif
    }

    static void TrackAd(string idStr, string plat)
    {
      //  MTTracker.Instance.Track(SoliTrack.ads, StatisticsMgr.current.WinsCount(), idStr, plat);
    }

    static void RegisterApplovin(string name)
    {
#if (APPLOVIN_ENABLED)
        AppLovin.SetSdkKey("S-97vthZQs06t7HeooDap3KnwTWEmpBDnLKgC0EdtElkM0N9oakbd3NTbvoLh-TTJt8WuM9lbuSyur5gPqH2te");
        AppLovin.InitializeSdk();
        AppLovin.SetUnityAdListener(name);
        AppLovin.PreloadInterstitial();
#else
        return;
#endif
    }

    public static void RegisterAllAd(LevelMgr lv)
    {
        return;
#if (APPLOVIN_ENABLED)
        RegisterApplovin(lv.gameObject.name);

#endif
#if (ADMOB_ENABLED)
        RegiseterAdmob(lv);
#endif

#if (VUNGLE_ENABLED)
        RegisterVungle(lv);
#endif
    }




    public static bool ApplovinHasPreloadedInterstitial()
    {      
#if (APPLOVIN_ENABLED)
        return AppLovin.HasPreloadedInterstitial();
#else
        return false;
#endif
    }


    public static void ShowApplovinInterstitial()
    {
#if (APPLOVIN_ENABLED)
        TrackApplovin("-3");
        AppLovin.ShowInterstitial();
#endif
    }

    public static void ApplovinPreloadInterstitial()
    {
#if (APPLOVIN_ENABLED)
        AppLovin.PreloadInterstitial();
#endif
    }
#endregion Applovin

#region Vungle
    static void  RegisterVungle(LevelMgr lv)
    {
#if (VUNGLE_ENABLED)
        Vungle.init("58c21f7bf97347fe69000060", "58c21e1be5ffdf116e000060");
        
        //Vungle.adPlayableEvent += lv.VunlgePlayAbleEvent;
            
#endif
    }

    public static void OnApplicationPause(bool isPause)
    {
#if (VUNGLE_ENABLED)
        if (isPause)
        {
            Vungle.onPause();
        }
        else
        {
            Vungle.onResume();
        }
#endif
    }

    public static void PlayVungleAd()
    {
#if (VUNGLE_ENABLED)
        if (Vungle.isAdvertAvailable())
        {
            Vungle.playAd();
        }
#endif
    }


#endregion Vungle

}
