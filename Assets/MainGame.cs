using UnityEngine;
using System.Collections;

public class MainGame : MonoBehaviour {

	public static int width = 10;
	public static int height = 21; // height should be one more than the desired visible height
	public Block[,] grid = new Block[height,width]; // access a location with grid[row, column]
	public GameObject piece;
	private Piece currentPlayerPiece;

	private float tempo = .5f;
	private float timeElapsed = 0f;
	private float lastShiftTime = 0f; // last time the pieces shifted down

	private bool gameOver = false;
	private bool spawnPieceNextTurn = true;
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("shifting = " + shifting);
		timeElapsed += Time.deltaTime;

		if (!gameOver) {
			if (spawnPieceNextTurn) {
				spawnPieceNextTurn = false;
				bool successfulSpawn = spawnPiece ();
				if (!successfulSpawn) {
					gameOver = true;
				}
			}

			if (timeElapsed - lastShiftTime > tempo) {
				shiftPlayerDown ();
				lastShiftTime = timeElapsed;
			} else if (gameOver) {
				Debug.Log ("YOU SUCK");
			}
			playerInput ();
		}
	}

	private void playerInput () {
		bool drawFlag = false;

		if (currentPlayerPiece != null) {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				bool atBottom = currentPlayerPiece.isAtBottom ();
				if (currentPlayerPiece.rotate ()) {
					if (atBottom)
						lastShiftTime = timeElapsed; // performed a floor kick, reset timer for freezing block
					drawFlag = true;
				}
			}
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				shiftPlayerDown ();
				drawFlag = true;
			}
			int dir = 0;
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				dir++;
			}
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				dir--;
			}
			if (dir != 0) {
				currentPlayerPiece.shiftHorizontal (dir);
				drawFlag = true;
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				while (shiftPlayerDown ());
				drawFlag = true;
			}
		}
		
		if (drawFlag)
			drawPieces ();
	}

	// Shifts the player down. If it can't, then the piece is now frozen,
	// and a new player piece is spawned.
	private bool shiftPlayerDown (bool draw = true) {
		bool result = currentPlayerPiece.shiftDown ();
		if (!result) { // piece couldn't move down
			spawnPieceNextTurn = true;
			rowTest ();
		} else if (draw) {
			drawPieces ();
		}
		return result;
	}

	// Tests for complete rows and deletes them
	private void rowTest () {
		int rowsDestroyed = 0;
		for (int r = height - 1; r >= 0; r--) {
			bool completeRow = true;
			for (int c = 0; c < width; c++) {
				if (grid[r, c] == null) {
					completeRow = false;
					break;
				}
			}
			if (completeRow) {
				rowsDestroyed++;
				for (int c = 0; c < width; c++)
					if (grid[r, c] != null)
						grid[r, c].smash ();
				for (int r2 = r; r2 > 0; r2--) {
					for (int c = 0; c < width; c++) {
						grid[r2, c] = grid[r2 - 1, c];
					}
				}
				for (int c = 0; c < width; c++)
					grid[0, c] = null;
				// have to increment so we search this row again next time through the loop
				r++;
			}
		}
	}

	// draws the top n rows
	private void drawPieces (int n = -1) {
		if (n == -1)
			n = height;
		for (int r = 0; r < n; r++) {
			for (int c = 0; c < width; c++) {
				Block b = grid[r,c];
				if (b != null) {
					if (b.transform.position.x != c || b.transform.position.y != -r)
						b.transform.position = new Vector3 (c, -r);
				}
			}
		}
	}

	// spawns the new currentPlayerPiece
	private bool spawnPiece () {
		if (currentPlayerPiece != null)
			currentPlayerPiece.setPlayerControlled (false);
		GameObject pieceObject = Instantiate (piece);
		currentPlayerPiece = pieceObject.GetComponent<Piece> ();
		currentPlayerPiece.setType (Random.Range (0, Piece.shapes.GetLength(0)));
		GridCoord gridTopLeft = new GridCoord (0, width/2 - (currentPlayerPiece.width + 1)/2 + (currentPlayerPiece.width == 3 ? Random.Range(0, 2) : 0));
		bool result = currentPlayerPiece.addToGrid (grid, gridTopLeft);
		drawPieces ();
		return result;
	}
}
