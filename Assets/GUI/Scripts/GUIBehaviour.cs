using UnityEngine;
using System.Collections;

public class GUIBehaviour : MonoBehaviour {
	public GameObject GUIRobot;
	public GameObject GUITractor;
	public GameObject GUIDrone;

	public DigitsDisplayer speedometer;
	public GameObject MiniMap;
	public UnityEngine.UI.Image MiniMapDisplayer;
	[HideInInspector] public PlayerSpecs player;

	GUIDrone drone;
	GameObject leanCamera;
    Camera mainCamera;

    public Camera MainCamera
    {
        get
        {
            return mainCamera;
        }

        set
        {
            mainCamera = value;
            leanCamera.GetComponent<Camera>().targetDisplay = mainCamera.targetDisplay;
        }
    }

    // Use this for initialization
    void Awake () {
        // transform.SetParent (StageManager.Instance.canvas.transform);

        transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
		transform.localRotation = Quaternion.identity;

		GUIDrone = Instantiate (GUIDrone);
		PlayerSpecs.ParentAndZero (transform, GUIDrone.transform);
		GUIDrone.transform.localScale  = Vector3.one;
		GUIRobot = Instantiate (GUIRobot);
		PlayerSpecs.ParentAndZero (transform, GUIRobot.transform);
		GUIRobot.transform.localScale  = Vector3.one;
		GUITractor = Instantiate (GUITractor);
		PlayerSpecs.ParentAndZero (transform, GUITractor.transform);
		GUITractor.transform.localScale  = Vector3.one;

		if (!speedometer) {
			Debug.LogError ("No speedometer!");
		}
		if (!MiniMapDisplayer) {
			Debug.LogError ("No miniMap displayer!");
		}
		if (!MiniMap) {
			Debug.LogError ("No miniMap!");
		}
		int textID = Shader.PropertyToID ("_MainTex");
        MiniMapDisplayer.material = new Material(MiniMapDisplayer.material);
        MiniMapDisplayer.material.SetTexture (textID, new RenderTexture (256, 256, 24));
		MiniMap = Instantiate (MiniMap);
		StageManager.Instance.addMiniMap(MiniMap.GetComponentInChildren<Camera> ());
		//MiniMap.transform.SetParent (transform);
		MiniMap.GetComponentInChildren<Camera> ().targetTexture = (RenderTexture)MiniMapDisplayer.material.GetTexture (textID);

		leanCamera = GUIDrone.GetComponentInChildren<LeanCamera> ().gameObject;
		leanCamera.transform.SetParent (null);
        
	}
	public void takeScreen(int numplayers, int playernum){
		if (playernum > 0) {
			if (numplayers > 1) {
				UnityEngine.UI.CanvasScaler CS = GetComponentInParent<UnityEngine.UI.CanvasScaler> ();
				Vector2 res = CS.referenceResolution;

				RectTransform rectTransform = GetComponent<RectTransform> ();
				float offsetx = 0;
				switch (playernum) {
				case 1:
					offsetx = rectTransform.localPosition.x * (1 + 1 / numplayers);
					break;
				case 2:
					offsetx = rectTransform.localPosition.x * (1 - 1 / numplayers);
					break;
				}
				print (rectTransform.localPosition);
				rectTransform.localPosition = new Vector3 (offsetx, rectTransform.localPosition.y / numplayers, rectTransform.localPosition.z);
				rectTransform.localScale = new Vector3 (1 / numplayers, 1 / numplayers, 1 / numplayers);
			}
		} else {
			Debug.LogError ("Wrong index of player, cannot be less than 1");
		}
	}
	public void Setup(){
		MiniMap.GetComponent<CopyTransform> ().toCopy = player.transform;
		leanCamera.GetComponent<LeanCamera>().target = player.transform;
	}
	
	void OnGUI(){
		speedometer.value = player.actualRigidbody.velocity.magnitude*3.6f;
		switch (player.actualVehicle.vehicleType) {
		case VehicleType.Drone:
			    drone.lightPanel.ChangeState (new bool[]{player.playerSteering.steeringDrone.motorsOn, 
				    player.playerSteering.steeringDrone.selfLeveling, 
				    player.playerSteering.steeringDrone.keepAltitude, 
				    player.playerSteering.steeringDrone.stabilize});
                drone.rotateInfo.UpdateData(player.transform);
                break;
		case VehicleType.Tractor:
			break;
		case VehicleType.Robot:
			break;
		}
	}
	public void setGUI(VehicleTypeDefiner what){
		switch (what.vehicleType) {
		case VehicleType.Drone:
			    if (!GUIDrone.activeSelf) {
				    GUIDrone.SetActive (true);
			    }
			    if (GUIRobot.activeSelf) {
				    GUIRobot.SetActive (false);
			    }
			    if (GUITractor.activeSelf) {
				    GUITractor.SetActive (false);
			    }
			    leanCamera.SetActive (false);
			    drone = GUIDrone.GetComponent<GUIDrone> ();
			    drone.altmeter = what.GetComponent<AltitudeMeter> ();
			    drone.lightPanel.SetState (new bool[] {player.playerSteering.steeringDrone.motorsOn, 
				    player.playerSteering.steeringDrone.selfLeveling, 
				    player.playerSteering.steeringDrone.keepAltitude, 
				    player.playerSteering.steeringDrone.stabilize
			    });
			    GUIDrone.GetComponentInChildren<HealthBarScript> ().life = what.GetComponent<Life> ();
			break;
		case VehicleType.Tractor:
			    if (GUIDrone.activeSelf) {
				    GUIDrone.SetActive (false);
			    }
			    if (GUIRobot.activeSelf) {
				    GUIRobot.SetActive (false);
			    }
			    if (!GUITractor.activeSelf) {
				    GUITractor.SetActive (true);
			    }
			    GUITractor.GetComponentInChildren<HealthBarScript> ().life = what.GetComponent<Life> ();
			    leanCamera.SetActive (false);
			break;
		case VehicleType.Robot:
			    if (GUIDrone.activeSelf) {
				    GUIDrone.SetActive (false);
			    }
			    if (!GUIRobot.activeSelf) {
				    GUIRobot.SetActive (true);
			    }
			    if (GUITractor.activeSelf) {
				    GUITractor.SetActive (false);
			    }
			    GUIRobot.GetComponentInChildren<HealthBarScript> ().life = what.GetComponent<Life> ();
			    leanCamera.SetActive (false);
			break;
		}
	}
}
