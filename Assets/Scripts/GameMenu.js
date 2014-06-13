var isQuit = false;
var defaultColor : Color = Vector4();

function OnMouseEnter(){
	guiText.color = Color.white;
}

function OnMouseExit(){
	guiText.color = defaultColor;
}

function OnMouseUp(){
	if (isQuit == true) {
		Application.Quit();
	}
	else {
		Application.LoadLevel(1);
	}
}

function Update(){
	if (Input.GetKey(KeyCode.Escape)) {
		Application.Quit();
	}
}