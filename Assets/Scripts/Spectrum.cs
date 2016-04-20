using UnityEngine;
using System.Collections;

public class Spectrum : MonoBehaviour {

	public GameObject visualizer;
	public int numberOfVisualizers;
	public float distance;
	public GameObject[] visualizers;
	public int scale;
	public float init;

	void Awake() {
		Camera camera = Camera.main;
		init = - (numberOfVisualizers-1f)*distance/ 2f;
		for (int i = 0; i < numberOfVisualizers; i++) {
			Vector3 pos = new Vector3(init + i*distance, 0f, -1f);
			GameObject instance = (GameObject)Instantiate (visualizer, pos, Quaternion.identity);
			instance.transform.parent = transform;
		}
		visualizers = GameObject.FindGameObjectsWithTag ("Visualizer");
	}

	// Update is called once per frame
	void Update () {
		float[] spectrum = AudioListener.GetSpectrumData (1024, 0, FFTWindow.Hamming);
		for (int i = 0; i < numberOfVisualizers; i++) {
			Vector3 previousScale = visualizers [i].transform.localScale;
			previousScale.y = spectrum [i] * scale;
			visualizers [i].transform.localScale = previousScale;
		}
	}
}
