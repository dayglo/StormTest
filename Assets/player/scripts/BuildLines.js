#pragma strict

var c1 : Color = Color.yellow;
var c2 : Color = Color.red;

function Start() {
	
	MakeLineRenderer(transform , gameObject.AddComponent(LineRenderer) as LineRenderer , 0);
	
}


function MakeLineRenderer(currentNode:Transform , line : LineRenderer,nodeNumber : int) {
	
	if (nodeNumber == 0) {
		line.material = new Material (Shader.Find("Mobile/VertexLit"));
		line.SetColors(c1, c2);
		line.SetWidth(0.05,0.05);
		

			
	}
	
	line.SetVertexCount(nodeNumber+1);
	line.SetPosition(nodeNumber,currentNode.position);
	
	//Debug.Log("The current node ("+ currentNode.name +") has " + currentNode.childCount + " children");
	switch (currentNode.childCount) 
	{
		case 0:
			//the end of the node, finish the linerenderer
			//Debug.Log("ending line at " + currentNode.name);
		break;
		
		case 1:
			//continue the current line
			nodeNumber++;
			for (var child : Transform in currentNode) {
				//Debug.Log("continuing line through " + currentNode.name);
    			MakeLineRenderer(child,line,nodeNumber);

			}
		break;
		
		default:
			
			//iterate children and work out what to do with each one.
			for (var child : Transform in currentNode) {
				
					//delete all the head subnodes, they arent needed.
					if (currentNode.name == 'Head'){
						//Debug.Log("Destroying "+ child.gameObject.name);
						Destroy(child.gameObject);
						
					} else {
						//make a new line
						//Debug.Log("Building new line from " + currentNode.name);
						var newLine = child.gameObject.AddComponent(LineRenderer) as LineRenderer;
						
						newLine.SetVertexCount(1);
						newLine.SetPosition(0,currentNode.position);
						
						newLine.material = new Material (Shader.Find("VertexLit"));
						newLine.SetColors(c1, c2);
						newLine.SetWidth(0.03,0.03);
						
								if (currentNode.name.Contains("Hand")){
									newLine.SetWidth(0.02,0.02);
	
								}
						
						MakeLineRenderer(child,newLine,1);
	    			}
				
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
		break;
		
		case 1:
			//continue the current line
			nodeNumber++;
			for (var child : Transform in currentNode) {
    			UpdateLineRenderer(child,line,nodeNumber);
			}
		break;
		
		default:
			//start new rendererers
			for (var child : Transform in currentNode) {
				var newLine = child.gameObject.GetComponent(LineRenderer) as LineRenderer;
    			newLine.SetPosition(0,currentNode.position);
    			UpdateLineRenderer(child,newLine,1);
			}
		break;	
	
	}

}