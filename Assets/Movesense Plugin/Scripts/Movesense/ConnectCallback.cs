using System;
using UnityEngine;


public class ConnectCallback
		#if UNITY_ANDROID && !UNITY_EDITOR
			: AndroidJavaProxy
		#endif
{
	private const string TAG = "ConnectCallback; ";
	private const bool isLogging = false;
	private string invokeMacId;
	
	[Serializable]
	public sealed class EventArgs : System.EventArgs {
		/// <summary>IsConnect; true: sensor is connected, false sensor is disconnected</summary>
		public bool IsConnect { get; private set; }
		public string MacID { get; private set; }
		public string Serial { get; private set; }

		public EventArgs (bool isConnect, string macID, string serial) {
			IsConnect = isConnect;
			MacID = macID;
			Serial = serial;
		}
	}
	//provide Events
	public static event	EventHandler<EventArgs> Event;
	

	public ConnectCallback()
		#if UNITY_ANDROID && !UNITY_EDITOR
			: base("com.movesense.mds.MdsConnectionListener")
		#endif
	{
		LogNative.Log(isLogging, TAG + "assigned");
	}

	/// <summary>Called when Mds / Whiteboard link-layer connection (BLE) has been succesfully established</summary>
	public void onConnect(string macID) {
		LogNative.Log(isLogging, TAG + "onConnect: " + macID + ", waiting for onConnectionComplete");
	}

	/// <summary>Called when the full Mds / Whiteboard connection has been succesfully established</summary>
	public void onConnectionComplete(string macID, string serial) {
		LogNative.Log(TAG + "onConnectionComplete: " + macID + ", serial: " + serial);
		
		SetMovesenseDeviceConnectState(macID, serial, true);
	}

	/// <summary>Called when Mds connect() call fails with error</summary>
	public void onError(
	#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject
	#else
		long statusCode, string
	#endif
		error
	) {
		#if UNITY_ANDROID && !UNITY_EDITOR
			LogNative.LogError(TAG + "onError, error: " + error);
		#else
			LogNative.LogError(TAG + "onError, statusCode: " + statusCode + ", error: " + error);
		#endif
	}

	/// <summary>Called when Mds connection disconnects (e.g. device out of range)</summary>
	public void onDisconnect(string macID) {
		LogNative.LogWarning(TAG + "onDisconnect: " + macID);
		
		string serial = MovesenseDevice.GetSerial(macID);

		SetMovesenseDeviceConnectState(macID, serial, false);
	}

	private void SetMovesenseDeviceConnectState(string macID, string serial, bool isConnect) {
		LogNative.Log(isLogging, TAG + "SetMovesenseDeviceConnectState: " + macID + " (" + serial + "): " + (isConnect ? "connected" : "disconnected"));
		
		// NOTE: sometimes there is no onConnectionComplete-Callback and the library is trying to reconnect without success.
		// depends on which mobile android device is used.
		// take a look at https://bitbucket.org/suunto/movesense-docs/wiki/Mobile/Movesense%20compatible%20mobile%20devices.md
		
		// Fix: sometimes whiteboard reconnects a device
		if (!MovesenseDevice.ContainsMacID(macID) && isConnect) {
			MovesenseDevice movesenseDevice = new MovesenseDevice(macID, serial, -600, false, false, null);

			LogNative.Log(TAG + "onConnectionComplete: " + macID + " (" + serial + "): reconnected, adding device");
			MovesenseDevice.Add(movesenseDevice);
		}

		if (MovesenseDevice.SetConnectionState(macID, isConnect) || isConnect) {
			if (Event != null) {
				Event(null, new EventArgs(isConnect, macID, serial));
			}
		}
	}
}