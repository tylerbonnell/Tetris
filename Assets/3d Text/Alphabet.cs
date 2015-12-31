using UnityEngine;
using System.Collections.Generic;

public class Alphabet : MonoBehaviour {
	
	public GameObject voxel;
	public static Alphabet singleton;
	
	void Awake () {
		singleton = this;
	}
	
	// each letter in each array represents a row of voxels, where
	// a 1 is a voxel and a 0 is just a blank space
	private static string[] a = {"0110", "1001", "1111", "1001", "1001"};
	private static string[] b = {"111", "1001", "111", "1001", "111"};
	private static string[] c = {"0111", "1", "1", "1", "0111"};
	private static string[] d = {"111", "1001", "1001", "1001", "111"};
	private static string[] e = {"111", "1", "111", "1", "111"};
	private static string[] f = {"111", "1", "111", "1", "1"};
	private static string[] g = {"0111", "1", "1011", "1001", "0111"};
	private static string[] h = {"1001", "1001", "1111", "1001", "1001"};
	private static string[] i = {"1", "1", "1", "1", "1"};
	private static string[] j = {"0001", "0001", "0001", "1001", "011"};
	private static string[] k = {"1001", "101", "11", "101", "1001"};
	private static string[] l = {"1", "1", "1", "1", "111"};
	private static string[] m = {"10001", "11011", "10101", "10001", "10001"};
	private static string[] n = {"1001", "1101", "1011", "1001", "1001"};
	private static string[] o = {"011", "1001", "1001", "1001", "011"};
	private static string[] p = {"111", "1001", "111", "1", "1"};
	private static string[] q = {"011", "1001", "1001", "101", "0101"};
	private static string[] r = {"111", "1001", "111", "101", "1001"};
	private static string[] s = {"0111", "1", "011", "0001", "111"};
	private static string[] t = {"11111", "001", "001", "001", "001"};
	private static string[] u = {"1001", "1001", "1001", "1001", "011"};
	private static string[] v = {"10001", "10001", "0101", "0101", "001"};
	private static string[] w = {"10101", "10101", "10101", "0101", "0101"};
	private static string[] x = {"1001", "1001", "011", "1001", "1001"};
	private static string[] y = {"1001", "1001", "1111", "0001", "111"};
	private static string[] z = {"1111", "0001", "011", "1", "1111"};
	private static string[] n1 = {"01", "11", "01", "01", "111"};
	private static string[] n2 = {"011", "1001", "001", "01", "1111"};
	private static string[] n3 = {"111", "0001", "011", "0001", "111"};
	private static string[] n4 = {"0011", "0101", "1001", "1111", "0001"};
	private static string[] n5 = {"1111", "1", "1111", "0001", "111"};
	private static string[] n6 = {"011", "1", "111", "1001", "011"};
	private static string[] n7 = {"1111", "0001", "001", "001", "001"};
	private static string[] n8 = {"011", "1001", "011", "1001", "011"};
	private static string[] n9 = {"011", "1001", "0111", "0001", "011"};

	private static Dictionary<char, string[]> letters = new Dictionary<char, string[]> () {
		{'a', a}, {'b', b}, {'c', c}, {'d', d}, {'e', e}, {'f', f}, {'g', g}, {'h', h}, {'i', i},
		{'j', j}, {'k', k}, {'l', l}, {'m', m}, {'n', n}, {'o', o}, {'p', p}, {'q', q}, {'r', r},
		{'s', s}, {'t', t}, {'u', u}, {'v', v}, {'w', w}, {'x', x}, {'y', y}, {'z', z}, {'0', o},
		{'1', n1}, {'2', n2}, {'3', n3}, {'4', n4}, {'5', n5}, {'6', n6}, {'7', n7}, {'8', n8}, {'9', n9}
	};

	// Prints the given string out of 3d voxels. The top-left voxel will be centered at topLeftCorner,
	// the word will be rotated to eulerAngles, and the word will be scaled to scale. Default scale 
	// is each voxel = 1x1x1
	public void write (string str, Vector3 topLeftCorner, Vector3 eulerAngles, Vector3 scale) {
		str = str.ToLower ();
		GameObject parent = new GameObject ();
		parent.name = "3d text";
		parent.transform.position = Vector3.zero;
		int letterHorizontalOffset = 0;
		for (int i = 0; i < str.Length; i++) {
			if (str[i] == ' ') {
				letterHorizontalOffset += 2;
			} else if (letters.ContainsKey (str[i])) {
				string[] letter = letters[str[i]];
				int letterWidth = 0;
				for (int r = 0; r < letter.Length; r++) {
					letterWidth = Mathf.Max (letterWidth, letter[r].Length);
					for (int c = 0; c < letter[r].Length; c++) {
						if (letter[r][c] == '1') {
							GameObject vox = Instantiate (voxel, Vector3.zero + (letterHorizontalOffset + c) * 
								Vector3.right + r * Vector3.down, Quaternion.identity) as GameObject;
							vox.transform.parent = parent.transform;
						}
					}
				}
				letterHorizontalOffset += letterWidth + 1;
			} else {
				Debug.Log (("Unrecognized character!"));
			}
		}
		parent.transform.position = topLeftCorner;
		parent.transform.eulerAngles = eulerAngles;
		parent.transform.localScale = scale;
	}
}
