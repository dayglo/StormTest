#pragma strict

// Object which created obstacles will be parented to
public var pathObject : Transform;
// Spawn frequency for objects
public var framesBetweenSpawns : int = 200;
// Array of jump over game objects
public var jumpObstacles : Transform [];
// Array of slide under game objects
public var slideObstacles : Transform [];

// Countdown for respawning
private var frameCountdown : int;

function Start () {
	// Initialise frame countdown for object spawns
	frameCountdown = framesBetweenSpawns;
}

function Update () {

}

// Spawn objects in Fixed Update so we can control the exact frequency
function FixedUpdate() {
	// Check if we have hit zero for the frame countdown
	if(!frameCountdown) {
		Debug.Log("Spawning object");
		// Spawn a new object, first pick if it is a jump or a slide object
		var r : int = Random.Range(0, 2);
		var p: int;
		var s: Transform;
		var obstacle : Transform;
		if(r) {
			// 1, so jump object.  Pick a random object from the list
			p = Random.Range(0, jumpObstacles.length);
			// Get object to spawn
			s = jumpObstacles[p];
			// Instantiate it where we are currently located
			obstacle = Instantiate(s, transform.position, transform.rotation);
			// Attach it to the path object
			obstacle.parent = pathObject;
		} else {
			// 1, so slide object.  Pick a random object from the list
			p = Random.Range(0, slideObstacles.length);
			// Get object to spawn
			s = slideObstacles[p];
			// Instantiate it where we are currently located
			obstacle = Instantiate(s, transform.position, transform.rotation);
			// Attach it to the path object
			obstacle.parent = pathObject;
		}		
		frameCountdown = framesBetweenSpawns;
	} else {
		frameCountdown = frameCountdown - 1;
	}
}
