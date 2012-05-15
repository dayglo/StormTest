#pragma strict

public var topPos : float = 10.0f;
public var botPos : float = 7.3f;

// stores the saved calibration setting, current set at startup
private var calibrationMatrix : Matrix4x4;

function Start () {
	// Calibrate accelormeter
//	if(Application.platform == RuntimePlatform.IPhonePlayer) {
//		CalibrateAccelerometer();
//	}
}

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
	/* tilt controls
 		var acceleration : Vector3 = iPhoneInput.acceleration; 	// Get acceleration
		var fixedAcceleration : Vector3 = FixAcceleration (acceleration); // Adjust for calibration
		tilt = fixedAcceleration.y / 2.0f;	// Scale speed
		tilt = -tilt;	// Invert so tilt back is up
		*/
		for (var evt : Touch in Input.touches)
		{
			// Check we are above the bottom of the screen (push buttons)
			if(evt.position.y > Screen.height / 4) {
					if (evt.phase == TouchPhase.Moved || evt.phase == TouchPhase.Began)
					{
						var ray : Ray = Camera.main.ScreenPointToRay( evt.position );
						var pos : Vector3 = ray.GetPoint( (Camera.main.transform.position - transform.position).magnitude );
						//var cameraTransform = Camera.main.transform.InverseTransformPoint(0, 0, 0);
						//var pos : Vector3 = Camera.main.ScreenToWorldPoint(new Vector3 (evt.position.x, evt.position.y, transform.position.z));

					    //var p : Vector3 = Camera.main.ScreenToWorldPoint (Vector3 (0.0,evt.position.y,0.0));
						transform.position.y = pos.y;
					}
			}
		}
 	}
}

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