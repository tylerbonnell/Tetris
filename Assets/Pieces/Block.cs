using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	public MeshRenderer mesh;
	public GameObject explosion;

	// Makes the square disappear, shows an explosion based on the bool,
	// and invokes the block's imminent destruction :'( RIP block
	public void smash (bool showExplosion = true) {
		mesh.enabled = false;
		if (showExplosion)
			explosion.SetActive (true);
		Invoke ("selfDestruct", 3f);
	}

	private void selfDestruct() {
		Destroy (gameObject);
	}
}
