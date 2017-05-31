using UnityEngine;
using Valve.VR;
using System.Collections;

public class IntroBehavior : MonoBehaviour {
    public GameObject tables;
    public GameObject basketHoop;
    public GameObject introUi;
    public GameObject introEnvironment;
    public GameObject fadeToBlack;
    public ChangeScene changeScene;

    int timerDetectController;
    Renderer fadeRenderer;
    bool rightDetected = false;
    bool leftDetected = false;

    private float rotationFinal;
    private float rotationEased;

    private Vector3 position;
    private Quaternion rotation;
    private Vector3 rotationAngles;

    private bool removeIntro = false;
    private bool introVisible = true;
    float elapsedTime = 0;

    private float easedRotation;

    // Use this for initialization
    void Start () {
        tables.SetActive(false);
        basketHoop.SetActive(false);
        introUi.SetActive(true);
        introEnvironment.SetActive(true);
        rotationFinal = Camera.main.transform.rotation.eulerAngles.y;
        rotationEased = rotationFinal;

        timerDetectController = 0;
        fadeRenderer = fadeToBlack.GetComponent<Renderer>();
        updateAlpha(0);
    }
	
	// Update is called once per frame
	void Update () {
        if (introVisible)
        {
            Debug.Log(Camera.main.transform.position);

            /* Update intro position. */
            position = transform.position;
            position.x = Camera.main.transform.position.x;
            position.z = Camera.main.transform.position.z;
            position.y = 1.0f;
            transform.position = position;

            /* Update intro eased rotation. */
            rotationFinal = Camera.main.transform.rotation.eulerAngles.y;
            rotationEased = Mathf.LerpAngle(rotationEased, rotationFinal, Time.deltaTime * 4);

            /* Update intro rotation. */
            rotation = transform.rotation;
            rotationAngles = rotation.eulerAngles;
            rotationAngles.y = rotationEased;
            rotationAngles.x = 0;
            rotationAngles.z = 0;
            rotation.eulerAngles = rotationAngles;
            transform.rotation = rotation;


            /* Update ui rotation. */
            //Quaternion qUI = introUi.transform.localRotation;
            //Vector3 rotUI = qUI.eulerAngles;
            //rotUI.x = Mathf.LerpUnclamped(120, 60, Camera.main.transform.position.y * 0.5f);
            //qUI.eulerAngles = rotUI;
            //introUi.transform.localRotation = qUI;


            if (!removeIntro && !changeScene.updatingScene)
            {
                /* Check if we need to remove intro. */
                var allTrackedObjects = FindObjectsOfType<SteamVR_TrackedObject>();

                int rightIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
                int leftIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);

                // Let's start by finding our controllers
                foreach (var trackedObject in allTrackedObjects)
                {
                    Debug.Log((int)trackedObject.index);
                    var system = OpenVR.System;
                    if (system != null && system.GetTrackedDeviceClass((uint)trackedObject.index) == ETrackedDeviceClass.Controller)
                    {
                        var device = SteamVR_Controller.Input((int)trackedObject.index);
                        if (device != null && device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger))
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
                        removeIntro = true;
                        elapsedTime = 0;
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

        /* Removes the intro */
        if (removeIntro)
        {
            if (introVisible)
            {
                elapsedTime += Time.deltaTime * 2.1f;
                if (elapsedTime >= 1)
                {
                    tables.SetActive(true);
                    basketHoop.SetActive(true);
                    introUi.SetActive(false);
                    introEnvironment.SetActive(false);
                    elapsedTime = 1f;
                    introVisible = false;
                }
            }
            else
            {
                elapsedTime -= Time.deltaTime * 2.1f;
                if (elapsedTime >= 1)
                {
                    elapsedTime = 0f;
                    removeIntro = false;
                }
            }
            updateAlpha(elapsedTime);
        }


    }

    private void updateAlpha(float position)
    {
        Color c = fadeRenderer.material.color;
        c.a = Mathf.Sin((Mathf.PI / 2f) * Mathf.Clamp(position, 0, 1));
        fadeRenderer.material.color = c;
    }
}
