using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using System.Collections;

public class ChangeScene : MonoBehaviour {

    public string SceneToLoad;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        var allTrackedObjects = FindObjectsOfType<SteamVR_TrackedObject>();
        
        // Let's start by finding our controllers
        foreach (var trackedObject in allTrackedObjects)
        {
            var system = OpenVR.System;
            if (system != null && system.GetTrackedDeviceClass((uint)trackedObject.index) == ETrackedDeviceClass.Controller)
            {
                var device = SteamVR_Controller.Input((int)trackedObject.index);
                if (device != null && device.GetPressDown(EVRButtonId.k_EButton_Grip))
                    SceneManager.LoadScene(SceneToLoad);
            }       
        }
    }
}
