using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InputPlusControl;
public enum InputType{
	GamePad,
	Buttons,
	MouseMovement
}
public enum MouseAxis{
	X,
	Y,
	Scroll
}
[System.Serializable]
public class UnityLikeInput {
	public string Name;

	[Tooltip("For keyboard only")]
	public KeyCode PositiveButton;
	[Tooltip("For keyboard only")]
	public KeyCode NegativeButton;

	public float Gravity;
	[Tooltip("It's better to be smaller than 1")]
	public float Dead;
	public float Sensivity = 1f;

	[Tooltip("For keyboard only")]
	public bool Snap;
	public bool Invert;

	public InputType Type;
	[Tooltip("For GamePad instead of Negative/Positive Button")]
	public ControllerVarEnum GamePadInput;
	[Tooltip("For GamePad buttons and DPad axis simulation")]
	public ControllerVarEnum GamePadInputNegative;
	public MouseAxis mouseAxis;
	[HideInInspector] public int con;
	bool previous_state_up;
	bool previous_state_down;
	float virtual_axis = 0;

	bool needsVirtual = false;
	public void Init(){
		if (Type == InputType.GamePad) {
			List<ControllerVarEnum> AxisTypes = new List<ControllerVarEnum>(){ ControllerVarEnum.ThumbLeft_x, ControllerVarEnum.ThumbLeft_y, ControllerVarEnum.ThumbRight_x, ControllerVarEnum.ThumbRight_y };	
			if (!AxisTypes.Contains (GamePadInput)&&!AxisTypes.Contains (GamePadInputNegative)) {
				needsVirtual = true;
			}
		}
	}

	public float GetAxis(){
		float result = 0f;
		switch (Type) {
		case InputType.GamePad:
			if (needsVirtual) {
				if (InputPlus.GetData (con, GamePadInput)==1f) {
					if (Snap && virtual_axis < 0f) {
						virtual_axis = 0;
					}
					virtual_axis += Sensivity * Time.deltaTime;
				} else if (InputPlus.GetData (con, GamePadInputNegative)==1f) {
					if (Snap && virtual_axis > 0f) {
						virtual_axis = 0;
					}
					virtual_axis -= Sensivity * Time.deltaTime;
				} else {
					virtual_axis = Mathf.MoveTowards (virtual_axis, 0, Gravity * Time.deltaTime);
				}
				virtual_axis = Mathf.Clamp (virtual_axis, -1f, 1f);
				result = virtual_axis;
			}else{
				result = InputPlus.GetData (con, GamePadInput) * Sensivity;
				result = Mathf.Clamp (result, -1f, 1f);
			}
			break;
		case InputType.Buttons:
			if (Input.GetKey (PositiveButton)) {
				if (Snap && virtual_axis < 0f) {
					virtual_axis = 0;
				}
				virtual_axis += Sensivity * Time.deltaTime;
			} else if (Input.GetKey (NegativeButton)) {
				if (Snap && virtual_axis > 0f) {
					virtual_axis = 0;
				}
				virtual_axis -= Sensivity * Time.deltaTime;
			} else {
				virtual_axis = Mathf.MoveTowards (virtual_axis, 0, Gravity * Time.deltaTime);
			}
			virtual_axis = Mathf.Clamp (virtual_axis, -1f, 1f);
			result = virtual_axis;
			break;
		case InputType.MouseMovement:
			float inv = 1f;
			if (Invert) {
				inv = -1f;
			}
			switch (mouseAxis) {
			case MouseAxis.X:
				return Input.GetAxis ("Mouse X")*inv;
			case MouseAxis.Y:
				return Input.GetAxis ("Mouse Y")*inv;
			case MouseAxis.Scroll:
				return Input.GetAxis ("Mouse ScrollWheel")*inv;
			}
			break;
		}

		if (Mathf.Abs (result) < Dead) {
			return 0;
		} else {
			if (Invert) {
				result *= -1f;
			}
			return result;
		}
	}
	public bool Get(){
		bool result = false;
		switch (Type) {
		case InputType.GamePad:
			result = InputPlus.GetData (con, GamePadInput)!=0f||InputPlus.GetData (con, GamePadInputNegative)!=0f;
			break;
		case InputType.Buttons:
			result = Input.GetKey (PositiveButton) || Input.GetKey (NegativeButton);
			break;
		case InputType.MouseMovement:
			result = false;
			break;
		}
		return result;
	}
	public bool GetDown(){
		bool result = false;
		switch (Type) {
		case InputType.GamePad:
			bool temp = InputPlus.GetData (con, GamePadInput)!=0f || InputPlus.GetData (con, GamePadInputNegative)!=0f;
			if (temp && !previous_state_down) {
				result = true;
			} else {
				result = false;
			}
			previous_state_down = temp;
			break;
		case InputType.Buttons:
			result = Input.GetKeyDown (PositiveButton) || Input.GetKeyDown (NegativeButton);
			break;
		case InputType.MouseMovement:
			result = false;
			break;
		}
		return result;
	}
	public bool GetUp(){
		bool result = false;
		switch (Type) {
		case InputType.GamePad:
			bool temp = InputPlus.GetData (con, GamePadInput)!=0f || InputPlus.GetData (con, GamePadInputNegative)!=0f;
			if (!temp && previous_state_up) {
				result = true;
			} else {
				result = false;
			}
			previous_state_up = temp;
			break;
		case InputType.Buttons:
			result = Input.GetKeyUp (PositiveButton) || Input.GetKeyUp (NegativeButton);
			break;
		case InputType.MouseMovement:
			result = false;
			break;
		}
		return result;
	}
}
