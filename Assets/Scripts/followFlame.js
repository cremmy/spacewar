#pragma strict
var target : Transform;

function Update () {
	transform.position = Vector3(target.position.x + 0.1, target.position.y, target.position.z);
}