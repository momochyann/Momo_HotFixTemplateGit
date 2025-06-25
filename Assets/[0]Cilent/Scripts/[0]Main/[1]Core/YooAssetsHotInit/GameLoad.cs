using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
public class GameLoad : MonoBehaviour
{
    // Start is called before the first frame update
    LoadDll loadDll;
    void Start()
    {
        LoadStart().Forget();
    }
    public async UniTaskVoid LoadStart()
    {
        await UniTask.DelayFrame(1);
        loadDll = gameObject.GetComponent<LoadDll>();
        bool isNetwork = await NetWorkCheck.CheckNetworkAsync();//判断是否联网
        Debug.Log($"isNetWork:{isNetwork}DefaultPackageGameVersion:{PlayerPrefs.GetString("DefaultPackageGameVersion")}");
        if (isNetwork) //如果联网就加载在线资源
        {
#if !UNITY_EDITOR
            loadDll.PlayMode = EPlayMode.HostPlayMode;
#endif
            loadDll.LoadStart(false).Forget();

        }
        else//如果断网就加载本地资源
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("DefaultPackageGameVersion")))
            {
                loadDll.PlayMode = EPlayMode.OfflinePlayMode; //加载打包时的资源
                loadDll.LoadStart(true).Forget();
            }
            else
            {
                loadDll.PlayMode = EPlayMode.HostPlayMode; //加载最后更新的资源
                loadDll.LoadStart(true).Forget();          
            }
        }
    }
}
