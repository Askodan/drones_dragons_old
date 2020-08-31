using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class StageManager : Singleton<StageManager> {
	public float miniMapScale = 1f;
	public float symbolsScale = 0.5f;
	public GameObject[] Players;
	//[HideInInspector]
	public List<Camera> MiniMaps;
	//[HideInInspector]
	public List<Transform> dragonsInDangerZone;
	MiniMapObject[] allSymbols;
	void Awake(){
		dragonsInDangerZone = new List<Transform>();
        MiniMaps = new List<Camera>();
		MiniMapObject.universalParent = transform;
	}
	void Start(){
		allSymbols = FindObjectsOfType<MiniMapObject> ();

		setMinimapScale (miniMapScale, 0.5f);
	}
	void OnGUI(){
		setMinimapScale (miniMapScale, symbolsScale);
	}
	public void DragonFlewIntoDangerZone(Transform dragon){
		dragonsInDangerZone.Add (dragon);
	}
	public void DragonFlewOutOfDangerZone(Transform dragon){
		dragonsInDangerZone.Remove (dragon);
	}
	public Transform GetClosestDragonFromDangerZone(Transform closest2, out float dist){
		Transform closest = null;
		float sqrdist = Mathf.Infinity;
		foreach (Transform dragon in dragonsInDangerZone) {
			float dragonsqrdist = (dragon.position - closest2.position).sqrMagnitude;
			if ( dragonsqrdist < sqrdist) {
				closest = dragon;
				sqrdist = dragonsqrdist;
			}
		}
		dist = sqrdist;
		return closest;
	}
	public void setMinimapScale(float MinimapScale, float symbolScale){
		symbolScale = Mathf.Clamp01 (symbolScale)*2f;
		foreach (MiniMapObject symbol in allSymbols) {
			symbol.ChangeSize (miniMapScale*symbolScale);
		}
        foreach(var MiniMap in MiniMaps) MiniMap.orthographicSize = 100f * MinimapScale;
	}
    public void addMiniMap(Camera minimap)
    {
        MiniMaps.Add(minimap);
    }
}
