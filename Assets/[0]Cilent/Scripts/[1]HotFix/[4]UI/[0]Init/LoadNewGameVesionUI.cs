using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
public class LoadNewGameVesionUI : MonoBehaviour
{
    // Start is called before the first frame update
    InputField badgeInputField;
    Button ConfimButton;
    Text LogText;
    void Awake()
    {
        transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero);
    }
    void Start()
    {
        badgeInputField = transform.Find("BadgeInput").GetComponent<InputField>();
        ConfimButton = transform.Find("ConfimButton").GetComponent<Button>();
        transform.Find("ExictButton").GetComponent<Button>().onClick.AddListener(()=>{
            transform.DOScale(Vector3.zero, 0.5f);
            Destroy(gameObject,0.6f);
        });
        ConfimButton.onClick.AddListener(OnClickConfimButton);
        LogText = transform.Find("LogText").GetComponent<Text>();
    }

    async void OnClickConfimButton() //这里要加网络确认 或者从网络获取 或者从热更新脚本
    {
        var _badge = badgeInputField.text;
        var hotBadgeConfig = await LoadYooAssetsTool.LoadAsset<HotBadgeConfig>("HotBadgeConfig");
        if (hotBadgeConfig.badges.Contains(_badge))
        {
            PlayerPrefs.SetString("Badge", badgeInputField.text);
            LogText.text = "版本切换成功,请重新进入游戏";
        }
        else
        {
            LogText.text = "版本不存在,请输入正确的版本";
        }
    }
 
}
