using UnityEngine;
using System.Collections;
using System;

namespace VRToolkit {
    public class DragByChangingPhysics : Interactable {

        /**
         * Three different ways of moving objects
         * */
        public enum MoveBy
        {
            ApplyingForce,
            ChangingVelocity,
            Slerp
        }

        /**
         * Method used to lock object position and rotation
         * Minimum threshold
         * */
        public enum AttachMode
        {
            Never,
            Timer,
            Work
        }

        /**
         * When to attach, after threshold
         * */
        public enum AttachCondition
        {
            ProximityToBounds,
            CollisionWithObject
        }

        /**
         * Variables exposed on Unity interface
         * */
        public AttachMode attachMode = AttachMode.Work;
        public AttachCondition attachCondition = AttachCondition.ProximityToBounds;
        public bool hapticsOnDrag = true;
        public MoveBy moveBy = MoveBy.ApplyingForce;
        public bool lineBreaks = true;
        public bool hideControllerOnSnap;
        public float attachTimer = 1.5f;
        public float attachWork = 1f;
        public float breakForce = float.PositiveInfinity;

        /**
         * Internal variables
         * */
        private float MaxVelocity = 2;
        private Vector3 collisionOffset;
        private SteamVR_TrackedObject trackedController;
        private Quaternion rotationOffset;
        private float interactionTime;
        private float currentWork = 0;
        private Joint currentJoint;
        private Color mainColor;
        private float droppedAt;
        private bool collidingWithController;
        private Transform snapPoint;

        void Awake()
        {
            mainColor = GetComponent<Renderer>().material.color;
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Snap Point")) snapPoint = child;
            }
        }

        public void FixedUpdate() {

            // Not doing anything if not interacting
            if (trackedController == null)
                return;

            var device = SteamVR_Controller.Input((int)trackedController.index);
            var rigidBody = GetComponent<Rigidbody>();

            // Line between object and controller
            var lineRenderer = trackedController.GetComponent<LineRenderer>();
            lineRenderer.enabled = currentJoint == null;

            // Move object towards you while there is no joint
            if (currentJoint == null)
            {
                // distance betwen where it is and where it should be
                var positionTarget = trackedController.transform.position + trackedController.transform.forward * 0.06f;
                var attachPoint = transform.TransformPoint(collisionOffset);
                var distanceDelta = positionTarget - attachPoint;
                var distanceMassRelation = (float)Math.Sqrt((float)Math.Pow(rigidBody.mass, 2f) / distanceDelta.sqrMagnitude) / 1000f;

                // Increase amount of work done
                currentWork += distanceMassRelation;

                // Haptics
                if (hapticsOnDrag)
                    SteamVR_Controller.Input((int)trackedController.index).TriggerHapticPulse((ushort)(distanceDelta.magnitude * 1000));

                // Drop it if pulled too fast
                var lineTension= distanceDelta.magnitude * rigidBody.mass / 7f; // Constant for line tension strength
                
                // Set line initial and final position
                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, positionTarget);
                    lineRenderer.SetPosition(1, attachPoint);
                    var lineW = Math.Max((1 - lineTension) * 0.01f, 0.001f);
                    lineRenderer.SetWidth(lineW, lineW);
                    var c = Math.Max(Math.Min(1, lineTension), 0);
                    lineRenderer.material.color = new Color(c, 1 - c, 0);
                }

                // Break the line based on its tension
                if (lineTension > 1 && lineBreaks)
                {
                    EndInteraction(trackedController);
                    return;
                }

                // Create joint if too close
                var bounds = GetComponent<Renderer>().bounds;
                bounds.Expand(0.2f);
                if ((
                    (attachMode == AttachMode.Timer && interactionTime + attachTimer * rigidBody.mass < Time.time) ||
                    (attachMode == AttachMode.Work && currentWork > attachWork * rigidBody.mass)
                    ) && (
                    (attachCondition == AttachCondition.ProximityToBounds && bounds.Contains(positionTarget)) ||
                    (attachCondition == AttachCondition.CollisionWithObject && collidingWithController)
                    ))
                {
                    if (hideControllerOnSnap)
                    {
                        foreach (Renderer renderer in trackedController.GetComponentsInChildren<Renderer>())
                            renderer.enabled = false;
                        foreach (Collider collider in trackedController.GetComponents<Collider>())
                            if (!collider.isTrigger) collider.enabled = false;
                        transform.position = trackedController.transform.position;
                    }

                    var createJoint = true;
                    if (snapPoint != null)
                    {
                        // Disable colliders
                        foreach (var collider in GetComponents<Collider>()) collider.enabled = false;

                        var finalTarget = positionTarget + (transform.position - snapPoint.position);
                        createJoint = (snapPoint.position - positionTarget).magnitude < 0.04;

                        rigidBody.transform.position = Vector3.Slerp(
                            transform.position, 
                            finalTarget, 
                            10f * Time.fixedDeltaTime
                        );
                        rigidBody.transform.rotation = Quaternion.Slerp(
                            rigidBody.transform.rotation,
                            Quaternion.LookRotation(trackedController.transform.forward * -1, snapPoint.up),
                            15f * Time.fixedDeltaTime
                        );

                        Debug.Log("snapping!" + finalTarget + " -> " + device.transform.pos);
                        Debug.DrawLine(transform.TransformPoint(snapPoint.localPosition), positionTarget, Color.green);
                    }

                    // Attach
                    if (createJoint)
                    {
                        // Re enable colliders
                        foreach (var collider in GetComponents<Collider>()) collider.enabled = true;

                        // Disable colisions that would push this object and break the joint
                        foreach (Collider collider in trackedController.GetComponents<Collider>())
                        {
                            if (collider.isTrigger) continue;
                            foreach (Collider objectCollider in GetComponents<Collider>())
                            {
                                Physics.IgnoreCollision(collider, objectCollider, true);
                            }
                        }

                        var joint = gameObject.AddComponent<ConfigurableJoint>();
                        joint.xMotion = ConfigurableJointMotion.Locked;
                        joint.yMotion = ConfigurableJointMotion.Locked;
                        joint.zMotion = ConfigurableJointMotion.Locked;
                        joint.angularXMotion = ConfigurableJointMotion.Locked;
                        joint.angularYMotion = ConfigurableJointMotion.Locked;
                        joint.angularZMotion = ConfigurableJointMotion.Locked;
                        joint.connectedBody = trackedController.GetComponent<Rigidbody>();
                        joint.breakForce = breakForce;

                        rigidBody.velocity = new Vector3();
                        rigidBody.angularVelocity = new Vector3();

                        currentJoint = joint;
                    }
                } else {
                    // Disable gravity if it's being pulled
                    rigidBody.useGravity = false;

                    /**
                     * Move objects according to selected option
                     * */
                    if (moveBy == MoveBy.ApplyingForce)
                    {
                        var wCollisionOffset = transform.TransformPoint(collisionOffset);
                        rigidBody.AddForceAtPosition((positionTarget - wCollisionOffset).normalized / (rigidBody.mass * .01f), wCollisionOffset);
                        rigidBody.maxAngularVelocity = 2f;
                        //Debug.DrawRay(wCollisionOffset, (positionTarget - wCollisionOffset) / rigidBody.mass *10, Color.cyan);
                    }
                    else if (moveBy == MoveBy.ChangingVelocity)
                    {
                        var wCollisionOffset = transform.TransformPoint(collisionOffset);
                        rigidBody.velocity = (positionTarget - wCollisionOffset) / (rigidBody.mass * 0.1f);
                        rigidBody.maxAngularVelocity = 2f;

                        // Throttle velocity when object is too light
                        if (rigidBody.velocity.sqrMagnitude > Mathf.Pow(MaxVelocity, 2))
                        {
                            rigidBody.velocity = rigidBody.velocity.normalized * MaxVelocity;
                        }
                        //Debug.DrawRay(wCollisionOffset, (positionTarget - wCollisionOffset) / rigidBody.mass *10, Color.cyan);
                    }
                    else if (moveBy == MoveBy.Slerp)
                    {
                        var interpolationAmount = 0.005f; // 10f / rigidBody.mass * Time.fixedDeltaTime;
                        rigidBody.velocity *= Time.fixedDeltaTime; // Decrease physics velocity because we don't want other objecs interfearing with our pull
                        rigidBody.transform.position = Vector3.Slerp(rigidBody.transform.position, positionTarget, interpolationAmount);
                        rigidBody.transform.rotation = trackedController.transform.rotation * rotationOffset;
                    }
                }
            }

            if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                EndInteraction(trackedController);
            }
        }

        public new void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            if (trackedController != null && collision.rigidbody == trackedController.GetComponent<Rigidbody>())
                collidingWithController = true;
        }

        public void OnCollisionExit(Collision collision)
        {
            if (trackedController != null && collision.rigidbody == trackedController.GetComponent<Rigidbody>() && collision.collider.enabled)
                collidingWithController = false;
        }

        public new void OnTriggerEnter(Collider collider)
        {
            base.OnTriggerEnter(collider);

            // Activation color
            if (trackedController == null)
                GetComponent<Renderer>().material.color = Color.blue;
        }

        public new void OnTriggerExit(Collider collider)
        {
            if (!collider.enabled) return;

            base.OnTriggerExit(collider);
            ResetOriginalMaterial();
        }

        void OnJointBreak(float breakForce)
        {
            currentJoint = null;
            if (trackedController != null)
                EndInteraction(trackedController);
        }

        /**
         * Pick up object when you press the trigger close to it
         * */
        public override void OnTriggerDownColliding(SteamVR_TrackedObject controller)
        {
            if (trackedController != null)
                EndInteraction(trackedController);

            // Try to acquire a lock for this controller
            if (!InputController.Instance.Lock(controller)) return; // Stop if someone already has it

            // Get positions
            collisionOffset = transform.InverseTransformPoint(GetComponent<Rigidbody>().ClosestPointOnBounds(controller.transform.position + controller.transform.forward * 0.1f));
            rotationOffset = Quaternion.Inverse(transform.rotation) * controller.transform.rotation;
            
            // Reference of the current interacting controller
            trackedController = controller;

            // How long until we can create a joint?
            // This enables us to pick up the object right after 
            // dropping it
            if (droppedAt + 1f > Time.time)
            {
                interactionTime = 0;
            } else
            {
                interactionTime = Time.time; // Grab it now!
            }   

            // Reset to original color
            ResetOriginalMaterial();
        }

        /**
         * Disable hover state
         * */
        void ResetOriginalMaterial()
        {
            var hoverControllers = currentControllers.Count;
            if (trackedController != null && currentControllers.Contains(trackedController))
                hoverControllers--;

            if (hoverControllers == 0)
            {
                // Reset color
                GetComponent<Renderer>().material.color = mainColor;
            }
        }

        /**
         * Release object and restore initial state
         * */
        public override void EndInteraction(SteamVR_TrackedObject controller)
        {
            if (trackedController == null || trackedController != controller) return;

            // Unlock controller
            InputController.Instance.Release(controller);
            // Remove it manually from the list because a collider might be disabled
            if (currentControllers.Contains(trackedController))
                currentControllers.Remove(trackedController);

            // Re-enable colisions that would push this object
            foreach (Collider collider in controller.GetComponents<Collider>())
            {
                if (collider.isTrigger) continue;
                foreach (Collider objectCollider in GetComponents<Collider>())
                {
                    Physics.IgnoreCollision(collider, objectCollider, false);
                }
            }

            var device = SteamVR_Controller.Input((int)trackedController.index);

            // Set velocity of the object when releasing based on controller velocity
            var rigidbody = GetComponent<Rigidbody>();
            var origin = trackedController.origin ? trackedController.origin : trackedController.transform.parent;
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
            rigidbody.useGravity = true;
            rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;

            // Destroy joint
            if (currentJoint != null)
            {
                currentJoint.connectedBody.useGravity = true;
                DestroyImmediate(currentJoint);
                currentJoint = null;
                // Pick it up if within a time th
                droppedAt = Time.time;
            }

            // Reset gauge
            var controllerGauge = trackedController.GetComponent<IForceGauge>();
            if (controllerGauge != null) controllerGauge.ForceValue = 0;
            var lineRenderer = trackedController.GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
            Debug.Log("Line renderer disabled");

            // Reset colliders - snap
            foreach (var collider in GetComponents<Collider>()) collider.enabled = true;

            currentWork = 0;

            // Reset controller properties
            StartCoroutine(RestoreController(trackedController));

            // No controller interacting anymore
            trackedController = null;
            collidingWithController = false;
        }

        /**
         * Reset controller after a couple seconds after releasing an object.
         * We have to wait otherwise colliders would push each other in unnatural ways
         * */
        private IEnumerator RestoreController(SteamVR_TrackedObject trackedController)
        {
            foreach (var renderer in trackedController.GetComponentsInChildren<MeshRenderer>())
                renderer.enabled = true;
            yield return new WaitForSeconds(0.15f);
            foreach (Collider collider in trackedController.GetComponents<Collider>())
                collider.enabled = true;
        }
    }
}