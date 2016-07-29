using UnityEngine;
using System;
using System.Collections;

namespace VRToolkit
{
    [Serializable]
    public class DragWithJoint : Interactable
    {
        /**
         * Exposed in Unity interface
         * */
        public float breakForce = float.PositiveInfinity;
        public bool HapticsOnDrag = true;
        public bool ShowGauge = true;

        /**
         * Internal stuff
         * */
        private SteamVR_TrackedObject currentController;
        private Joint currentJoint;

        void FixedUpdate()
        {
            if (currentController == null) return;
            var device = SteamVR_Controller.Input((int)currentController.index);

            //
            // End interation if the object is pulled too fast
            var rigidBody = GetComponent<Rigidbody>();
            var controllerVelocitySq = device.velocity.sqrMagnitude;
            var maxControllerVelocitySq = Mathf.Pow(10f / rigidBody.mass, 2);
            var forceAmout = Math.Min(controllerVelocitySq / maxControllerVelocitySq, 1f);
            var controllerGauge = currentController.GetComponent<IForceGauge>();

            // Update gauge based on interaction
            if (controllerGauge != null && ShowGauge)
            {
                controllerGauge.ForceValue = forceAmout;
            }

            // Trigger haptics
            if (HapticsOnDrag)
            {
                var hapticsAmout = Math.Max(forceAmout - 0.2f, 0f);
                SteamVR_Controller.Input((int)currentController.index).TriggerHapticPulse((ushort)(hapticsAmout * 5000));
            }

            // Drop object when moving faster than allowed
            if (controllerVelocitySq > maxControllerVelocitySq)
            {
                EndInteraction(currentController);

            // Dropped by releasing the trigger
            } else if (currentJoint != null && device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                EndInteraction(currentController);
            }
        }

        public override void OnTriggerDownColliding(SteamVR_TrackedObject controller)
        {
            // Clean previous interaction
            if (currentController != null)
                EndInteraction(currentController);

            // Try to acquire a lock for this controller
            if (!InputController.Instance.Lock(controller)) return; // Stop if someone already has it


            // Disable colisions that would push this object
            foreach (Collider collider in controller.GetComponents<Collider>())
            {
                if (collider.isTrigger) continue;
                foreach (Collider objectCollider in GetComponents<Collider>())
                {
                    Debug.Log("Collision disabled" + collider + " " + objectCollider);
                    Physics.IgnoreCollision(collider, objectCollider, true);
                }
            }

            // Attach
            currentController = controller;
            var joint = gameObject.AddComponent<ConfigurableJoint>();
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            joint.connectedBody = currentController.GetComponent<Rigidbody>();
            joint.breakForce = breakForce;
            
            currentJoint = joint;
        }

        void OnJointBreak(float breakForce)
        {
            currentJoint = null;
            if (currentController != null)
                EndInteraction(currentController);
        }

        /**
         * Restore controller and object state when releasing
         * */
        public override void EndInteraction(SteamVR_TrackedObject controller)
        {
            
            // Not interacting with this controller
            if (controller != currentController)
                return;
            InputController.Instance.Release(controller);

            // Re-enable colisions that would push this object
            foreach (Collider collider in controller.GetComponents<Collider>())
            {
                if (collider.isTrigger) continue;
                foreach (Collider objectCollider in GetComponents<Collider>())
                {
                    Physics.IgnoreCollision(collider, objectCollider, false);
                }
            }

            var device = SteamVR_Controller.Input((int)currentController.index);

            // Set velocity of the object when releasing based on controller velocity
            var rigidbody = GetComponent<Rigidbody>();
            var origin = currentController.origin ? currentController.origin : currentController.transform.parent;
            if (origin != null)
            {
                rigidbody.velocity = origin.TransformVector(device.velocity);
                rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
            }
            else
            {
                rigidbody.velocity = device.velocity;
                rigidbody.angularVelocity = device.angularVelocity;
            }
            //
            rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;

            // Hide gauge
            var controllerGauge = currentController.GetComponent<IForceGauge>();
            if (controllerGauge != null)
                controllerGauge.ForceValue = 0;

            // Destroy joint
            if (currentJoint != null)
            {
                currentJoint.connectedBody.useGravity = true;
                DestroyImmediate(currentJoint);
                currentJoint = null;
            }
            currentController = null;

        }
    }

}