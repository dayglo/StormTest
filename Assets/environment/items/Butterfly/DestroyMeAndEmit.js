#pragma strict
var pT : Transform;
var p : ParticleSystem;
public var collectScore : int;


function Start () {

	p = pT.gameObject.GetComponent(ParticleSystem);

}

function Update () {

}

function OnTriggerEnter (theCollision : Collider) {
	if (theCollision.gameObject.tag == "Player")  {
		scoring.addscore(collectScore);
		iTween.ScaleTo(gameObject,Vector3.zero,1);
		p.Play();
		yield WaitForSeconds(1);
		Debug.Log("Destroyed object");
		Destroy(gameObject);
 	}
 	if (theCollision.gameObject.tag == "Gecko")  {
 		scoring.addscore(10);
		iTween.ScaleTo(gameObject,Vector3.zero,1);
		p.Play();
		theCollision.gameObject.animation.Play("modo_Anim");
		// Make Gecko stick out his tongue
		yield WaitForSeconds(1);
		Debug.Log("Hit the gecko");
		Destroy(gameObject);
 	}

}