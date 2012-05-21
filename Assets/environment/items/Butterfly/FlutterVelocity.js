var maxSpeed : float = 0.001;

var targetVelocity: Vector3;
private var currentVelocity: Vector3 = Vector3.zero;
private var timeDelay : float = 1.0;
private var myTransform : Transform;

var maxDistance : float = 0.001;

var targetPosition :Vector3; 
private var vectorToTarget : Vector3; 

private var correction : float;

var paths : Transform[];
private var currentPath : Transform;
private var currentPathIndex : int = 0;

private var currentiTweenPath : Vector3[];

enum MoveMode {target,randomPath,setPath}

var mode = MoveMode.setPath;


function Start () {
	myTransform = transform;
	targetPosition =  Vector3(-5,Random.Range(8.4,9.0),0);
	
	if (mode == MoveMode.target) {
	
		StartCoroutine("BuzzAround");
		
	} else if (mode == MoveMode.setPath) {
	
		StartCoroutine("FollowSetPath",  paths);
	
	} else if (mode == MoveMode.randomPath) {
	
		StartCoroutine("FollowRandomPath",  paths[Random.Range(0,paths.length)]   );
	
	}
}
	
function Update() {
	
 	 	
 	if (mode == MoveMode.target) {
	
		Debug.DrawRay(myTransform.position,vectorToTarget,Color.cyan,Time.deltaTime);
		currentVelocity = (Vector3.Lerp(currentVelocity,targetVelocity,0.02));
		
		Debug.DrawRay(myTransform.position,currentVelocity * 100,Color.white,Time.deltaTime); 
		myTransform.Translate((currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude,0,maxSpeed)),Space.World);
		
	} else if (mode == MoveMode.setPath) {
		
		//do nothing
		
	
	} 	
 	 	

}

function FollowRandomPath(path : Transform){

	Debug.Log(path.name);
	currentPath = Instantiate(path, myTransform.position, myTransform.rotation);
	
	Debug.Log(currentPath.name);
	
	currentiTweenPath = iTweenPath.GetPath(path.name);
	
	for (i = 0; i < currentiTweenPath.Length; i++) {
   		currentiTweenPath[i] += myTransform.position;
	}
	
	iTween.MoveTo(gameObject, iTween.Hash("path" , currentiTweenPath, "speed", 1    , "oncomplete" , "FollowRandomPath" , "oncompleteparams" , paths[Random.Range(0,paths.length)] , "easetype" , "linear" ));
	Destroy(currentPath.gameObject);

}

function FollowSetPath(){

	currentPath = Instantiate(paths[currentPathIndex], myTransform.position, myTransform.rotation);
	currentiTweenPath = iTweenPath.GetPath(paths[currentPathIndex].name);
	for (i = 0; i < currentiTweenPath.Length; i++) {
   		currentiTweenPath[i] += myTransform.position;
	}
	
	
	currentPathIndex++;
	
	if (currentPathIndex > (paths.Length -1)) {
		currentPathIndex = 0;
	}
	
	iTween.MoveTo(gameObject, iTween.Hash("path" , currentiTweenPath, "speed", 1    , "oncomplete" , "FollowSetPath" , "oncompleteparams" , paths[currentPathIndex] , "easetype" , "linear" ));
	Destroy(currentPath.gameObject);

	

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
		
		
		
		//iTween.RotateTo(gameObject,{"x":3,"time":4,"delay":1,"onupdate","myUpdateFunction","looptype","pingpong"});	

		iTween.LookTo(gameObject,myTransform.position + targetVelocity,timeDelay);
		
		
		targetVelocity.z = 0; 
		

		yield WaitForSeconds(timeDelay);
	}
}


