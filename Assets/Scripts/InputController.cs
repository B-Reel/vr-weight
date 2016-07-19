using UnityEngine;
using Valve.VR;
using System.Collections;
using System.Collections.Generic;

namespace VRToolkit
{
    public class InputController : MonoBehaviour
    {

        private SteamVR_TrackedObject[] trackedObjects;
        private List<SteamVR_TrackedObject> lockedControllers;

        /**
         * Singleton implementation
         * */
        private static InputController instance;
        public static InputController Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject("InputController");
                    instance = obj.AddComponent<InputController>();
                }
                return instance;
            }
        }

        /**
         * Get a list of all available tracked objects (controllers, hmd etc)
         * */
        public void Awake()
        {
            trackedObjects = FindObjectsOfType<SteamVR_TrackedObject>();
        }

        public void Update()
        {
            trackedObjects = FindObjectsOfType<SteamVR_TrackedObject>();

            // Let's start by finding our controllers
            foreach (var trackedObject in trackedObjects)
            {
                var system = OpenVR.System;
                if (system != null && system.GetTrackedDeviceClass((uint)trackedObject.index) == ETrackedDeviceClass.Controller)
                {
                    // Let's guarantee this controller can collide with objects
                    if (trackedObject.GetComponent<Collider>() == null) SetupControllerCollider(trackedObject);
                }
            }
        }

        private void SetupControllerCollider(SteamVR_TrackedObject controller)
        {
            var collider = controller.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = Resources.Load<Mesh>("lowPoly_vive_001");
            var tipCollider = controller.gameObject.AddComponent<SphereCollider>();
            tipCollider.radius = 0.05f;
            tipCollider.center += new Vector3(0, -0.045f, 0.01f);

            // Interaction radius
            var interactionCollider = controller.gameObject.AddComponent<SphereCollider>();
            interactionCollider.radius = 0.1f;
            interactionCollider.center += new Vector3(0, 0, 0.025f);
            interactionCollider.isTrigger = true;

            // Gauge
            //var gauge = controller.gameObject.AddComponent<SphereGauge>();
            controller.gameObject.AddComponent<SliderGauge>();

            // Line renderer
            var lineRenderer = controller.gameObject.AddComponent<LineRenderer>();
            lineRenderer.SetWidth(0.2f, 0.2f);
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
            lineRenderer.enabled = false;

            var body = controller.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
        }

        public void Release(SteamVR_TrackedObject controller)
        {
            if (lockedControllers.Contains(controller))
                lockedControllers.Remove(controller);
        }

        public bool Lock(SteamVR_TrackedObject controller)
        {
            if (lockedControllers.Contains(controller)) return false; // Can't lock!

            lockedControllers.Add(controller);
            return true;
        }

        /**
         * No one can instantiate this class but me
         * */
        public InputController() {
            lockedControllers = new List<SteamVR_TrackedObject>();
        }
    }

}