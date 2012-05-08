#pragma strict
var pT : Transform;
var p : ParticleSystem;



function Start () {

	p = pT.gameObject.GetComponent(ParticleSystem);

}

function Update () {

}

function OnTriggerEnter (theCollision : Collider) {
	if (theCollision.gameObject.tag == "Player")  {
		// Need to destroy object as it hits the kill zone
		
		iTween.ScaleTo(gameObject,Vector3.zero,1);
		
		p.Play();
		yield WaitForSeconds(1);
		Debug.Log("Destroyed object");
		Destroy(gameObject);
 	}
}