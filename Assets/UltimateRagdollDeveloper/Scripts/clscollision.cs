using UnityEngine;
using System.Collections;

/// <summary>
/// 2011-09-15
/// ULTIMATE RAGDOLL GENERATOR V1.0
/// © THE ARC GAMES STUDIO 2011
/// DESIGNED WITH UNITY 3.4.0f5
/// 
/// simple class to trigger ragdoll transition on a trigger
/// </summary>
public class clscollision : MonoBehaviour {
	public Transform vargampassenger;
	private clsragdollify varlocalragdollifier; 
	private bool varengaged = false; //fake semaphore to avoid multiple triggers

	void OnTriggerEnter() {
		if (vargampassenger != null) {
			varlocalragdollifier = vargampassenger.GetComponent<clsragdollify>();
			if (varlocalragdollifier != null) {
				if (!varengaged) {
					varlocalragdollifier.metgoragdoll();
					varengaged = true;
				}			
				Destroy(transform.root.constantForce,1);
				Destroy(vargampassenger.gameObject);
			}
		}
	}
}
