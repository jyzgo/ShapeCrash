using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


#if UNITY_IOS
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
#endif

public class NotiMgr : MonoBehaviour {

    public static void NotificationMessage(string message, int hour, bool isRepeatDay)
    {
#if UNITY_IOS
        int year = System.DateTime.Now.Year;
        int month = System.DateTime.Now.Month;
        int day = System.DateTime.Now.Day;
        System.DateTime newDate = new System.DateTime(year, month, day, hour, 0, 0);
        NotificationMessage(message, newDate, isRepeatDay);
#endif
    }
    //本地推送 你可以传入一个固定的推送时间
    public static void NotificationMessage(string message, System.DateTime newDate, bool isRepeatDay)
    {
#if UNITY_IOS
        //推送时间需要大于当前时间
        if (newDate > System.DateTime.Now)
        {
            UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
            localNotification.fireDate = newDate;
            localNotification.alertBody = message;
            localNotification.applicationIconBadgeNumber = -1;

            IDictionary notiInfo = new Dictionary<string, string>();
            notiInfo.Add(notiType, "value");

            localNotification.userInfo = notiInfo;

            localNotification.hasAction = true;
            if (isRepeatDay)
            {
                //是否每天定期循环
               // localNotification.repeatCalendar = UnityEngine.iOS.CalendarIdentifier. ChineseCalendar;
                localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
            }
            localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
            UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);

        }
#endif
    }
    const string notiType = "daynoti";

    public static NotiMgr current;
    void Awake()
    {
        current = this;
        //第一次进入游戏的时候清空，有可能用户自己把游戏冲后台杀死，这里强制清空
#if UNITY_IOS
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(
             UnityEngine.iOS.NotificationType.Alert |
             UnityEngine.iOS.NotificationType.Badge |
            UnityEngine.iOS.NotificationType.Sound);
#endif

        CleanNotification();
        enterTime = DateTime.Now;
        RegNoti();
       
       
    }
    DateTime enterTime;


    void RegNoti()
    {
        TimeSpan start = new TimeSpan(8, 0, 0);
        TimeSpan end = new TimeSpan(22, 0, 0);

   
        if (TimeBetween(enterTime, start,end))
        {
            NotificationMessage(NOTI_MSG,enterTime.AddHours(24), true);
        }
        else
        {
            int year = System.DateTime.Now.Year;
            int month = System.DateTime.Now.Month;
            int day = System.DateTime.Now.Day;
            System.DateTime newDate = new System.DateTime(year, month, day, 12, 0, 0);
            NotificationMessage(NOTI_MSG, newDate.AddHours(24), true);
        }

    }

    bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
    {
        // convert datetime to a TimeSpan
        TimeSpan now = datetime.TimeOfDay;
        // see if start comes before end
        if (start < end)
            return start <= now && now <= end;
        // start is after end, so do the inverse comparison
        return !(end < now && now < start);
    }

    const string NOTI_MSG = "Play some solitaire to keep your mind active!";
    void OnApplicationPause(bool paused)
    {
        //程序进入后台时
        if (paused)
        {

            //RegNoti();
        }
        else
        {
            //程序从后台进入前台时
            CleanNotification();
        }
    }

    //清空所有本地消息
    void CleanNotification()
    {
#if UNITY_IOS
        
        UnityEngine.iOS.LocalNotification l = new UnityEngine.iOS.LocalNotification();
        l.applicationIconBadgeNumber = -1;
        l.hasAction = false;
        UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow(l);
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
        UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
#endif
    }

    public void CancelScheduledNotifications(string type)
    {

#if UNITY_IOS
        UnityEngine.iOS.LocalNotification[] locals = NotificationServices.scheduledLocalNotifications;

        for (int i = 0; i < locals.Length; i++)
        {
            if (locals[i].userInfo != null && locals[i].userInfo.Contains(type))
            {
                NotificationServices.CancelLocalNotification(locals[i]);
            }
        }
#endif

    }
}
