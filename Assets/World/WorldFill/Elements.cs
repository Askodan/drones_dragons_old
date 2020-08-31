using UnityEngine;
using System.Collections;

[System.Serializable]
public class Elements{
	[SerializeField]
	string name;
	[SerializeField]
	Transform parent;
	[Tooltip("How many per square meter")]
	public float density;
	[SerializeField]
	Vector2 minMaxSize;
	[SerializeField]
	float maxAngle;
	[Tooltip("On which surfaces can be projected")]
	public bool[] ranges;
	[SerializeField]
	GameObject[] prefabs;
	[SerializeField]
	Material[] materials;
	[SerializeField]
	bool addLight;
	[SerializeField]
	Vector3 MoveFromWall;
	public GameObject Generate(Vector3 position, Quaternion rotation){
		GameObject myRandomness = GameObject.Instantiate (prefabs[Random.Range(0, prefabs.Length)], position, rotation) as GameObject;

		myRandomness.transform.localScale = Vector3.one * Random.Range (minMaxSize.x, minMaxSize.y);
		myRandomness.transform.Rotate (Random.value*maxAngle, Random.value*360f, Random.value*maxAngle);
		myRandomness.transform.Translate (rotation*MoveFromWall, Space.World);
		myRandomness.transform.SetParent (parent);
		if (materials.Length > 0) {
			Renderer[] rends = myRandomness.GetComponentsInChildren<Renderer> ();
			int mat = Random.Range (0, materials.Length);
			for (int i = 0; i < rends.Length; i++) {
				rends [i].material = materials [mat];
				if (addLight) {
					Light myLight = myRandomness.AddComponent<Light> ();
					myLight.type = LightType.Point;

					float H, S, V;
					Color.RGBToHSV (materials [mat].GetColor ("_EmissionColor"), out H, out S, out V);
					myLight.color = Color.HSVToRGB (H, S, 1f);
					myLight.range = myRandomness.transform.localScale.x * 20f;
					myLight.intensity = 2f;
					myLight.shadows = LightShadows.Soft;
					myLight.renderMode = LightRenderMode.ForcePixel;
					//po dodaniu świateł trzeba ręcznie ustawić na bake :(
				}
			}
		}
		return myRandomness;
	}
}