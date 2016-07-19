using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using Valve.VR;
using System.Collections;

namespace VRToolkit
{
    public class SliderGauge : MonoBehaviour, IForceGauge
    {

        private Slider slider;
        private GameObject model;

        public float ForceValue
        {
            set
            {
                slider.value = value;
                model.GetComponent<Canvas>().enabled = value > 0;
            }
            get { return slider.value; }
        }

        void Update()
        {
            if (model != null)
            {
                model.transform.LookAt(SteamVR_Render.Top().transform.position);
                model.transform.Rotate(0, 180, 0);
            }
        }

        // Use this for initialization
        void Start()
        {
            model = (GameObject)Instantiate(Resources.Load("Gradient Bar Slider"));
            model.transform.position = gameObject.transform.position + new Vector3(0, 0.1f, 0.1f);
            model.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(90f, 0, 0);
            model.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            model.transform.SetParent(gameObject.transform);

            slider = model.gameObject.GetComponentInChildren<Slider>();
            ForceValue = 0;
        }
    }
}