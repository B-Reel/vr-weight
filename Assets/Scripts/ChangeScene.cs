using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using System.Collections;

public class ChangeScene : MonoBehaviour {

    public string SceneToLoad;
    public int timerDetectController;
    bool rightDetected = false;
    bool leftDetected = false;

    // Use this for initialization
    void Start () {
        timerDetectController = 0;
    }
	
	// Update is called once per frame
	void Update () {
        var allTrackedObjects = FindObjectsOfType<SteamVR_TrackedObject>();

        int rightIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
        int leftIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
//        uint leftIndex2 = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedContro‌​llerRole.LeftHand);
  //      uint rightIndex2 = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedContro‌​llerRole.RightHand);

        // Let's start by finding our controllers
        foreach (var trackedObject in allTrackedObjects)
        {
            Debug.Log((int)trackedObject.index);
            var system = OpenVR.System;
            if (system != null && system.GetTrackedDeviceClass((uint)trackedObject.index) == ETrackedDeviceClass.Controller)
            {
                var device = SteamVR_Controller.Input((int)trackedObject.index);
                if (device != null && device.GetPressDown(EVRButtonId.k_EButton_Grip))
                {
                    rightDetected |= (int)trackedObject.index == rightIndex;
                    leftDetected |= (int)trackedObject.index == leftIndex;
                    timerDetectController = 15;
                }
                  
            }       
        }

        if (timerDetectController > 0)
        {
            timerDetectController--;
            if (rightDetected && leftDetected)
            {
                SceneManager.LoadScene(SceneToLoad);
                timerDetectController = 0;
            }

            if (timerDetectController == 0)
            {
                rightDetected = false;
                leftDetected = false;
            }
        } 
    }
}
