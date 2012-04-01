#pragma strict

import System.Collections.Generic;

var c1 : Color = Color.yellow;
var c2 : Color = Color.red;
var root: Transform;
var lineMaterial : Material;
var lineName : String;

var myVectorLines : Dictionary.<VectorLine, List.<Transform> > = new Dictionary.<VectorLine, List.<Transform> >();

var line : VectorLine;
var c : Color;

var currentLineNumber : int = 0;
var currentNodeNumber : int = 0;


function Start() {
	
	var myTransformList = new List.<Transform>();		
	myTransformList.Add(root);
    HierarchytoDictionary(root,myTransformList);
    
}


function Update(){
	
	//for each vectorline
	for (v in myVectorLines.Keys) {
		//get the transform list from the dictionary for the current vectorline, convert it to an array of vector3, and update the vector line.
		v.points3 = TransformsToVectors(myVectorLines[v]);
		Vector.DrawLine3D(v);
	 
	  
	} 
		
}

function TransformsToVectors(transformList : List.<Transform>) : Vector3[] {

// converts a list of transforms to a list of vectors.
	
	var vectorList = new List.<Vector3>();
	
		for (var t : Transform in transformList) {
			vectorList.Add(t.position);					
		}
		
	return vectorList.ToArray();

}


function HierarchytoDictionary (currentNode : Transform, transformList : List.<Transform>)  {

	switch (currentNode.childCount) 
	{
		case 0:		
			Debug.Log("Adding final point "+ currentNode.name +"("+ currentNodeNumber + ") to line" +currentLineNumber);
			
			//add the final position to the line
			transformList.Add(currentNode);
			
			//make a color
			c = Color(Random.value, Random.value, Random.value);
			
			//build a proper name for the vector line object
			lineName = transformList[0].name + " to " + currentNode.name;
			
			//create the line
			line = new VectorLine(lineName, TransformsToVectors(transformList), lineMaterial,2.0, LineType.Continuous);
			Vector.DrawLine3D(line);

			//cache reference to this line so that it's point positions can be updated in the update loop
			myVectorLines[line] = transformList;

		break;
		
		case 1:
			for (var child : Transform in currentNode) {		
				Debug.Log("Adding point at " + currentNode.name +"("+  currentNodeNumber +") to continue line" +currentLineNumber);
    			currentNodeNumber++;
    			
    			//continue the line
    			transformList.Add(currentNode);
    			HierarchytoDictionary(child,transformList);

			}
		break;
		default:
		
			
			//add the final position to the line
			transformList.Add(currentNode);
			
			if (transformList[0] === currentNode) {
				//Do not create a line here, because the line is only one point long.
			} else {

				//make a color
				c = Color(Random.value, Random.value, Random.value);
				
<<<<<<< HEAD
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
						
						newLine.material = new Material (Shader.Find("Mobile/VertexLit"));
						newLine.SetColors(c1, c2);
						newLine.SetWidth(0.03,0.03);
						
								if (currentNode.name.Contains("Hand")){
									newLine.SetWidth(0.02,0.02);
=======
				//build a proper name for the vector line object
				lineName = transformList[0].name + " to " + currentNode.name;
				
				//create the line
				line = new VectorLine(lineName, TransformsToVectors(transformList), lineMaterial,2.0, LineType.Continuous);
				Vector.DrawLine3D(line);
>>>>>>> Complete line drawing re-write using Vectrosity.
	
				//cache reference to this line so that it's point positions can be updated in the update loop
				myVectorLines[line] = transformList;
			}
		
			//recurse each child node.
			for (var child : Transform in currentNode) {
				currentLineNumber++;
				currentNodeNumber = 0;
				Debug.Log("Creating line " + currentLineNumber + " starting from " + currentNode.name + " going through " + child.name);
				
				//make a new list of transforms...
				var myTransformList = new List.<Transform>();
				
				//.. add the current node (which is the starting point) to the list.
				myTransformList.Add(currentNode);
				
				//send the list off down towards the end of this line...
    			HierarchytoDictionary(child,myTransformList);
			}
		break;	
	}


}


