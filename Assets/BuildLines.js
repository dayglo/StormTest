#pragma strict

var c1 : Color = Color.yellow;
var c2 : Color = Color.red;
var child : Transform;
//var lengthOfLineRenderer : int = 20;

// Comment 2
// Comment 4


function Start() {

	MakeLineRenderer(transform , gameObject.AddComponent(LineRenderer) as LineRenderer , 0);
	
	while (1) {
		//UpdateLineRenderer(transform , gameObject.GetComponent(LineRenderer) as LineRenderer , 0);
		yield WaitForSeconds(0.5);
	}
	
}


function MakeLineRenderer(currentNode:Transform , line : LineRenderer,nodeNumber : int) {
	
	if (nodeNumber == 0) {
		line.material = new Material (Shader.Find("Particles/Additive"));
		line.SetColors(c1, c2);
		line.SetWidth(0.05,0.05);
			
	}
	
	line.SetVertexCount(nodeNumber+1);
	line.SetPosition(nodeNumber,currentNode.position);
	
	switch (currentNode.childCount) 
	{
		case 0:
			//the end of the node, finish the linerenderer
			Debug.Log("ending line at " + currentNode.name);
		break;
		
		case 1:
			//continue the current line
			nodeNumber++;
			for (child  in currentNode as Transform ) {
    			MakeLineRenderer(child,line,nodeNumber);
    			Debug.Log("continuing line through " + currentNode.name);
			}
		break;
		
		default:
			//start new rendererers
			for (child in currentNode as Transform ) {
				if (currentNode.name == 'Head'){
					Debug.Log("Destroying "+ child.gameObject.name);
					Destroy(child.gameObject);
					
				} else {
					Debug.Log("Building new line from " + currentNode.name);
    				MakeLineRenderer(child,child.gameObject.AddComponent(LineRenderer) as LineRenderer,0);
    			}
			}
			
		break;	
	
	}

}

function Update() {

	UpdateLineRenderer(transform , gameObject.GetComponent(LineRenderer) as LineRenderer , 0);
}


function UpdateLineRenderer(currentNode:Transform , line : LineRenderer,nodeNumber : int)  {

	Debug.Log(line.gameObject.name);
	line.SetPosition(nodeNumber,currentNode.position);
	
	switch (currentNode.childCount) 
	{
		case 0:
			//the end of the node, finish the linerenderer
			//Debug.Log("ending line at " + currentNode.name);
		break;
		
		case 1:
			//continue the current line
			nodeNumber++;
			for (child in currentNode as Transform ) {
    			UpdateLineRenderer(child,line,nodeNumber);
    			//Debug.Log("continuing line through " + currentNode.name);
			}
		break;
		
		default:
			//start new rendererers
			
			for (child in currentNode as Transform ) {

					//Debug.Log("Building new line from " + currentNode.name);
    				UpdateLineRenderer(child,child.gameObject.GetComponent(LineRenderer) as LineRenderer,0);
    			
			}
			
		break;	
	
	}

}