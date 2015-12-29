using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {
	// Each 2d array represents the 4 rotated state of each shape.
	//                        		        rotation:     0      1      2      3
	private static string[,] tShape = new string[3,4] {{"010", "010", "000", "010"},
											 		   {"111", "011", "111", "110"},
													   {"000", "010", "010", "010"}};

	private static string[,] lShape1 = new string[3,4] {{"100", "011", "000", "010"},
														{"111", "010", "111", "010"},
														{"000", "010", "001", "110"}};

	private static string[,] lShape2 = new string[3,4] {{"001", "010", "000", "110"},
														{"111", "010", "111", "010"},
														{"000", "011", "100", "010"}};

	private static string[,] zShape1 = new string[3,4] {{"011", "010", "000", "100"},
														{"110", "011", "011", "110"},
														{"000", "001", "110", "010"}};

	private static string[,] zShape2 = new string[3,4] {{"110", "001", "000", "010"},
														{"011", "011", "110", "110"},
														{"000", "010", "011", "100"}};

	private static string[,] barShape = new string[4,4] {{"0000", "0100", "0000", "0010"},
													   	 {"0000", "0100", "1111", "0010"},
														 {"1111", "0100", "0000", "0010"},
														 {"0000", "0100", "0000", "0010"}};

	private static string[,] sqShape = new string[4,4] {{"0000", "0000", "0000", "0000"},
														{"0110", "0110", "0110", "0110"},
														{"0110", "0110", "0110", "0110"},
														{"0000", "0000", "0000", "0000"}};

	public static string[][,] shapes = new string[][,] {tShape, lShape1, lShape2, zShape1, zShape2, barShape, sqShape};
	public string[,] shape;
	private int currentRotation = 2;
	public int width = 3;
	public int height = 3;
	public Block[,] grid;
	public GridCoord topLeft;
	private Block[] blocks;
	public Material[] materials;
	public GameObject blockPrefab;

	// Sets the type of the shape (of the 7 types up above)
	public void setType (int shapeType) {
		blocks = new Block[4];
		for (int i = 0; i < blocks.Length; i++)
			blocks [i] = Instantiate (blockPrefab).GetComponent<Block> ();
		shape = shapes [shapeType];
		width = shape [0, 0].Length;
		height = shape.GetLength(0);
		Material blockColor = materials [Random.Range (0, materials.Length)];
		foreach (Block b in blocks)
			b.gameObject.GetComponent<MeshRenderer> ().material = blockColor;
	}

	// Passes in the grid it should be added to and the top left coordinate
	// where the piece should be placed
	public bool addToGrid (Block[,] grid, GridCoord topLeft) {
		this.grid = grid;
		this.topLeft = topLeft;
		GridCoord[] tests = {new GridCoord (topLeft.row - 3, topLeft.col), 
			new GridCoord (topLeft.row - 2, topLeft.col), 
			new GridCoord (topLeft.row - 1, topLeft.col), topLeft};
		GridCoord resultCoord = null;
		foreach (GridCoord coord in tests) {
			if (canAddAt (coord)) {
				resultCoord = coord;
			}
		}
		if (resultCoord != null) {
			addAt (resultCoord);
		}
		if (resultCoord != topLeft) {
			return false;
		}
		return true;
	}

	// Attempts to rotate the piece (uses wall kicks if it can't rotate in place)
	// If it's at the bottom, it can perform floor kicks
	// If the piece cannot rotate, returns false;
	public bool rotate () {
		bool atBottom = isAtBottom ();
		removeFromGrid ();
		cycleRotation ();
		GridCoord left = new GridCoord (topLeft.row, topLeft.col - 1);
		GridCoord right = new GridCoord (topLeft.row, topLeft.col + 1);
		if (canAddAt (topLeft)) {
			addAt (topLeft);
			return true;
		} else if (atBottom) { // it has blocks below it and can't go down, so try a floor kick
			GridCoord up = new GridCoord (topLeft.row - 1, topLeft.col);
			if (canAddAt (up)) {
				addAt (up);
				topLeft = up;
				return true;
			}
		} else if (canAddAt (left)) {
			addAt (left);
			topLeft = left;
			return true;
		} else if (canAddAt (right)) {
			addAt (right);
			topLeft = right;
			return true;
		} else if (shape == barShape) { // the bar shape can have 2 columns of whitespace, so it can kick 2
			GridCoord left2 = new GridCoord (topLeft.row, topLeft.col - 2);
			GridCoord right2 = new GridCoord (topLeft.row, topLeft.col + 2);
			if (canAddAt (left2)) {
				addAt (left2);
				topLeft = left2;
				return true;
			} else if (canAddAt (right2)) {
				addAt (right2);
				topLeft = right2;
				return true;
			}
		}
		cycleRotation (-1);
		addAt (topLeft);
		return false;
	}

	// Removes the shape from the grid, checks if it can be readded one space below.
	// If it can't, then it's at the bottom (and true will be returned). Re-adds the
	// shape to the grid, then returns the result
	public bool isAtBottom () {
		removeFromGrid ();
		GridCoord belowCoords = new GridCoord (topLeft.row + 1, topLeft.col);
		if (!canAddAt (belowCoords)) { // it can't go down further
			addAt (topLeft);
			return true;
		}
		addAt (topLeft);
		return false;
	}

	// Shifts the shape in the given direction if possible. Returns whether or not
	// the given shift was successful.
	public bool shiftHorizontal (int dir) {
		return shift (dir, 0);
	}

	// Shifts the shape down if possible. Returns whether or not the shift succeeded.
	public bool shiftDown () {
		return shift (0, 1);
	}

	// Shifts the shape in the given horizontal and vertical directions. Returns whether
	// or not the shift succeeded.
	private bool shift (int horizontal, int vertical) {
		removeFromGrid ();
		GridCoord newCoords = new GridCoord (topLeft.row + vertical, topLeft.col + horizontal);
		if (canAddAt (newCoords)) {
			addAt (newCoords);
			topLeft = newCoords;
			return true;
		}
		addAt (topLeft);
		return false;
	}
	
	// returns whether or not the rotated block can be added back in at the
	// given top left coord.
	private bool canAddAt (GridCoord topLeft) {
		for (int r = 0; r < shape.GetLength(0); r++) {
			for (int c = 0; c < shape[r, currentRotation].Length; c++) {
				// It can't add if there is a conflicting block or it is trying to place a block outside the grid.
				if (shape[r, currentRotation][c] == '1' && (topLeft.row + r < 0 ||
				                                            topLeft.row + r >= grid.GetLength(0) ||
				                                            topLeft.col + c < 0 || 
				                                            topLeft.col + c >= grid.GetLength(1) || 
				                                            grid[topLeft.row + r, topLeft.col + c] != null)) {
					return false;
				}
			}
		}
		return true;
	}

	// adds the piece back at the designated location. to avoid collisions/exceptions, use canAddAt first
	private void addAt (GridCoord topLeft) {
		int i = 0;
		for (int r = 0; r < shape.GetLength(0); r++) {
			for (int c = 0; c < shape[r, currentRotation].Length; c++) {
				if (shape[r, currentRotation][c] == '1') {
					grid[topLeft.row + r, topLeft.col + c] = blocks[i];
					i++;
				}
			}
		}
	}

	// Increments rotation in the direction given (default 1). should only take 1 or -1.
	private void cycleRotation (int dir = 1) {
		if (currentRotation + dir == 4)
			currentRotation = 0;
		else if (currentRotation + dir == -1)
			currentRotation = 3;
		else
			currentRotation += dir;
	}

	// removes the piece from its grid
	private void removeFromGrid () {
		for (int r = 0; r < shape.GetLength(0); r++) {
			for (int c = 0; c < shape[r, currentRotation].Length; c++) {
				if (shape[r, currentRotation][c] == '1') {
					int rCoord = topLeft.row + r;
					int cCoord = topLeft.col + c;
					if (rCoord >= 0 && rCoord < grid.GetLength(0) &&
					    cCoord >= 0 && cCoord < grid.GetLength(1))
						grid[rCoord, cCoord] = null;
				}
			}
		}
	}
}
