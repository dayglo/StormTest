#pragma strict
 var Carl : GameObject;
var jumpForce : int = 4.0f;
var grounded : boolean;
private var sliding : boolean;
private var jumpCount : int;

private var ouchText : GameObject;

//collider, rigidbody, emitter collections
private var colliderComponents : Component[];
private var rigidbodyComponents : Component[];
//private var playerModelCrashEmitterComponents : Component[]; TODO: GC -import crashemitter component for flying dust on crash.

private var colliders : Collider[];
private var rigidbodies : Rigidbody[];
//private var playerModelCrashEmitters : CrashEmitter[];  TODO: GC -import crashemitter component for flying dust on crash.

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
        rigid.velocity = Vector3(1,1.4,0) * 2;
    }
        
    for (var comp1 : Component in colliderComponents) {
        var coll = comp1 as Collider;
        coll.isTrigger = false;
    }
    
    //for (var comp1 : Component in playerModelCrashEmitterComponents) { TODO: GC -import crashemitter component for flying dust on crash.
    //    var crashEmitter = comp1 as CrashEmitter;
    //    crashEmitter.switchedOn = true;
    //}
	
	worldRotator.masterSpeed = 0;
	
	StartCoroutine(WaitForDeadPlayerToSettle());

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
	//set the wrapmode of the different animations
	Carl.animation["run"].wrapMode =WrapMode.Loop;
	Carl.animation["walk"].wrapMode =WrapMode.Loop;
	//Carl.animation["run"].speed = 1.7;
	Carl.animation["run"].speed = 1.4;
	
//	Carl.animation["slidedownevent"].speed = 1.4;
//	Carl.animation["slidedown"].wrapMode = WrapMode.ClampForever;
//	Carl.animation["slideupevent"].speed = 1.4;
	
	Carl.animation.Play("run");
	
	ouchText = GameObject.Find("CarlOuchText");
	//ouchText.active = false;
}

function FixedUpdate() {
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
//	previousVelocity = myRigidbody.velocity;
}

function Update() {
	if(grounded) {
		CheckButtons();
	}
}

// Check for virtual buttons
function CheckButtons()
{
	for (var evt : Touch in Input.touches)
	{
		// Check we are at the bottom of the screen (push buttons)
		if(evt.position.y < Screen.height / 6) {
			if(evt.position.x > Screen.width/2) {	// Right.. so slide pressed
				if (evt.phase == TouchPhase.Began)
				{
					SlideDown();
				} else if(evt.phase == TouchPhase.Ended) {
					SlideUp();
				}
			} else {	// Left.. so jump pressed
				if(evt.phase == TouchPhase.Began) {
					// Fire pressed
					Jump();
				}
			}
		}
	}
	// Check for keyboard / mouse buttons instead
	if(Application.platform != RuntimePlatform.IPhonePlayer) {
		if(Input.GetButtonDown("Fire1")) Jump();
		if(Input.GetButtonDown("Fire2")) SlideDown();
		if(Input.GetButtonUp("Fire2")) SlideUp();
	}
}

function Jump() {
	Carl.animation["runold"].speed = 0.3;
	Carl.animation.Play("jump");
	Carl.animation.CrossFade("jump");
	rigidbody.AddRelativeForce(transform.up * jumpForce,ForceMode.Impulse);
	//Carl.animation.CrossFadeQueued("run",0.3,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");	
}

function Jump2() {
	Carl.animation.Play("jump");
	//Carl.animation.CrossFade("jump");
	rigidbody.AddRelativeForce(transform.up * jumpForce,ForceMode.Impulse);
	//Carl.animation.CrossFadeQueued("run",0.3,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");	
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
	
	Carl.animation.CrossFade("slidedown");
	sliding=true;
	//Carl.animation.CrossFade("swat");
	//Carl.animation.CrossFadeQueued("slideup",1.0,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");

	
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
    // subscribe to the global tap event
  // FingerGestures.OnFingerDown += MyFingerGestures_OnFingerDown;
   //     FingerGestures.OnFingerUp += FingerGestures_OnFingerUp; 
   //     FingerGestures.OnFingerStationaryBegin += FingerGestures_OnFingerStationaryBegin;
   //     FingerGestures.OnFingerStationary += FingerGestures_OnFingerStationary;
   //     FingerGestures.OnFingerStationaryEnd += FingerGestures_OnFingerStationaryEnd;
        
    /* -- Disabled to use tap events instead (SE)    
    FingerGestures.OnTap += MyFingerGestures_OnTap;
     FingerGestures.OnDoubleTap += MyFingerGestures_OnDoubleTap;
      FingerGestures.OnSwipe += MyFingerGestures_OnSwipe;
     FingerGestures.OnFingerSwipe += MyFingerGestures_OnFingerSwipe;
     */
     // FingerGestures.OnSwipeDirection += MyFingerGestures_OnSwipe;

    
}
 
function OnDisable()
{
    // unsubscribe from the global tap event
    /* -- Disabled to use tap events instead
   FingerGestures.OnTap -= MyFingerGestures_OnTap;
 FingerGestures.OnDoubleTap -= MyFingerGestures_OnDoubleTap;
        FingerGestures.OnSwipe -= MyFingerGestures_OnSwipe;
         FingerGestures.OnFingerSwipe -= MyFingerGestures_OnFingerSwipe;
        */ 
        
       // FingerGestures.OnFingerStationary -= MyFingerGestures_OnFingerStationary;
    //  FingerGestures.OnFingerStationaryEnd -= MyFingerGestures_OnFingerStationaryEnd;
        // FingerGestures.OnLongPress -= MyFingerGestures_OnLongPress;
}
 
// Our tap event handler. The method name can be whatever you want.


/*
function MyFingerGestures_OnTap( fingerPos : Vector2 )
{
   // Debug.Log( "TAP detected at " + fingerPos );
for (var scp : Collider in colliderComponents) {

Debug.Log("scp " + scp.name );
}

}


function MyFingerGestures_OnDoubleTap( fingerPos : Vector2 )
{
  // Debug.Log( " Double TAP  at " + fingerPos );
  // Punch();
  //BigJump();
  
    
}

function MyFingerGestures_OnSwipe( fingerPos : Vector2 )
{
  // Debug.Log( " Double TAP  at " + fingerPos );
  // Punch();
 // SlideDown();
    
}

function MyFingerGestures_OnFingerSwipe(fingerIndex : int,startPos : Vector2,  direction : FingerGestures.SwipeDirection,  velocity : float)
{

Debug.Log("direction " + direction.ToString());
if (grounded) {
 if (direction.ToString() == "Down") {
 	
 	SlideDown();
 	}
 	else if (direction.ToString() == "Up") {
 		if (sliding) {
 			SlideUp();
 			}
 			else
 			{
 			
 			Jump();
 			}
 	}
 }

}
*/

/*

function OnCollisionEnter (theCollision : Collision) {
Debug.Log("scp");
	for (var contact : ContactPoint in theCollision.contacts) {
        print(contact.thisCollider.name + " hit " + contact.otherCollider.name + " " + contact.thisCollider.tag + " " + contact.otherCollider.tag);
        if(contact.thisCollider.tag=="Environment" || contact.otherCollider.tag=="Environment") {
			// Show or hide ouch text
			ouchText.active=true;
			KillPlayer();
		}
	}
	Debug.Log("OnCollisionEnter: End");
}

function OnCollisionExit (theCollision : Collision) {
	for (var contact : ContactPoint in theCollision.contacts) {
        print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
        if(contact.thisCollider.tag=="Environment" || contact.otherCollider.tag=="Environment") {
			// Show or hide ouch text
			ouchText.active=false;
		}
 	}
	Debug.Log("OnCollisionExit: End");
} 
*/