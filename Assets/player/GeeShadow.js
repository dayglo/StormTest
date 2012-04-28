#pragma strict
var myTransform : Transform;
var objectWeHit : Transform;
var layerMask : LayerMask;
var shadowObject : Transform;
var hit : RaycastHit;

function Awake() {
	myTransform = transform;	
	
}
function LateUpdate(){

    if (Physics.Raycast (myTransform.position,Vector3(0,-100,0), hit, Mathf.Infinity,layerMask)) {
    	//Debug.Log("hit");
        
        shadowObject.position = hit.point;
        shadowObject.rotation = Quaternion.LookRotation(Vector3.forward, hit.normal);
       // Debug.DrawRay(hit.point, hit.normal*5, Color.red);
       // shadowObject.transform.localScale = shadowObject.transform.localScale * hit.distance;

    }
}