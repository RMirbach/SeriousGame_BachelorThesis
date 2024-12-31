using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
/*#if UNITY_IOS && !UNITY_EDITOR
	using AOT;
	using System.Runtime.InteropServices;
#endif*/


public class MovesenseController : MonoBehaviour {
    private const string TAG = "MovesenseController; ";
    public const bool isLogging = false;
    

    private const string URI_EVENTLISTENER = "suunto://MDS/EventListener";

    public enum EventType {
        CONNECTING,
        CONNECTED,
        DISCONNECTED,
        NOTIFICATION,
        RESPONSE,
        SUBSCRIPTIONSUCCESS
    }

    #region Plugin import
		#if UNITY_ANDROID && !UNITY_EDITOR
			private static AndroidJavaObject movesensePlugin;
		/*#elif UNITY_IOS && !UNITY_EDITOR
    		private delegate void CallbackConnect(string macID);
    		private delegate void CallbackConnectionComplete(string macID, string serial);
    		private delegate void CallbackConnectError(long statusCode, string error);
    		private delegate void CallbackDisconnect(string macID);
			[DllImport ("__Internal")]
			private static extern void InitMDS(CallbackConnect onConnect, CallbackConnectionComplete onConnectionComplete, CallbackConnectError onError, CallbackDisconnect onDisconnect);
			[DllImport ("__Internal")]
			private static extern void ConnectMDS(string macID);
			[DllImport ("__Internal")]
			private static extern bool DisConnectMDS(string macID);
			private static ConnectCallback callbackObject;

    		private delegate void CallbackNotification(string data, string serial, string subscriptionPath);
    		private delegate void CallbackNotificationResponse(long statusCode, string body, string serial, string subscriptionPath);
			[DllImport ("__Internal")]
			private static extern void SubscribeMDS(string serial, string subscriptionPath, string sampleRate, string jsonParameters, CallbackNotification onNotification, CallbackNotificationResponse onResponse);
			[DllImport ("__Internal")]
			private static extern void UnSubscribeMDS(string serial, string path);
			private static NotificationCallback notificationObject = new NotificationCallback();

			private delegate void CallbackResponse(string data, string method, string uri);
    		private delegate void CallbackResponseError(long statusCode, string uri, string error);
			[DllImport ("__Internal")]
			private static extern void GetMDS(string serial, string path, string jsonParameters, CallbackResponse onSuccess, CallbackResponseError onError);
			[DllImport ("__Internal")]
			private static extern void PutMDS(string serial, string path, string jsonParameters, CallbackResponse onSuccess, CallbackResponseError onError);
			[DllImport ("__Internal")]
			private static extern void PostMDS(string serial, string path, string jsonParameters, CallbackResponse onSuccess, CallbackResponseError onError);
			[DllImport ("__Internal")]
			private static extern void DeleteMDS(string serial, string path, string jsonParameters, CallbackResponse onSuccess, CallbackResponseError onError);
			private static ResponseCallback responseObject = new ResponseCallback();*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    #endregion

    #region Variables
		public static bool isInitialized;

		private List<System.EventArgs> notificationCallbackEventArgs = new List<System.EventArgs>();
		private ReaderWriterLockSlim notificationLock = new ReaderWriterLockSlim();

		private List<System.EventArgs> connectEventArgs = new List<System.EventArgs>();
		private ReaderWriterLockSlim connectLock = new ReaderWriterLockSlim();

		private List<System.EventArgs> disConnectEventArgs = new List<System.EventArgs>();
		private ReaderWriterLockSlim disConnectLock = new ReaderWriterLockSlim();

		private List<System.EventArgs> responseEventArgs = new List<System.EventArgs>();
		private ReaderWriterLockSlim responseLock = new ReaderWriterLockSlim();

		private List<System.EventArgs> subscriptionSuccessEventArgs = new List<System.EventArgs>();
		private ReaderWriterLockSlim subscriptionSuccessLock = new ReaderWriterLockSlim();
    #endregion

    /*#region iOS-Callback methods
		#if UNITY_IOS && !UNITY_EDITOR
		[MonoPInvokeCallback(typeof(CallbackConnect))]
		private static void onConnect(string macID) {
			callbackObject.onConnect(macID);
		}

		[MonoPInvokeCallback(typeof(CallbackConnectionComplete))]
		private static void onConnectionComplete(string macID, string serial) {
			callbackObject.onConnectionComplete(macID, serial);
		}

		[MonoPInvokeCallback(typeof(CallbackConnectError))]
		private static void onConnectError(long statusCode, string error) {
			callbackObject.onError(statusCode, error);
		}

		[MonoPInvokeCallback(typeof(CallbackDisconnect))]
		private static void onDisconnect(string macID) {
			callbackObject.onDisconnect(macID);
		}

		[MonoPInvokeCallback(typeof(CallbackNotification))]
		private static void onNotification(string data, string serial, string subscriptionPath) {
			notificationObject.onNotification(data, serial, subscriptionPath);
		}

		[MonoPInvokeCallback(typeof(CallbackNotificationResponse))]
		private static void onNotificationResponse(long statusCode, string body, string serial, string subscriptionPath) {
			notificationObject.onResponse(statusCode, body, serial, subscriptionPath);
		}

		[MonoPInvokeCallback(typeof(CallbackResponse))]
		private static void onResponse(string data, string method, string uri) {
			responseObject.onSuccess(data, method, uri);
		}

		[MonoPInvokeCallback(typeof(CallbackResponseError))]
		private static void onResponseError(long statusCode, string uri, string error) {
			responseObject.onError(statusCode, uri, error);
		}
		#endif
    #endregion*/

    #region Event
		[Serializable]
		public sealed class EventArgs : System.EventArgs {
			public EventType Type { get; private set; }
			public string InvokeMethod { get; private set; }
			public List<System.EventArgs> OriginalEventArgs { get; private set; }

			public EventArgs(EventType type, string invokeMethod, List<System.EventArgs> originalEventArgs) {
				Type = type;
				InvokeMethod = invokeMethod;
				OriginalEventArgs = originalEventArgs;
			}
		}
		//provide Events
		public static event EventHandler<EventArgs> Event;
    #endregion


    private void OnDestroy() {
        ConnectCallback.Event -= OnConnectCallbackEvent;
        NotificationCallback.Event -= OnNotificationCallbackEvent;
        NotificationCallback.EventResponse += OnNotificationResponseCallbackEvent;
        ResponseCallback.Event -= OnResponseCallbackEvent;
        notificationLock.Dispose();
        responseLock.Dispose();
    }

    void Awake() {
        LogNative.Log(isLogging, TAG + "Awake");

        if (FindObjectsOfType(GetType()).Length > 1) {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(transform.gameObject);
            ConnectCallback.Event += OnConnectCallbackEvent;
            NotificationCallback.Event += OnNotificationCallbackEvent;
            NotificationCallback.EventResponse += OnNotificationResponseCallbackEvent;
            ResponseCallback.Event += OnResponseCallbackEvent;
        }
    }

    void Start()  {
        LogNative.Log(isLogging, TAG + "Start: Initializing Movesense-Plugin");

        Initialize();
    }

    void Initialize() {
        if (!isInitialized) {
            LogNative.Log(isLogging, TAG + "Initialize");

			#if UNITY_ANDROID && !UNITY_EDITOR
				AndroidJavaClass jcUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        		AndroidJavaObject currentActivity = jcUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

				AndroidJavaClass jcMds = new AndroidJavaClass("com.movesense.mds.Mds"); // name of the class not the plugin-file

				var builder = jcMds.CallStatic<AndroidJavaObject>("builder");

				movesensePlugin = builder.Call<AndroidJavaObject>("build", currentActivity);
			/*#elif UNITY_IOS && !UNITY_EDITOR
				callbackObject = new ConnectCallback();

				InitMDS(onConnect, onConnectionComplete, onConnectError, onDisconnect);*/
			#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
			#endif

            isInitialized = true;

            LogNative.Log(isLogging, TAG + "Mds initialized");
        }
    }

    public static void Connect(string MacID) {
        // mds checks, if already connecting or connected
        Debug.Log("Connect is happening");//Try 
      

        if (!isInitialized) {
            LogNative.LogError(TAG + "Connect: MovesenseController is not initialized. Did you forget to add MovesenseController object in the scene?");

            return;
        }

        string serial = MovesenseDevice.GetSerial(MacID);

        if (serial != null) {
            LogNative.Log(isLogging, TAG + "Connect: " + MacID + " (" + serial + ")");

            MovesenseDevice.SetConnecting(MacID);

            LogNative.Log(isLogging, TAG + "Connect, raising Connecting-event");

            if (Event != null) {
                Event(null, new EventArgs(EventType.CONNECTING, TAG + "Connect", new List<System.EventArgs> { new ConnectCallback.EventArgs(false, MacID, serial) }));
            }
        }

		#if UNITY_ANDROID && !UNITY_EDITOR
			movesensePlugin.Call("connect", MacID, new ConnectCallback());
		/*#elif UNITY_IOS && !UNITY_EDITOR
			ConnectMDS(MacID);*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    }

    public static void Disconnect(string MacID) {
        // mds checks, if device is connected

        string serial = MovesenseDevice.GetSerial(MacID);

        LogNative.Log(TAG + "Disconnect: " + MacID + " (" + serial + ")");

		#if UNITY_ANDROID && !UNITY_EDITOR
			movesensePlugin.Call("disconnect", MacID);
		/*#elif UNITY_IOS && !UNITY_EDITOR
			DisConnectMDS(MacID);

			if (MovesenseDevice.GetConnectingState(MacID)) {
				// connection has not been completed => there will be no callback if disconnect is called
				MovesenseDevice.SetConnectionState(MacID, false);

				LogNative.Log(isLogging, TAG + "Disconnect, while connecting, raising Disconnect-event");

				if (Event != null) {
					Event(null, new EventArgs(EventType.DISCONNECTED, TAG + "Disconnect",new List<System.EventArgs> { new ConnectCallback.EventArgs(false, MacID, serial) }));
				}
			}*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    }

    public static void Subscribe(string Serial, string Subscriptionpath, int? Samplerate) {
        LogNative.Log(TAG + "Subscribe: " + Serial + ", Subscriptionpath: " + Subscriptionpath + ", Samplerate: " + Samplerate);

        // check correct format
        if ((Subscriptionpath == SubscriptionPath.LinearAcceleration || Subscriptionpath == SubscriptionPath.AngularVelocity || Subscriptionpath == SubscriptionPath.MagneticField) && Samplerate == null) {
            LogNative.LogError(TAG + "Subscribe, Samplerate missing");

		    return;
        } else if ((Subscriptionpath == SubscriptionPath.HeartRate || Subscriptionpath == SubscriptionPath.Temperature) && Samplerate != null) {
            LogNative.LogWarning(TAG + "Subscribe, ignoring Samplerate");

		    Samplerate = null;

		    return;
        }

		#if UNITY_ANDROID && !UNITY_EDITOR
			MovesenseDevice.AddSubscription(Serial, Subscriptionpath,
											new MovesenseDevice.SubscriptionSection(Samplerate,
												movesensePlugin.Call<AndroidJavaObject>("subscribe", URI_EVENTLISTENER, BuildContract(Serial, Subscriptionpath+Samplerate.ToString()),
												new NotificationCallback(Serial, Subscriptionpath))));
		/*#elif UNITY_IOS && !UNITY_EDITOR
			SubscribeMDS(Serial, Subscriptionpath, Samplerate.ToString(), "{}", onNotification, onNotificationResponse);

			MovesenseDevice.AddSubscription(Serial, Subscriptionpath, new MovesenseDevice.SubscriptionSection(Samplerate));*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    }

    private static string BuildContract(string name, string uri) {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        string returnString = sb.Append("{\"Uri\": \"").Append(name).Append("/").Append(uri).Append("\"}").ToString();

        return returnString;
    }

    public static void UnSubscribe(string Serial, string SubscriptionPath) {
        LogNative.Log(TAG + "UnSubscribe: " + Serial + ", SubscriptionPath: " + SubscriptionPath); // keep true

		#if UNITY_ANDROID && !UNITY_EDITOR
			MovesenseDevice.GetSubscription(Serial, SubscriptionPath).Call("unsubscribe");

			MovesenseDevice.RemoveSubscription(Serial, SubscriptionPath);
		/*#elif UNITY_IOS && !UNITY_EDITOR
			LogNative.Log(isLogging, TAG + "GetSubscription: " + MovesenseDevice.GetSubscription(Serial, SubscriptionPath));

			if (MovesenseDevice.GetSubscription(Serial, SubscriptionPath) == null) {
				LogNative.LogWarning(TAG + "UnSubscribe, GetSubscription returns null");
			} else {
				UnSubscribeMDS(Serial, MovesenseDevice.GetSubscription(Serial, SubscriptionPath));

				MovesenseDevice.RemoveSubscription(Serial, SubscriptionPath);
			}*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    }

    public static void ResponseGet(string Serial, string Path) {
        LogNative.Log(TAG + "ResponseGet(Serial: " + Serial + ", Path: " + Path + ")"); // keep true

		#if UNITY_ANDROID && !UNITY_EDITOR
			string uri = "suunto://"+Serial+"/"+Path;

			movesensePlugin.Call("get", uri, null, new ResponseCallback("GET", uri));
		/*#elif UNITY_IOS && !UNITY_EDITOR
			GetMDS(Serial, Path, "{}", onResponse, onResponseError);*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    }

    /// <summary>
    /// Look at https://bitbucket.org/suunto/movesense-device-lib/src/3956a579c473/MovesenseCoreLib/resources/movesense-api/?at=master
    /// </summary>
    /// <param name="Serial"></param>
    /// <param name="Path"></param>
    /// <param name="JsonParameters">JSON formatted string</param>
    public static void ResponsePut(string Serial, string Path, string JsonParameters) {
        if (string.IsNullOrEmpty(JsonParameters)) {
            JsonParameters = "{}";
        }

        LogNative.Log(TAG + "ResponsePut(Serial: " + Serial + ", Path: " + Path + ", JsonParameters: " + JsonParameters + ")"); // keep true

		#if UNITY_ANDROID && !UNITY_EDITOR
			string uri = "suunto://"+Serial+"/"+Path;

			movesensePlugin.Call("put", uri, JsonParameters, new ResponseCallback("PUT", uri));
		/*#elif UNITY_IOS && !UNITY_EDITOR
			PutMDS(Serial, Path, JsonParameters, onResponse, onResponseError);*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    }

    public static void ResponsePost(string Serial, string Path) {
        LogNative.Log(TAG + "ResponsePost(Serial: " + Serial + ", Path: " + Path + ")"); // keep true

		#if UNITY_ANDROID && !UNITY_EDITOR
			string uri = "suunto://"+Serial+"/"+Path;

			movesensePlugin.Call("post", uri, null, new ResponseCallback("POST", uri));
		/*#elif UNITY_IOS && !UNITY_EDITOR
			PostMDS(Serial, Path, "{}", onResponse, onResponseError);*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    }

    public static void ResponseDelete(string Serial, string Path) {
        LogNative.Log(TAG + "ResponseDelete(Serial: " + Serial + ", Path: " + Path + ")"); // keep true

		#if UNITY_ANDROID && !UNITY_EDITOR
			string uri = "suunto://"+Serial+"/"+Path;

			movesensePlugin.Call("delete", uri, null, new ResponseCallback("DEL", uri));
		/*#elif UNITY_IOS && !UNITY_EDITOR
			DeleteMDS(Serial, Path, "{}", onResponse, onResponseError);*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
    }

    void OnNotificationCallbackEvent(object sender, NotificationCallback.EventArgs e) {
        LogNative.Log(isLogging, TAG + "OnNotificationCallbackEvent: " + e.Data);

        notificationLock.EnterWriteLock();

        try {
            notificationCallbackEventArgs.Add(e);
        } finally {
            notificationLock.ExitWriteLock();
        }
    }

    void OnConnectCallbackEvent(object sender, ConnectCallback.EventArgs e) {
        LogNative.Log(isLogging, TAG + "OnConnectCallbackEvent, IsConnect: " + e.IsConnect + ", MacID: " + e.MacID + ", Serial: " + e.Serial);

        if (e.IsConnect) {
            connectLock.EnterWriteLock();

            try {
                connectEventArgs.Add(e);
            } finally {
                connectLock.ExitWriteLock();
            }
        } else {
            disConnectLock.EnterWriteLock();

            try {
                disConnectEventArgs.Add(e);
            } finally {
                disConnectLock.ExitWriteLock();
            }
        }
    }

    void OnResponseCallbackEvent(object sender, ResponseCallback.EventArgs e) {
        LogNative.Log(isLogging, TAG + "OnResponseCallbackEvent, Uri: " + e.Uri + ", Method: " + e.Method + ", Data: " + e.Data);

        responseLock.EnterWriteLock();

        try {
            responseEventArgs.Add(e);
        } finally {
            responseLock.ExitWriteLock();
        }
    }

    void OnNotificationResponseCallbackEvent(object sender, NotificationCallback.EventArgs e) {
        LogNative.Log(isLogging, TAG + "OnNotificationResponseCallbackEvent: " + e.Data);

        subscriptionSuccessLock.EnterWriteLock();

        try {
            subscriptionSuccessEventArgs.Add(e);
        } finally {
            subscriptionSuccessLock.ExitWriteLock();
        }
    }

    private void Update() {
        //if (Event != null) { // Feature in case you forgot to subscribe to the event, no data will be lost

        if (connectEventArgs.Count > 0) {
            connectLock.EnterUpgradeableReadLock();

            try {
                if (Event != null) {
                    LogNative.Log(isLogging, TAG + "Update, raising CONNECT-event");

                    Event(null, new EventArgs(EventType.CONNECTED, TAG + "OnConnectCallbackEvent", connectEventArgs));
                }

                connectLock.EnterWriteLock();

                try {
                    connectEventArgs.Clear();
                } finally {
                    connectLock.ExitWriteLock();
                }
            } finally {
                connectLock.ExitUpgradeableReadLock();
            }
        }

        if (disConnectEventArgs.Count > 0) {
            disConnectLock.EnterUpgradeableReadLock();

            try {
                if (Event != null) {
                    LogNative.Log(isLogging, TAG + "Update, raising DISCONNECT-event");

                    Event(null, new EventArgs(EventType.DISCONNECTED, TAG + "OnConnectCallbackEvent", disConnectEventArgs));
                }

                disConnectLock.EnterWriteLock();

                try {
                    disConnectEventArgs.Clear();
                } finally {
                    disConnectLock.ExitWriteLock();
                }
            } finally {
                disConnectLock.ExitUpgradeableReadLock();
            }
        }

        if (notificationCallbackEventArgs.Count > 0) {
            notificationLock.EnterUpgradeableReadLock();

            try {
                if (Event != null) {
                    LogNative.Log(isLogging, TAG + "Update, raising NOTIFICATION-event");

                    Event(null, new EventArgs(EventType.NOTIFICATION, TAG + "OnNotificationCallbackEvent", notificationCallbackEventArgs));
                }

                notificationLock.EnterWriteLock();

                try {
                    notificationCallbackEventArgs.Clear();
                } finally {
                    notificationLock.ExitWriteLock();
                }
            } finally {
                notificationLock.ExitUpgradeableReadLock();
            }
        }

        if (responseEventArgs.Count > 0) {
            responseLock.EnterUpgradeableReadLock();

            try {
                if (Event != null) {
                    LogNative.Log(isLogging, TAG + "Update, raising RESPONSE-event");

                    Event(null, new EventArgs(EventType.RESPONSE, TAG + "OnResponseCallbackEvent", responseEventArgs));
                }

                responseLock.EnterWriteLock();

                try {
                    responseEventArgs.Clear();
                } finally {
                    responseLock.ExitWriteLock();
                }
            } finally {
                responseLock.ExitUpgradeableReadLock();
            }
        }

        if (subscriptionSuccessEventArgs.Count > 0) {
            subscriptionSuccessLock.EnterUpgradeableReadLock();

            try {
                if (Event != null) {
                    LogNative.Log(isLogging, TAG + "Update, raising SUBSCRIPTIONSUCCESS-event");

                    Event(null, new EventArgs(EventType.SUBSCRIPTIONSUCCESS, TAG + "OnNotificationResponseCallbackEvent", subscriptionSuccessEventArgs));
                }

                subscriptionSuccessLock.EnterWriteLock();

                try {
                    subscriptionSuccessEventArgs.Clear();
                } finally {
                    subscriptionSuccessLock.ExitWriteLock();
                }
            } finally {
                subscriptionSuccessLock.ExitUpgradeableReadLock();
            }
        }
        //}
    }
}


// take a look at: https://bitbucket.org/suunto/movesense-device-lib/src/master/MovesenseCoreLib/resources/movesense-api/
public class SubscriptionPath {
    public const string LinearAcceleration = "Meas/Acc/";
    public const string AngularVelocity = "Meas/Gyro/";
    public const string MagneticField = "Meas/Magn/";
    public const string HeartRate = "Meas/HR";
    public const string Temperature = "Meas/Temp";
    public const string GearID = "Misc/Gear/Id";
    public const string Battery = "System/Energy/Level";
    public const string Info = "Info";
    public const string ECG = "Meas/ECG/";
    public const string OneWire = "Comm/1Wire";
    public const string Peers = "Comm/1Wire/Peers";
}

/// <summary>The samplerates here are the ones supported by current Movesense sensor. You can query the current supported sample rates and other info from the sensor path /Meas/[sensor]/Info </summary>
public class SampleRate {
    /// <summary>Updatefrequnzy: 13Hz</summary>
    public const int slowest = 13;

    /// <summary>Updatefrequnzy: 26Hz</summary>
    public const int slower = 26;

    /// <summary>Updatefrequnzy: 52Hz</summary>
    public const int medium = 52;

    /// <summary>Updatefrequnzy: 104Hz</summary>
    public const int fast = 104;

    /// <summary>Updatefrequnzy: 208Hz</summary>
    public const int faster = 208;

    /// <summary>Updatefrequnzy: 416Hz</summary>
    public const int fastest = 416;
}