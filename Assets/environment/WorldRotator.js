#pragma strict

var objectsToRotate : Transform[];
var speeds: float[];
var i : int;

var increaseInSpeedEachSecond : float;

var masterSpeed : float;


function Start(){
	while (true){
		masterSpeed += increaseInSpeedEachSecond;
		yield WaitForSeconds(1);
	}
}

function Update () {
	for (i=0;i<objectsToRotate.length;i++) {
		objectsToRotate[i].Rotate(Vector3.forward * speeds[i] * Time.deltaTime * masterSpeed);
	}
	
	
	
}