var maxSpeed : float = 0.001;

var targetVelocity: Vector3;
private var currentVelocity: Vector3 = Vector3.zero;
private var timeDelay : float = 1.0;
private var myTransform : Transform;

var maxDistance : float = 0.001;

var targetPosition :Vector3; 
private var vectorToTarget : Vector3; 

private var correction : float;


function Start () {
	myTransform = transform;
	targetPosition =  Vector3(-5,Random.Range(6.4,7.0),0);
	StartCoroutine("BuzzAround");
}
	
function Update() {
	
 	 	
 	Debug.DrawRay(myTransform.position,vectorToTarget,Color.cyan,Time.deltaTime); 
 	

	currentVelocity = (Vector3.Lerp(currentVelocity,targetVelocity,0.02));
	Debug.DrawRay(myTransform.position,currentVelocity * 100,Color.white,Time.deltaTime); 
	
	
	myTransform.Translate((currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude,0,maxSpeed)),Space.World);
	
	

}

function BuzzAround(){
	while (1) {
		timeDelay = Random.Range(0.4,1);  
		
		vectorToTarget = targetPosition - myTransform.position ; 
		correction = (1 / maxDistance) * Vector3.Distance(myTransform.position,targetPosition) ;
		
		targetVelocity = Random.onUnitSphere;
		Debug.DrawRay(myTransform.position,targetVelocity * 10,Color.black,timeDelay);
		
		targetVelocity = Vector3.RotateTowards(targetVelocity,vectorToTarget,correction,0.001);
		  
		Debug.DrawRay(myTransform.position,targetVelocity * 10,Color.blue,timeDelay);
		
		
		//gameObject.GetComponents(iTweenEvent).Play();
		
		//iTween.RotateTo(gameObject,{"x":3,"time":4,"delay":1,"onupdate","myUpdateFunction","looptype","pingpong"});	
		
		
		//iTweenEvent.GetEvent(gameObject, "rotateToVelocity").Play();
		
		
		iTween.LookTo(gameObject,myTransform.position + targetVelocity,timeDelay);
		
		
		targetVelocity.z = 0; 
		

		yield WaitForSeconds(timeDelay);
	}
}


