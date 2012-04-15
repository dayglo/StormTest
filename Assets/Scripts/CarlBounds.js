#pragma strict
private var hips : SkinnedMeshRenderer;
private var box : BoxCollider;

function Start () {
	// Find and store the Hips subobject
	hips = transform.Find("Hips").GetComponent(SkinnedMeshRenderer);
	box = GetComponent(BoxCollider);
	box.size = hips.bounds.size;
	box.center = hips.bounds.center;}

function FixedUpdate () {
	// Update the bounding box of the collider to the bounds of the current animation frame

}