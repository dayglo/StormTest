#pragma strict
private var Carl : GameObject;
var jumpForce : int = 4.0f;
private var grounded : boolean;
private var sliding : boolean;
private var jumpCount : int;



function Awake() {
Carl = GameObject.FindGameObjectWithTag("Player");


}

function Start(){

//set the wrapmode of the different animations
Carl.animation["run"].wrapMode =WrapMode.Loop;
Carl.animation["walk"].wrapMode =WrapMode.Loop;
//Carl.animation["run"].speed = 1.7;
Carl.animation["run"].speed = 1.4;

Carl.animation["slidedownevent"].speed = 1.4;
Carl.animation["slideupevent"].speed = 1.4;

Carl.animation.Play("run");


}

function Update() {
// Debug.DrawRay (transform.position, -transform.up, Color.green);
 if (!Physics.Raycast(transform.position, -transform.up, 2)) {
// Debug.Log("grounded");
            grounded = true;
            //reset our jump count since we hit the ground
            jumpCount = 0;
            if (!sliding) {
            	Carl.animation.CrossFade("run");
            }
            	//Carl.animation.CrossFade("run");
            }else {
            grounded=false;
           // Carl.animation.CrossFade("jump");
           // Debug.Log("Not grounded");
            }



}

function Jump() {
	
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
//Carl.animation["run"].speed = 1.7;

}


function BigJump() {
Carl.animation.CrossFade("jump");
Carl.animation.CrossFadeQueued("run",0.3,QueueMode.CompleteOthers);
//Carl.animation["run"].speed = 1.7;

}

/*
function Punch() {
	

	Carl.animation.CrossFade("Punch");
	Carl.animation.CrossFadeQueued("Walk",0.3,QueueMode.CompleteOthers);
	
	
}
*/

function ResumeRunning() {
//Carl.animation.CrossFadeQueued("run",0.1,QueueMode.CompleteOthers);
sliding=false;

}

function SlideUp() {
	yield WaitForSeconds (1);
	Carl.animation.CrossFade("slideupevent");
	
	//Carl.animation.CrossFadeQueued("run",0.1,QueueMode.CompleteOthers);
	
	//sliding=false;
	//Carl.animation.CrossFade("swat");
	//Carl.animation.CrossFadeQueued("slideup",1.0,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");

	
}
function SlideDown() {
	
	Carl.animation.CrossFade("slidedownevent");
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
        
        
    FingerGestures.OnTap += MyFingerGestures_OnTap;
     FingerGestures.OnDoubleTap += MyFingerGestures_OnDoubleTap;
      FingerGestures.OnSwipe += MyFingerGestures_OnSwipe;
     FingerGestures.OnFingerSwipe += MyFingerGestures_OnFingerSwipe;
     // FingerGestures.OnSwipeDirection += MyFingerGestures_OnSwipe;

    
}
 
function OnDisable()
{
    // unsubscribe from the global tap event
   FingerGestures.OnTap -= MyFingerGestures_OnTap;
 FingerGestures.OnDoubleTap -= MyFingerGestures_OnDoubleTap;
        FingerGestures.OnSwipe -= MyFingerGestures_OnSwipe;
         FingerGestures.OnFingerSwipe -= MyFingerGestures_OnFingerSwipe;
        
       // FingerGestures.OnFingerStationary -= MyFingerGestures_OnFingerStationary;
    //  FingerGestures.OnFingerStationaryEnd -= MyFingerGestures_OnFingerStationaryEnd;
        // FingerGestures.OnLongPress -= MyFingerGestures_OnLongPress;
}
 
// Our tap event handler. The method name can be whatever you want.



function MyFingerGestures_OnTap( fingerPos : Vector2 )
{
   // Debug.Log( "TAP detected at " + fingerPos );
   if (grounded) {
  // Jump();
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
