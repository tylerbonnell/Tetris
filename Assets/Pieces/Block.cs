using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	// Any blocks that are player controlled will move down the grid
	public bool playerControlled = true;
	//public GridCoord coord;
	//public bool needsNewCoords = true;

	public void smash (bool showExplosion = true) {
		Destroy (gameObject);
	}
}
