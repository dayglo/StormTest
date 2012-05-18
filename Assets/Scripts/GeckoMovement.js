#pragma strict

public var topPos : float = 10.0f;
public var botPos : float = 7.3f;
public var minTilt : float = 0.3f;
public var maxTilt : float = 0.7f;
public var inputSmoothing : float = 1.0f / 9.0f;


// stores the saved calibration setting, current set at startup
private var calibrationMatrix : Matrix4x4;

function Start () {
	// Test Scale function
}

function Update() {
	if(Application.platform == RuntimePlatform.IPhonePlayer) {
		// Get input acceleration vector for Y
		var move : float = -Input.acceleration.y;
		// Normalise movement range
		var clamped : float = Mathf.Clamp(move, minTilt, maxTilt);
		// Convert to screen position via scaling
		var newy : float = topPos - ScaleToRange(clamped);
		// Smoth the input over time;
		var newyDamp : float = Mathf.Lerp(transform.position.y, newy, inputSmoothing);
        // And apply smoothed position
		transform.position.y = newyDamp;
	}
}

function ScaleToRange(input : float) : float
{
	var tiltRange : float = maxTilt - minTilt;
	var screenRange : float = topPos - botPos;
	var out : float = ((screenRange * (input - minTilt)) / tiltRange);
	return out;
}

/*
      (b-a)(x - min)
f(x) = --------------  + a
          max - min
*/
       
function FixedUpdate () {
	// Check for keyboard / mouse buttons instead
	if(Application.platform != RuntimePlatform.IPhonePlayer) {
		var tilt : float;
		// Not on device, use mouse wheel for movement
		tilt = Input.GetAxis("Mouse ScrollWheel");
		if(tilt != 0.0f) {
			rigidbody.AddRelativeForce(-transform.up * tilt,ForceMode.Impulse);
		}
	} else {

 	}
}

/*
//FixedUpdate () is advantageous over Update () for working with rigidbody physics.
function FixedUpdate () {
	// Retrieve input.  Note the use of GetAxisRaw (), which in this case helps responsiveness of the controls.
	// GetAxisRaw () bypasses Unity's builtin control smoothing.
	// Don't allow input if we are captured
	if(UFOMove.isCaptured) return;

	var newacc : Vector3 = Vector3.zero;
	newacc.x=Input.acceleration.x;
	newacc.y=Input.acceleration.y;
	if(newacc.sqrMagnitude >1) newacc.Normalize();
	
	// Smooth the input
	acc = Vector3.Lerp(acc, newacc, inputSmoothing);
	// Set range from 0 - 1
	var offy : float = (acc.y + 1.0) / 2.0;
	var ang : float = Mathf.Lerp(-135, 135, offy);
	var turn : float = ang;
	
	var thrust : float = 0.0;
	if (!canControl) {
		isThrusting=false;;
		turn = 0.0;
	} 
	
	if(isThrusting) {
		thrust = positionalMovement.positiveAcceleration;
		if(curFuel>0.0) {
			curFuel-=Time.deltaTime;
			var pct = curFuel / maxFuel * 100.0;
			//UIManager.SetFuelBar(pct);
			rigidbody.drag = positionalMovement.ComputeDrag (thrust, rigidbody.velocity);
			rigidbody.AddRelativeForce (forwardDirection * thrust * Time.deltaTime, ForceMode.VelocityChange);
		}
	}

	//turn = Mathf.Clamp(turn, -1.0, 1.0);
	//Use the MovementSettings class to determine which drag constant should be used for the positional movement.
	//Remember the MovementSettings class is a helper class we defined ourselves. See the top of this script.
	rigidbody.drag = positionalMovement.ComputeDrag (thrust, rigidbody.velocity);

	// Add torque and force to the rigidbody.  Torque will rotate the body and force will move it.
	// Always modify your forces by Time.deltaTime in FixedUpdate (), so if you ever need to change your Time.fixedTime setting,
	// your setup won't break.
	if(canRotate) {
		var rot : Quaternion = Quaternion.Slerp(rigidbody.rotation, Quaternion.Euler(0, 0, turn), rotateSmoothing);
		rigidbody.rotation=rot;
	}
}
*/

function LateUpdate () {
	if(transform.position.y > topPos) {
		transform.position.y = topPos;
		rigidbody.velocity = Vector3(0.0, 0.0, 0.0);
	}	
	if(transform.position.y < botPos) {
		transform.position.y = botPos;
		rigidbody.velocity = Vector3(0.0, 0.0, 0.0);
	}	

}

//Used to calibrate the initial iPhoneIput.acceleration input 
function CalibrateAccelerometer () { 
   var accelerationSnapshot : Vector3 = iPhoneInput.acceleration; 
   var rotateQuaternion : Quaternion = Quaternion.FromToRotation(new Vector3(0.0, 0.0, -1.0), accelerationSnapshot); 

   //create identity matrix ... rotate our matrix to match up with down vec
   var matrix : Matrix4x4 = Matrix4x4.TRS(Vector3.zero, rotateQuaternion, new Vector3(1.0, 1.0, 1.0)); 

   //get the inverse of the matrix 
   calibrationMatrix = matrix.inverse; 
} 

//   Get the 'calibrated' value from the iPhone Input 
function FixAcceleration (accelerator : Vector3) { 
   var fixedAcceleration : Vector3 = calibrationMatrix.MultiplyVector(accelerator); 
   return fixedAcceleration; 
}