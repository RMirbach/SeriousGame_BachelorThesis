using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class ScanScene : MonoBehaviour
{
    private const string TAG = "ScanSceneScript; ";
    private const bool isLogging = true;

    [SerializeField]
    private bool isColorizedScan;

    [SerializeField]
    private Button buttonSubscriptions;

    [SerializeField]
    private Button buttonScan;

    [SerializeField]
    private RectTransform scrollViewContentScan;

    [SerializeField]
    private GameObject ScanElement;

    private List<GameObject> ScanElements = new List<GameObject>();

    private float scanElementHeight = 0;

    // provides button from being pressed multiple times
    private bool isButtonSubscriptionsPressed = true;


    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && ScanController.IsInitialized)
        {
            buttonScan.GetComponentInChildren<TMP_Text>().text = "Start scanning";
            ScanController.StopScan();
        }
    }

    private void Start()
    {
        LogNative.Log(isLogging, TAG + "Start");

        isButtonSubscriptionsPressed = false;
        // attach events
        ScanController.Event += OnScanControllerCallbackEvent;
        MovesenseController.Event += OnMovesenseControllerCallbackEvent;

        // needed if came back from other scene
        RefreshScrollViewContentScan();
    }

    public void OnClickButtonScan()
    {
        // check if mothod starts or stops scan
        bool isScanning = ScanController.IsScanning;
        LogNative.Log(isLogging, TAG + "onClickButtonScan " + (isScanning ? "stop scanning" : "start scanning"));

        if (ScanController.IsInitialized)
        {
            if (isScanning)
            {
                ScanController.StopScan();
            }
            else
            {
                ScanController.StartScan();
            }
        }
        else
        {
            LogNative.LogError(TAG + "onClickButtonScan, ScanController is NOT initialized. Did you forget to add ScanController object in the scene?");
        }
    }

    public void OnClickButtonConnect(string macID, string serial, bool isConnecting, bool isConnected)
    {
        // attached method for ScanElementClone
        LogNative.Log(isLogging, TAG + "OnClickButtonConnect, " + ((isConnecting || isConnected) ? "disconnecting: " : "connecting: ") + macID + " (" + serial + ")");

        if (MovesenseController.isInitialized)
        {
            if (isConnecting || isConnected)
            {
                MovesenseController.Disconnect(macID);
            }
            else
            {
                MovesenseController.Connect(macID);
            }
        }
        else
        {
            LogNative.LogError(TAG + "OnClickButtonConnect, MovesenseController is NOT initialized. Did you forget to add MovesenseController object in the scene?");
        }
    }

    public void OnClickButtonSubscriptions()
    {
        if (isButtonSubscriptionsPressed)
        {
            isButtonSubscriptionsPressed = true;
            return;
        }
        LogNative.Log(isLogging, TAG + "onClickButtonSubscriptions");

        // detach events
        ScanController.Event -= OnScanControllerCallbackEvent;
        MovesenseController.Event -= OnMovesenseControllerCallbackEvent;

        ScanController.StopScan();

        MovesenseDevice.RemoveUnconnected();

        //ChangeSceneController.LoadSceneByName("SubscriptionScene");
    }

    void OnScanControllerCallbackEvent(object sender, ScanController.EventArgs e)
    {
        switch (e.Type)
        {
            case ScanController.EventType.SYSTEM_SCANNING:
                buttonScan.GetComponentInChildren<TMP_Text>().text = "Stop scanning";
                break;
            case ScanController.EventType.SYSTEM_NOT_SCANNING:
                buttonScan.GetComponentInChildren<TMP_Text>().text = "Start scanning";
                break;
            case ScanController.EventType.NEW_DEVICE:           // a new sensor was found
            case ScanController.EventType.RSSI:                 // RSSI of a sensor has changed and the MovesenseDevice sortorder has been updated
            case ScanController.EventType.REFRESH:              // a sensor got out of reach and was removed from MovesenseDevice-list
            case ScanController.EventType.REMOVE_UNCONNECTED:   // at scanstart all unconnected sensors are removed from MovesenseDevice-list
                RefreshScrollViewContentScan();
                break;
        }
    }

    void OnMovesenseControllerCallbackEvent(object sender, MovesenseController.EventArgs e)
    {
        switch (e.Type)
        {
            case MovesenseController.EventType.CONNECTING:      // a sensor is currently connecting
                LogNative.Log(isLogging, TAG + "RefreshScrollViewContentScan: EventType.CONNECTING");

                RefreshScrollViewContentScan();
                break;
            case MovesenseController.EventType.CONNECTED:       // a sensor is succesfully connected
                LogNative.Log(isLogging, TAG + "RefreshScrollViewContentScan: EventType.CONNECTED");

                RefreshScrollViewContentScan();

                buttonSubscriptions.gameObject.SetActive(true);

                // add "/" in the next line to activate Test response. 
                /*/ Test response 
				isResponseLogging = true;
				for (int i = 0; i < e.OriginalEventArgs.Count; i++) {
					var ce = (ConnectCallback.EventArgs) e.OriginalEventArgs[i];
					MovesenseController.ResponseGet(ce.Serial, "Info");
						
					MovesenseController.ResponseGet(ce.Serial, "System/Energy/Level");

					MovesenseController.ResponseGet(ce.Serial, "Misc/Gear/Id");

					//MovesenseController.Subscribe(ce.Serial, "Misc/Gear/Id", null); // IMPORTANT NOTICE: Subscription to Gear ID AND Heartrate or ECG do not work at the same time
				}
				// End Test response*/
                break;
            case MovesenseController.EventType.DISCONNECTED:    // a sensor disconnected
                LogNative.Log(isLogging, TAG + "RefreshScrollViewContentScan: EventType.DISCONNECTED");

                RefreshScrollViewContentScan();

                if (!MovesenseDevice.IsAnyConnectedOrConnecting())
                {
                    buttonSubscriptions.gameObject.SetActive(false);
                }
                break;
            case MovesenseController.EventType.RESPONSE:
                break;
            case MovesenseController.EventType.NOTIFICATION:
                break;
        }
    }

    void RefreshScrollViewContentScan()
    {
        LogNative.Log(isLogging, TAG + "RefreshScrollViewContentScan");

        int scannedDevices = MovesenseDevice.Devices.Count;
        int scanElementsCount = ScanElements.Count;

        if (scanElementsCount < scannedDevices)
        {
            LogNative.Log(isLogging, TAG + "RefreshScrollViewContentScan, add clones");

            for (int i = scanElementsCount; i < scannedDevices; i++)
            {
                GameObject ScanElementClone = Instantiate(ScanElement, scrollViewContentScan) as GameObject;
                // Positioning
                RectTransform ScanElementRect = ScanElementClone.GetComponent<RectTransform>();
                if (scanElementHeight == 0) scanElementHeight = ScanElementRect.sizeDelta.y;

                // change position
                ScanElementRect.anchoredPosition = new Vector2(0, -scanElementHeight / 2 - (i * scanElementHeight));

                ScanElements.Add(ScanElementClone);
                Debug.Log("ScanElement added");
            }
            scrollViewContentScan.sizeDelta = new Vector2(0, scanElementHeight * scannedDevices);
        }
        else if (scanElementsCount > scannedDevices)
        {
            LogNative.Log(isLogging, TAG + "RefreshScrollViewContentScan, destroy clones");

            for (int i = scanElementsCount - 1; i > scannedDevices - 1; i--)
            {
                Destroy(ScanElements[i]);
                ScanElements.RemoveAt(i);
            }
            scrollViewContentScan.sizeDelta = new Vector2(0, scanElementHeight * scannedDevices);
        }

        // LogNative.Log(TAG + "RefreshScrollViewContentScan, assign properties");
        for (int i = 0; i < scannedDevices; i++)
        {
            MovesenseDevice device = null;
            try
            {
                device = MovesenseDevice.Devices[i];
            }
            catch
            {
                LogNative.LogWarning(TAG + "RefreshScrollViewContentScan, Device is currently not available");
                continue;
            }

            // change texts
            string rssi;
            if (device.Rssi <= -500)
            {
                rssi = "~";
            }
            else
            {
                rssi = device.Rssi.ToString();
            }

            // define connectionColor
            Color32 fontColor;
            if (device.IsConnecting)
            {
                fontColor = Color.yellow;
            }
            else if (device.IsConnected)
            {
                fontColor = Color.green;
            }
            else
            {
                fontColor = Color.red;
            }

            TMP_Text[] scanElementTexts = ScanElements[i].GetComponentsInChildren<TMP_Text>();
            foreach (var text in scanElementTexts)
            {
                if (isColorizedScan) text.color = fontColor;
                if (text.name == "Text Serial")
                {
                    text.text = device.Serial;
                }
                else if (text.name == "Text MacID")
                {
                    text.text = device.MacID;
                }
                else if (text.name == "Text Rssi")
                {
                    text.text = rssi + "db";
                }
                else if (text.name == "Text Status")
                {
                    if (device.IsConnecting)
                    {
                        text.text = "connecting";
                    }
                    else
                    {
                        text.text = device.IsConnected ? "Connected" : "Disconnected";
                    }
                }
            }

            // change OnClickButtonConnect-methodparameters
            Button scanElementButton = ScanElements[i].GetComponentInChildren<Button>();

            scanElementButton.onClick.RemoveAllListeners();

            System.Func<string, string, bool, bool, UnityEngine.Events.UnityAction> actionBuilder = (macID, serial, connecting, connected) => () => OnClickButtonConnect(macID, serial, connecting, connected);
            UnityEngine.Events.UnityAction action1 = actionBuilder(device.MacID, device.Serial, device.IsConnecting, device.IsConnected);
            scanElementButton.onClick.AddListener(action1);
        }

        if (MovesenseDevice.NumberOfConnectedDevices() > 0)
        {
            buttonSubscriptions.gameObject.SetActive(true);
        }
    }
}