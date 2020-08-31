using UnityEngine;
using System.Collections;
using InputPlusControl;
public class PlayerSteering : MonoBehaviour {
	public LayerMask mask;
	public int GamePad_Num;
	PlayerSpecs specs;
	[HideInInspector] public SteeringDrone steeringDrone;

	[HideInInspector] public RobotLegsWheelsSteering robotWheelsSteering;
	[HideInInspector] public RobotGrabber robotGrabber;

	[HideInInspector] public SteeringTractor tractorScriptEasy;
	[HideInInspector] public TrailerTrolley trailerTrolley;

	[HideInInspector] public SimpleCamera simpleCamera;
	[HideInInspector] public SimpleSmooth simpleSmooth;
	[HideInInspector] public RobotSimpleSmoothHelper smoothHelper;
	void Awake(){
		specs = GetComponent<PlayerSpecs> ();
	}
	Input2 Input;
	void Start(){
		//InputPlus.LearnController (1);
		InputPlus.Initialize(); //Start up InputPlus. 
		InputPlus.SetDebugText(false);
		foreach (UnityLikeInput input in inputs) {
			input.con = GamePad_Num;
		}
		Input = new Input2 (inputs);
	}
	public UnityLikeInput[] inputs;

	void Update () {
		if (Input.GetButtonDown ("ChangeVehicle")) {
			if (specs.actualVehicle != specs.mainVehicle) {
				specs.OutofVehicle ();
				specs.IntoVehicle (specs.mainVehicle);		
			} else {
				RaycastHit hit;
				if (Physics.Raycast (transform.position, transform.forward, out hit, 10f, mask)) {
					specs.OutofVehicle ();
					specs.IntoVehicle (hit.collider.gameObject.transform.parent.GetComponent<VehicleTypeDefiner> ());
				
				} else {
					if (specs.inTrigger.Count > 0) {
						specs.OutofVehicle ();
						specs.IntoVehicle (specs.inTrigger [0]);
					}
				}
			}
		}
	
		if (specs.actualVehicle != null) {
			switch (specs.actualVehicle.vehicleType) {
			case VehicleType.Drone:
				steeringDrone.Steer (Input.GetAxis ("Thrust"), Input.GetAxis ("Pitch"), Input.GetAxis ("Roll"), Input.GetAxis ("Yaw"), Input.GetAxis ("PrototypeTurbo"),
					Input.GetButtonDown ("Lights"), Input.GetButtonDown ("Turn off motors"), Input.GetButtonDown ("Stabilize"), Input.GetButtonDown ("Keep altitude"), Input.GetButtonDown ("Self leveling"));
				break;
			case VehicleType.Tractor:
				tractorScriptEasy.Steer (Input.GetButtonDown ("Break"), Input.GetButtonUp ("Break"), Input.GetAxis ("Vertical"), Input.GetAxis ("Horizontal"), Input.GetButtonDown("Gear Down"), Input.GetButtonDown("Gear Up"));
				trailerTrolley.Steer (Input.GetAxis ("Trolley"));
				break;
			case VehicleType.Robot:
				robotWheelsSteering.Steer (Input.GetAxis ("Vertical"), Input.GetAxis ("Horizontal"), Input.GetAxis ("Spread(robot)"), 
					Input.GetAxis ("Height(robot)"), Input.GetButtonDown ("Turning in place"));
				robotGrabber.Steer (Input.GetButtonUp("Item outside"), Input.GetButtonUp ("Item inside"), Input.GetButton("Item outside"), 
					Input.GetButton("Item inside"), Input.GetButtonDown ("Next Item"), Input.GetButtonDown ("Previous Item"), Input.GetButtonDown ("Hands Up"));
				break;
			}
		}
		if (specs.weaponManager) {
			foreach(Shooter weapon in specs.weaponManager.weapons_Shooter){
				weapon.shoot = Input.GetButton ("Fire");
			}
		}
		/*if (Input.GetButtonDown ("ChangeVehicle")) {
			if (specs.actualVehicle != specs.mainVehicle) {
				specs.OutofVehicle ();
				specs.IntoVehicle (specs.mainVehicle);		
			} else {
				RaycastHit hit;
				if (Physics.Raycast (transform.position, transform.forward, out hit, 10f, mask)) {
					specs.OutofVehicle ();
					specs.IntoVehicle (hit.collider.gameObject.transform.parent.GetComponent<VehicleTypeDefiner> ());
				
				} else {
					if (specs.inTrigger.Count > 0) {
						specs.OutofVehicle ();
						specs.IntoVehicle (specs.inTrigger [0]);
					}
				}
			}
		}
	
		if (specs.actualVehicle != null) {
			switch (specs.actualVehicle.vehicleType) {
			case VehicleType.drone:
				steeringDrone.Steer (Input.GetAxis ("Thrust"), Input.GetAxis ("Pitch"), Input.GetAxis ("Roll"), Input.GetAxis ("Yaw"), Input.GetAxis ("PrototypeTurbo"),
					Input.GetButtonDown ("Lights"), Input.GetButtonDown ("Turn off motors"), Input.GetButtonDown ("Stabilize"), Input.GetButtonDown ("Keep altitude"), Input.GetButtonDown ("Self leveling"));
				break;
			case VehicleType.tractor:
				tractorScriptEasy.Steer (Input.GetButtonDown ("Break"), Input.GetButtonUp ("Break"), Input.GetAxis ("Vertical"), Input.GetAxis ("Horizontal"));
				trailerTrolley.Steer (Input.GetAxis ("Trolley"));
				break;
			case VehicleType.robot:
				robotWheelsSteering.Steer (Input.GetAxis ("Vertical"), Input.GetAxis ("Horizontal"), Input.GetAxis ("Spread(robot)"), Input.GetAxis ("Height(robot)"), Input.GetButtonDown ("Turning in place"));
				robotGrabber.Steer (Input.GetButtonUp("Item outside"), Input.GetButtonUp ("Item inside"), Input.GetButton("Item outside"), Input.GetButton("Item inside"), Input.GetButtonDown ("Next Item"), Input.GetButtonDown ("Previous Item"), Input.GetButtonDown ("Hands Up"));
				break;
			}
		}
		if (specs.weaponManager) {
			foreach(Shooter weapon in specs.weaponManager.weapons_Shooter){
				weapon.shoot = Input.GetButton ("Fire");
			}
		}*/
	}
	void LateUpdate () {
		
		if (specs.actualVehicle != null) {
			switch (specs.actualVehicle.vehicleType) {
			case VehicleType.Drone:
				break;
			case VehicleType.Tractor:
				simpleCamera.Steer(Input.GetButton ("Aim"), Input.GetAxis("Camera X"), Input.GetAxis("Camera Y"), Input.GetAxis("Camera distance"));
				break;
			case VehicleType.Robot:
				smoothHelper.Steer (Input.GetAxis ("Camera X"), Input.GetAxis ("Camera Y"));
				break;
			}
		}
	}
}
