#pragma strict

var myTransform : Transform ;

var blackHole : Transform;
var gravityStrength : float = 9.81;
var gravityVector : Vector3;
var force : ConstantForce;

function Start () {
	myTransform = transform;
	force = gameObject.AddComponent("ConstantForce");

}

function Update () {

	gravityVector = blackHole.position - transform.position;
	force.force = gravityVector.normalized * gravityStrength;
	myTransform.up = -gravityVector;
	myTransform.Rotate(0,90,0);

}