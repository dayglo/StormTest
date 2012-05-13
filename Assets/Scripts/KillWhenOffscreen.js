#pragma strict
var parent2 : int = 0;

 var hasAppeared : int = 0;

function OnBecameVisible() {
	hasAppeared = 1;
}

function OnBecameInvisible() {
	// Only kill if we have actually been made visible
	if(hasAppeared) {
		if (parent2 == 0) {
			Destroy(transform.gameObject);
		} else if (parent2 == 1) {
			Destroy(transform.parent.gameObject);
		} else if (parent2 == 2) {
			Destroy(transform.parent.parent.gameObject);
		}
	
	}
}