using UnityEngine;
using System.Collections;

public class CallbackExample : MonoBehaviour {

	void PlaySound(string volume) {
		Debug.Log("called back");
		GetComponent<AudioSource>().volume = float.Parse(volume);
		GetComponent<AudioSource>().Play();
	}
	
	void ChangeToYellow() {
		renderer.material.color = Color.yellow;	
	}
}