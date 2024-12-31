using System;
using UnityEngine;


public class ResponseCallback
		#if UNITY_ANDROID && !UNITY_EDITOR
			: AndroidJavaProxy
		#endif
{
	private const string TAG = "ResponseCallback; ";
	private const bool isLogging = false;
	
	#if UNITY_ANDROID && !UNITY_EDITOR
		private readonly string method;
	#endif

	private string _Uri;

	[Serializable]
	public class EventArgs : System.EventArgs {
		public long StatusCode { get; private set; }
		public string Uri { get; private set; }
		public string Method { get; private set; }
		public string Data { get; private set; }
		
		public EventArgs (long statusCode, string uri, string method, string data) {
			StatusCode = statusCode;
			Uri = uri;
			Method = method;
			Data = data;
		}
	}
	public static event EventHandler<EventArgs> Event;
	
	public ResponseCallback(
		#if UNITY_ANDROID && !UNITY_EDITOR
			string method, string uri
		#endif
	)
		#if UNITY_ANDROID && !UNITY_EDITOR
			: base("com.movesense.mds.MdsResponseListener")
		#endif
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
			_Uri = uri;
			
			this.method = method;
		#endif

		LogNative.Log(isLogging, TAG + "assigned");
	}


	/// <summary>Called when Mds operation has been succesfully finished</summary>
	public void onSuccess(string data,
	#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject header
	#else
		string method, string header
	#endif
	) {
		string uri = null;

		#if UNITY_ANDROID && !UNITY_EDITOR
			uri = header.Call<string>("getUri");
		
			string method = this.method;
		#elif UNITY_IOS && !UNITY_EDITOR
			uri = header;
		#endif
		
		LogNative.Log(isLogging, TAG + "onSuccess, uri: " + uri + ", method: " + method + ", data: " + data);
		
		if (Event != null) {
			Event(null, new EventArgs(0, uri, method, data));
		}
	}

	/// <summary>Called when an error occurs</summary>
	public void onError(
	#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject
	#else
		long statusCode, string uri, string
	#endif
		error
	) {

		#if UNITY_ANDROID && !UNITY_EDITOR
			// no statusCode for Android
			long statusCode = 1;

			LogNative.LogError(TAG + "onError, error: " + error.Call<string>("getMessage"));
		#else
			_Uri = uri;
			
			LogNative.LogError(TAG + "onError, statusCode: " + statusCode + ", uri: " + uri + ", error: " + error);
		#endif

		if (Event != null) {
			Event(null, new EventArgs(statusCode, _Uri, null, null));
		}
	}


	public class Data {
		public string Content = null;
	}

	[Serializable]
	public class Info {
		public InfoContent Content = null;

		[Serializable]
		public class InfoContent {
			public string manufacturerName = null;
			public string brandName = null;
			public string productName = null;
			public string variant = null;
			public string design = null;
			public string hwCompatibilityId = null;
			public string serial = null;
			public string pcbaSerial = null;
			public string sw = null;
			public string hw = null;
			public string additionalVersionInfo = null;
			public AddressInfo[] addressInfo = null;
			public string apiLevel = null;

			[Serializable]
			public class AddressInfo {
				public string name = null;
				public string address = null;
			}
		}
	}


	[Serializable]
	public class Peers {
		public PeersContent Content = null;

		[Serializable]
		public class PeersContent {
			public Connected_Peers[] ConnectedPeers = null;

			[Serializable]
			public class Connected_Peers {
				public string Address = null;
				public string handle = null;
			}

			public int[] Data = null;
		}
	}	
}