using UnityEngine;
using System.Collections;

public class TerrainFillingGenerator : MonoBehaviour {
	public Terrain Terrain2Fill;
	public Elements[] elements;
	public float[] angles;
	public float distance;
	// Use this for initialization
	void Start () {
		GenerateElementsMulti (angles);
	}
	
	void GenerateElementsMulti(float[] angles){
		int howManyx = (int)(Terrain2Fill.terrainData.size.x / distance), howManyz = (int)(Terrain2Fill.terrainData.size.z/ distance);
		for(int i = 0;i<howManyx;i++){
			for (int k = 0; k < howManyz; k++) {
				//krytetium podziału na zasięgi

				float ang = Terrain2Fill.terrainData.GetSteepness((float)i*distance/(float)Terrain2Fill.terrainData.heightmapResolution, (float)k*distance/(float)Terrain2Fill.terrainData.heightmapResolution);
				int range = 0;
				for (int j = 0; j < angles.Length; j++) {
					if (ang < angles [j]) {
						range = j;
						break;
					}
				}
				//teraz wiemy który voxel jest w którym range'u
				GameObject generated = null;
				for (int j = 0; j < elements.Length; j++) {
					if (elements [j].ranges [range]) {
						if (!generated) {
							if (Random.value < elements [j].density) {
								generated = elements [j].Generate (new Vector3((float)i*distance, Terrain2Fill.SampleHeight(new Vector3((float)i*distance, 0f, (float)k*distance)), (float)k*distance), Quaternion.identity);
							}
						}
					}
				}
			}
		}
	}
}
