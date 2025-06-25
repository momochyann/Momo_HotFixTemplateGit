using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
public static class NetWorkCheck
{
    public static async UniTask<bool> CheckNetworkAsync()
    {
        try
        {
            using (UnityWebRequest www = UnityWebRequest.Get("https://www.baidu.com"))
            {
                www.timeout = 1; // 5秒超时
                await www.SendWebRequest();
                return !www.result.ToString().Contains("Error");
            }
        }
        catch
        {
            return false;
        }
    }
}
