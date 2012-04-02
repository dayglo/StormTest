#pragma strict

var myRigidbody : Rigidbody;
var force : ConstantForce;
var strength : float = 3;

function Start () {
	
	myRigidbody = rigidbody;
}

function FixedUpdate(){
	myRigidbody.AddRelativeForce(Vector3(0,0,strength),ForceMode.VelocityChange);

}