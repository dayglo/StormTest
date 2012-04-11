#pragma strict

var leftwing : Transform;
var rightwing: Transform;
var flapspeed : float = 5;


function Start () {

}

function Update () {

	
	//rightwing.localRotation.eulerAngles.y = Mathf.PingPong(Time.time *flapspeed) * 90;
	//leftwing.localRotation.eulerAngles.y = Mathf.PingPong(Time.time *flapspeed) * -90;
	
	
	rightwing.localRotation.eulerAngles.y = Mathf.PingPong(180, 1) + 90;
	
	//float x = Mathf.PingPong(Time.time, 3);
   // float y = gameObject.transform.position.y;
    //float z = gameObject.transform.position.z;
    //gameObject.transform.position = new Vector3(x, y, z);

}