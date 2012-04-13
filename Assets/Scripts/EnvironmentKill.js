#pragma strict

function Start () {

}

function Update () {

}

function OnTriggerEnter (theCollision : Collider) {
	if (theCollision.gameObject.tag == "Environment") {
		// Need to destroy object as it hits the kill zone
		Destroy(theCollision.gameObject);
		Debug.Log("Killed environment object");
 	}
}