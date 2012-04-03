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

function FixedUpdate () {

	gravityVector = blackHole.position - myTransform.position;
	Debug.DrawRay(myTransform.position,gravityVector.right * 20,Color.red);
	Debug.DrawRay(myTransform.position,gravityVector.up * 20,Color.green);
	Debug.DrawRay(myTransform.position,gravityVector.forward * 20,Color.blue);
	
	force.force = gravityVector.normalized * gravityStrength;
	myTransform.up = -gravityVector;
	//myTransform.rotation.eulerAngles.y = 90;
	myTransform.Rotate(myTransform.up,90, Space.Self);

}