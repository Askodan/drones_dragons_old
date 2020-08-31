//ControlExample example script for InputPlus

//This is an example of setting up simple control by reading controller values through InputPlus.
//It uses the GuiManager from the GuiExample
//This script is attached to the Player object in this example.

using UnityEngine;
using System.Collections;
using InputPlusControl;

public class ControlExample : MonoBehaviour 
{

	public float Speed;

	//Bindings - This is a way you can set bindings to an action. Using this, a programmer could set up a
	//a system to rebind controls.
	ControllerVarEnum UpDown = ControllerVarEnum.ThumbLeft_y;
	ControllerVarEnum LeftRight = ControllerVarEnum.ThumbLeft_x;
	
	void Start () 
	{
	
	}//Start

	void Update () 
	{
		float moveX, moveY; //How much to move along X and Y coordinates
		moveX = InputPlus.GetData(1, LeftRight); //Read the controller's value for the assigned control
		moveY = -(InputPlus.GetData(1, UpDown)); //flip axis with negative
		Vector3 MoveVector = new Vector3(moveX, 0, moveY); //Put the read values into a vector representing movement direction
		Vector3 CurrentPos = transform.position; //the current position
		Vector3 NewPosition = CurrentPos + (MoveVector * Speed * Time.deltaTime); //apply speed and use delaTime to compensate for framerate
		GetComponent<Rigidbody>().MovePosition(NewPosition); //Move the object there
	}//Update
}
