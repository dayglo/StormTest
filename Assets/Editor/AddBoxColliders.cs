using UnityEngine;
using UnityEditor;
using System.Collections;

public class AddBoxColliders : MonoBehaviour {

    [MenuItem("My Tools/Collider/Add Box Colliders")]
    static void FitToChildren() {
       foreach (GameObject rootGameObject in Selection.gameObjects) {
       		if (!(rootGameObject.collider is BoxCollider)) continue;
			
			// Add box colliders
			rootGameObject.AddComponent("BoxCollider");
		}
    }

}