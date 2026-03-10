using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

/*

Finds tree instances within terrain data and create's real GameObjects within the scene. (used to bake shadows).

Steps:

	Turn of "Draw Trees" from Terrain config.

	Select the Terrain

	Run the editor script.

	Make sure "Auto Generate" is DISABLED

	Toggle Off or Delete "Tree Shadow Casters"

	Re-Enable "Draw Trees" 
*/
public class PlaceTreeShadowCasters 
{
	
	[@MenuItem ("Terrain/Place Tree Shadow Casters")]
	static void Run()
	{
		Terrain[] terrains = Object.FindObjectsOfType<Terrain>();
		foreach (Terrain terrain in terrains)
		{

			TerrainData td = terrain.terrainData;

			if (td.treeInstances.Length == 0)
			{
				continue;
			}

			GameObject parent = new GameObject("Tree Shadow Casters");

			foreach (TreeInstance tree in td.treeInstances)
			{
				Vector3 pos = Vector3.Scale(tree.position, td.size) + terrain.transform.position;
				TreePrototype treeProt = td.treePrototypes[tree.prototypeIndex];
				GameObject prefab = treeProt.prefab;

				Debug.Log ("tree : " + tree.rotation);

				GameObject obj = Object.Instantiate(prefab, pos, Quaternion.AngleAxis(tree.rotation * Mathf.Rad2Deg, Vector3.up)) as GameObject;

				MeshRenderer renderer = obj.GetComponentInChildren<MeshRenderer>();
				renderer.receiveShadows = false;
				renderer.shadowCastingMode = ShadowCastingMode.On;

				GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.ContributeGI);

				Transform t = obj.transform;
				t.localScale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);
				t.rotation = Quaternion.AngleAxis (tree.rotation * Mathf.Rad2Deg, Vector3.up);
				t.parent = parent.transform;
			}
		}
	}

	[MenuItem("Terrain/Enable Shadows")]
	static void EnableShadows()
	{
		Terrain[] terrains = Object.FindObjectsOfType<Terrain>();
		foreach (Terrain terrain in terrains)
		{
			terrain.shadowCastingMode = ShadowCastingMode.On;
		}
	}

	[MenuItem("Terrain/Disable Shadows")]
	static void DisableShadows()
	{
		Terrain[] terrains = Object.FindObjectsOfType<Terrain>();
		foreach (Terrain terrain in terrains)
		{
			terrain.shadowCastingMode = ShadowCastingMode.Off;
		}
	}

	[MenuItem("Terrain/Set Pixel Error")]
	static void SetPixelError()
	{
		Terrain[] terrains = Object.FindObjectsOfType<Terrain>();
		foreach (Terrain terrain in terrains)
		{
			terrain.heightmapPixelError = 200;
		}
	}
}