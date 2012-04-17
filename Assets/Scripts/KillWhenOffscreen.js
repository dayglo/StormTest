#pragma strict

private var hasAppeared : int = 0;

function OnBecameVisible() {
	hasAppeared = 1;
}

function OnBecameInvisible() {
	// Only kill if we have actually been made visible
	if(hasAppeared) {
		Destroy(transform.parent.gameObject);
	}
}