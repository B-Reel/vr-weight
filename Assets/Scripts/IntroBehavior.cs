using UnityEngine;
using System.Collections;

public class IntroBehavior : MonoBehaviour {
    public GameObject tables;
    public GameObject basketHoop;
    private float rotationFinal;
    private float rotationEased;

    private Vector3 position;
    private Quaternion rotation;
    private Vector3 rotationAngles;

    // Use this for initialization
    void Start () {
        tables.SetActive(false);
        basketHoop.SetActive(false);
        rotationFinal = Camera.main.transform.rotation.eulerAngles.y;
        rotationEased = rotationFinal;
    }
	
	// Update is called once per frame
	void Update () {
        Debug.Log(Camera.main.transform.position);

        position = transform.position;
        position.x = Camera.main.transform.position.x;
        position.z = Camera.main.transform.position.z;
        position.y = 1.0f;
        transform.position = position;


        rotationFinal = Camera.main.transform.rotation.eulerAngles.y;
        rotationEased = Mathf.LerpAngle(rotationEased, rotationFinal, Time.deltaTime * 4);

        rotation = transform.rotation;
        rotationAngles = rotation.eulerAngles;
        rotationAngles.y = rotationEased;
        rotationAngles.x = 0;
        rotationAngles.z = 0;
        rotation.eulerAngles = rotationAngles;
        transform.rotation = rotation;
    }
}
