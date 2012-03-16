#pragma strict

function OnEnable()
{
    // subscribe to the global tap event
    FingerGestures.OnTap += MyFingerGestures_OnTap;
}
 
function OnDisable()
{
    // unsubscribe from the global tap event
    FingerGestures.OnTap -= MyFingerGestures_OnTap;
}
 
// Our tap event handler. The method name can be whatever you want.

function MyFingerGestures_OnTap( fingerPos : Vector2 )
{
    Debug.Log( "TAP detected at " + fingerPos );
}
