using UnityEngine;
using System.Collections;

/// <summary>
/// 2011-09-15
/// ULTIMATE RAGDOLL GENERATOR V1.0
/// © THE ARC GAMES STUDIO 2011
/// DESIGNED WITH UNITY 3.4.0f5
/// 
/// helper class to trigger the ragdoll function in a gameobject with a ragdollifier attached
/// </summary>
public class clsragdollhelper : MonoBehaviour {
	private bool varcanragdoll = false;
	private clsragdollify varlocalragdollifier;
	
	void Start () {
		animation.wrapMode = WrapMode.Loop;
		varlocalragdollifier = GetComponent<clsragdollify>();
		if (varlocalragdollifier != null) {
			if (varlocalragdollifier.vargamragdoll != null)
				varcanragdoll = true;
		}
	}
	
	void OnGUI() {
		if (varcanragdoll) {
			if(GUILayout.Button("Go ragdoll")) {
				varlocalragdollifier.metgoragdoll();
				Destroy(gameObject);
			}
		}
	}
	
}
