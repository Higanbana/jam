using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform rails;
    private Camera mainCamera;

    void Start ()
    {
        mainCamera = GetComponent<Camera> ();
    }
	
	void Update () {
        Vector3 position = transform.position;
        position.x = rails.transform.position.x + mainCamera.aspect * mainCamera.orthographicSize - 2.5f;
        transform.position = position;
	}
}
