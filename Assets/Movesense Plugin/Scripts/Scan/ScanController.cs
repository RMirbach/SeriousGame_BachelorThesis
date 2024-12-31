using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
#if PLATFORM_ANDROID && UNITY_2018_3_OR_NEWER
using UnityEngine.Android;
#endif
using System.Collections;


public enum BleHardwareState {
	UNKNOWN = 0,
	RESETTING = 1,
	UNSUPPORTED = 2,
	UNAUTHORIZED = 3,
	POWERED_OFF = 4,
	POWERED_ON = 5,
	LOCATION_OFF = 6,
	LOCATION_ON = 7,
	SCAN_READY = 8
}


public class ScanController : MonoBehaviour {
	private const string TAG = "ScanController; ";

	private const bool isLogging = true;

	public enum EventType {
		SYSTEM_NOT_SCANNING,
		SYSTEM_SCANNING,
        NEW_DEVICE,
        RSSI,
        REFRESH, // every ScanController.deviceRefreshTime seconds the list is updated. Devices, which are no more available are removed from the Devicelist
        REMOVE_UNCONNECTED,
    }

	#region Plugin import
		#if UNITY_ANDROID && !UNITY
			private static AndroidJavaObject scanPlugin;
		/*#elif UNITY_IOS && !UNITY_EDITOR	
			[DllImport ("__Internal")]
			private static extern void InitScanPluginiOS(bool shouldLog);

			[DllImport ("__Internal")]
			private static extern void Dispose();
			
			[DllImport ("__Internal")]
			private static extern int GetBLEStatus(); // see iOS-Ble-Hardware-States

			[DllImport ("__Internal")]
			private static extern void EnableBluetooth();

			[DllImport ("__Internal")]
			private static extern void Scan_iOS(string[] devices);
			
			[DllImport ("__Internal")]
			private static extern void Stop_iOS();*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
	#endregion

	#region Variables
		private static ScanController instance = null;

		private const string uuidStringBelow2 = "61353090-8231-49cc-b57a-886370740041"; // for Movesensesensors with firmware below 2.x.x
		private const string uuidStringFrom2 = "0000FDF3-0000-1000-8000-00805F9B34FB"; // for Movesensesensors with firmware from 2.x.x
		private readonly static string[] uuidStrings = { uuidStringBelow2, uuidStringFrom2 };

		/// <summary>
		/// false: everytime rssi changes, event with EventType.RSSI is raised. Set MovesenseDevice.ShouldSortByRssi = false, otherwise the List changes rapidly
		/// <para></para>
		/// true: event with EventType.RSSI is raised every rssiBlockTime and Devices which get powered off will be removed from List. Most effective in combination with MovesenseDevice.ShouldSortByRssi = true
		/// </summary>
		private static bool isRefreshingScanList = false;

		private const float rssiBlockTime = 1.0F;
		
		private const float deviceRefreshTime = 2.0F;

		private bool isInitialized = false;
		public static bool IsInitialized {
			get {
				if (instance == null) {
					return false;
				}
				return instance.isInitialized;
			}
			private set {
				instance.isInitialized = value;
			}
		}

		private BleHardwareState bleState;
		public static BleHardwareState BleState {
			get {
				return instance.bleState;
			}
			private set {
				instance.bleState = value;
			}
		}

		
		private bool isScanning = false;
		public static bool IsScanning {
			get {
				if (instance == null) {
					return false;
				}
				return instance.isScanning;
			}
			private set {
				instance.isScanning = value;
			}
		}

		private List<String> refresherList = new List<string>();
		public static List<String> RefresherList {
			get {
				return instance.refresherList;
			}
			private set {
				instance.refresherList = value;
			}
		}
		
		
		private bool isIgnoringScanReport = false;
		public static bool IsIgnoringScanReport {
			get {
				return instance.isIgnoringScanReport;
			}
			private set {
				instance.isIgnoringScanReport = value;
			}
		}

		private bool isRefreshing = false;
		public static bool IsRefreshing {
			get {
				return instance.isRefreshing;
			}
			private set {
				instance.isRefreshing = value;
			}
		}

		private bool isRefreshingRssiBlocked = false;
		public static bool IsRefreshingRssiBlocked {
			get {
				return instance.isRefreshingRssiBlocked;
			}
			private set {
				instance.isRefreshingRssiBlocked = value;
			}
		}
		
		private bool isStartRefresh = false;
		public static bool IsStartRefresh {
			get {
				return instance.isStartRefresh;
			}
			private set {
				instance.isStartRefresh = value;
			}
		}
	#endregion

	#region Event
		[Serializable]
		public sealed class EventArgs : System.EventArgs {
			public EventType Type { get; private set; }
			public string InvokeMethod { get; private set; }
			public string MacID { get; private set; }
			public EventArgs (EventType type, string invokeMethod, string macID) {
				Type = type;
				InvokeMethod = invokeMethod;
				MacID = macID;
			}
		}
		//provide Events
		public static event	EventHandler<EventArgs> Event;

	#endregion


	private void OnDestroy() {
		LogNative.Log(isLogging, TAG + "OnDestroy");

		if (instance != this) return;

		LogNative.Log(isLogging, TAG + "OnDestroy, Dispose()");

		// Garbage native plugin
		#if UNITY_ANDROID && !UNITY_EDITOR
			if (scanPlugin != null) scanPlugin.Dispose();
		/*#elif UNITY_IOS && !UNITY_EDITOR
			Dispose();*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
	}

	void Awake() {
		if (FindObjectsOfType(GetType()).Length > 1) {
			LogNative.Log(isLogging, TAG + "Already awake");
            
			Destroy(gameObject);
        } else {
			LogNative.Log(isLogging, TAG + "Awake");

			DontDestroyOnLoad(transform.gameObject);
			
			instance = this;
		}
	}

	void Start() {
		LogNative.Log(isLogging, TAG + "Start: Initializing Scan-Plugin");

		Initialize(isLogging);
	}

	void OnApplicationFocus(bool focusStatus)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
			if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
			{
				LogNative.Log($"{TAG}OnApplicationFocus: Permission.FineLocation granted");
				fineLocationPermissionGranted = true;
			}
			else
			{
				LogNative.LogError($"{TAG}OnApplicationFocus: Permission.FineLocation denied");
				fineLocationPermissionGranted = false;
			}

			if (!fineLocationPermissionAsked && focusStatus)
				fineLocationPermissionAsked = true;
		#endif
	}

	void Initialize(bool shouldSanPluginLog) {
		if (!isInitialized) {
			LogNative.Log(TAG + "Initialize");

			#if UNITY_ANDROID && !UNITY_EDITOR
				using (AndroidJavaClass jc = new AndroidJavaClass("com.kaasa.blescan.Ble_Scan_Android")) { // name of the class not the plugin-file
					scanPlugin = jc.CallStatic<AndroidJavaObject>("instance");
					scanPlugin.Call("InitScanPluginAndroid", shouldSanPluginLog);	
				}

				IsInitialized = true;
			/*#elif UNITY_IOS && !UNITY_EDITOR
				InitScanPluginiOS(shouldSanPluginLog);*/	
			#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
				IsInitialized = true;
			#endif
		}
	}

	#if UNITY_STANDALONE_OSX || UNITY_EDITOR
	public static void ReportActualBleState(string s_actualState) {
	#else
	public void ReportActualBleState(string s_actualState) {
	#endif
		LogNative.Log(isLogging, TAG + "ReportActualBleState: state: " + s_actualState);

		/*#if UNITY_IOS
			IsInitialized = true;
		#endif*/

		BleState = (BleHardwareState)int.Parse(s_actualState);

		string logErrorString;

		switch (BleState) {
			case BleHardwareState.POWERED_OFF:
				logErrorString = "Bluetooth state: POWERED_OFF";

				/*#if UNITY_IOS && !UNITY_EDITOR
					logErrorString += "\nmaybe check Bluetooth in control center";
				#endif*/

				LogNative.LogError(TAG + logErrorString);
				
				if (Event != null) {
					Event(null, new EventArgs(EventType.SYSTEM_NOT_SCANNING, TAG + "ReportActualBleState", logErrorString));
				}
				break;
			case BleHardwareState.POWERED_ON:
				#if UNITY_ANDROID && !UNITY_EDITOR
					LogNative.Log(isLogging, TAG + "Bluetooth-Hardware is turned on, checking Location");

					if (!scanPlugin.Call<bool>("IsLocationTurnedOn")) {
						CheckBleStatus();
					} else {
						// Scan(); // Avoid scanstart at init
					}
				/*#elif UNITY_IOS && !UNITY_EDITOR
					LogNative.Log(isLogging, TAG + "Bluetooth-Hardware is turned on");*/

					// Scan(); // Avoid scanstart at init
				#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
				#endif
				break;
			case BleHardwareState.LOCATION_OFF:
				#if UNITY_ANDROID && !UNITY_EDITOR
					logErrorString = "Bluetooth state: LOCATION_OFF";
					
					LogNative.LogError(TAG + logErrorString);

					scanPlugin.Call("StopScan");

					if (Event != null) {
						Event(null, new EventArgs(EventType.SYSTEM_NOT_SCANNING, TAG + "ReportActualBleState", logErrorString));
					}
				#endif
				break;
			case BleHardwareState.LOCATION_ON:
				#if UNITY_ANDROID && !UNITY_EDITOR
					LogNative.Log(isLogging, TAG + "Location is on");

					if (!scanPlugin.Call<bool>("IsBluetoothTurnedOn")) {
						CheckBleStatus();
					} else {
						// Scan(); // Avoid scanstart at init
					}
				#endif
				break;
			case BleHardwareState.UNKNOWN:
			case BleHardwareState.RESETTING:
			case BleHardwareState.UNSUPPORTED:
			case BleHardwareState.UNAUTHORIZED:
				logErrorString = "Bluetooth state: " + (BleHardwareState)BleState;

				LogNative.LogError(TAG + logErrorString);
				
				if (Event != null) {
					Event(null, new EventArgs(EventType.SYSTEM_NOT_SCANNING, TAG + "ReportActualBleState", logErrorString));
				}
				break;
		}
	}

	private static void CheckBleStatus() {
		LogNative.Log(isLogging, TAG + "checking Ble status");

		string logErrorString = null;

		#if UNITY_ANDROID && !UNITY_EDITOR
			if (!scanPlugin.Call<bool>("IsBleFeatured")) {
				logErrorString = "Bluetooth is not featured";
				return;
			} 
			if (!scanPlugin.Call<bool>("IsBluetoothAvailable")) {
				logErrorString = "Bluetooth is not available";
				return;
			} 
			if (!scanPlugin.Call<bool>("IsBluetoothTurnedOn")) {
				logErrorString = "Bluetooth is powered_off, try to turn on";

				scanPlugin.Call("EnableBluetooth");
				return;
			}
			if (!scanPlugin.Call<bool>("IsLocationTurnedOn")) {
				logErrorString = "Location is off, try to turn on";

				scanPlugin.Call("EnableLocation");
			}
		/*#elif UNITY_IOS && !UNITY_EDITOR
			BleHardwareState state = (BleHardwareState)GetBLEStatus();
			switch (state) {
				case BleHardwareState.UNKNOWN:
					logErrorString = "Bluetooth state: UNKNOWN";
					break;
				case BleHardwareState.RESETTING:
					logErrorString = "Bluetooth state: RESETTING";
					break;
				case BleHardwareState.UNSUPPORTED:
					logErrorString = "Bluetooth state: UNSUPPORTED";
					break;
				case BleHardwareState.UNAUTHORIZED:
					logErrorString = "Bluetooth state: UNAUTHORIZED";
					break;
				case BleHardwareState.POWERED_OFF:
					logErrorString = "Bluetooth state: POWERED_OFF\nTrying to turn on";

					EnableBluetooth();
					break;
			}*/
		#endif

		if (logErrorString != null) {
			LogNative.LogError(TAG + logErrorString);

			if (Event != null) {
				Event(null, new EventArgs(EventType.SYSTEM_NOT_SCANNING, TAG + "CheckBleStatus", logErrorString));
			}
		}
	}

	public static void StartScan() {
		LogNative.Log(isLogging, TAG + "StartScan, checking Ble-status");

		if (instance == null || !IsInitialized) {
			LogNative.LogError(TAG + "StartScan: ScanController is not initialized. Did you forget to add ScanController object in the scene?");
			
			return;
		}

		#if UNITY_ANDROID && !UNITY_EDITOR
			if (!scanPlugin.Call<bool>("IsBleFeatured") || !scanPlugin.Call<bool>("IsBluetoothAvailable") || !scanPlugin.Call<bool>("IsBluetoothTurnedOn") || !scanPlugin.Call<bool>("IsLocationTurnedOn")) {
				LogNative.LogWarning(TAG + "Scan not possible");

				CheckBleStatus();
			} else {
				Scan();
			}
		/*#elif UNITY_IOS && !UNITY_EDITOR
			if (GetBLEStatus() != 5) {
				LogNative.LogWarning(TAG + "Scan not possible");
				
				CheckBleStatus();
			} else {
				Scan();
			}*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif
	}

	private static void Scan() {
		if (IsScanning) {
			return;
		}
		LogNative.Log(isLogging, TAG + "Scan");

		IsIgnoringScanReport = false;

		IsScanning = true;

		MovesenseDevice.RemoveUnconnected();

		if (Event != null) {
			Event(null, new EventArgs(EventType.REMOVE_UNCONNECTED, TAG + "Scan", null));
		}

		#if UNITY_ANDROID && !UNITY_EDITOR
			#if UNITY_2018_3_OR_NEWER
					instance.StartCoroutine(RequestPermissionRoutine());
					return;
				#else
					scanPlugin.Call("Scan", uuidStrings);
				#endif
		/*#elif UNITY_IOS && !UNITY_EDITORios
			Scan_iOS(uuidStrings);*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif

		if (Event != null) {
			Event(null, new EventArgs(EventType.SYSTEM_SCANNING, TAG + "Scan", null));
		}
	}

		static bool fineLocationPermissionAsked;
		static bool fineLocationPermissionGranted = true;
		private static IEnumerator RequestPermissionRoutine()
		{
			Debug.Log($"{TAG}RequestPermissionRoutine");
#if UNITY_ANDROID && !UNITY_EDITOR
			if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
				Debug.Log($"{TAG}RequestPermissionRoutine: already granted");
			else
			{
				fineLocationPermissionAsked = false;
				Permission.RequestUserPermission(Permission.FineLocation);
				yield return new WaitUntil(() => fineLocationPermissionAsked == true);
				Debug.Log($"{TAG}RequestPermissionRoutine: next");
			}

			if (Event != null) {
				if (fineLocationPermissionGranted)
				{
					scanPlugin.Call("Scan", uuidStrings);
					Event(null, new EventArgs(EventType.SYSTEM_SCANNING, null, "scanning"));
				}
				else
					Event(null, new EventArgs(EventType.SYSTEM_NOT_SCANNING, null, "scanning"));
			}
#else
 			yield break;
#endif
    }

	public static void StopScan() {
		if (!IsScanning) {
			return;
		}
		LogNative.Log(isLogging, TAG + "StopScan");

		IsIgnoringScanReport = true;

		IsScanning = false;

		StopRefreshDeviceList();

		#if UNITY_ANDROID && !UNITY_EDITOR
			scanPlugin.Call("StopScan");
		/*#elif UNITY_IOS && !UNITY_EDITOR
			Stop_iOS();*/
		#elif UNITY_STANDALONE_OSX || UNITY_EDITOR
		#endif

		if (Event != null) {
			Event(null, new EventArgs(EventType.SYSTEM_NOT_SCANNING, TAG + "StopScan", null));
		}
	}

	#if UNITY_STANDALONE_OSX || UNITY_EDITOR
	public static void ReportScan(string Device) {
	#else
	public void ReportScan(string Device) {
	#endif
		if (IsIgnoringScanReport) {
			return;
		}
		LogNative.Log(isLogging, TAG + "ReportScan: " + Device);

		if (isRefreshingScanList) StartRefreshDeviceList();

		//Structure from native connect:[MacAdress or Identifier],[Name from AdvertisementData],[rssi]
		string[] splitString = Device.Split(',');

		string s_rssi = splitString[2];

		int i_rssi = int.Parse(s_rssi);
		if (i_rssi == 127) {
			LogNative.LogError(TAG + "ReportScan: RSSI failure, setting MinValue");

			i_rssi = int.MinValue;
		}

		string serial = splitString[1].Split(' ')[1];

		string macID = splitString[0];

		if (MovesenseDevice.GetConnectingState(macID)) {
			LogNative.Log(isLogging, TAG + macID + " is connecting, cancel further processing");

			return;
		}

		if (!RefresherList.Contains(macID) && isRefreshingScanList) {
			RefresherList.Add(macID);
		}

		if (MovesenseDevice.ContainsMacID(macID)) {
			if (MovesenseDevice.GetRssi(macID) != i_rssi && !IsRefreshingRssiBlocked) {
				LogNative.Log(isLogging, TAG + macID + " (" + serial + ") already scanned, refreshing rssi");

				MovesenseDevice.RefreshRssi(macID, i_rssi);

				if (Event != null) {
					Event(null, new EventArgs(EventType.RSSI, TAG + "ReportScan", macID));
				}
			} else {
				LogNative.Log(isLogging, TAG + macID + " (" + serial + ") already scanned, " + (IsRefreshingRssiBlocked ? "refreshRssi blocked" : "same rssi") + ", cancel further processing");

				return;
			}
		} else {
			LogNative.Log(isLogging, TAG + macID + " (" + serial + ") is new");

			MovesenseDevice movesenseDevice = new MovesenseDevice(macID, serial, i_rssi, false, false, null);
			MovesenseDevice.Add(movesenseDevice);

			if (Event != null) {
				Event(null, new EventArgs(EventType.NEW_DEVICE, TAG + "ReportScan", macID));
			}
		}

		if (isRefreshingScanList) StartRssiRefreshBlocker();
	}

	private static void StartRssiRefreshBlocker() {
		LogNative.Log(isLogging, TAG + "StartRssiRefreshBlocker: isRefreshingRssiBlocked = true");

		IsRefreshingRssiBlocked = true;

		if (IsStartRefresh) {
			return;
		}

		IsStartRefresh = true;

		instance.InvokeRepeating("SetisRefreshingRssiBlocked", rssiBlockTime, rssiBlockTime);
	}

	private void SetisRefreshingRssiBlocked() {
		LogNative.Log(isLogging, TAG + "SetisRefreshingRssiBlocked");

		isRefreshingRssiBlocked = false;
	}

	public static void StartRefreshDeviceList() {
		if (!IsRefreshing) {
			LogNative.Log(isLogging, TAG + "StartRefreshDeviceList");

			IsRefreshing = true;

			instance.InvokeRepeating("RefreshDeviceList", deviceRefreshTime, deviceRefreshTime);

			RefresherList.Clear();
		}
	}

	public static void StopRefreshDeviceList() {
		if (!IsRefreshing) {
			return;
		}		
		LogNative.Log(isLogging, TAG + "StopRefreshDeviceList");

		if (RefresherList.Count == 0 || !IsScanning) {
			IsRefreshing = false;

			instance.CancelInvoke("RefreshDeviceList");
		}

		RefresherList.Clear();

		IsRefreshingRssiBlocked = false;

		instance.CancelInvoke("SetisRefreshingRssiBlocked");
	}
	
	private void RefreshDeviceList() {
		LogNative.Log(isLogging, TAG + "RefreshDeviceList");

		isIgnoringScanReport = true;

		MovesenseDevice.RemoveAllExcept(refresherList);

		if (Event != null) {
			Event(null, new EventArgs(EventType.REFRESH, TAG + "RefreshDeviceList", null));
		}

		StopRefreshDeviceList();

		isIgnoringScanReport = false;
	}
}