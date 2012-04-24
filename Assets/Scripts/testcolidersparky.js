#pragma strict

function OnTriggerEnter (other : Collider) {


//if (other.tag.ToString() != "LeftArm" || other.tag.ToString() != "LeftForeArm")

Debug.Log("***********Simon collider " + other.tag);

if (other.tag != "Player")
SendMessageUpwards ("KillPlayer");


}