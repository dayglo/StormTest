#pragma strict

var object : Transform;
var delay : float = 1.4;

function Start () {

	while (1) {
	
		Instantiate(object,Vector3.zero,Quaternion.identity);
	
		yield WaitForSeconds(delay);
	}

}

function Update () {

}