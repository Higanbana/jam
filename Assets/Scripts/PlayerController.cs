using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Awake ()
	{
		spriteRenderer = GetComponent<SpriteRenderer> ();
		spriteRenderer.color = Color.black;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SwapColor ()
	{
		if (spriteRenderer.color == Color.black)
		{
			spriteRenderer.color = Color.white;
		} else {
			spriteRenderer.color = Color.black;
		}
	}
}
