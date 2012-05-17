using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class Mesh2ColliderMenu {

static List<string> primitiveNames = new List<string>(new string[]{"box", "sphere", "capsule"});
static List<Transform> selection;

// Grey out the menu item if there are no non-skinned meshes in the selection,
// or if no mesh names contain the name of any primitive collider type.
[MenuItem("GameObject/Create Colliders from Meshes", true)] static bool ValidateMesh2Collider () {
	selection = new List<Transform>(Selection.GetTransforms(SelectionMode.Unfiltered | SelectionMode.ExcludePrefab));	
	return selection.Exists(transform => transform.GetComponent<MeshFilter>() 
		&& primitiveNames.Exists(primitiveName => transform.name.Contains(primitiveName))
	);
}

[MenuItem("GameObject/Create Colliders from Meshes")] static void Mesh2Collider () {
	Undo.RegisterSceneUndo("Create Colliders from Meshes");	
	selection.ForEach(transform => {
		MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
		if (!meshFilter) return;
		
		string name = meshFilter.name.ToLower();
		if (name.Contains("box")) ChangeMeshTo<BoxCollider>(meshFilter);
		else if (name.Contains("sphere")) ChangeMeshTo<SphereCollider>(meshFilter);
		else if (name.Contains("capsule")) ChangeMeshTo<CapsuleCollider>(meshFilter);
	});
}

static void ChangeMeshTo <T> (MeshFilter meshFilter) where T : Collider {
	T collider = meshFilter.GetComponent<T>();
	if (collider) Object.DestroyImmediate(collider);
	collider = meshFilter.gameObject.AddComponent<T>();
	
	// The capsule is not created as nicely as the other types of colliders are.
	// Instead of actually being a capsule, AddComponent(typeof(CapsuleCollider))
	// creates a sphere that fully encompases the mesh.
	// I suggested to Unity Technologies, on April 1, 2010, via the Bug Reporter,
	// that they implement automatic orientation, similar to what follows,
	// so a workaround like this can be avoided.
	if (typeof(T) == typeof(CapsuleCollider)) {
		CapsuleCollider capsuleCollider = collider as CapsuleCollider;
		Bounds meshBounds = meshFilter.sharedMesh.bounds;			
		for (int xyOrZ = 0; xyOrZ < 3; ++xyOrZ) 
		    if (meshBounds.size[xyOrZ] > capsuleCollider.height) {
		    	capsuleCollider.direction = xyOrZ;
		    	capsuleCollider.height = meshBounds.size[xyOrZ];
		    } else capsuleCollider.radius = meshBounds.extents[xyOrZ];
	}
	
	Object.DestroyImmediate(meshFilter.renderer);
	Object.DestroyImmediate(meshFilter);
}

}