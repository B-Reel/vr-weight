using UnityEngine;
using Valve.VR;
using System.Collections.Generic;

namespace VRToolkit
{
    public abstract class Interactable : MonoBehaviour
    {
        public bool hapticsOnCollision = true;
        
        protected SteamVR_TrackedObject lastTrackedObject;
        internal List<SteamVR_TrackedObject> currentControllers;

        // Use this for initialization
        public void Start()
        {
            currentControllers = new List<SteamVR_TrackedObject>();

            var rigidBody = GetComponent<Rigidbody>();
            // Ensure we have a rigid body to react to physics
            if (!rigidBody)
                rigidBody = this.gameObject.AddComponent<Rigidbody>();
        }

        // Update is called once per frame
        public void Update()
        {
            foreach (var trackedObject in currentControllers)
            {
                var device = SteamVR_Controller.Input((int)trackedObject.index);
                if (device != null)
                {
                    if (device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger))
                    {
                        Debug.Log("down colliding!");
                        // Disable interactions for all interactabled that are also touching this controller
                        OnTriggerDownColliding(trackedObject);
                    }
                    else if (device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger))
                    {
                        OnTriggerUpColliding(trackedObject);
                    }
                }
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            Debug.Log("trigger entered");
            var trackedObject = collider.gameObject.GetComponent<SteamVR_TrackedObject>();
            if (trackedObject && !currentControllers.Contains(trackedObject)) {
                currentControllers.Add(trackedObject);
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            /**
             * Shake controller when it reaches an object
             * */
            var trackedObject = collision.gameObject.GetComponent<SteamVR_TrackedObject>();
            if (trackedObject && hapticsOnCollision)
            {
                SteamVR_Controller.Input((int)trackedObject.index).TriggerHapticPulse(500);
            }   
        }

        public void OnTriggerExit(Collider collider)
        {
            var trackedObject = collider.gameObject.GetComponent<SteamVR_TrackedObject>();
            if (trackedObject && currentControllers.Contains(trackedObject))
                currentControllers.Remove(trackedObject);
        }

        public virtual void OnTriggerDownColliding(SteamVR_TrackedObject controller) { }
        public virtual void OnTriggerUpColliding(SteamVR_TrackedObject controller) { }
        public abstract void EndInteraction(SteamVR_TrackedObject controller);
    }

}