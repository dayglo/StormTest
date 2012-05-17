#pragma strict
private var ouchText : GameObject;
var currentscore : int;

function Start (){

	ouchText = GameObject.Find("CarlOuchText");

}

function OnTriggerEnter (other : Collider) {

	//if (other.tag.ToString() != "LeftArm" || other.tag.ToString() != "LeftForeArm")

	switch(other.tag) {
		case "Environment":
			Debug.Log("died " + other.name);
			SendMessageUpwards ("KillPlayer");
		break;
		
		case "Butterflies":
			//currentscore = System.Convert.ToInt32(ouchText.guiText.text);
			//currentscore += 1;
			//ouchText.guiText.text =  currentscore.ToString();
		break;
		
		default:
		break;
	}
}