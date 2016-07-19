using UnityEngine;
using System.Collections;
using System;

namespace VRToolkit
{
    public class DragByParenting : Interactable
    {
        private SteamVR_TrackedObject trackedController;
        private bool kinematic;

        // Use this for initialization
        public new void Start()
        {
            base.Start();
        }

        void Awake()
        {
            var rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                Debug.Log("Object " + name + " does not contain a rigid body");
                return;
            }
            kinematic = rigidBody.isKinematic;
        }

        // Update is called once per frame
        public new void Update()
        {
            base.Update();
            if (trackedController != null)
            {
                var device = SteamVR_Controller.Input((int)trackedController.index);
                if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    EndInteraction(trackedController);
                }
            }
        }

        public override void OnTriggerDownColliding(SteamVR_TrackedObject controller)
        {
            if (InputController.Instance.Lock(controller))
            {
                trackedController = controller;
                transform.parent = controller.transform;
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        public override void EndInteraction(SteamVR_TrackedObject controller)
        {
            // Not interacting with this controller!
            if (controller != trackedController)
                return;
            InputController.Instance.Release(controller);

            var device = SteamVR_Controller.Input((int)controller.index);

            transform.parent = null;
            var rigidBody = GetComponent<Rigidbody>();
            rigidBody.isKinematic = kinematic;

            // Keep the object on the same velocity that the controller is
            rigidBody.velocity = device.velocity;
            rigidBody.angularVelocity = device.angularVelocity;

            // Not affected by this anymore
            trackedController = null;
        }
    }
}