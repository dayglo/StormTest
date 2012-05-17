using UnityEngine;
using System.Collections;

/// <summary>
/// 2011-09-15
/// ULTIMATE RAGDOLL GENERATOR V1.0
/// © THE ARC GAMES STUDIO 2011
/// DESIGNED WITH UNITY 3.4.0f5
/// 
/// simple class to add a constant forward force to the demo scene vehicles
/// </summary>
public class clscar : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//ugly direction tweak because of bad model orientation
		Vector3 varforward = -transform.up;
		constantForce.force = varforward*3000;
	}
}
