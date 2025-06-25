using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SerialPortSetPanel : MonoBehaviour
{
    // Start is called before the first frame update
    InputField portInputField;
    InputField baudRateInputField;
    Button confirmButton;
    void Start()
    {
        portInputField = transform.Find("PortInputField").GetComponent<InputField>();
        baudRateInputField = transform.Find("BaudRateInputField").GetComponent<InputField>();
        confirmButton = transform.Find("ConfirmButton").GetComponent<Button>();
        confirmButton.onClick.AddListener(OnClickConfirmButton);
        SerialPortUtilityManager.Instance.RegisterSerialPortSetPanel(true);
    }

    private void OnClickConfirmButton()
    {
        if (int.TryParse(baudRateInputField.text, out int baudRate))
        {
            SerialPortUtilityManager.Instance.SetBaudRate(baudRate);
        }
        if (int.TryParse(portInputField.text, out int port))
        {
            SerialPortUtilityManager.Instance.SetComPortName(port);
        }
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        SerialPortUtilityManager.Instance.RegisterSerialPortSetPanel(false);
    }

}
