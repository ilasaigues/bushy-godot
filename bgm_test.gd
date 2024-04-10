extends Node

@onready var audio : FmodEventEmitter2D = $FmodEventEmitter2D
@onready var boss : Button = $"../CanvasLayer/Control/Boss"
@onready var finisher : Button = $"../CanvasLayer/Control/Finisher"
@onready var death : Button = $"../CanvasLayer/Control/Death"
# Called when the node enters the scene tree for the first time.
func _ready():
	finisher.disabled = true
	death.disabled = true
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
	


func _on_boss_button_down():
	audio.set_parameter("Boss", 1)
	audio.set_parameter("Boss Status",0)
	boss.disabled = true;
	finisher.disabled = false;
	

func _on_finisher_button_down():
	audio.set_parameter("Boss Status",2)
	finisher.disabled = true;
	death.disabled = false;
	

func _on_death_button_down():
	audio.set_parameter("Boss Status",3)
	audio.set_parameter("Boss", 0)
	death.disabled = true;
	boss.disabled = false;
