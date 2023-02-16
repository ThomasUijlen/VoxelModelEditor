extends Node3D

@export var mouseSensitivity = 1.0
@export var flySpeed = 1.0

var cameraRotation := Vector3.ZERO

func _ready():
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func _process(delta):
	move(delta)

func move(delta):
	var localMovement = Vector3.ZERO
	var globalMovement = Vector3.ZERO
	
	if Input.is_action_pressed("Forward"): localMovement += Vector3.FORWARD
	if Input.is_action_pressed("Backward"): localMovement += Vector3.BACK
	if Input.is_action_pressed("Left"): localMovement += Vector3.LEFT
	if Input.is_action_pressed("Right"): localMovement += Vector3.RIGHT
	if Input.is_action_pressed("Up"): globalMovement += Vector3.UP
	if Input.is_action_pressed("Down"): globalMovement += Vector3.DOWN
	
	var totalMovement = (globalMovement + (to_global(localMovement)-global_position)).normalized()
	if Input.is_action_pressed("Shift"): totalMovement *= 2.0
	
	totalMovement *= flySpeed*delta
	
	global_position += totalMovement

func _input(event):
	if event is InputEventMouseMotion:
		var motion = event.relative*0.01
		
		cameraRotation.x -= motion.y*mouseSensitivity*0.8
		cameraRotation.y -= motion.x*mouseSensitivity
		
		if cameraRotation.x < -PI/2: cameraRotation.x = -PI/2
		if cameraRotation.x > PI/2: cameraRotation.x = PI/2
		
		rotation = cameraRotation
