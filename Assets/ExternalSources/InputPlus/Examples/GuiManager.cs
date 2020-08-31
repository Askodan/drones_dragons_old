//GuiManager Example script for InputPlus

//This is an example of setting up simple UI to handle players setting up their controllers.

using UnityEngine;
using System.Collections;
using InputPlusControl; //Add this to access InputPlus's functions and data.


public class GuiManager : MonoBehaviour {

	/* Images to show the player what controls to use. You can use something like this to display images for
	 when the player is setting up the controller or even during gameplay as tutorials or hints. The images
	 are set in the inspector when viewing the GuiManagerObject */
	public Texture texThumbLeft_x, texThumbLeft_y, texThumbLeft, texThumbRight_x, texThumbRight_y, texThumbRight, 
	texdpad_up, texdpad_down, texdpad_left, texdpad_right,
	texFP_top, texFP_bottom, texFP_left, texFP_right,
	texInterface_left, texInterface_right,
	texShoulderTop_left, texShoulderTop_right, texShoulderBottom_left, texShoulderBottom_right;

	~GuiManager()
	{
		//Unsubscribing from events is important.
		InputPlus.On_EVENT_Disconnect -= Disconnect;
	}//~Cube

	void Start () 
	{
		InputPlus.Initialize(); //Start up InputPlus. 
		InputPlus.SetDebugText(true); //Do you want to see Debug text from InputPlus?

		/* Subscribe to the disconnect event. This event happens when a controller is disconnected. Unfortunately
		 * it doesn't seem like Unity's core control features (v4.3.4) handle connecting and reconnecting controllers during
		 * execution in a standalone build. So it's STRONGLY recommended to condiser handling this event to allow a
		 * game save. Hopefully this will change in the future */
		InputPlus.On_EVENT_Disconnect += Disconnect;

		
	}//Start
	
	// Update is called once per frame
	void Update () 
	{
		ListenForLearnRequest(); //Explained in declaration
		ListenForDisplayMode(); //Listen for 'Tab' to toggle InputPlus's on screen data

		if(InputPlus.GetProgrammingStatus())
		{
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				InputPlus.CancelProgramming();
			}
		} //If the controller is receiving assignments the user can press 'Escape' to Cancel.

	}//Update

	void OnGUI()
	{
		//Write out basic usage instructions
		GUILayout.Label("Welcome to InputPlus - Press Tab to cycle between Raw Data / Programmed Values / Off");
		GUILayout.Label("Press keys 1-9 to program corresponding controller. 0 and A for 10 and 11, respectively\n");
		if(InputPlus.GetProgrammingStatus())
		{
			GUI.DrawTexture(new Rect(360,160,160,160), ControllerVarEnum_to_Texture(InputPlus.GetWaitingForControl()), 
			                ScaleMode.ScaleToFit, true, 1.0f);
			GUILayout.Label("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + 
	                "Programming for " + InputPlus.GetControllerName(InputPlus.GetListeningFor()));
		} //Show image of the control InputPlus is waiting to program. Display the name of the controller.
	}//OnGUI

	Texture ControllerVarEnum_to_Texture(ControllerVarEnum cve)
	{
		//Return related texture for each respective ControllerVarEnum 
		Texture image = null;
		switch(cve)
		{
		case ControllerVarEnum.ThumbLeft_x:
			image = texThumbLeft_x;
			break;
		case ControllerVarEnum.ThumbLeft_y:
			image = texThumbLeft_y;
			break;
		case ControllerVarEnum.ThumbLeft:
			image = texThumbLeft;
			break;
		case ControllerVarEnum.ThumbRight_x:
			image = texThumbRight_x;
			break;
		case ControllerVarEnum.ThumbRight_y:
			image = texThumbRight_y;
			break;
		case ControllerVarEnum.ThumbRight:
			image = texThumbRight;
			break;
		case ControllerVarEnum.dpad_up:
			image = texdpad_up;
			break;
		case ControllerVarEnum.dpad_down:
			image = texdpad_down;
			break;
		case ControllerVarEnum.dpad_left:
			image = texdpad_left;
			break;
		case ControllerVarEnum.dpad_right:
			image = texdpad_right;
			break;
		case ControllerVarEnum.FP_top:
			image = texFP_top;
			break;
		case ControllerVarEnum.FP_bottom:
			image = texFP_bottom;
			break;
		case ControllerVarEnum.FP_left:
			image = texFP_left;
			break;
		case ControllerVarEnum.FP_right:
			image = texFP_right;
			break;
		case ControllerVarEnum.Interface_left:
			image = texInterface_left;
			break;
		case ControllerVarEnum.Interface_right:
			image = texInterface_right;
			break;
		case ControllerVarEnum.ShoulderTop_left:
			image = texShoulderTop_left;
			break;
		case ControllerVarEnum.ShoulderTop_right:
			image = texShoulderTop_right;
			break;
		case ControllerVarEnum.ShoulderBottom_left:
			image = texShoulderBottom_left;
			break;
		case ControllerVarEnum.ShoulderBottom_right:
			image = texShoulderBottom_right;
			break;
		}
		return image;
	}//ControllerVarEnum_to_Texture

	void Disconnect()
	{
		// Currently Windows Unity stand alone build can't add joysticks or reconnect them, so handling a
		// disconnect is very important. Generally, for all OSes, adding and removing controllers could
		// possibly be a bad thing. Better to warm players to connect their controllers before starting.
		// In windows, the wireless xbox 360 controller, potentially a very popular controller, should
		// have special pre-app launching instructions.
		Debug.Log ("DISCONNECT OCCURED!");
	}//Disconnect

	void ListenForLearnRequest()
	{
		//Check for controller programming request. In this case keys '1' - '9' will trigger 
		//controllers 1-9 into learn mode, (only one at a time please!) and '0' and 'a' will trigger
		//controllers 10 and 11 respectively. You'll most likely want to build the learning mode into your
		//project's UI


		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			InputPlus.LearnController(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			InputPlus.LearnController(2);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			InputPlus.LearnController(3);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			InputPlus.LearnController(4);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			InputPlus.LearnController(5);
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			InputPlus.LearnController(6);
		}
		if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			InputPlus.LearnController(7);
		}
		if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			InputPlus.LearnController(8);
		}
		if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			InputPlus.LearnController(9);
		}
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			InputPlus.LearnController(10);
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			InputPlus.LearnController(11);
		}
		if (Input.GetKeyDown(KeyCode.Z))
		{
			//InputPlus.LearnControllerSingle(1, ControllerVarEnum.dpad_up);
			// LearnControllerSingle is currently an experimental feature, it's likely problems exist
		}
	}//ListenForLearnRequest

	void ListenForDisplayMode()
	{
		if (Input.GetKeyDown(KeyCode.Tab)) InputPlus.ToggleDataView();
	}//ListenForLearnRequest
}//GuiManager

