using System.Collections;
using QFramework;
using UnityEngine;
using SerialPortUtility;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class SerialPortUtilityManager : MonoSingleton<SerialPortUtilityManager>
{
    // Start is called before the first frame update
    [SerializeField] int ComPortName;
    public UnityAction<byte[]> OnSerialDataReceived;
    SerialPortUtilityPro serialPortUtilityPro;
    [HideInInspector] public bool serialPortSetPanelIsActive = false;
    void Start()
    {
        serialPortUtilityPro = GetComponent<SerialPortUtilityPro>();
        serialPortUtilityPro.OpenMethod = SerialPortUtilityPro.OpenSystem.NumberOrder;
        serialPortUtilityPro.BaudRate = PlayerPrefs.GetInt("BaudRate", 115200);
        serialPortUtilityPro.Skip = PlayerPrefs.GetInt("ComPortName", ComPortName);
        serialPortUtilityPro.ReadCompleteEventObject.AddListener(OnDataReceived);  // 绑定接收函数
        serialPortUtilityPro.Open();  // 打开串口
        Debug.Log("打开串口");
        // Xunhuanfasong().Forget();
    }
    void OnDataReceived(object data)
    {
        byte[] bytes = (byte[])data;

        // string str = System.Text.Encoding.UTF8.GetString(bytes);
        OnSerialDataReceived?.Invoke(bytes);
    }
    async UniTaskVoid Xunhuanfasong()
    {
        await UniTask.Delay(1000);
        while (serialPortUtilityPro.IsOpened())
        {
            serialPortUtilityPro.Write(new byte[] { 0x77, 0x73, 0x3a, 0x0a });
            await UniTask.Delay(1000);
            Debug.Log("发送数据");
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            OpenSerialPortSetPanel().Forget();
        }
    }
    async UniTaskVoid OpenSerialPortSetPanel()
    {
        var panel = await LoadYooAssetsTool.LoadAsset<GameObject>("SerialPortSetPanel", true);
        if (serialPortSetPanelIsActive == false)
        {
            Instantiate(panel, FindObjectOfType<Canvas>().transform);
        }
    }
    public void SetBaudRate(int baudRate)
    {
        if (serialPortSetPanelIsActive == false)
            return;
        serialPortUtilityPro.Close();
        serialPortUtilityPro.BaudRate = baudRate;
        PlayerPrefs.SetInt("BaudRate", baudRate);
        serialPortUtilityPro.Open();
    }
    public void SetComPortName(int comPortName)
    {
        if (serialPortSetPanelIsActive == false)
            return;
        serialPortUtilityPro.Close();
        serialPortUtilityPro.Skip = comPortName;
        PlayerPrefs.SetInt("ComPortName", comPortName);
        serialPortUtilityPro.Open();
    }
    public void RegisterSerialPortSetPanel(bool isActive)
    {
        serialPortSetPanelIsActive = isActive;
    }
    // Update is called once per frame

}
