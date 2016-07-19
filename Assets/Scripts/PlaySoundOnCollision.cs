using UnityEngine;
using System.Collections;

namespace VRToolkit
{
    public class PlaySoundOnCollision : MonoBehaviour
    {

        public AudioSource audioSource;
        private float velToVol = .05F;

        public void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        void OnCollisionEnter(Collision collision)
        {
            //// Do not play sounds when colliding with a controller
            //var trackedObject = collision.gameObject.GetComponent<SteamVR_TrackedObject>();
            //if (trackedObject != null || audioSource == null) return;

            float hitVol = Mathf.Min(1f, collision.relativeVelocity.magnitude * velToVol);

            audioSource.volume = hitVol;
            audioSource.Play();
        }
    }

}