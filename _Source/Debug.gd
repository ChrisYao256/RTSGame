extends Node

func _input(_InputEvent):
	# Press F10 to list all orphan nodes
	if Input.is_key_pressed(KEY_F):
		print_orphan_nodes();
