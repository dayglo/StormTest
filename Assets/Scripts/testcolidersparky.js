#pragma strict
private var ouchText : GameObject;
var currentscore : int;

function Start (){

	ouchText = GameObject.Find("CarlOuchText");

}

function OnTriggerEnter (other : Collider) {

	//if (other.tag.ToString() != "LeftArm" || other.tag.ToString() != "LeftForeArm")

	Debug.Log("***********Simon collider " + other.tag);

	switch(other.tag) {
		case "Environment":
			SendMessageUpwards ("KillPlayer");
		break;
		
		case "Butterflies":
			currentscore = System.Convert.ToInt32(ouchText.guiText.text);
			currentscore += 1;
			ouchText.guiText.text =  currentscore.ToString();
		break;
		
		default:
		break;
	}
}