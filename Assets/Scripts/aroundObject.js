var degrees = 10;
var target : Transform;
 
function Update() {
    transform.RotateAround (target.position, Vector3.down, degrees * Time.deltaTime);
}