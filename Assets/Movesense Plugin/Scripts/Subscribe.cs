using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Subscribe : MonoBehaviour
{

    private const string TAG = "SubscriptionSceneScript; ";
    private const bool isLogging = false;

    [SerializeField]
    private Button buttonVisualize;

    [SerializeField]
    private RectTransform contentConnected;

    [SerializeField]
    private Button[] buttonsSubscription;

   

    [SerializeField]
    private GameObject ConnectedElement;

    private List<GameObject> connectedElements = new List<GameObject>();

    private int connectedElementHighlitedIndex = 0; // selected Sensor to display

    private float connectedElementHeight = 0;

    [SerializeField]
    private Sprite buttonOn;

    [SerializeField]
    private Sprite buttonOff;

    private Color colorDefault = new Color32(0x0A, 0x0A, 0x0A, 0xFF);//0A0A0AFF

    private Color colorHighlited = new Color32(0x4C, 0x4C, 0x4C, 0xFF);//4C4C4CFF

    // provides button from being pressed multiple times
    private bool isButtonVisualizePressed = false;


    private void Start()
    {
        LogNative.Log(isLogging, TAG + "Start");

        isButtonVisualizePressed = false;

        // refresh sensorlist
        RefreshScrollViewContentConnectedDevices(true);

        // refresh subscriptionlist
        RefreshPanelSubscription(0, null);

        // ButtonVisualize gets active, if any Subscription is active
        if (MovesenseDevice.isAnySubscribed(SubscriptionPath.AngularVelocity, SubscriptionPath.LinearAcceleration))
        {
            buttonVisualize.gameObject.SetActive(true);
        }
        else
        {
            buttonVisualize.gameObject.SetActive(false);
        }

        // attach event
        MovesenseController.Event += OnMovesenseControllerCallbackEvent;
    }

    public void OnClickButtonBack()
    {
        LogNative.Log(isLogging, TAG + "OnClickButtonBack");

        // detach event
        MovesenseController.Event -= OnMovesenseControllerCallbackEvent;

        //ChangeSceneController.GoSceneBack();
    }

    public void OnClickButtonVisualize()
    {
        if (isButtonVisualizePressed)
        {
            isButtonVisualizePressed = true;
            return;
        }
        LogNative.Log(isLogging, TAG + "OnClickButtonVisualize");

        // detach event
        MovesenseController.Event -= OnMovesenseControllerCallbackEvent;

    }

    void OnMovesenseControllerCallbackEvent(object sender, MovesenseController.EventArgs e)
    {
        LogNative.Log(isLogging, TAG + "OnMovesenseControllerCallbackEvent, e.Type: " + e.Type);

        switch (e.Type)
        {
            case MovesenseController.EventType.NOTIFICATION:    // got data from a sensor
                LogNative.Log(isLogging, TAG + "OnMovesenseControllerCallbackEvent, MovesenseController.EventType.NOTIFICATION");

                for (int i = 0; i < e.OriginalEventArgs.Count; i++)
                {
                    var ne = (NotificationCallback.EventArgs)e.OriginalEventArgs[i];

                    LogNative.Log(isLogging, TAG + "OnMovesenseControllerCallbackEvent: EventType.NOTIFICATION" +
                        "\nStatusCode      : " + ne.StatusCode +
                        "\nSerial          : " + ne.Serial +
                        "\nSubscriptionPath: " + ne.SubscriptionPath +
                        "\nData            : " + ne.Data);

                    RefreshPanelSubscription(MovesenseDevice.ContainsSerialAt(ne.Serial), ne);
                }
                break;
            case MovesenseController.EventType.CONNECTED:       // a sensor succesfully connected (in the background)
                RefreshScrollViewContentConnectedDevices(false);
                break;
            case MovesenseController.EventType.DISCONNECTED:    // a sensor disconnected
                RefreshScrollViewContentConnectedDevices(true);

                RefreshPanelSubscription(0, null);
                break;
        }
    }

    void RefreshScrollViewContentConnectedDevices(bool isInit)
    {
        LogNative.Log(isLogging, TAG + "RefreshScrollViewContentConnectedDevices");

        int connectedDevices = MovesenseDevice.NumberOfConnectedDevices();
        int connectedElementsCount = connectedElements.Count;

        if (connectedElementsCount < connectedDevices)
        {
            LogNative.Log(isLogging, TAG + "RefreshScrollViewContentConnectedDevices, add clones");

            for (int i = connectedElementsCount; i < connectedDevices; i++)
            {
                GameObject connectedElementClone = Instantiate(ConnectedElement, contentConnected) as GameObject;

                // Positioning
                RectTransform connectedElementRect = connectedElementClone.GetComponent<RectTransform>();
                if (connectedElementHeight == 0) connectedElementHeight = connectedElementRect.sizeDelta.y;

                // change position
                connectedElementRect.anchoredPosition = new Vector2(0, -connectedElementHeight / 2 - (i * connectedElementHeight));

                connectedElements.Add(connectedElementClone);
            }
            contentConnected.sizeDelta = new Vector2(0, connectedElementHeight * connectedDevices);
            if (isInit)
            {
                foreach (var item in connectedElements[0].GetComponentInChildren<Button>().GetComponentsInChildren<Image>())
                {
                    if (item.name == "Image Background")
                    {
                        item.color = colorHighlited;
                        break;
                    }
                }
                connectedElementHighlitedIndex = 0;
            }
        }
        else if (connectedElementsCount > connectedDevices)
        {
            LogNative.Log(isLogging, TAG + "RefreshScrollViewContentConnectedDevices, destroy clones");

            for (int i = connectedElementsCount - 1; i > connectedDevices - 1; i--)
            {
                Destroy(connectedElements[i]);

                connectedElements.RemoveAt(i);
            }
            contentConnected.sizeDelta = new Vector2(0, connectedElementHeight * connectedDevices);
        }

        for (int i = 0; i < connectedDevices; i++)
        {
            // change texts
            TMP_Text[] connectedElementTexts = connectedElements[i].GetComponentsInChildren<TMP_Text>();
            foreach (var text in connectedElementTexts)
            {
                if (text.name == "Text Serial")
                {
                    text.text = MovesenseDevice.Devices[i].Serial;
                }
                else if (text.name == "Text MacID")
                {
                    text.text = MovesenseDevice.Devices[i].MacID;
                }
            }

            // Highlight
            Button btn = connectedElements[i].GetComponentInChildren<Button>();

            btn.onClick.RemoveAllListeners();

            System.Func<int, UnityEngine.Events.UnityAction> actionBuilder = (connectedElementIndex) => () => OnClickButtonConnectElement(connectedElementIndex);
            UnityEngine.Events.UnityAction action1 = actionBuilder(i);
            btn.onClick.AddListener(action1);
        }
    }

    private void OnClickButtonConnectElement(int connectedElementIndex)
    {
        // connectedElementIndex: Index of which Index in List was clicked
        LogNative.Log(isLogging, TAG + "OnClickButtonConnectElement, connectedElementIndex: " + connectedElementIndex);

        // de-highlight all buttons if another button is pressed
        if (connectedElementIndex == connectedElementHighlitedIndex)
        {
            return;
        }

        // reset BackgroundColors:
        foreach (var connectedElement in connectedElements)
        {
            foreach (var item in connectedElement.GetComponentInChildren<Button>().GetComponentsInChildren<Image>())
            {
                if (item.name == "Image Background")
                {
                    item.color = colorDefault;
                    break;
                }
            }
        }
        // set BackgroundColor
        foreach (var item in connectedElements[connectedElementIndex].GetComponentInChildren<Button>().GetComponentsInChildren<Image>())
        {
            if (item.name == "Image Background")
            {
                item.color = colorHighlited;
                break;
            }
        }
        connectedElementHighlitedIndex = connectedElementIndex;

        RefreshPanelSubscription(connectedElementIndex, null);
    }

    public void OnClickButtonSubscribe(int index)
    {
        LogNative.Log(isLogging, TAG + "OnClickButtonSubscribe, Button: " + buttonsSubscription[index].name);

        // Toggle state
        bool isOn = (buttonsSubscription[index].image.sprite.name.Split('_')[1] == "On") ? true : false;
        buttonsSubscription[index].image.sprite = isOn ? buttonOff : buttonOn;
        isOn = !isOn;

        string serial = MovesenseDevice.Devices[connectedElementHighlitedIndex].Serial;

        string subscriptionPath = null;
        int? sampleRate = null;
        string subscriptionChar = null; // only for logging

        switch (index)
        {
            case 0:
                subscriptionPath = SubscriptionPath.LinearAcceleration;
                sampleRate = SampleRate.slowest;
                subscriptionChar = "LinearAcceleration";
                break;
        }

        LogNative.Log(isLogging, TAG + "onClickButtonSubscribe, " + (isOn ? "" : "un") + "subscribe " + subscriptionChar + " for " + serial);

        if (isOn)
        {
            MovesenseController.Subscribe(serial, subscriptionPath, sampleRate);
        }
        else
        {
            MovesenseController.UnSubscribe(serial, subscriptionPath);
            // clear values
            Invoke("RefreshPanelSubscriptionDelayed", 0.2F);
        }

        // ButtonVisualize gets active, if any Subscription is active
        if (MovesenseDevice.isAnySubscribed(SubscriptionPath.AngularVelocity, SubscriptionPath.LinearAcceleration))
        {
            buttonVisualize.gameObject.SetActive(true);
        }
        else
        {
            buttonVisualize.gameObject.SetActive(false);
        }
    }

    void RefreshPanelSubscriptionDelayed()
    {
        RefreshPanelSubscription(connectedElementHighlitedIndex, null);
    }

    void RefreshPanelSubscription(int connectedElementIndex, NotificationCallback.EventArgs e)
    {
        LogNative.Log(isLogging, TAG + "RefreshPanelSubscription, connectedElementIndex: " + connectedElementIndex + ", e.Data: " + (e == null ? "e == null!" : e.Data));

        if (e == null)
        { // @Start or on DisconnectEvent or if another Sensor is selected
            LogNative.Log(isLogging, TAG + "RefreshPanelSubscription, refreshing Sensorlist");

            // check subscriptionTypes per serial in MovesenseDevice
            if (MovesenseDevice.Devices.Count == 0)
            {
                LogNative.LogError(TAG + "RefreshPanelSubscription, MovesenseDevice.Devices.Count == 0");
                return;
            }

            Dictionary<string, int?> subscriptionTypes = new Dictionary<string, int?>();
            if (MovesenseDevice.GetAllSubscriptionPaths(MovesenseDevice.Devices[connectedElementIndex].Serial) != null)
            {
                subscriptionTypes = new Dictionary<string, int?>(MovesenseDevice.GetAllSubscriptionPaths(MovesenseDevice.Devices[connectedElementIndex].Serial));
            }

           
        }
        else
        {
            LogNative.Log(isLogging, TAG + "RefreshPanelSubscription, refreshing Subscriptionlist");

            // only highlighted Sensordata will be updated
            if (connectedElementIndex != connectedElementHighlitedIndex)
            {
                LogNative.Log(isLogging, TAG + "Values for " + MovesenseDevice.Devices[connectedElementIndex].Serial + " do not match displayed " + MovesenseDevice.Devices[connectedElementHighlitedIndex].Serial);

                return;
            }

           
        }
    }
}