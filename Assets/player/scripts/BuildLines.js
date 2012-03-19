#pragma strict

var c1 : Color = Color.yellow;
var c2 : Color = Color.red;
var lineContinuationHintKeys : Transform[];
var lineContinuationHintValues : Transform[];
var i : int; //iterator


function Start() {
	
	Debug.Log(lineContinuationHintKeys.Length);
	
	
	MakeLineRenderer(transform , gameObject.AddComponent(LineRenderer) as LineRenderer , 0);
	
	while (1) {
		//UpdateLineRenderer(transform , gameObject.GetComponent(LineRenderer) as LineRenderer , 0);
		yield WaitForSeconds(0.5);
	}
	
}


function MakeLineRenderer(currentNode:Transform , line : LineRenderer,nodeNumber : int) {
	
	if (nodeNumber == 0) {
		line.material = new Material (Shader.Find("Mobile/VertexLit"));
		line.SetColors(c1, c2);
		line.SetWidth(0.05,0.05);
			
	}
	
	line.SetVertexCount(nodeNumber+1);
	line.SetPosition(nodeNumber,currentNode.position);
	
	Debug.Log("The current node ("+ currentNode.name +") has " + currentNode.childCount + " children");
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
				Debug.Log("continuing line through " + currentNode.name);
    			MakeLineRenderer(child,line,nodeNumber);

			}
		break;
		
		default:

			// work out if there is a hinted child for this node.
			var hintedChild : Transform;			
			for (i=0;i<lineContinuationHintKeys.Length;i++) {
				if (lineContinuationHintKeys[i] == currentNode) {
					//hint found, store hinted child for use in child enumerator loop
					Debug.Log("hint found, continue line from " +currentNode.name + " to " +lineContinuationHintValues[i].name);
					hintedChild = lineContinuationHintValues[i];		
				}
				
			}
			
			//iterate children and work out what to do with each one.
			for (var child : Transform in currentNode) {
				
				if (child == hintedChild) {
				
					nodeNumber++;
					MakeLineRenderer(hintedChild,line,nodeNumber);;
				
				} else {
					//delete all the head subnodes, they arent needed.
					if (currentNode.name == 'Head'){
						Debug.Log("Destroying "+ child.gameObject.name);
						Destroy(child.gameObject);
						
					} else {
						//a hint wasnt found, so make a new line
						Debug.Log("Building new line from " + currentNode.name);
						MakeLineRenderer(child,child.gameObject.AddComponent(LineRenderer) as LineRenderer,0);
	    			}
				}
			}
			
		break;	
	}

}

function Update() {

	//UpdateLineRenderer(transform , gameObject.GetComponent(LineRenderer) as LineRenderer , 0);
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
    				UpdateLineRenderer(child,child.gameObject.GetComponent(LineRenderer) as LineRenderer,0);
			}
		break;	
	
	}

}