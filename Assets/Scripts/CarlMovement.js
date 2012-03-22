#pragma strict
private var Carl : GameObject;



function Awake() {
Carl = GameObject.FindGameObjectWithTag("Player");


}

function Start(){

//set the wrapmode of the different animations
Carl.animation["Run"].wrapMode =WrapMode.Loop;
Carl.animation["Walk"].wrapMode =WrapMode.Loop;
Carl.animation["Walk"].speed = 1.7;

Carl.animation.Play("Walk");


}


function Jump() {
	

	Carl.animation.CrossFade("Jump");
	Carl.animation.CrossFadeQueued("Walk",0.3,QueueMode.CompleteOthers);
	
	
}

function Punch() {
	

	Carl.animation.CrossFade("Punch");
	Carl.animation.CrossFadeQueued("Walk",0.3,QueueMode.CompleteOthers);
	
	
}


function OnEnable()
{
    // subscribe to the global tap event
    FingerGestures.OnTap += MyFingerGestures_OnTap;
     // FingerGestures.OnDoubleTap += MyFingerGestures_OnDoubleTap;
      FingerGestures.OnSwipe += MyFingerGestures_OnSwipe;
    
}
 
function OnDisable()
{
    // unsubscribe from the global tap event
    FingerGestures.OnTap -= MyFingerGestures_OnTap;
      // FingerGestures.OnDoubleTap -= MyFingerGestures_OnDoubleTap;
        FingerGestures.OnSwipe -= MyFingerGestures_OnSwipe;
}
 
// Our tap event handler. The method name can be whatever you want.

function MyFingerGestures_OnTap( fingerPos : Vector2 )
{
   // Debug.Log( "TAP detected at " + fingerPos );
   Jump();
    
}

function MyFingerGestures_OnSwipe( fingerPos : Vector2 )
{
  // Debug.Log( " Double TAP  at " + fingerPos );
   Punch();
    
}
