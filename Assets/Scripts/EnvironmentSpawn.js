#pragma strict

// Object which created obstacles will be parented to
public var pathObject : Transform;
public var butterflyPathObject : Transform;
// Object tpo spawn for butterflies
public var butterflyPrefab : Transform[];
// Spawn frequency for objects
public var framesBetweenSpawns : int = 200;
// Bird Height Offset
public var birdHeightOffset : float = 0.7f;
// Array of jump over game objects
public var jumpObstacles : Obstacle [];
// Array of slide under game objects
public var slideObstacles : Obstacle [];
// Butterfly spawn heights
public var butterflyMinHeight : float = 0.0;
public var butterflyMaxHeight : float = 2.7;
public var framesBetweenButterflySpawns : int = 100;
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
private var localrot : Vector4;

// Class for each Obstacle to spawn
class Obstacle
{
	var prefab : Transform;					// Object to spawn
	var rotateY : float = 270.0f;			// Rotation around y axis
	var animMove : String = "Walk";			// Name of normal anim		
	var animDie : String = "Death";			// Name of normal anim		
}
	
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
		var o : Obstacle;
		var s: Transform;
		var obstacle : Transform;
		if(r) {
			// 1, so jump object.  Pick a random object from the list
			p = Random.Range(0, jumpObstacles.length);
			// Get object to spawn
			o = jumpObstacles[p];
			// Instantiate it where we are currently located
			obstacle = Instantiate(o.prefab, transform.position, transform.rotation);
			// Attach it to the path object
			obstacle.parent = pathObject;
			// Now rotate it to path right direction
			obstacle.Rotate(Vector3(0, o.rotateY, 0));
			// Play walk animation
			obstacle.animation.Play(o.animMove);
		} else {
			// 1, so slide object.  Pick a random object from the list
			p = Random.Range(0, slideObstacles.length);
			// Get object to spawn
			o = slideObstacles[p];
			// Instantiate it where we are currently located plus rotation
			obstacle = Instantiate(o.prefab, transform.position, transform.rotation);
			// Attach it to the path object
			obstacle.parent = pathObject;
			// Now rotate it to path right direction
			obstacle.Rotate(Vector3(0, o.rotateY, 0));
			// Check if it is a bird
			var sOff : int = o.prefab.name.IndexOf("Raven");
			if(sOff > 1) {
				// Raven so apply height offset
				obstacle.Translate(Vector3(0, birdHeightOffset, 0));
				Debug.Log("Offset Height");
			}
			// Play walk animation
			obstacle.animation.Play(o.animMove);
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
				
				//select a butterfly
				//currently random
				
				// Spawn a new butterfly at that position
				// Instantiate it where we are currently located
				var butterfly : Transform = Instantiate(butterflyPrefab[Random.Range(0,butterflyPrefab.length)], v, transform.rotation);
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
