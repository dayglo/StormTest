using UnityEngine;
using System.Collections;

/// <summary>
/// 2011-09-15
/// ULTIMATE RAGDOLL GENERATOR V1.0
/// © THE ARC GAMES STUDIO 2011
/// DESIGNED WITH UNITY 3.4.0f5
/// 
/// simple class for a top down view of a target
/// 
/// NOTE v1.8: the class is used as initializer for the "Fixed timestep" parameter of the
/// EDIT=> PROJECT SETTINGS=> TIME menu, that determines the frequency of physic updates for the game
/// It is suggested that the value be kept consistent throughout development to avoid
/// unexpected physic behavior
/// </summary>
public class clscameratarget : MonoBehaviour {
	public Transform vargamtarget = null;
	private Vector3 varpositionfixer = new Vector3();
	private Quaternion varrotationfixer = new Quaternion();
	
	void Awake() {
		//set the Fixed timestep to 100 calls
		Time.fixedDeltaTime = 0.01f;
	}
	
	void Update () {
		if (vargamtarget != null) {
			transform.LookAt(vargamtarget);
			varpositionfixer = transform.position;
			varpositionfixer.x = vargamtarget.position.x;
			varpositionfixer.z = vargamtarget.position.z;
			transform.position = varpositionfixer;
			varrotationfixer = transform.rotation;
			varrotationfixer.y = 0;
			varrotationfixer.z = 0;
			transform.rotation = varrotationfixer;
		}
		
	}
}
