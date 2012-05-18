#pragma strict
// Script will send Kill player message when it hits an environment object

private var carlMovement : CarlMovement;

function Start()
{
	carlMovement = GameObject.FindGameObjectWithTag("Player").GetComponent(CarlMovement);
}

// Scan up tree to find parent that is Environment object, or heirarchy root
function FindTopParent() : Transform
{
	var p : Transform = transform.parent;
	while (p.name != "prop_origin" && p.tag != "Environment") {
		Debug.Log(p.name);
		p = p.parent;
	} ;
	if(p.name == "prop_origin") return null;
	return p;
}

function OnTriggerEnter (other : Collider) 
{
	if(other.gameObject.transform.root.tag == "Player") {
		carlMovement.KillPlayer();
		var top : Transform = FindTopParent();
		// Play death animation on self
		top.animation.Play("Death");
		Debug.Log("Environment Kill script");
	}
}
