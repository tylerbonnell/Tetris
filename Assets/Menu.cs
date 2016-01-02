using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Alphabet.singleton.write ("press enter", Vector3.zero, Quaternion.identity.eulerAngles, Vector3.one);
	}
	
	void Update () {
		if (Input.GetKeyDown (KeyCode.Return)) {
			Application.LoadLevel ("Game");
		} else if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
	}
}
