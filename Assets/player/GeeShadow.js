#pragma strict
private var myTransform : Transform;
var objectWeHit : Transform;
var layerMask : LayerMask;
private var shadow : Transform;
private var distanceFromShadow : float;
private var shadowMat : Material;

var s : float;

private var startScale : Vector3;
var size : Vector3;
var positionAdjust : Vector3;

var shadowPrefab : Transform;

var hit : RaycastHit;

function Awake() {
	myTransform = transform;	
	
}

function Start(){
	var v : Vector3 = myTransform.position;
	shadow = Instantiate(shadowPrefab, v, transform.rotation);
	startScale = shadow.localScale;
	
	shadow.localScale = (startScale + size) ;
	
	shadowMat = shadow.GetComponentInChildren(Renderer).material;
	
	

}

function LateUpdate(){

    if (Physics.Raycast (myTransform.position,Vector3(0,-100,0), hit, Mathf.Infinity,layerMask)) {
    	//Debug.Log("hit");
    	
    	distanceFromShadow = Vector3.Distance(myTransform.position,hit.point);
        shadow.localScale = (startScale + size );
        shadow.localScale -= shadow.localScale * (distanceFromShadow * 0.5);
        
        shadowMat.color.a = 1- (distanceFromShadow * 0.5);
        //Debug.Log(shadowMat.color.a);
        
        shadow.position = hit.point + positionAdjust;
        shadow.rotation = Quaternion.LookRotation(Vector3.forward, hit.normal);

    }
}