#pragma strict

var c1 : Color = Color.yellow;
var c2 : Color = Color.red;
//var lengthOfLineRenderer : int = 20;




function Start() {

	MakeLineRenderer(transform , gameObject.AddComponent(LineRenderer) as LineRenderer , 0);

}


function MakeLineRenderer(currentNode:Transform , line : LineRenderer,nodeNumber : int) {
	
	if (nodeNumber == 0) {
		line.material = new Material (Shader.Find("Particles/Additive"));
		line.SetColors(c1, c2);
		line.SetWidth(0.2,0.2);
			
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
			for (var child : Transform in currentNode) {
    			MakeLineRenderer(child,line,nodeNumber);
    			Debug.Log("continuing line through " + currentNode.name);
			}
		break;
		
		default:
			//start new rendererers
			for (var child : Transform in currentNode as Transform) {
			
				Debug.Log("Building new line from " + currentNode.name);
    			MakeLineRenderer(child,child.gameObject.AddComponent(LineRenderer) as LineRenderer,0);
			}
			
		break;	
	
	}

}

function Update() {

	UpdateLineRenderer(transform , gameObject.GetComponent(LineRenderer) as LineRenderer , 0);

}


function UpdateLineRenderer(currentNode:Transform , line : LineRenderer,nodeNumber : int)  {

	
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
			for (var child : Transform in currentNode) {
    			UpdateLineRenderer(child,line,nodeNumber);
    			//Debug.Log("continuing line through " + currentNode.name);
			}
		break;
		
		default:
			//start new rendererers
			for (var child : Transform in currentNode as Transform) {
			
				//Debug.Log("Building new line from " + currentNode.name);
    			UpdateLineRenderer(child,child.gameObject.GetComponent(LineRenderer) as LineRenderer,0);
			}
			
		break;	
	
	}

}