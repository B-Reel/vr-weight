using UnityEngine;
using System.Collections;

namespace VRToolkit
{
    public class SphereGauge : MonoBehaviour, IForceGauge
    {

        private GameObject model;
        public float ForceValue
        {
            set
            {
                var renderer = model.GetComponent<Renderer>();
                renderer.enabled = value > 0;
                renderer.material.color = new Color(
                    renderer.material.color.r,
                    renderer.material.color.g,
                    renderer.material.color.b,
                    value
                );
            }
            get
            {
                return model.GetComponent<Renderer>().material.color.a;
            }
        }

        // Use this for initialization
        void Start()
        {
            model = (GameObject)Instantiate(Resources.Load("SphereGauge"));
            model.transform.position = gameObject.transform.position;
            model.transform.rotation = gameObject.transform.rotation;
            model.transform.parent = gameObject.transform;
            ForceValue = 0;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
