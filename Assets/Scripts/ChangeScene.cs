using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using System.Collections;

public class ChangeScene : MonoBehaviour {

    public string SceneToLoad;
    public GameObject fadeOutGO;

    public bool updatingScene = false;

    int timerDetectController;
    Renderer fadeOutRenderer;
    bool rightDetected = false;
    bool leftDetected = false;

    bool loadScene = false;
    bool initScene = true;

    float elapsedTime = 0;

    // Use this for initialization
    void Start () {
        timerDetectController = 0;
        fadeOutRenderer = fadeOutGO.GetComponent<Renderer>();
        updateAlpha(1);
    }
	
	// Update is called once per frame
	void Update () {
        updatingScene = loadScene | initScene;

        if (loadScene || initScene)
        {
            elapsedTime += Time.deltaTime * 0.9f;
            updateAlpha(initScene ? 1-elapsedTime : elapsedTime);
            if (elapsedTime >= 1) {
                elapsedTime = 0f;
                initScene = false;

                if (loadScene)
                    SceneManager.LoadScene(SceneToLoad);
            }
                
            return;
        }

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
                loadScene = true;
                timerDetectController = 0;
            }

            if (timerDetectController == 0)
            {
                rightDetected = false;
                leftDetected = false;
            }
        } 
    }

    private void updateAlpha(float position)
    {
        Color c = fadeOutRenderer.material.color;
        c.a =QuartEaseInOut(Mathf.Clamp(position, 0, 1), 0, 1, 1);
        fadeOutRenderer.material.color = c;
    }

    private float QuartEaseInOut(float t, float b, float c, float d)
    {
        if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;
        return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
    }
}
