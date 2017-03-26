using UnityEngine;
using System.Collections;

using System.IO;

using MTXxtea;
using MTUnity.Utils;

public class SoFileMgr  {

    const bool isEn = false;
    public static void Save(string fileName ,string content)
    {
        var path = Application.persistentDataPath + "/" + fileName;

        if (isEn)
        {
            var bt = MTXXTea.Encrypt(content.ToString(), SettingMgr.SKEY);

            FileUtil.WriteAllBytes(path, bt);

        }
        else
        {
            FileUtil.WriteAllText(path, content);
        }
    }

    public static bool Exists(string fileName)
    {
      //  Debug.Log("p " + Application.persistentDataPath);
        var path = Application.persistentDataPath + "/" + fileName;
        if (File.Exists(path))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static string Load(string fileName)
    {
        var path = Application.persistentDataPath + "/" + fileName;
        if (isEn)
        {
            var bt = FileUtil.ReadAllBytes(path);
            if (bt == null)
            {
                return "";
            }
            string content = MTXXTea.DecryptToString(bt,SettingMgr.SKEY);
            return content;
        }
        else
        {
            return File.ReadAllText(path);
        }
    }
}
