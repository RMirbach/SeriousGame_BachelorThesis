using System.Collections;
using System.IO.IsolatedStorage;
using TMPro;
using UnityEngine;
using static NotificationCallback;
using static ResponseCallback;
using UnityEngine.SceneManagement;
using System;
using static UnityEngine.GraphicsBuffer;

public class MovesenseSensors : MonoBehaviour {
	private float angle = 90;
    private float theta = 0.0f;
    private Vector3 axisout = Vector3.zero;
    public string connectedSensor;
     


    public static MovesenseSensors Instance { get; private set; }

	private void Awake() {
		Instance = this;
        //Adding Movesense Plugins Events
		ScanController.Event += OnScanControllerCallbackEvent;
		MovesenseController.Event += OnMovesenseControllerCallbackEvent;
	}

	void Start () {
		StartCoroutine(StartScanning());
    }


	void OnScanControllerCallbackEvent(object sender, ScanController.EventArgs e) {
		//LogNative.Log("OnScanControllerCallbackEvent, Type: " + e.Type + ", invoked by: " + e.InvokeMethod);
		switch (e.Type) {
			case ScanController.EventType.NEW_DEVICE:
				LogNative.Log("OnScanControllerCallbackEvent, NEW_DEVICE with MacID: "+e.MacID+", connecting...");
				StartCoroutine(Connect(e.MacID));
			break;
		}
	}

	void OnMovesenseControllerCallbackEvent(object sender, MovesenseController.EventArgs e) {
		//LogNative.Log("OnMovesenseControllerCallbackEvent, Type: " + e.Type + ", invoked by: " + e.InvokeMethod);
		switch (e.Type) {
			case MovesenseController.EventType.CONNECTING:
				for (int i = 0; i < e.OriginalEventArgs.Count; i++) {
					var ce = (ConnectCallback.EventArgs) e.OriginalEventArgs[i];

					LogNative.Log("OnMovesenseControllerCallbackEvent, CONNECTING " + ce.MacID);
				}
			break;
			case MovesenseController.EventType.CONNECTED:
				for (int i = 0; i < e.OriginalEventArgs.Count; i++) {
					var ce = (ConnectCallback.EventArgs) e.OriginalEventArgs[i];

					LogNative.Log("OnMovesenseControllerCallbackEvent, CONNECTED " + ce.MacID + ", subscribing linearAcceleration");
                    //other SampleRates possible (slower, faster, medium, fast)
					MovesenseController.Subscribe(ce.Serial, SubscriptionPath.LinearAcceleration, SampleRate.slowest);
                    //Store sensor ID to be called from MainMenu to disconnect the sensor when the application is quit
                    connectedSensor = ce.MacID;
                    //Calling method that informs user about the connection of the sensor                    
                    MainMenu.Instance.ConnectionSuccessfull();                    
                    
				}
			break;
			case MovesenseController.EventType.NOTIFICATION:
				for (int i = 0; i < e.OriginalEventArgs.Count; i++) {
					var ne = (NotificationCallback.EventArgs) e.OriginalEventArgs[i];
					
					LogNative.Log("OnMovesenseControllerCallbackEvent, NOTIFICATION for " + ne.Serial + ", SubscriptionPath: " + ne.SubscriptionPath + ", Data: " + ne.Data);
                    // Casting the Acceleration Data from the sensors 
                    if (ne.Data != string.Empty)
					{
                        NotificationCallback.Notification notification = JsonUtility.FromJson<NotificationCallback.Notification>(ne.Data);
                        Debug.Log("MovesenseSensors " + notification);
                        if (notification != null && notification.Body != null && notification.Body.ArrayAcc != null)//ArrayAcc
                        {
                            //Storing double x, double y and double z from SubriptionPath.LinearAcceleration in MeasurementValues
                            MeasurementValues[] mv = notification.Body.ArrayAcc;
                            //Debug.Log("arrayacc x= " + mv[0].x  + " arrayacc y=  " + mv[0].y + "arrayacc z= " + mv[0].z);
                            
                            //Calculations to convert into quaternion
                            var v1 = Mathf.Cos(angle / 2);
                            var vx = (((float)mv[0].x) * Mathf.Sin(angle / 2));
                            var vy = (((float)mv[0].y) * Mathf.Sin(angle / 2));
                            var vz = (((float)mv[0].z) * Mathf.Sin(angle / 2));
                            
                            //create quaternion
                            var quaternion = new Quaternion(v1,vx, vy, vz);
                            //Debug.Log("quaternion " + quaternion);

                            //Normalize quaternion
                            quaternion.Normalize();
                            //Debug.Log("quaternion normalized " + quaternion);
                            
                            //Conversion of rotation to angle-axis representation
                            quaternion.ToAngleAxis(out theta, out axisout);
                            //Debug.Log("quaternion2 toangleaxis" + quaternion + "theta "+ theta + "axis "+ axisout);

                            //Creation of rotation with angle in degrees around axis
                            var finalquaternion = Quaternion.AngleAxis(theta / 2, axisout);
                            //Debug.Log("finalquaternion " + finalquaternion + " Angle: " + theta/2);

                            //Normalize finalquaternion 
                            finalquaternion.Normalize();
                            //Debug.Log("finalquaternion normalized " + finalquaternion);

                            //Pass finalquaternion to method that moves the stick
                            RotatePaddle.UpdatePaddleRotation(finalquaternion);
                            //Debug.Log("RotatePaddle finalquaternion " + finalquaternion);

                        }
                    }
                }
            break;
        }
	}

    IEnumerator StartScanning() {
		yield return new WaitUntil(() => ScanController.IsInitialized); // wait for ScanController to be initialized
		ScanController.StartScan();
	}

	IEnumerator Connect(string macID) {
		yield return new WaitUntil(() => MovesenseController.isInitialized); // wait for MovesenseController to be initialized
		MovesenseController.Connect(macID);
	}
        
}
