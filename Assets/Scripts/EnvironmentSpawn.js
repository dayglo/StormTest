#pragma strict

// Object which created obstacles will be parented to
public var pathObject : Transform;
public var butterflyPathObject : Transform;
// Object tpo spawn for butterflies
public var butterflyPrefab : Transform;
// Spawn frequency for objects
public var framesBetweenSpawns : int = 200;
// Array of jump over game objects
public var jumpObstacles : Transform [];
// Array of slide under game objects
public var slideObstacles : Transform [];
// Butterfly spawn heights
public var butterflyMinHeight : float = 0.0;
public var butterflyMaxHeight : float = 2.0;
public var framesBetweenButterflySpawns : int = 400;
public var framesBetweenButterflies : int = 15;
public var butterflySeqMin : int = 3;
public var butterflySeqMax : int = 10;
// Countdown for respawning environment
private var frameCountdown : int;
// Countdown for respawning butterflies
private var butterflySpawnCountdown : int;		// Countdown to next spawn of butterfly row
private var butterfliesLeft : int;				// How many butterflies are left in current sequence
private var butterflyGapLeft : int;				// How many frames are left until the next butterfly in the sequence
private var butterflyHeight : float;			// Current height for spawned butterflies

function Start () {
	// Initialise frame countdown for object spawns
	frameCountdown = framesBetweenSpawns;
	// Initialise butterfly spawn settings
	butterflySpawnCountdown=framesBetweenButterflySpawns;
	butterfliesLeft = Random.Range(butterflySeqMin, butterflySeqMax);
	butterflyHeight = Random.Range(butterflyMinHeight, butterflyMaxHeight);
	butterflyGapLeft = framesBetweenButterflies;
}

function Update () {

}

// Spawn objects in Fixed Update so we can control the exact frequency
function FixedUpdate() {
	// Check if we have hit zero for the frame countdown for Environment objects
	if(!frameCountdown) {
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
	// Check if we need to generate butterflies
	if(!butterflySpawnCountdown) {
		// Currently drawing butterflies and counting down
		if(butterfliesLeft) {
			if(!butterflyGapLeft) {
				// We have counted down sequence to zero, so draw another one
				// Create a position vector for it
				var v : Vector3 = transform.position;
				v.y = v.y + butterflyHeight;
				// Spawn a new butterfly at that position
				// Instantiate it where we are currently located
				var butterfly : Transform = Instantiate(butterflyPrefab, v, transform.rotation);
				// Attach it to the path object
				butterfly.parent = butterflyPathObject;
				// Reduce butterflies left count
				butterfliesLeft--;
				if(butterfliesLeft) {
					// Still some left so restart gap counter
					butterflyGapLeft=framesBetweenButterflies;
				} else {
					// Else end of sequence, so start new spawn count for gap
					butterflySpawnCountdown=framesBetweenButterflySpawns;
					butterfliesLeft = Random.Range(butterflySeqMin, butterflySeqMax);
					butterflyHeight = Random.Range(butterflyMinHeight, butterflyMaxHeight);
				}
			} else {
				// Reduce butterfly gap countdown
				butterflyGapLeft--;
			}
		}
	} else {
		butterflySpawnCountdown--;
	} 	
}
