using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(PlayerSteering))]

public class PlayerSpecs : MonoBehaviour {
	public int numPlayersOffline;
    public bool ownDisplay = true;
	[Tooltip("Player 1, Player2... there's no such thing as Player 0!")]
	public int number;
	public VehicleTypeDefiner mainVehicle;

	[HideInInspector] public VehicleTypeDefiner actualVehicle;
	[HideInInspector] public Rigidbody actualRigidbody;
	[HideInInspector] public WeaponManager weaponManager;
	public new Camera camera;
    public Canvas canvas;

	public GUIBehaviour GUI;

	[HideInInspector] public PlayerSteering playerSteering;

	[HideInInspector] public List<VehicleTypeDefiner> inTrigger;
	void Start(){
        camera = Instantiate(camera);
        camera.targetDisplay = ownDisplay ? number-1 : 0;
        foreach (Camera cam in camera.GetComponentsInChildren<Camera>()) { cam.targetDisplay = camera.targetDisplay; }
        canvas = Instantiate(canvas);
        canvas.targetDisplay = camera.targetDisplay;

        playerSteering = GetComponent<PlayerSteering> ();

		GUI = Instantiate (GUI);
        GUI.MainCamera = camera;
        GUI.transform.SetParent(canvas.transform);
        GUI.transform.localPosition = Vector3.zero;
        GUI.transform.localScale = Vector3.one;
		GUI.player = this;
        GUI.Setup();
        // GUI.takeScreen (numPlayersOffline, number);
        //GUI.GetComponent<Canvas> ().worldCamera = camera.GetComponent<Camera> ();

        /*switch (numPlayersOffline) {
		case 2:
			UnityEngine.UI.CanvasScaler CS = GUI.GetComponent<UnityEngine.UI.CanvasScaler> ();
			Vector2 res = CS.referenceResolution;
			res.y *= 2f;
			CS.referenceResolution = res;

			break;
		default:
			break;
		}*/
        playerSteering.simpleCamera = camera.GetComponent<SimpleCamera> ();
		playerSteering.simpleSmooth = camera.GetComponent<SimpleSmooth> ();

		IntoVehicle (mainVehicle);
	}

	public void IntoVehicle(VehicleTypeDefiner newVehicle){
		ParentAndZero (newVehicle.transform, transform);
		UnparentRigidbodies UNPR = newVehicle.GetComponent<UnparentRigidbodies> ();
		Transform[] manytransforms;
		if (UNPR != null) {
			manytransforms = UNPR.originalChildren;
		} else {
			manytransforms = newVehicle.GetComponentsInChildren<Transform> ();
		}
		Transform cameraPoint = null;
		Transform Driver = null;
		for (int i = 0; i < manytransforms.Length; i++) {
			if (manytransforms [i].name == "CameraPoint") {
				cameraPoint = manytransforms [i];
			}
			if (manytransforms [i].name == "Driver") {
				Driver = manytransforms [i];
			}
		}
		if (cameraPoint == null) {
			Debug.LogError ("There's no \"CameraPoint\" in " + newVehicle.name);
			return;
		}
		weaponManager = newVehicle.GetComponent<WeaponManager> ();
		if (weaponManager == null) {
			Debug.Log ("There's no \"WeaponManager\" in " + newVehicle.name);
		}
		//camera = GameManager.Instance.camera.transform;


		switch (newVehicle.vehicleType) {
		case VehicleType.Drone:
			ParentAndZero (cameraPoint, camera.transform);

			playerSteering.steeringDrone = newVehicle.GetComponent<SteeringDrone> ();
			playerSteering.steeringDrone.enabled = true;

			break;
		case VehicleType.Tractor:
			playerSteering.simpleCamera.enabled = true;
			playerSteering.simpleCamera.target = cameraPoint;

			if (Driver) {
				mainVehicle.GetComponent<Rigidbody> ().isKinematic = true;
				ParentAndZero (Driver, mainVehicle.transform);
			}
			foreach (Shooter weapon in weaponManager.weapons_Shooter) {
				PlayerAim PA = weapon.aimer.GetComponent<PlayerAim> ();
				PA.enabled = true;
				PA.camera = camera.transform;
			}

			playerSteering.tractorScriptEasy = newVehicle.GetComponent<SteeringTractor> ();
			playerSteering.trailerTrolley = newVehicle.GetComponent<TrailerTrolley> ();
			playerSteering.tractorScriptEasy.enabled = true;
			playerSteering.trailerTrolley.enabled = true;
			break;
		case VehicleType.Robot:
			playerSteering.simpleSmooth.enabled = true;
			playerSteering.simpleSmooth.target = cameraPoint;
			playerSteering.simpleSmooth.lookTarget = newVehicle.transform;
			playerSteering.smoothHelper = newVehicle.GetComponentInChildren<RobotSimpleSmoothHelper> ();

			ParentAndZero (camera.transform, mainVehicle.transform);
			mainVehicle.GetComponent<Rigidbody> ().isKinematic = true;

			playerSteering.robotWheelsSteering = newVehicle.GetComponent<RobotLegsWheelsSteering> ();
			playerSteering.robotWheelsSteering.enabled = true;

			playerSteering.robotGrabber = newVehicle.GetComponent<RobotGrabber> ();
			playerSteering.robotGrabber.enabled = true;
			break;
		}

		actualVehicle = newVehicle;
		actualRigidbody = actualVehicle.GetComponent<Rigidbody> ();
		GUI.setGUI (newVehicle);
	}

	public void OutofVehicle(){
		switch (actualVehicle.vehicleType) {
		case VehicleType.Drone:
			camera.transform.parent = null;
			actualVehicle.GetComponent<SteeringDrone> ().enabled = false;
			break;
		case VehicleType.Tractor:
			playerSteering.simpleCamera.enabled = false;

			mainVehicle.transform.parent = null;
			mainVehicle.GetComponent<Rigidbody> ().isKinematic = false;


			foreach (Shooter weapon in weaponManager.weapons_Shooter) {
				weapon.aimer.GetComponent<PlayerAim> ().enabled = false;
			}

			actualVehicle.GetComponent<SteeringTractor> ().enabled = false;
			actualVehicle.GetComponent<TrailerTrolley> ().enabled = false;
			break;
		case VehicleType.Robot:
			playerSteering.simpleSmooth.enabled = false;

			mainVehicle.transform.parent = null;
			mainVehicle.GetComponent<Rigidbody> ().isKinematic = false;
			actualVehicle.GetComponent<RobotLegsWheelsSteering> ().enabled = false;
			actualVehicle.GetComponent<RobotGrabber> ().enabled = false;
			break;
		}
	}

	static public void ParentAndZero(Transform newParent, Transform newChild){
		newChild.SetParent(newParent);
		newChild.localPosition = Vector3.zero;
		newChild.localRotation = Quaternion.identity;
	}
	void OnTriggerEnter(Collider other){
		if(((1<<other.gameObject.layer) & playerSteering.mask) != 0){
			inTrigger.Add( other.gameObject.transform.parent.GetComponent<VehicleTypeDefiner> ());
		}
	}

	void OnTriggerExit(Collider other){
		if(((1<<other.gameObject.layer) & playerSteering.mask) != 0){
			inTrigger.Remove( other.gameObject.transform.parent.GetComponent<VehicleTypeDefiner> ());
		}
	}

}
