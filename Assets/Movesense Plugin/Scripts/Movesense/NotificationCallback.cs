using System;
using UnityEngine;


public class NotificationCallback
		#if UNITY_ANDROID && !UNITY_EDITOR
			: AndroidJavaProxy
		#endif
{
	private const string TAG = "NotificationCallback; ";
	private const bool isLogging = false;

	#pragma warning disable CS0649
		private string serial;
		private string subscriptionPath; // without samplerate
	#pragma warning restore CS0649

	[Serializable]
	public class EventArgs : System.EventArgs {
		public long StatusCode { get; private set; }
		public string Serial { get; private set; }
		public string SubscriptionPath { get; private set; }
		public string Data { get; private set; }
		public EventArgs (long statusCode, string serial, string subscriptionPath, string data) {
			StatusCode = statusCode;
			Serial = serial;
			SubscriptionPath = subscriptionPath;
			Data = data;
		}
	}
	public static event EventHandler<EventArgs> Event;
	public static event EventHandler<EventArgs> EventResponse;

	[Serializable]
	public sealed class FieldArgs : EventArgs {
		public int SampleRate { get; private set; }
		public int DeviceTimestamp { get; private set; }
		public MeasurementValues[] Values { get; private set; }
		public FieldArgs (long statusCode, string serial, string subscriptionPath, string data, int sampleRate, int deviceTimestamp, MeasurementValues[] values) : base(statusCode, serial, subscriptionPath, data) {
			SampleRate = sampleRate;
			DeviceTimestamp = deviceTimestamp;
			Values = values;
		}
	}

	[Serializable]
	public sealed class HeartRateArgs : EventArgs {
		public double Pulse { get; private set; }
		public int[] RrData { get; private set; }
		public HeartRateArgs (long statusCode, string serial, string data, double pulse, int[] rrData) : base(statusCode, serial, global::SubscriptionPath.HeartRate, data) {
			Pulse = pulse;
			RrData = rrData;
		}
	}

	[Serializable]
	public sealed class TemperatureArgs : EventArgs {
		public double Temperature { get; private set; }
		public TemperatureArgs (long statusCode, string serial, string data, double temperature) : base(statusCode, serial, global::SubscriptionPath.Temperature, data) {
			Temperature = temperature;
		}
	}
	
	public NotificationCallback(
		#if UNITY_ANDROID && !UNITY_EDITOR
			string serial, string subscriptionPath) : base("com.movesense.mds.MdsNotificationListener"
		#endif
		)
	{
		LogNative.Log(isLogging, TAG + "assigned");

		#if UNITY_ANDROID && !UNITY_EDITOR
			this.serial = serial;

			this.subscriptionPath = subscriptionPath;
		#endif
	}

	
	/*#if UNITY_IOS && !UNITY_EDITOR
	public void onNotification(string data, string serial, string subscriptionPath) {
		#pragma warning disable CS0162
		if (isLogging) {
			LogNative.Log(TAG + "onNotification(iOS), data: " + data);
			LogNative.Log(TAG + "onNotification(iOS), serial:" + serial);
			LogNative.Log(TAG + "onNotification(iOS), subscriptionPath: "+ subscriptionPath);
		}
		#pragma warning restore CS0162
		
		this.serial = serial;
		
		this.subscriptionPath = subscriptionPath;
		
		onNotification(data);
	}
	#endif*/ //delete
	
	/// <summary>Called when data(JSON formatted string) arrives</summary>
	public void onNotification(string data) {
		Notification notification = JsonUtility.FromJson<Notification>(data);
		
		#pragma warning disable CS0162
		if (isLogging) {
			LogNative.Log(TAG + "onNotification, notification.Uri:" + notification.Uri);
			LogNative.Log(TAG + "onNotification, serial: "+ serial);
			LogNative.Log(TAG + "onNotification, subscriptionPath: "+ subscriptionPath);
			LogNative.Log(TAG + "onNotification, data: " + data);
			LogNative.Log(TAG + "onNotification, timestamp: " + notification.Body.Timestamp);
		}
		#pragma warning restore CS0162

		int sampleRate;
		int.TryParse(notification.Uri.Substring(notification.Uri.LastIndexOf("/") + 1), out sampleRate);
		LogNative.Log(isLogging, TAG + "onNotification, samplerate: " + sampleRate);

		EventArgs eventArgs;

		if (subscriptionPath == SubscriptionPath.LinearAcceleration) {
			eventArgs = new FieldArgs(0, serial, subscriptionPath, data, sampleRate, notification.Body.Timestamp, notification.Body.ArrayAcc);
		} else if (subscriptionPath == SubscriptionPath.AngularVelocity) {
			eventArgs = new FieldArgs(0, serial, subscriptionPath, data, sampleRate, notification.Body.Timestamp, notification.Body.ArrayGyro);
		} else if (subscriptionPath == SubscriptionPath.MagneticField) {
			eventArgs = new FieldArgs(0, serial, subscriptionPath, data, sampleRate, notification.Body.Timestamp, notification.Body.ArrayMagn);
		} else if (subscriptionPath == SubscriptionPath.HeartRate) {
			double pulse = notification.Body.average;
		
			eventArgs = new HeartRateArgs(0, serial, data, pulse, notification.Body.rrData);
		} else if (subscriptionPath == SubscriptionPath.Temperature) {
			double temperature = notification.Body.Measurement;
		
			eventArgs = new TemperatureArgs(0, serial, data, temperature);
		} else {
			// for custom paths
			eventArgs = new EventArgs(0, serial, subscriptionPath, data);
		}
		
		if (Event != null) {
			Event(null, eventArgs);
		}
	}

	/*#if UNITY_IOS && !UNITY_EDITOR
    public void onResponse(long statusCode, string body, string serial, string subscriptionPath) {
		#pragma warning disable CS0162
		if (isLogging) {
			LogNative.Log(TAG + "onResponse(iOS), statusCode: " + statusCode);
			LogNative.Log(TAG + "onResponse(iOS), body: " + body);
			LogNative.Log(TAG + "onResponse(iOS), serial:" + serial);
			LogNative.Log(TAG + "onResponse(iOS), subscriptionPath: "+ subscriptionPath);
		}
		#pragma warning restore CS0162

		this.serial = serial;

		this.subscriptionPath = subscriptionPath;

		if (statusCode >= 300) {
			onError(statusCode, body);
		} else {
			if (EventResponse != null) {
				EventResponse(null, new EventArgs(statusCode, serial, subscriptionPath, body));
			}
		}
	}
	#endif*/

	/// <summary>Called when an error occurs</summary>
    public void onError(
	#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject
	#else
		long statusCode, string
	#endif
		error
	) {
		#if UNITY_ANDROID && !UNITY_EDITOR
			// no statusCode for Android
			long statusCode = 1;

			LogNative.LogError(TAG + "onError, serial: "+serial+", subscriptionPath: "+subscriptionPath+", error: " + error.Call<string>("getMessage"));
		#else
			LogNative.LogError(TAG + "onError, serial: "+serial+", subscriptionPath: "+subscriptionPath+", statusCode: " + statusCode + ", error: " + error);
		#endif

		if (Event != null) {
			Event(null, new EventArgs(statusCode, serial, subscriptionPath, null));
		}
	}

	[Serializable]
	public class Notification //changed to public from private to be able to use it in MovesenseSensors: case MovesenseController.EventType.NOTIFICATION
	{
        public Body Body = null;
		public string Uri = null;
		public string Method = null;
	}

	[Serializable]
	public class Body //changed to public from private
    {
		public int Timestamp = 0;
		public MeasurementValues[] ArrayAcc = null;
		public MeasurementValues[] ArrayGyro = null;
		public MeasurementValues[] ArrayMagn = null;
		public double average = 0;
		public int[] rrData = null;
		public double Measurement = 0;

	}
	
	[Serializable]
	public class MeasurementValues {
		public double x = 0;
		public double y = 0;
		public double z = 0;
	}
}