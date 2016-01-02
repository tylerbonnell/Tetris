using UnityEngine;
using System.Collections;

public class PieceStorage : MonoBehaviour {

	private static string[,] shapes = {{"111", "01"}, {"111", "001"}, {"111", "1"}, {"011", "11"}, {"11", "011"}, {"1111", ""}, {"11", "11"}};

	// The piece that this PieceStorage holds
	public Piece piece;

	// PieceStorage stores/displays 4 blocks
	private Block[] blocks = new Block[4];
	private MeshRenderer[] meshes = new MeshRenderer[4];
	private bool containsPiece = false;

	public GameObject BlockPrefab;

	void Awake () {
		for (int i = 0; i < blocks.Length; i++) {
			blocks[i] = Instantiate (BlockPrefab).GetComponent<Block> ();
			blocks[i].transform.parent = transform;
			meshes[i] = blocks[i].gameObject.GetComponent<MeshRenderer> ();
			blocks[i].gameObject.SetActive (false);
		}
	}

	// Saves and displays the given piece. If piece == null, displays nothing.
	public void set (Piece piece) {
		this.piece = piece;
		if (piece == null) {
			containsPiece = false;
			foreach (Block b in blocks)
					b.gameObject.SetActive (false);
		} else {
			if (!containsPiece)
				foreach (Block b in blocks)
					b.gameObject.SetActive (true);
			containsPiece = true;
			piece.currentRotation = 2;
			int i = 0;
			for (int r = 0; r < shapes.GetLength(1); r++) {
				for (int c = 0; c < shapes[piece.shapeType, r].Length; c++) {
					if (shapes[piece.shapeType, r][c] == '1') {
						blocks[i].transform.position = Vector3.down * r + 
							transform.position + Vector3.right * c;
						meshes[i].material = piece.blockColor;
						i++;
					}
				}
			}
		}
	}
}
