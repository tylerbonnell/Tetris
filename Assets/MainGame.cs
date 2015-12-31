using UnityEngine;
using System.Collections;

public class MainGame : MonoBehaviour {

	public static int width = 10;
	public static int height = 21; // height should be one more than the desired visible height
	public Block[,] grid = new Block[height,width]; // access a location with grid[row, column]
	public GameObject piece;
	private Piece currentPlayerPiece;

	private float baseTempo = .75f;
	private float tempo;
	private float timeElapsed = 0f;
	private float lastShiftTime = 0f; // last time the pieces shifted down

	private bool gameOver = false;
	private bool spawnPieceNextTurn = true;

	void Start () {
		write3dText ();
		initialSpawnNextPieces ();
	}

	// Update is called once per frame
	void Update () {
		tempo = Mathf.Max (.01f, baseTempo - timeElapsed / 1000);
		//Debug.Log (tempo);

		timeElapsed += Time.deltaTime;

		if (!gameOver) {
			if (spawnPieceNextTurn) {
				spawnPieceNextTurn = false;
				bool successfulSpawn = spawnPiece ();
				lastShiftTime = timeElapsed;
				if (!successfulSpawn) {
					gameOver = true;
				}
			}

			if (timeElapsed - lastShiftTime > tempo) {
				shiftPlayerDown ();
				lastShiftTime = timeElapsed;
			}
			playerInput ();
		}
		
		if (gameOver) {
			endGame ();
		}
	}

	private bool drawFlag;
	private void playerInput () {
		if (currentPlayerPiece != null) {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				bool atBottom = currentPlayerPiece.isAtBottom ();
				if (currentPlayerPiece.rotate ()) {
					if (atBottom)
						lastShiftTime = timeElapsed; // performed a floor kick, reset timer for freezing block
					drawFlag = true;
				}
			}

			// Moving down
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				InvokeRepeating ("moveDown", 0f, .075f);
			}

			// Moving left and right
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				currentPlayerPiece.shiftHorizontal (1);
				InvokeRepeating ("moveRight", .25f, .075f);
				drawFlag = true;
			} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				currentPlayerPiece.shiftHorizontal (-1);
				InvokeRepeating ("moveLeft", .25f, .075f);
				drawFlag = true;
			}

			// Hard drop
			if (Input.GetKeyDown (KeyCode.Space)) {
				while (shiftPlayerDown ());
				drawFlag = true;
				ShakyCam.singleton.shake (.3f);
			}
		}
		
		if (drawFlag)
			drawPieces ();

		drawFlag = false;
	}

	private void moveRight () {
		if (Input.GetKey (KeyCode.LeftArrow) || !Input.GetKey (KeyCode.RightArrow)) {
			CancelInvoke ("moveRight");
		} else {
			currentPlayerPiece.shiftHorizontal (1);
			drawFlag = true;
		}
	}

	private void moveLeft () {
		if (Input.GetKey (KeyCode.RightArrow) || !Input.GetKey (KeyCode.LeftArrow)) {
			CancelInvoke ("moveLeft");
		} else {
			currentPlayerPiece.shiftHorizontal (-1);
			drawFlag = true;
		}
	}

	private void moveDown () {
		if (!Input.GetKey (KeyCode.DownArrow)) {
			CancelInvoke ("moveDown");
		} else {
			shiftPlayerDown ();
			lastShiftTime = timeElapsed;
			drawFlag = true;
		}
	}

	// Shifts the player down. If it can't, then the piece is now frozen,
	// and a new player piece is spawned.
	private bool shiftPlayerDown (bool draw = true) {
		bool result = currentPlayerPiece.shiftDown ();
		if (!result) { // piece couldn't move down
			// make sure this piece didn't just go above the top row
			for (int c = 0; c < width; c++)
				if (grid[0, c] != null)
					gameOver = true;
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
		if (rowsDestroyed > 0)
			ShakyCam.singleton.shake (Mathf.Max (1f, rowsDestroyed * 1f/2f));
	}

	private int rowToDestroyNext = 1;
	private void endGame () {
		for (int i = 0; i < width; i++)
			if (grid[0, i] != null)
				grid[0, i].smash (false);
		InvokeRepeating ("endDestroyRow", 0f, .1f);
	}
	private void endDestroyRow () {
		if (rowToDestroyNext >= grid.GetLength (0)) {
			CancelInvoke ("endDestroyRow");
		} else {
			for (int i = 0; i < width; i++)
				if (grid[rowToDestroyNext, i] != null)
					grid[rowToDestroyNext, i].smash ();
			rowToDestroyNext++;
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

	public PieceStorage[] nextPieces;

	private void initialSpawnNextPieces () {
		for (int i = 0; i < nextPieces.Length; i++) {
			nextPieces[i].set (Instantiate(piece).GetComponent<Piece> ());
		}
	}

	// spawns the new currentPlayerPiece
	private bool spawnPiece () {		
		// shift all the next pieces up and spawn the next piece
		currentPlayerPiece = nextPieces[0].piece;
		for (int i = 0; i < nextPieces.Length - 1; i++) {
			nextPieces[i].set (nextPieces[i + 1].piece);
		}
		nextPieces[nextPieces.Length - 1].set (Instantiate (piece).GetComponent<Piece> ());

		GridCoord gridTopLeft = new GridCoord (0, width/2 - (currentPlayerPiece.width + 1)/2 + (currentPlayerPiece.width == 3 ? Random.Range(0, 2) : 0));
		bool result = currentPlayerPiece.addToGrid (grid, gridTopLeft);
		drawPieces ();
		return result;
	}
	
	// Writes out all the GUI words in 3d voxels
	private void write3dText () {
		Alphabet.singleton.write ("next", new Vector3 (13.75f, .25f, 0), Quaternion.identity.eulerAngles, Vector3.one * .5f);
		//Alphabet.singleton.write ()
	}
}
