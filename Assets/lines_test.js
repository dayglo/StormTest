var lineWidth = 2.0;
var numberOfPoints = 50; // Should be an evennumber, since we use a discrete line

function Start () {
	var linePoints = new Vector3[numberOfPoints]; 
	
	for (p in linePoints)
		p = Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height),200); 
		var lineColors = new Color[numberOfPoints/2];
		for (c in lineColors)
			c = Color(Random.value, Random.value, Random.value);
			var line = new VectorLine("Line", linePoints, lineColors, null, lineWidth); 
			Vector.DrawLine(line);
}