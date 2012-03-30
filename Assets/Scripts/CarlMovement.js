#pragma strict
private var Carl : GameObject;



function Awake() {
Carl = GameObject.FindGameObjectWithTag("Player");


}

function Start(){

//set the wrapmode of the different animations
Carl.animation["run"].wrapMode =WrapMode.Loop;
Carl.animation["walk"].wrapMode =WrapMode.Loop;
Carl.animation["walk"].speed = 1.7;
//Carl.animation["run"].speed = 1.7;

Carl.animation.Play("run");


}

function Update() {




}


function Jump() {
	

	Carl.animation.CrossFade("runandjump");
	Carl.animation.CrossFadeQueued("run",0.3,QueueMode.CompleteOthers);
	//Carl.animation["walk"].speed = 1.7;
	//Carl.animation.Play("walk");

	
}

function Run() {
Carl.animation.CrossFade("runold");
Carl.animation.Play("runold");
//Carl.animation["run"].speed = 1.7;

}


function BigJump() {
Carl.animation.CrossFade("bigjump");
Carl.animation.CrossFadeQueued("run",0.3,QueueMode.CompleteOthers);
//Carl.animation["run"].speed = 1.7;

}

/*
function Punch() {
	

	Carl.animation.CrossFade("Punch");
	Carl.animation.CrossFadeQueued("Walk",0.3,QueueMode.CompleteOthers);
	
	
}
*/


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
    FingerGestures.OnTap += MyFingerGestures_OnTap;
      FingerGestures.OnDoubleTap += MyFingerGestures_OnDoubleTap;
      FingerGestures.OnSwipe += MyFingerGestures_OnSwipe;
    
}
 
function OnDisable()
{
    // unsubscribe from the global tap event
    FingerGestures.OnTap -= MyFingerGestures_OnTap;
 FingerGestures.OnDoubleTap -= MyFingerGestures_OnDoubleTap;
        FingerGestures.OnSwipe -= MyFingerGestures_OnSwipe;
}
 
// Our tap event handler. The method name can be whatever you want.

function MyFingerGestures_OnTap( fingerPos : Vector2 )
{
   // Debug.Log( "TAP detected at " + fingerPos );
   Jump();
    
}


function MyFingerGestures_OnDoubleTap( fingerPos : Vector2 )
{
  // Debug.Log( " Double TAP  at " + fingerPos );
  // Punch();
  BigJump();
    
}

function MyFingerGestures_OnSwipe( fingerPos : Vector2 )
{
  // Debug.Log( " Double TAP  at " + fingerPos );
  // Punch();
  Swat();
    
}
