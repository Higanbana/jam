using UnityEngine;
using System.Collections;

public class ShiftScreenController : MonoBehaviour {

    public RectTransform panel;
    private Camera mainCamera;

    void Awake ()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }
    
    public void Increment()
    {
        panel.position += Vector3.right * mainCamera.pixelWidth;
    }

    public void Decrement()
    {
        panel.position += Vector3.left * mainCamera.pixelWidth;
    }
}
