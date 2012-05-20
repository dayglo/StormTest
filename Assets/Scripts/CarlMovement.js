#pragma strict

import System.Collections;

 var Carl : GameObject;
var jumpForce : float = 6.5f;
var bigjumpThreshhold = 100.0f;
var grounded : boolean;
private var sliding : boolean;
private var jumpCount : int;
private var dragStart : Vector2;
// Path movement variables
public var pathSwapStep : float = 0.2f;
private var topSpawn : Transform;
private var bottomSpawn : Transform;
private var topPath : boolean = true;
private var changingPath : boolean = false;
var runSpeed : float = 1.4;

private var ouchText : GameObject;
//collider, rigidbody, emitter collections
private var colliderComponents : Component[];
private var rigidbodyComponents : Component[];
//private var playerModelCrashEmitterComponents : Component[]; TODO: GC -import crashemitter component for flying dust on crash.

private var colliders : Collider[];
private var rigidbodies : Rigidbody[];
//private var playerModelCrashEmitters : CrashEmitter[];  TODO: GC -import crashemitter component for flying dust on crash.
public var maxSlideTime : float = 2.0f;	// Maximum length of time sliding
private var slideTime : float = 2.0f;	// current slide time

var myRigidbody : Rigidbody;
var previousVelocity: Vector3;
var rootOfPhysicsHierarchy : Transform;

var worldRotator : WorldRotator;

function Awake() {
	Carl = GameObject.FindGameObjectWithTag("Player");
	myRigidbody = rigidbody;

	
	rootOfPhysicsHierarchy = GameObject.FindGameObjectWithTag("PlayerPhysicsRoot").transform;
	rigidbodyComponents = rootOfPhysicsHierarchy.GetComponentsInChildren (Rigidbody);
    colliderComponents = rootOfPhysicsHierarchy.GetComponentsInChildren (Collider);
    //playerModelCrashEmitterComponents = Carl.transform.GetComponentsInChildren (CrashEmitter); - TODO: GC -import crashemitter component for flying dust on crash.
    
    worldRotator = GameObject.FindGameObjectWithTag("WorldRotator").GetComponent("WorldRotator") as WorldRotator;
	
	
}

function KillPlayer () {
	
	
     Debug.Log("ANIMATION: Disabling anim due to death");
     Carl.animation.enabled = false;

	
	//disable the main collider
	myRigidbody.isKinematic = true;
    myRigidbody.detectCollisions = false;
    gameObject.collider.isTrigger = true;

	//enable the ragdoll
	for (var comp1 : Component in rigidbodyComponents) {
        var rigid = comp1 as Rigidbody;
        rigid.isKinematic = false;
        rigid.velocity = Vector3(1,1.4,0) * (worldRotator.masterSpeed /1.8);
    }
        
    for (var comp1 : Component in colliderComponents) {
        var coll = comp1 as Collider;
        coll.isTrigger = false;
    }
 

	iTween.ValueTo(gameObject,{"from":worldRotator.masterSpeed,"time":worldRotator.masterSpeed /2.2,"to":0,"onupdate":"updateFromValue","easetype":iTween.EaseType.easeOutQuart}	);

	StartCoroutine(WaitForDeadPlayerToSettle());

}


function updateFromValue(newValue : float){
	
	worldRotator.masterSpeed = newValue;

}



function WaitForDeadPlayerToSettle(){

 Debug.Log("Waiting for dead player to settle");

 //while (rootOfPhysicsHierarchy.rigidbody.velocity.magnitude > 0.1) { 
     yield WaitForSeconds (2.0);
 //}
  
  Debug.Log("Dead player stopped moving, restarting level");
  yield WaitForSeconds (1);
  
  
  Application.LoadLevel(0);

}


function Start(){
	// Find top and bottom spawn objects
	topSpawn = GameObject.Find("Spawn Zone Top").transform;
	bottomSpawn = GameObject.Find("Spawn Zone Bottom").transform;
	//set the wrapmode of the different animations
	Carl.animation["run"].wrapMode =WrapMode.Loop;
	Carl.animation["walk"].wrapMode =WrapMode.Loop;
	//Carl.animation["run"].speed = 1.7;
	Carl.animation["run"].speed = runSpeed;
	
//	Carl.animation["slidedownevent"].speed = 1.4;
//	Carl.animation["slidedown"].wrapMode = WrapMode.ClampForever;
//	Carl.animation["slideupevent"].speed = 1.4;
	
	Carl.animation.Play("run");
	
	ouchText = GameObject.Find("CarlOuchText");
	//ouchText.active = false;
}

function FixedUpdate() {
	Carl.animation["run"].speed = runSpeed;

	// Add check to make sure we are falling before raycasting to prevent double jumps (SE)
	if(rigidbody.velocity.y <0.0) {
		Debug.DrawRay (transform.position, -transform.up * 0.05, Color.green);
		 if (Physics.Raycast(transform.position, -transform.up, 0.05)) {
			// Debug.Log("grounded");
		    grounded = true;
		    //reset our jump count since we hit the ground
		    jumpCount = 0;
		    if (!sliding) {
		    	Carl.animation.CrossFade("run");
		    }
	    	//Carl.animation.CrossFade("run");
	    } else {
		    grounded=false;
		   // Carl.animation.CrossFade("jump");
		   // Debug.Log("Not grounded");
	    }
	}
	// Check if we are changing path and have reached destination
	if(changingPath) {
		if(topPath) {	// currently top so moving to bottom
			if(transform.position.z <= bottomSpawn.position.z) {	// Reach destination
				rigidbody.velocity=Vector3(0.0, 0.0, 0.0);	// Stop moving
				changingPath=false;
				topPath=false;
				transform.position.z = bottomSpawn.position.z;
			} else transform.position.z -= pathSwapStep;
		} else {	// moving from bottom to top
			if(transform.position.z >= topSpawn.position.z) {	// Reach destination
				rigidbody.velocity=Vector3(0.0, 0.0, 0.0);	// Stop moving
				changingPath=false;
				topPath=true;
				transform.position.z = topSpawn.position.z;
			} else transform.position.z += pathSwapStep;
		}
	}
			
//	previousVelocity = myRigidbody.velocity;
}

function Update() {
	if(grounded) {
		CheckButtons();
		if(sliding) {
			slideTime -= Time.deltaTime;
			if(slideTime < 0.0f) {
				Debug.Log("--Slide up flag--");
				SlideUp();
			}
		}	
	}
}

// Check for virtual buttons
function CheckButtons()
{
/*
	for (var evt : Touch in Input.touches)
	{
		// Check we are at the bottom of the screen (push buttons)
		if(evt.position.y < Screen.height / 4) {
			if(evt.position.x > Screen.width/2) {	// Right.. so slide pressed
				if (evt.phase == TouchPhase.Began)
				{
					SlideDown();
				} else if(evt.phase == TouchPhase.Ended) {
					//SlideUp();
				}
			} else {	// Left.. so jump pressed
				if(evt.phase == TouchPhase.Began) {
					// Fire pressed
					Jump();
				}
			}
		}
	}
	*/
	// Check for keyboard / mouse buttons instead
	if(Application.platform != RuntimePlatform.IPhonePlayer) {
		if(Input.GetButtonDown("Fire1")) Jump();
		if(Input.GetButtonDown("Fire2")) SlideDown();
		//if(Input.GetButtonUp("Fire2")) SlideUp();
		var move : float = Input.GetAxis("Horizontal");
		if(move == -1) MoveBottomToTop();
		if(move == 1) MoveTopToBottom();
	}
}

function Jump() {
	grounded = false;
	Carl.animation["runold"].speed = 0.3;
	Carl.animation.Play("jump");
	Carl.animation.CrossFade("jump");
	//rigidbody.AddRelativeForce(transform.up * jumpForce,ForceMode.Impulse);
	rigidbody.velocity=Vector3(0.0, jumpForce, 0.0);
	//Carl.animation.CrossFadeQueued("run",0.3,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");	
	Debug.Log("-- Normal Jump");
}

function BigJump() {
	Carl.animation["runold"].speed = 0.3;
	Carl.animation.Play("jump");
	Carl.animation.CrossFade("jump");
	rigidbody.velocity=Vector3(0.0, jumpForce * 1.6, 0.0);
//	rigidbody.AddRelativeForce(transform.up * (jumpForce*2),ForceMode.Impulse);
	//Carl.animation.CrossFadeQueued("run",0.3,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");	
	Debug.Log("-- Big Jump");
}

function Run() {
	Carl.animation.CrossFade("runold");
	Carl.animation.Play("runold");
	Carl.animation["runold"].speed = 1.4;
	//Carl.animation["run"].speed = 1.7;
}

function ResumeRunning() {
	yield WaitForSeconds(0.7);
		//Carl.animation.CrossFadeQueued("run",0.1,QueueMode.CompleteOthers);
	sliding=false;
}


function Punch() {
	Carl.animation.CrossFade("Punch");
	Carl.animation.CrossFadeQueued("Walk",0.3,QueueMode.CompleteOthers);
}

function SlideUp() {
//	yield WaitForSeconds (1);
	Carl.animation.CrossFade("slideup");
	StartCoroutine(ResumeRunning());
	
	//Carl.animation.CrossFadeQueued("run",0.1,QueueMode.CompleteOthers);
	
	//sliding=false;
	//Carl.animation.CrossFade("swat");
	//Carl.animation.CrossFadeQueued("slideup",1.0,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");

	
}
function SlideDown() {
	sliding=true;
	Carl.animation.CrossFade("slidedown");
	slideTime = maxSlideTime;	// Set sliding time
	//Carl.animation.CrossFade("swat");
	//Carl.animation.CrossFadeQueued("slideup",1.0,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");

	
}

// Change path functions
function MoveTopToBottom() {
	// Ensure we aren't already moving and aren't already on the right path
	if(!changingPath && topPath) {
		Debug.Log("Move: TopToBottom");
		changingPath=true;
	}
}

function MoveBottomToTop() {
	// Ensure we aren't already moving and aren't already on the right path
	if(!changingPath && !topPath) {
		Debug.Log("Move: BottomToTop");
		changingPath=true;
	}
}

function Swat() {
	
	Carl.animation.Blend("swat",1.0f,0.1f);
	//Carl.animation.CrossFade("swat");
	//Carl.animation.CrossFadeQueued("run",0.3,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");

	
}
function OnEnable()
{
    FingerGestures.OnDragBegin += FingerGestures_OnDragBegin;
    FingerGestures.OnDragMove += FingerGestures_OnDragMove;
    FingerGestures.OnDragEnd += FingerGestures_OnDragEnd;
}
 
function OnDisable()
{
    FingerGestures.OnDragBegin -= FingerGestures_OnDragBegin;
    FingerGestures.OnDragMove -= FingerGestures_OnDragMove;
    FingerGestures.OnDragEnd -= FingerGestures_OnDragEnd;
}

// New drag event handlers
function FingerGestures_OnDragBegin( fingerPos : Vector2, startPos : Vector2 )
{
	dragStart = startPos;
}

function FingerGestures_OnDragMove( fingerPos : Vector2, delta : Vector2 )
{
	Debug.Log(String.Format("OnDragMove: offset x={0}, y={1}", delta.x, delta.y));
	// Check which direction we are moving in
	// Moving up?
	if(delta.y > (3 * Mathf.Abs(delta.x))) {	// moving up
		if(sliding) {
			SlideUp();
			return;
		}
		if(grounded) {
//			if(delta.y > bigjumpThreshhold) BigJump();
			Jump();
		}
		return;
	}
	// Moving down?
	if(delta.y < 0 && Mathf.Abs(delta.y) > (3 * Mathf.Abs(delta.x))) {
		if(!sliding && grounded) SlideDown();
	}
	// Moving Right or left?
	if(grounded && !sliding) {
		if(delta.x > (3 * Mathf.Abs(delta.y))) {	// Moving right (top to bottom)
			MoveTopToBottom();
		} else if(delta.x < 0 && Mathf.Abs(delta.x) > (3 * Mathf.Abs(delta.y))) {	// Moving Left to right (bottom to top)
			MoveBottomToTop();
		}
	}
}

function FingerGestures_OnDragEnd( fingerPos : Vector2 )
{

}
