
using UnityEngine;
using System.Runtime.InteropServices;

public static class LogNative {
	#if UNITY_IOS && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern void NSLog_iOS(string device);
	#endif

    public static void Log(string logString)
    {
        Log(true, logString);
    }

	public static void Log(bool isLogging, string logString) {
		if (!isLogging) {
			return;
		}
		#if UNITY_ANDROID || UNITY_EDITOR
			Debug.Log(logString);
		#elif UNITY_IOS && !UNITY_EDITOR
			NSLog_iOS(logString);
		#endif
	}

	public static void LogWarning(string logString) {
		#if UNITY_ANDROID || UNITY_EDITOR
			Debug.LogWarning(logString);
		#elif UNITY_IOS && !UNITY_EDITOR
			NSLog_iOS("WARNING: " + logString);
		#endif
	}

	public static void LogError(string logString) {
		#if UNITY_ANDROID || UNITY_EDITOR
			Debug.LogError(logString);
		#elif UNITY_IOS && !UNITY_EDITOR
			NSLog_iOS("ERROR: " + logString);
		#endif
	}
}

