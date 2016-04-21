using UnityEngine;
using System.Collections;

public class Spectrum : MonoBehaviour {

	public GameObject visualizer;
	public int numberOfVisualizers;
	public float distance;
	public int scale;

    private GameObject[] visualizers;
    private float[] spectrum;

    void Awake()
    {
        spectrum = new float[1024];

		float init = (1f - numberOfVisualizers) * distance / 2f;
		for (int i = 0; i < numberOfVisualizers; i++)
        {
			Vector3 pos = new Vector3(init + i * distance, 0f, -9f);
			GameObject instance = (GameObject) Instantiate (visualizer, pos, Quaternion.identity);
			instance.transform.parent = transform;
		}
		visualizers = GameObject.FindGameObjectsWithTag ("Visualizer");
	}

	void Update ()
    {
        AudioListener.GetSpectrumData (spectrum, 0, FFTWindow.Hamming);

        for (int i = 0; i < numberOfVisualizers; i++)
        {
			Vector3 previousScale = visualizers [i].transform.localScale;
			previousScale.y = spectrum [i] * scale;
			visualizers [i].transform.localScale = previousScale;
		}
	}
}
