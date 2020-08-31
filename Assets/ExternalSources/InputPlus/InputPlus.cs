//InputPlus
//Version 1.0
//by David Bax
//for support or to report bugs please contact me at inputplusforunity@gmail.com


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ControllerVarEnum 
{
	ThumbLeft_x, ThumbLeft_y, ThumbLeft, ThumbRight_x, ThumbRight_y, ThumbRight, 
	ShoulderTop_left, ShoulderTop_right, ShoulderBottom_left, ShoulderBottom_right,
	Interface_left, Interface_right,
	dpad_up, dpad_down, dpad_left, dpad_right,
	FP_top, FP_bottom, FP_left, FP_right
};

static class Constants
{
	public const int MAX_CTRL_SIZE = 11;	//11 controllers is what unity is supporting at the moment
	public const float DEAD_ZONE = 0.15f; 
}

namespace InputPlusControl
{
	
	public class InputPlus : MonoBehaviour 
	{
			
		private int ic = 0; //Current controller that is being programmed
		private enum ControllerModeEnum {OFF, DISPLAY, DISPLAY2, SET};
		private enum SetModeEnum {ENTER, WAIT, WAIT_DPAD, LISTEN, EXIT};
		private bool SingleSetMode;
		private ControllerModeEnum ControllerMode;
		private SetModeEnum SetMode;
		private ControllerVarEnum ListenMode;

		private bool CurrentlyProgramming; 

		public delegate void EVENT_Disconnect();
		public static event EVENT_Disconnect On_EVENT_Disconnect;

		//public delegate void EVENT_Connect();
		//public static event EVENT_Connect On_EVENT_Connect;

		private static InputPlus instance;

		private List <string> LockedInput = new List<string>();

		private int NumControllers;

		private Controller[] ControllerArray; 

		private bool DebugText;

		private bool[] negFlag = new bool[10];

		private static InputPlus IPInstance
		{
			//the ?? operator returns 'instance' if 'instance' does not equal null
			//otherwise assign instance to a new component and return that
			get { return instance ?? (instance = new GameObject("InputPlus").AddComponent<InputPlus>()); }
		}//InputPlus Instance


		// Use this for initialization
		void Start () 
		{
			DontDestroyOnLoad(this); //Keep InputPlus running after scene loading.
			LoadControllers();
		}//Start
		
		// Update is called once per frame
		void Update () 
		{

			string listen;

			//Handle Joysticks removed.
			if (Input.GetJoystickNames().Length < IPInstance.NumControllers)
			{
				//Controller disconnected
				if (On_EVENT_Disconnect != null) On_EVENT_Disconnect();
			}
			//DONE Handle Joysticks removed.

			UpdateControllers();

			//handle controller mode
			switch(ControllerMode)
			{
				
			case ControllerModeEnum.DISPLAY :
				break;
			case ControllerModeEnum.DISPLAY2 :
				break;	
			case ControllerModeEnum.SET :
				CurrentlyProgramming = true;
				switch(SetMode)
				{
				case SetModeEnum.ENTER:
					SetMode = SetModeEnum.WAIT;
					//("enter mode");
					break;
				case SetModeEnum.WAIT:
					//check to see if key has been released.

					if (!ContinueWait(ic))
					{
						ScanForLocked();
						SetMode = SetModeEnum.LISTEN;
					}
					//Debug.Log("Wait mode");
					break;

				case SetModeEnum.WAIT_DPAD:
					//check to see if key has been released.
					
					if (!ContinueWait_dPad(ic))
					{
						SetMode = SetModeEnum.LISTEN;
					}
					//Debug.Log("Wait dPad mode");
					break;

				case SetModeEnum.LISTEN:
					switch(ListenMode)
					{	
					case ControllerVarEnum.ThumbLeft_x:
						//Debug.Log("Thumbx listen");
						listen = SetModeGetString(ic);
						//Debug.Log("ThumbLeft_x: " + listen);
						if(listen != "")
						{
							ControllerArray[ic].set_sThumbLeft_x(listen);
							ListenMode = ControllerVarEnum.ThumbLeft_y;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
					case ControllerVarEnum.ThumbLeft_y:
						listen = SetModeGetString(ic);
						//Debug.Log("ThumbLeft_y");
						if(listen != "")
						{
							ControllerArray[ic].set_sThumbLeft_y(listen);
							ListenMode = ControllerVarEnum.ThumbLeft;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
					case ControllerVarEnum.ThumbLeft:
						listen = SetModeGetString(ic);
						//Debug.Log("ThumbLeft");
						if(listen != "")
						{
							ControllerArray[ic].set_sThumbLeft(listen);
							ListenMode = ControllerVarEnum.ThumbRight_x;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
					case ControllerVarEnum.ThumbRight_x:
						listen = SetModeGetString(ic);
						//Debug.Log("ThumbRight_x");
						if(listen != "")
						{
							ControllerArray[ic].set_sThumbRight_x(listen);
							ListenMode = ControllerVarEnum.ThumbRight_y;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
					case ControllerVarEnum.ThumbRight_y:
						listen = SetModeGetString(ic);
						//Debug.Log("ThumbRight_y");
						if(listen != "")
						{
							ControllerArray[ic].set_sThumbRight_y(listen);
							ListenMode = ControllerVarEnum.ThumbRight;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
					case ControllerVarEnum.ThumbRight:
						listen = SetModeGetString(ic);
						//Debug.Log("ThumbRight");
						if(listen != "")
						{
							ControllerArray[ic].set_sThumbRight(listen);
							ListenMode = ControllerVarEnum.ShoulderTop_left;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
					
					case ControllerVarEnum.ShoulderTop_left:
						listen = SetModeGetString(ic);
						//Debug.Log("ShoulderTop_left");
						if(listen != "")
						{
							ControllerArray[ic].set_sShoulderTop_left(listen);
							ListenMode = ControllerVarEnum.ShoulderTop_right;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
					case ControllerVarEnum.ShoulderTop_right:
						listen = SetModeGetString(ic);
						//Debug.Log("ShoulderTop_right");
						if(listen != "")
						{
							ControllerArray[ic].set_sShoulderTop_right(listen);
							ListenMode = ControllerVarEnum.ShoulderBottom_left;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
					case ControllerVarEnum.ShoulderBottom_left:
						//CheckNegFlags(ic);
						listen = SetModeGetStringTrigger(ic);
						//Debug.Log("ShoulderBottom_left");
						if(listen != "")
						{
							ControllerArray[ic].set_sShoulderBottom_left(listen);
							ListenMode = ControllerVarEnum.ShoulderBottom_right;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
							
						}
						break;

					case ControllerVarEnum.ShoulderBottom_right:
						//CheckNegFlags(ic);
						listen = SetModeGetStringTrigger(ic);
						//Debug.Log("ShoulderBottom_right");
						if(listen != "")
						{
							ControllerArray[ic].set_sShoulderBottom_right(listen);
							ListenMode = ControllerVarEnum.Interface_left;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;

					case ControllerVarEnum.Interface_left:
						listen = SetModeGetString(ic);
						//Debug.Log("Interface_left");
						if(listen != "")
						{
							ControllerArray[ic].set_sInterface_left(listen);
							ListenMode = ControllerVarEnum.Interface_right;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;
						
					case ControllerVarEnum.Interface_right:
						listen = SetModeGetString(ic);
						//Debug.Log("Interface_right");
						if(listen != "")
						{
							ControllerArray[ic].set_sInterface_right(listen);
							ListenMode = ControllerVarEnum.dpad_up;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;

					

					case ControllerVarEnum.dpad_up:
						listen = SetModeGetString(ic);
						//Debug.Log("dpad_up");
						if(listen == "")
						{
							listen = SetModeGetStringDPadAxis(ic); //because down can produce negative values
							if(listen != "")
							{
								//Check for a negative axis, if it's negative, the dPad is inverted.
								ControllerArray[ic].dPadInverted = true;
							}
						}
						if(listen != "")
						{
							if( listen[3] == 'A')
							{
								ControllerArray[ic].dPadIsAxis = true; //Need to flag that the dPad is using axis control
							}
							else
							{
								ControllerArray[ic].dPadIsAxis = false; //revert back, in case user made a mistake
							}
							ControllerArray[ic].set_sdpad_up(listen);
							ListenMode = ControllerVarEnum.dpad_down;
							SetMode = SetModeEnum.WAIT_DPAD;
							if (SingleSetMode) EndProgramming();
						}
						break;
					case ControllerVarEnum.dpad_down:
						listen = SetModeGetString(ic);
						if(listen == "")
						{
							listen = SetModeGetStringDPadAxis(ic); //because down can produce negative values
						}
						//Debug.Log("dpad_down");
						if(listen != "")
						{
							if( listen[3] == 'A')
							{
								ControllerArray[ic].dPadIsAxis = true; //Need to flag that the dPad is using axis control
							}
							else
							{
								ControllerArray[ic].dPadIsAxis = false; //revert back, in case user made a mistake
							}
							ControllerArray[ic].set_sdpad_down(listen);
							ListenMode = ControllerVarEnum.dpad_left;
							SetMode = SetModeEnum.WAIT_DPAD;
							if (SingleSetMode) EndProgramming();
						}
						break;

					case ControllerVarEnum.dpad_left:
						listen = SetModeGetString(ic);
						if(listen == "")
						{
							listen = SetModeGetStringDPadAxis(ic); //because left can produce negative values
						}
						//Debug.Log("dpad_left");
						if(listen != "")
						{
							if( listen[3] == 'A')
							{
								ControllerArray[ic].dPadIsAxis = true; //Need to flag that the dPad is using axis control
							}
							else
							{
								ControllerArray[ic].dPadIsAxis = false; //revert back, in case user made a mistake
							}
							ControllerArray[ic].set_sdpad_left(listen);
							ListenMode = ControllerVarEnum.dpad_right;
							SetMode = SetModeEnum.WAIT_DPAD;
							if (SingleSetMode) EndProgramming();
						}
						break;

					case ControllerVarEnum.dpad_right:
						listen = SetModeGetString(ic);
						//Debug.Log("dpad_right");
						if(listen != "")
						{
							if( listen[3] == 'A')
							{
								ControllerArray[ic].dPadIsAxis = true; //Need to flag that the dPad is using axis control
							}
							else
							{
								ControllerArray[ic].dPadIsAxis = false; //revert back, in case user made a mistake
							}
							ControllerArray[ic].set_sdpad_right(listen);
							ListenMode = ControllerVarEnum.FP_top;
							SetMode = SetModeEnum.WAIT_DPAD;
							if (SingleSetMode) EndProgramming();
						}
						break;

					case ControllerVarEnum.FP_top:
						listen = SetModeGetString(ic);
						//Debug.Log("FP_top");
						if(listen != "")
						{
							ControllerArray[ic].set_sFP_top(listen);
							ListenMode = ControllerVarEnum.FP_bottom;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;

					case ControllerVarEnum.FP_bottom:
						listen = SetModeGetString(ic);
						//Debug.Log("FP_bottom");
						if(listen != "")
						{
							ControllerArray[ic].set_sFP_bottom(listen);
							ListenMode = ControllerVarEnum.FP_left;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;

					case ControllerVarEnum.FP_left:
						listen = SetModeGetString(ic);
						//Debug.Log("FP_left");
						if(listen != "")
						{
							ControllerArray[ic].set_sFP_left(listen);
							ListenMode = ControllerVarEnum.FP_right;
							SetMode = SetModeEnum.WAIT;
							if (SingleSetMode) EndProgramming();
						}
						break;

					case ControllerVarEnum.FP_right:
						listen = SetModeGetString(ic);
						//Debug.Log("FP_right");
						if(listen != "")
						{
							ControllerArray[ic].set_sFP_right(listen);
							//ListenMode = ControllerVarEnum.Interface_left;
							SetMode = SetModeEnum.WAIT;
							EndProgramming();
						}
						break;

					}//switch(ListenMode)
					break;
				case SetModeEnum.EXIT:
					break;
				}//switch(SetMode)
				
				
				break;
			}//switch(ControllerMode)

		}//Update

		private void EndProgramming()
		{
			//Finish the controller programming cycle
			ControllerMode = ControllerModeEnum.OFF;
			SetMode = SetModeEnum.ENTER;
			ListenMode = ControllerVarEnum.ThumbLeft_x;
			//check for full range triggers
			if(ControllerArray[ic].get_ShoulderBottom_left() < -0.99f)
			{
				ControllerArray[ic].FullRangeTriggers = true;
			}
			SaveController(ic);
			//LoadController(ic);
			CurrentlyProgramming = false;
			SingleSetMode = false;
			ic = 0;
		}//EndProgramming

		private void UpdateControllers()
		{
			for (int i = 0; i < NumControllers; i++)
			{
				ControllerArray[i].UpdateValues();
			}
		}//Updatecontrollers

		private bool ContinueWait(int cntrlr)
		{
			//return true if any of the controller values are 1.0
			//needs to read ALL inputs

			//don't need to check negatives or lock lists for anything not dPad
			string result;
			for(int i = 1; i < 11; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_A" + i.ToString());
				
				if (Input.GetAxis(result) >= 0.5f )
				{
					return true;
				}
			}
			for (int i = 0; i <20; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_B" + i.ToString());
				
				if (Input.GetAxis(result) >= 0.5f)
				{
					return true;
				}
			}


			
			return false;	
		}//ContinueWait

		private bool ContinueWait_dPad(int cntrlr)
		{
			//since dpad can be inverted, look for both positive and negative, but avoid 
			//checking the axes in the locked list
			string result;
			for(int i = 1; i < 11; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_A" + i.ToString());
				
				if (Input.GetAxis(result) >= 0.5f ||Input.GetAxis(result) <= -0.5f)
				{
					if(!InLockedList(result))
						return true;
				}
			}
			for (int i = 0; i <20; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_B" + i.ToString());
				
				if (Input.GetAxis(result) >= 0.5f || Input.GetAxis(result) <= -0.5f)
				{
					if(!InLockedList(result))
						return true;
				}
			}
			return false;
		}//ContinueWait_dPad

		private bool InLockedList(string ctr)
		{
			bool locked = false;
			foreach(string element in IPInstance.LockedInput)
			{
				if (ctr == element) 
				{
					locked = true;
				}
			}
			return locked;
		}//InLockedList

		private void RemoveFromLockedList(string rmv)
		{
			List <string> templist = new List<string>();

			foreach (string element in IPInstance.LockedInput)
			{
				//copy to new list, excluding the element we want to remove
				if (rmv != element) 
					templist.Add(element);
			}
			IPInstance.LockedInput = templist;
		}

		private string CheckNegFlags(int cntrlr)
		{
			//for the detection of full range triggers on the left and right bottom shoulder buttons.
			//detect movement in previously locked controls, this will indicate a likely full range trigger
			string result;


			for(int i = 0; i < 10; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_A" + (i + 1).ToString());
				
				if (Input.GetAxis(result) <= -0.01f)
				{
					negFlag[i] = true;
				}
			}
			return "";

		}//SetModeGetStringTrigger

		private string SetModeGetStringDPadAxis(int cntrlr)
		{
			//special case for dPad
			//only returns a non-blank result on a negative read value
			string result;
			for(int i = 1; i < 11; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_A" + i.ToString());
				
				if (Input.GetAxis(result) <= -0.9f)
				{
					//Search confirm we are not ignoring/locking this axis (some controllers report unused axis as -1)
					if(!InLockedList(result))
						return result;
				}
			}
			return "";
		}//SetModeGetStringDPadAxis

		private string SetModeGetStringTrigger(int cntrlr)
		{
			//search for the trigger, because it can be a full range trigger, InLockedList is not used.
			string result;
			for(int i = 1; i < 11; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_A" + i.ToString());
				if((Input.GetJoystickNames()[cntrlr] == "Controller (Xbox 360 Wireless Receiver for Windows)"
				   || Input.GetJoystickNames()[cntrlr] == "Controller (XBOX 360 For Windows)") && i == 3)
				{
					//do nothing
				}
				else
				{
					if (Input.GetAxis(result) >= 0.9f)//|| Input.GetAxis(result) <= -0.9f)
					{
						return result;
					}
				}
			}
			for (int i = 0; i <20; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_B" + i.ToString());
				
				if (Input.GetAxis(result) >= 0.9f)// || Input.GetAxis(result) <= -0.9f)
				{
					return result;
				}
			}
			return "";
		}//SetModeGetStringTrigger

		private string SetModeGetString(int cntrlr)
		{
			string result;
			for(int i = 1; i < 11; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_A" + i.ToString());
				
				if (Input.GetAxis(result) >= 0.9f)//|| Input.GetAxis(result) <= -0.9f)
				{
					//Search confirm we are not ignoring/locking this axis (some controllers report unused axis as -1)

					if(!InLockedList(result))
						return result;
				}
			}
			for (int i = 0; i <20; i++)
			{
				result = ("J" + (cntrlr + 1).ToString() + "_B" + i.ToString());
				
				if (Input.GetAxis(result) >= 0.9f)// || Input.GetAxis(result) <= -0.9f)
				{
					return result;
				}
			}
			
			// no results
			return "";
			
		}//SetModeGetString

		private void SaveController(int ctr)
		{
			string fileName = Application.persistentDataPath + "/" + "ControllerData.txt";
			string input;
			string output = "";

			if (System.IO.File.Exists (fileName)) 
			{
				input = System.IO.File.ReadAllText (fileName);
				string [] linesArray = input.Split ('\r'); //file is split into array of lines

				bool controllerExists = false;
				for(int i = 0; i < linesArray.Length; i++)
				{
					if(linesArray[i] != "")
					{
						string [] valuesArray = linesArray[i].Split(',');
						string joystick = Input.GetJoystickNames()[ctr];
						if (joystick.Contains(","))
						{
							joystick = joystick.Replace(",", " "); //remove comma, as it's the seperator for the text file, and it's also removed in saving.
						}

						if (valuesArray[0] != joystick) //zero is the joystick name
						{
							output = output + linesArray[i] + "\r"; //skip this line and add it, as is, into the output
						}
						else
						{
							output = output + MakeCtrLine(ctr);
							controllerExists = true;
						}
					}
				}
				if(!controllerExists)
				{
					output = output + MakeCtrLine(ctr);
					controllerExists = true;
				}
			}
			else
			{
				output = MakeCtrLine(ctr);
			}

			System.IO.File.WriteAllText(fileName, output);
			if(IPInstance.DebugText) Debug.Log("wrote to " + fileName);		
		}//SaveController

		string MakeCtrLine(int ctr)
		{

			string output = "";
			output = output + Input.GetJoystickNames()[ctr];
			if (output.Contains(","))
			{
				output = output.Replace(",", " "); //remove comma for parsing, else splitting won't work with names with commas
			}
			output = output + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sThumbLeft_x()) + ","; //remove specific controller number, 
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sThumbLeft_y()) + ","; //this way it can be load regardless of controller number
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sThumbLeft()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sThumbRight_x()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sThumbRight_y()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sThumbRight()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sShoulderTop_left()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sShoulderTop_right()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sShoulderBottom_left()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sShoulderBottom_right()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sInterface_left()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sInterface_right()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sdpad_up()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sdpad_down()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sdpad_left()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sdpad_right()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sFP_top()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sFP_bottom()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sFP_left()) + ",";
			output = output + RemoveCtrNum(ControllerArray[ctr].get_sFP_right()) + ",";

			output = output + ControllerArray[ctr].dPadIsAxis.ToString() + ",";
			output = output + ControllerArray[ctr].dPadInverted.ToString() + ",";
			output = output + ControllerArray[ctr].FullRangeTriggers.ToString() + ",";
			output = output + ControllerArray[ctr].DeadZone.ToString();
			output = output + "\r";
			return output;
		}//MakeCtrLine

		string RemoveCtrNum(string ctr)
		{
			string [] readArray = ctr.Split ('_');
			return readArray [1];
		}//RemoveCtrNum

		private void LoadControllers()
		{
			string fileName = Application.persistentDataPath + "/" + "ControllerData.txt";
			string input;

			if(System.IO.File.Exists(fileName))
			{
				input = System.IO.File.ReadAllText(fileName);
			}
			else
			{
				return; //do nothing if the file doesn't exist.
			}

			string [] readArray = input.Split('\r');

			for(int i = 0; i < readArray.Length; i++)
			{
				if(readArray[i] != "")
				{
					string [] lineArray = readArray[i].Split(',');
					for(int k = 0; k < Input.GetJoystickNames().Length; k++)
					{
						int j = 0;
						string joystick = Input.GetJoystickNames()[k];
						if(joystick.Contains(","))
						{
							joystick = joystick.Replace(",", " "); //matching comma replacement method used in save method
						}
						if (joystick == lineArray[j]) //lineArray 0 is saved joystick name
						{
							//load joystick here
							if(lineArray[i] != "")
							{
								//assign values
								int ctr = k+1;
								ControllerArray[k].set_sThumbLeft_x("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sThumbLeft_y("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sThumbLeft("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sThumbRight_x("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sThumbRight_y("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sThumbRight("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sShoulderTop_left("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sShoulderTop_right("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sShoulderBottom_left("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sShoulderBottom_right("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sInterface_left("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sInterface_right("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sdpad_up("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sdpad_down("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sdpad_left("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sdpad_right("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sFP_top("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sFP_bottom("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sFP_left("J" + ctr.ToString() + "_" + lineArray[++j]);
								ControllerArray[k].set_sFP_right("J" + ctr.ToString() + "_" + lineArray[++j]);

								ControllerArray[k].dPadIsAxis = bool.Parse(lineArray[++j]);
								ControllerArray[k].dPadInverted = bool.Parse(lineArray[++j]);
								ControllerArray[k].FullRangeTriggers = bool.Parse(lineArray[++j]);
								ControllerArray[k].DeadZone = float.Parse(lineArray[++j]);
							}
						}
					}
				}
			}
			if(IPInstance.DebugText) Debug.Log("Done Loading Controllers");
		}


		private void ScanForLocked()
		{
			/*
			 * ScanForLocked should be called before recording any controller data(see mac info below). 
			 * A message should be displayed to the user informing them to refrain from pressing any buttons or moving any joysticks until further
			 * instructed. The controllers are then scanned for any controllers which have buttons or axes that are always on.
			 * 
			 * While programming generally searches for positive values, some controllers have inverted axis reporting dPads.
			 * So -1 detection is needed for those types of dPads.
			 * 
			 * Previously there was support for axes always reporting 1, but that was removed as it is though that this isn't likely exist.
			 * 
			 * Mac OS X note: it seems on the mac that until the controls on a controller are used, they always report 0.
			 * So confirming full range triggers can only be done after those triggers have moved. Full range triggers have been discovered on
			 * OS X (specifically with the Xbox 360 controller), and these start reporting at -1 and move to 1 when pulled, using an axis. 
			 * It's possible that there could be controllers like this on other platforms so the choice is to support them.
			 */ 

			IPInstance.LockedInput.Clear();
			for(int cntrlr = 0; cntrlr < IPInstance.NumControllers; cntrlr++)
			{
				string result;
				for(int i = 1; i < Constants.MAX_CTRL_SIZE; i++)
				{
					result = ("J" + (cntrlr + 1).ToString() + "_A" + i.ToString());
					
					if (Input.GetAxis(result) <= -0.99f)
					{
						bool exists = false;
						foreach(string element in IPInstance.LockedInput)
						{
							if(element == result)
							{
								exists = true; //already in the list, no need to add it.
							}
						}
						if(!exists)
						{
							IPInstance.LockedInput.Add(result);
						}
					}
				}
				for (int i = 0; i <20; i++)
				{
					result = ("J" + (cntrlr + 1).ToString() + "_B" + i.ToString());
					
					if (Input.GetAxis(result) <= -0.99f)
					{
						bool exists = false;
						foreach(string element in IPInstance.LockedInput)
						{
							if(element == result)
							{
								exists = true; //already in the list, no need to add it.
							}
						}
						if(!exists)
						{
							IPInstance.LockedInput.Add(result);
						}
					}
				}
				if(Input.GetJoystickNames()[cntrlr] == "Controller (Xbox 360 Wireless Receiver for Windows)"
				   || Input.GetJoystickNames()[cntrlr] == "Controller (XBOX 360 For Windows)")	{
					//ignore A3 on xbox wireless and wired... Axis 3 is the spawn of evil in the land of xbox controllers
					//normally a specific controller shouldn't get an exception, but given the popularity of the 360
					//controller, there is this exception. Hopefully there won't be too many of these.
					result = ("J" + (cntrlr + 1).ToString() + "_A" + 3.ToString());
					IPInstance.LockedInput.Add(result);
				}
			}	
		}//ScanForLocked

		private void ResetNegFlag()
		{
			for(int i = 0; i < 10; i++)
			{
				negFlag[i] = false;
			}
		}

		//--------------------------- GUI ---------------------------------//

		void OnGUI()
		{
			if (DebugText)
			{
				switch(IPInstance.ControllerMode)
				{		
				case ControllerModeEnum.DISPLAY: //Display Unity controller values and assignments
					GUILayout.Label("\n\n\nRaw Values:");
					GUILayout.BeginHorizontal();
					
					for (int j = 1; j < 12; j++)
					{
						string output = string.Empty;

						GUILayout.BeginVertical(GUILayout.Width(128));
						GUILayout.Label("\t\tController " + j.ToString());
						for (int a = 1; a < Constants.MAX_CTRL_SIZE; a++)
						{
							string input = "J" + j.ToString() + "_A" + a.ToString();
							output += "\t\t\t" + input + " " + Input.GetAxis(input).ToString("0.00") + "\n";
						}
						
						for (int b = 0; b < 20; b++)
						{
							string input = "J" + j.ToString() + "_B" + b.ToString();
							output += "\t\t\t" + input + " " + Input.GetAxis(input).ToString("0.00") + "\n";
						}
						GUILayout.Label(output);
						if (Input.GetJoystickNames().Length < (j))
						{
							GUILayout.Label("\t\t\t" + "null " + Input.GetJoystickNames().Length.ToString());
						}
						else
						{
							GUILayout.Label("\t\t\t" + Input.GetJoystickNames()[j-1]);
						}
						GUILayout.EndVertical();
						
					}
					
					GUILayout.EndHorizontal();	
					break;
				case ControllerModeEnum.DISPLAY2: //display InputPlus controller value

					GUILayout.BeginHorizontal();
					GUILayout.Label("\n\n\nProgrammed results as InputPlus see the controllers\n");
					GUILayout.EndHorizontal();
					GUILayout.Label("\n");
					for (int i = 0; i < NumControllers; i++)
					{
						string output = string.Empty;
						GUILayout.BeginVertical(GUILayout.Width(190));
						GUILayout.Label("\nController " + (i + 1).ToString());
						output += "\t" + "ThumbLeft_x: \t" + IPInstance.ControllerArray[i].get_ThumbLeft_x().ToString("0.00") + "\n";
						output += "\t" + "ThumbLeft_y: \t" + IPInstance.ControllerArray[i].get_ThumbLeft_y().ToString("0.00") + "\n";
						output += "\t" + "ThumbLeft: \t" + IPInstance.ControllerArray[i].get_ThumbLeft().ToString("0.00") + "\n";
						output += "\t" + "ThumbRight_x: \t" + IPInstance.ControllerArray[i].get_ThumbRight_x().ToString("0.00") + "\n";
						output += "\t" + "ThumbRight_y: \t" + IPInstance.ControllerArray[i].get_ThumbRight_y().ToString("0.00") + "\n";
						output += "\t" + "ThumbRight: \t" + IPInstance.ControllerArray[i].get_ThumbRight().ToString("0.00") + "\n";
						output += "\t" + "ShoulderTop_left: \t" + IPInstance.ControllerArray[i].get_ShoulderTop_left().ToString("0.00") + "\n";
						output += "\t" + "ShoulderTop_right: \t" + IPInstance.ControllerArray[i].get_ShoulderTop_right().ToString("0.00") + "\n";
						output += "\t" + "ShoulderBottom_left: \t" + IPInstance.ControllerArray[i].get_ShoulderBottom_left().ToString("0.00") + "\n";
						output += "\t" + "ShoulderBottom_right: \t" + IPInstance.ControllerArray[i].get_ShoulderBottom_right().ToString("0.00") + "\n";
						output += "\t" + "Interface_left: \t" + IPInstance.ControllerArray[i].get_Interface_left().ToString("0.00") + "\n";
						output += "\t" + "Interface_right: \t" + IPInstance.ControllerArray[i].get_Interface_right().ToString("0.00") + "\n";
						output += "\t" + "dpad_up: \t" + IPInstance.ControllerArray[i].get_dpad_up().ToString("0.00") + "\n";
						output += "\t" + "dpad_down: \t" + IPInstance.ControllerArray[i].get_dpad_down().ToString("0.00") + "\n";
						output += "\t" + "dpad_left: \t" + IPInstance.ControllerArray[i].get_dpad_left().ToString("0.00") + "\n";
						output += "\t" + "dpad_right: \t" + IPInstance.ControllerArray[i].get_dpad_right().ToString("0.00") + "\n";
						output += "\t" + "FP_top: \t" + IPInstance.ControllerArray[i].get_FP_top().ToString("0.00") + "\n";
						output += "\t" + "FP_bottom: \t" + IPInstance.ControllerArray[i].get_FP_bottom().ToString("0.00") + "\n";
						output += "\t" + "FP_left: \t" + IPInstance.ControllerArray[i].get_FP_left().ToString("0.00") + "\n";
						output += "\t" + "FP_right: \t" + IPInstance.ControllerArray[i].get_FP_right().ToString("0.00") + "\n";

						GUILayout.Label(output);
						GUILayout.EndVertical();
					}
					break;
				case ControllerModeEnum.SET:
					GUILayout.Label("\n");
					GUILayout.BeginVertical(GUILayout.Width(250));
					{
						string output = string.Empty;
						GUILayout.Label("\n\n\nNow Programming - Controller: " + (ic + 1).ToString());
						output += "\t" + "ThumbLeft_x: \t" + IPInstance.ControllerArray[ic].get_sThumbLeft_x() + "\n";
						output += "\t" + "ThumbLeft_y: \t" + IPInstance.ControllerArray[ic].get_sThumbLeft_y() + "\n";
						output += "\t" + "ThumbLeft: \t" + IPInstance.ControllerArray[ic].get_sThumbLeft() + "\n";
						output += "\t" + "ThumbRight_x: \t" + IPInstance.ControllerArray[ic].get_sThumbRight_x() + "\n";
						output += "\t" + "ThumbRight_y: \t" + IPInstance.ControllerArray[ic].get_sThumbRight_y() + "\n";
						output += "\t" + "ThumbRight: \t" + IPInstance.ControllerArray[ic].get_sThumbRight() + "\n";
						output += "\t" + "ShoulderTop_left: \t" + IPInstance.ControllerArray[ic].get_sShoulderTop_left() + "\n";
						output += "\t" + "ShoulderTop_right: \t" + IPInstance.ControllerArray[ic].get_sShoulderTop_right() + "\n";
						output += "\t" + "ShoulderBottom_left: \t" + IPInstance.ControllerArray[ic].get_sShoulderBottom_left() + "\n";
						output += "\t" + "ShoulderBottom_right: \t" + IPInstance.ControllerArray[ic].get_sShoulderBottom_right() + "\n";
						output += "\t" + "Interface_left: \t" + IPInstance.ControllerArray[ic].get_sInterface_left() + "\n";
						output += "\t" + "Interface_right: \t" + IPInstance.ControllerArray[ic].get_sInterface_right() + "\n";
						output += "\t" + "dpad_up: \t" + IPInstance.ControllerArray[ic].get_sdpad_up() + "\n";
						output += "\t" + "dpad_down: \t" + IPInstance.ControllerArray[ic].get_sdpad_down() + "\n";
						output += "\t" + "dpad_left: \t" + IPInstance.ControllerArray[ic].get_sdpad_left() + "\n";
						output += "\t" + "dpad_right: \t" + IPInstance.ControllerArray[ic].get_sdpad_right() + "\n";
						output += "\t" + "FP_top: \t" + IPInstance.ControllerArray[ic].get_sFP_top() + "\n";
						output += "\t" + "FP_bottom: \t" + IPInstance.ControllerArray[ic].get_sFP_bottom() + "\n";
						output += "\t" + "FP_left: \t" + IPInstance.ControllerArray[ic].get_sFP_left() + "\n";
						output += "\t" + "FP_right: \t" + IPInstance.ControllerArray[ic].get_sFP_right() + "\n";

						GUILayout.Label(output);
					}
					break;
				}
			}
		}//OnGUI




		//-------------- Interface Functions --------------//




		public static void Initialize()
		{
			Debug.Log("Initialize InputPlus\n");

			IPInstance.NumControllers = Input.GetJoystickNames().Length;

			IPInstance.ScanForLocked();

			IPInstance.ControllerArray = new Controller[IPInstance.NumControllers];
			for(int i = 0; i < IPInstance.ControllerArray.Length; i++)
			{
				IPInstance.ControllerArray[i] = new Controller();
			}

			foreach (Controller element in IPInstance.ControllerArray)
			{
				element.DeadZone = Constants.DEAD_ZONE;
			}
		}

		public static void SetDebugText(bool setting) 
		{
			IPInstance.DebugText = setting;
			Debug.Log("InputPlus DebugText set to " + setting.ToString());
		}

		public static float GetData(int ctrlnum, ControllerVarEnum control)
		{
			if ((ctrlnum < 1) || (ctrlnum > Constants.MAX_CTRL_SIZE))
			{
				if (IPInstance.DebugText) Debug.Log("cntrlnum in InputPlus.GetData must be between 1 - " 
				          + Constants.MAX_CTRL_SIZE.ToString() + ", using 1 as default");
				ctrlnum = 1;
			}
			
			switch (control)
			{
			case ControllerVarEnum.dpad_up :
				return IPInstance.ControllerArray[ctrlnum - 1].get_dpad_up();
			case ControllerVarEnum.dpad_down:
				return IPInstance.ControllerArray[ctrlnum - 1].get_dpad_down();
			case ControllerVarEnum.dpad_left :
				return IPInstance.ControllerArray[ctrlnum - 1].get_dpad_left();
			case ControllerVarEnum.dpad_right :
				return IPInstance.ControllerArray[ctrlnum - 1].get_dpad_right();
			case ControllerVarEnum.FP_top :
				return IPInstance.ControllerArray[ctrlnum - 1].get_FP_top();
			case ControllerVarEnum.FP_bottom :
				return IPInstance.ControllerArray[ctrlnum - 1].get_FP_bottom();
			case ControllerVarEnum.FP_left :
				return IPInstance.ControllerArray[ctrlnum - 1].get_FP_left();
			case ControllerVarEnum.FP_right :
				return IPInstance.ControllerArray[ctrlnum - 1].get_FP_right();
			case ControllerVarEnum.Interface_right :
				return IPInstance.ControllerArray[ctrlnum - 1].get_Interface_right();
			case ControllerVarEnum.Interface_left :
				return IPInstance.ControllerArray[ctrlnum - 1].get_Interface_left();
			case ControllerVarEnum.ShoulderTop_right :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ShoulderTop_right();
			case ControllerVarEnum.ShoulderTop_left :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ShoulderTop_left();
			case ControllerVarEnum.ShoulderBottom_right :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ShoulderBottom_right();
			case ControllerVarEnum.ShoulderBottom_left :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ShoulderBottom_left();
			case ControllerVarEnum.ThumbRight_x :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ThumbRight_x();
			case ControllerVarEnum.ThumbRight_y :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ThumbRight_y();
			case ControllerVarEnum.ThumbRight :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ThumbRight();
			case ControllerVarEnum.ThumbLeft_x :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ThumbLeft_x();
			case ControllerVarEnum.ThumbLeft_y :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ThumbLeft_y();
			case ControllerVarEnum.ThumbLeft :
				return IPInstance.ControllerArray[ctrlnum - 1].get_ThumbLeft();
			}//switch (control) 
			
			if (IPInstance.DebugText) Debug.Log("InputPlus.GetData failed to get any data, returning 0");
			return 0;
		}//GetData

		static public void LearnController(int controller)
		{
			// learn a controller
			if(IPInstance.NumControllers >= controller)
			{
				IPInstance.ControllerMode = ControllerModeEnum.SET;
				IPInstance.SetMode = SetModeEnum.ENTER;
				IPInstance.ListenMode = ControllerVarEnum.ThumbLeft_x;
				IPInstance.ic = controller - 1;
				IPInstance.ControllerArray[IPInstance.ic].ClearSettings();
				IPInstance.SingleSetMode = false;
				IPInstance.ResetNegFlag(); 
				//reset all negFlag bools to false, for properly identifying full range triggers
				if(IPInstance.DebugText) Debug.Log ("Setting IC: " + IPInstance.ic.ToString());
			}
			else
			{
				if (IPInstance.DebugText) Debug.Log ("LearnController request out of range");
			}
		}//LearnController

		static public void LearnControllerSingle(int num, ControllerVarEnum ctrl)
		{
			//learn a single control. ex. dpad_down

			if (num <= IPInstance.NumControllers)
			{
				IPInstance.SingleSetMode = true;
				IPInstance.ControllerMode = ControllerModeEnum.SET;
				IPInstance.SetMode = SetModeEnum.ENTER;
				IPInstance.ListenMode = ctrl;
				IPInstance.ic = num - 1;
				if(IPInstance.ListenMode == ControllerVarEnum.ShoulderBottom_left || 
				   IPInstance.ListenMode == ControllerVarEnum.ShoulderBottom_right)
				{
					//need to reset FullRangeTriggers, will be set again when learning
					IPInstance.ControllerArray[IPInstance.ic].FullRangeTriggers = false;
				}
			}
			
		}//LearnControllerSignle

		static public void ToggleDataView()
		{

			switch(IPInstance.ControllerMode)
			{
			case ControllerModeEnum.OFF :
				IPInstance.ControllerMode = ControllerModeEnum.DISPLAY;
				break;
			case ControllerModeEnum.DISPLAY :
				IPInstance.ControllerMode = ControllerModeEnum.DISPLAY2;
				break;
			case ControllerModeEnum.DISPLAY2 :
				IPInstance.ControllerMode = ControllerModeEnum.OFF;
				break;	
			case ControllerModeEnum.SET :
				if (IPInstance.DebugText) Debug.Log("Can't toggle out of SET mode - ToggleDataView()");
				break;
			}

		}//ToggleDataView

		static public bool GetProgrammingStatus()
		{
			return IPInstance.CurrentlyProgramming;
		}//GetProgrammingStatus

		static public ControllerVarEnum GetWaitingForControl()
		{
			return IPInstance.ListenMode;
		}//GetListenMode

		static public void CancelProgramming()
		{
			//Stop the current programming sequence, currently programmed values will hold, but won't be saved.
			//This is not an Undo at the moment

			//Finish the controller programming cycle
			IPInstance.ControllerMode = ControllerModeEnum.OFF;
			IPInstance.SetMode = SetModeEnum.ENTER;
			IPInstance.ListenMode = ControllerVarEnum.ThumbLeft_x;
			//check for full range triggers
			if(IPInstance.ControllerArray[IPInstance.ic].get_ShoulderBottom_left() < -0.99f)
			{
				IPInstance.ControllerArray[IPInstance.ic].FullRangeTriggers = true;
			}
			//SaveController(ic);
			//LoadController(ic);
			IPInstance.CurrentlyProgramming = false;
			IPInstance.SingleSetMode = false;
			IPInstance.ic = 0;

		}//CancelProgramming

		static public string GetControllerName(int controller)
		{
			//get the name of the controller
			if (controller > Input.GetJoystickNames().Length || controller < 1)
			{
				if(IPInstance.DebugText) Debug.Log("InputPlus.GetControllerName request out of range");
				return "";
			}
			else
			{
				return Input.GetJoystickNames()[controller - 1];
			}
		}

		static public int GetListeningFor()
		{
			//return the controller number that is currently being listened to for programming
			return IPInstance.ic +1;
		}

		static public float GetDeadZone(int controller)
		{
			controller--;
			if (controller < 0 || controller >= IPInstance.ControllerArray.Length)
			{
				if (IPInstance.DebugText) Debug.Log("InputPlus.GetDeadZone out of range, returning 0.0");
				return 0.0f;
			}
			else
			{
				return IPInstance.ControllerArray[controller].DeadZone;
			}
		}

		static public void SetDeadZone(int controller, float dead_zone)
		{
			controller--;
			if (controller < 0 || controller >= IPInstance.ControllerArray.Length)
			{
				if (IPInstance.DebugText) Debug.Log("InputPlus.SetDeadZone controller out of range");
			}
			else
			{
				IPInstance.ControllerArray[controller].DeadZone = dead_zone;
			}
		}

	}//InputPlus

	
}//namespace InputPlusControl



class Controller
{
	public bool dPadIsAxis;
	public bool dPadInverted;
	public bool FullRangeTriggers;
	public float DeadZone;

	private bool LeftTriggerMoved = false;
	private bool RightTriggerMoved = false;

	private float 	//axis/button values
		dpad_up,
		dpad_down,
		dpad_left,
		dpad_right,
		FP_top,
		FP_bottom,
		FP_left,
		FP_right,
		Interface_right,
		Interface_left,
		ShoulderTop_right,
		ShoulderTop_left,
		ShoulderBottom_right,
		ShoulderBottom_left,
		ThumbRight_x,
		ThumbRight_y,
		ThumbRight,
		ThumbLeft_x,
		ThumbLeft_y,
		ThumbLeft;
	
	private bool bdpad_up,
		bdpad_down,
		bdpad_left,
		bdpad_right,
		bFP_top,
		bFP_bottom,
		bFP_left,
		bFP_right,
		bInterface_right,
		bInterface_left,
		bShoulderTop_right,
		bShoulderTop_left,
		bShoulderBottom_right,
		bShoulderBottom_left,
		bThumbRight_x,
		bThumbRight_y,
		bThumbRight,
		bThumbLeft_x,
		bThumbLeft_y,
		bThumbLeft;
	
	private string 	//axis/button assignments
		sdpad_up,
		sdpad_down,
		sdpad_left,
		sdpad_right,
		sFP_top,
		sFP_bottom,
		sFP_left,
		sFP_right,
		sInterface_right,
		sInterface_left,
		sShoulderTop_right,
		sShoulderTop_left,
		sShoulderBottom_right,
		sShoulderBottom_left,
		sThumbRight_x,
		sThumbRight_y,
		sThumbRight,
		sThumbLeft_x,
		sThumbLeft_y,
		sThumbLeft;

	private float DeadZoneFilter(float value)
	{
		if (value < DeadZone && value > -DeadZone)
		{
			return 0.0f;
		}
		else
		{
			return value;
		}
	}
	
	public float get_dpad_up()
	{
		if(dPadIsAxis)
		{
			if(dPadInverted)
			{
				if(dpad_down < -0.7f)
					return 1.0f;
				else
					return 0.0f;
			}
			if(dpad_up > 0.7f)
				return 1.0f;
			else
				return 0.0f;
		}
		else
		{
			return dpad_up;
		}
	}
	public float get_dpad_down()
	{
		if(dPadIsAxis)
		{
			if(dPadInverted)
			{
				if(dpad_down > 0.7f)
					return 1.0f;
				else
					return 0.0f;
			}
			if(dpad_down < -0.7f)
			{
				return 1.0f;
			}
			else
				return 0.0f;
		}
		else
		{
			return dpad_down;
		}
	}
	public float get_dpad_left()
	{
		if(dPadIsAxis)
		{
			if(dpad_left < -0.7f)
				return 1.0f;
			else
				return 0.0f;
		}
		else
		{
			return dpad_left;
		}
	}
	public float get_dpad_right()
	{
		if(dPadIsAxis)
		{
			if(dpad_right > 0.7f)
				return 1.0f;
			else
				return 0.0f;
		}
		else
		{
			return dpad_right;
		}
	}
	public float get_FP_top()
	{
		return FP_top;
	}
	public float get_FP_bottom()
	{
		return FP_bottom;
	}
	public float get_FP_left()
	{
		return FP_left;
	}
	public float get_FP_right()
	{
		return FP_right;
	}
	public float get_Interface_right()
	{
		return Interface_right;
	}
	public float get_Interface_left()
	{
		return Interface_left;
	}
	public float get_ShoulderTop_right()
	{
		return ShoulderTop_right;
	}
	public float get_ShoulderTop_left()
	{
		return ShoulderTop_left;
	}
	public float get_ShoulderBottom_right()
	{
		if(FullRangeTriggers)
		{
			if(RightTriggerMoved == false)
			{
				return 0.0f; //return 0 if triggers haven't been moved and are still reporting 0
			}
			return DeadZoneFilter((ShoulderBottom_right + 1.0f) / 2.0f);
		}
		return DeadZoneFilter(ShoulderBottom_right);
	}
	public float get_ShoulderBottom_left()
	{
		if(FullRangeTriggers)
		{
			if(LeftTriggerMoved == false)
			{
				return 0.0f; //return 0 if triggers haven't been moved and are still reporting 0
			}
			return DeadZoneFilter((ShoulderBottom_left + 1.0f) / 2.0f);
		}
		return DeadZoneFilter(ShoulderBottom_left);
	}
	public float get_ThumbRight_x()
	{
		return DeadZoneFilter(ThumbRight_x);
	}
	public float get_ThumbRight_y()
	{
		return DeadZoneFilter(ThumbRight_y);
	}
	public float get_ThumbRight()
	{
		return ThumbRight;
	}
	public float get_ThumbLeft_x()
	{
		return DeadZoneFilter(ThumbLeft_x);
	}
	public float get_ThumbLeft_y()
	{
		return DeadZoneFilter(ThumbLeft_y);
	}
	public float get_ThumbLeft()
	{
		return ThumbLeft;
	}
	
	
	
	
	
	public string get_sdpad_up()
	{
		return sdpad_up;
	}
	public string get_sdpad_down()
	{
		return sdpad_down;
	}
	public string get_sdpad_left()
	{
		return sdpad_left;
	}
	public string get_sdpad_right()
	{
		return sdpad_right;
	}
	public string get_sFP_top()
	{
		return sFP_top;
	}
	public string get_sFP_bottom()
	{
		return sFP_bottom;
	}
	public string get_sFP_left()
	{
		return sFP_left;
	}
	public string get_sFP_right()
	{
		return sFP_right;
	}
	public string get_sInterface_right()
	{
		return sInterface_right;
	}
	public string get_sInterface_left()
	{
		return sInterface_left;
	}
	public string get_sShoulderTop_right()
	{
		return sShoulderTop_right;
	}
	public string get_sShoulderTop_left()
	{
		return sShoulderTop_left;
	}
	public string get_sShoulderBottom_right()
	{
		return sShoulderBottom_right;
	}
	public string get_sShoulderBottom_left()
	{
		return sShoulderBottom_left;
	}
	public string get_sThumbRight_x()
	{
		return sThumbRight_x;
	}
	public string get_sThumbRight_y()
	{
		return sThumbRight_y;
	}
	public string get_sThumbRight()
	{
		return sThumbRight;
	}
	public string get_sThumbLeft_x()
	{
		return sThumbLeft_x;
	}
	public string get_sThumbLeft_y()
	{
		return sThumbLeft_y;
	}
	public string get_sThumbLeft()
	{
		return sThumbLeft;
	}		

	
	public void set_sdpad_up(string s)
	{
		sdpad_up = s;
	}
	public void set_sdpad_down(string s)
	{
		sdpad_down = s;
	}
	public void set_sdpad_left(string s)
	{
		sdpad_left = s;
	}
	public void set_sdpad_right(string s)
	{
		sdpad_right = s;
	}
	public void set_sFP_top(string s)
	{
		sFP_top = s;
	}
	public void set_sFP_bottom(string s)
	{
		sFP_bottom = s;
	}
	public void set_sFP_left(string s)
	{
		sFP_left = s;
	}
	public void set_sFP_right(string s)
	{
		sFP_right = s;
	}
	public void set_sInterface_right(string s)
	{
		sInterface_right = s;
	}
	public void set_sInterface_left(string s)
	{
		sInterface_left = s;
	}
	public void set_sShoulderTop_right(string s)
	{
		sShoulderTop_right = s;
	}
	public void set_sShoulderTop_left(string s)
	{
		sShoulderTop_left = s;
	}
	public void set_sShoulderBottom_right(string s)
	{
		sShoulderBottom_right = s;
	}
	public void set_sShoulderBottom_left(string s)
	{
		sShoulderBottom_left = s;
	}
	public void set_sThumbRight_x(string s)
	{
		sThumbRight_x = s;
	}
	public void set_sThumbRight_y(string s)
	{
		sThumbRight_y = s;
	}
	public void set_sThumbRight(string s)
	{
		sThumbRight = s;
	}
	public void set_sThumbLeft_x(string s)
	{
		sThumbLeft_x = s;
	}
	public void set_sThumbLeft_y(string s)
	{
		sThumbLeft_y = s;
	}
	public void set_sThumbLeft(string s)
	{
		sThumbLeft = s;
	}
	
	
	public void ClearSettings()
	{
		sdpad_up = null;
		sdpad_down = null;
		sdpad_left = null;
		sdpad_right = null;
		sFP_top = null;
		sFP_bottom = null;
		sFP_left = null;
		sFP_right = null;
		sInterface_right = null;
		sInterface_left = null;
		sShoulderTop_right = null;
		sShoulderTop_left = null;
		sShoulderBottom_right = null;
		sShoulderBottom_left = null;
		sThumbRight_x = null;
		sThumbRight_y = null;
		sThumbRight = null;
		sThumbLeft_x = null;
		sThumbLeft_y = null;
		sThumbLeft = null;
		dPadIsAxis = false;
		dPadInverted = false;
		FullRangeTriggers = false;
	}//ClearSettings
	
	public void UpdateValues()
	{
		if (sdpad_up != null)
		{
			dpad_up = Input.GetAxis(sdpad_up);
		}
		if (sdpad_down != null)
		{
			dpad_down = Input.GetAxis(sdpad_down);
		}
		if (sdpad_left != null)
		{
			dpad_left = Input.GetAxis(sdpad_left);
		}
		if (sdpad_right != null)
		{		
			dpad_right = Input.GetAxis(sdpad_right);
		}
		if (sFP_top != null)
		{		
			FP_top = Input.GetAxis(sFP_top);
		}
		if (sFP_bottom != null)
		{					
			FP_bottom = Input.GetAxis(sFP_bottom);
		}
		if (sFP_left != null)
		{					
			FP_left = Input.GetAxis(sFP_left);
		}
		if (sFP_right != null)
		{					
			FP_right = Input.GetAxis(sFP_right);
		}
		if (sInterface_right != null)
		{					
			Interface_right = Input.GetAxis(sInterface_right);
		}
		if (sInterface_left != null)
		{					
			Interface_left = Input.GetAxis(sInterface_left);
		}
		if (sShoulderTop_right != null)
		{					
			ShoulderTop_right = Input.GetAxis(sShoulderTop_right);
		}
		if (sShoulderTop_left != null)
		{					
			ShoulderTop_left = Input.GetAxis(sShoulderTop_left);
		}
		if (sShoulderBottom_right != null)
		{					
			ShoulderBottom_right = Input.GetAxis(sShoulderBottom_right);
			if(RightTriggerMoved == false)
			{
				if (ShoulderBottom_right != 0.0f)
				{
					RightTriggerMoved = true;
				}
			} //For full range triggers
		}
		if (sShoulderBottom_left != null)
		{					
			ShoulderBottom_left = Input.GetAxis(sShoulderBottom_left);
			if(LeftTriggerMoved == false)
			{
				if (ShoulderBottom_left != 0.0f)
				{
					LeftTriggerMoved = true;
				}
			} //For Full range triggers
		}
		if (sThumbRight_x != null)
		{					
			ThumbRight_x = Input.GetAxis(sThumbRight_x);
		}
		if (sThumbRight_y != null)
		{					
			ThumbRight_y = Input.GetAxis(sThumbRight_y);
		}
		if (sThumbRight != null)
		{					
			ThumbRight = Input.GetAxis(sThumbRight);
		}
		if (sThumbLeft_x != null)
		{					
			ThumbLeft_x = Input.GetAxis(sThumbLeft_x);
		}
		if (sThumbLeft_y != null)
		{					
			ThumbLeft_y = Input.GetAxis(sThumbLeft_y);
		}
		if (sThumbLeft != null)
		{					
			ThumbLeft = Input.GetAxis(sThumbLeft);
		}
		
	}//UpdateValues
	
}//Controller
