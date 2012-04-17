#pragma strict

var objectsToRotate : Transform[];
var speeds: float[];
var i : int;

var masterSpeed : float;

function Update () {
	for (i=0;i<objectsToRotate.length;i++) {
		objectsToRotate[i].Rotate(Vector3.forward * speeds[i] * Time.deltaTime * masterSpeed);
	}
}