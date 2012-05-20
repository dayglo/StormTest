#pragma strict
public var jumpOverScore : int = 10;
function Start () {

}

function Update () {

}

function OnTriggerEnter (theCollision : Collider) {
	Debug.Log(String.Format("Object {0} hit object {1}", gameObject.name, theCollision.gameObject.name));
	// Need to destroy object as it hits the kill zone
	var env : EnvironmentCollider = theCollision.gameObject.GetComponent("EnvironmentCollider") as EnvironmentCollider;
	env.KillSelf();
	scoring.addscore(jumpOverScore);
}