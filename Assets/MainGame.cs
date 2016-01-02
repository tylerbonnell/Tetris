using UnityEngine;
using System.Collections;

public class MainGame : MonoBehaviour {

	public static int width = 10;
	public static int height = 21; // height should be one more than the desired visible height
	public Block[,] grid = new Block[height,width]; // access a location with grid[row, column]
	public GameObject piece;
	private Piece currentPlayerPiece;
	private Transform[] ghostBlockPieces;
	public GameObject ghostBlockPrefab;

	private float baseTempo = .75f;
	private float tempo;
	private float timeElapsed = 0f;
	private float lastShiftTime = 0f; // last time the pieces shifted down

	private bool gameOver = false;
	private bool gameEnded = false;
	private bool spawnPieceNextTurn = true;
	private bool atBottom = false;
	
	private bool canHold = true;
	private bool spawnNextFromHeldPiece = false;
	public PieceStorage heldPiece;

	void Start () {
		write3dText ();
		initialSpawnNextPieces ();
		//spawnGhostPieces ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.LoadLevel ("MainMenu");
		}
		
		tempo = Mathf.Max (.01f, baseTempo - timeElapsed / 800);
		//Debug.Log (tempo);

		timeElapsed += Time.deltaTime;

		if (gameOver && !gameEnded) {
			endGame ();
		} else if (!gameOver) {
			if (spawnPieceNextTurn) {
				spawnPieceNextTurn = false;
				bool successfulSpawn = spawnPiece (spawnNextFromHeldPiece);
				spawnNextFromHeldPiece = false;
				lastShiftTime = timeElapsed;
				if (!successfulSpawn) {
					gameOver = true;
				}
			}
			if (!gameOver) {
				playerInput ();
				if (timeElapsed - lastShiftTime > tempo) {
					shiftPlayerDown ();
					lastShiftTime = timeElapsed;
				}
			}
		}
		
		if (drawFlag)
			drawPieces ();

		drawFlag = false;
	}

	private bool drawFlag;
	private void playerInput () {
		if (currentPlayerPiece != null) {
			atBottom = currentPlayerPiece.isAtBottom ();
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				if (currentPlayerPiece.rotate ()) { // successfully rotated
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
				bool success = currentPlayerPiece.shiftHorizontal (1);
				checkBottomMovement (success);
				InvokeRepeating ("moveRight", .25f, .075f);
				drawFlag = true;
			} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				bool success = currentPlayerPiece.shiftHorizontal (-1);
				checkBottomMovement (success);
				InvokeRepeating ("moveLeft", .25f, .075f);
				drawFlag = true;
			}

			// Hard drop
			if (Input.GetKeyDown (KeyCode.Space)) {
				while (shiftPlayerDown (false));
				drawFlag = true;
				ShakyCam.singleton.shake (.3f);
			}
			
			// Store a piece
			if (Input.GetKeyDown (KeyCode.LeftShift) && canHold) {
				canHold = false;
				spawnNextFromHeldPiece = true;
				currentPlayerPiece.deleteFromGrid ();
				spawnPieceNextTurn = true;
				drawFlag = true;
			}
		}
	}

	private void moveRight () {
		if (Input.GetKey (KeyCode.LeftArrow) || !Input.GetKey (KeyCode.RightArrow)) {
			CancelInvoke ("moveRight");
		} else {
			bool success = currentPlayerPiece.shiftHorizontal (1);
			checkBottomMovement (success);
			drawFlag = true;
		}
	}

	private void moveLeft () {
		if (Input.GetKey (KeyCode.RightArrow) || !Input.GetKey (KeyCode.LeftArrow)) {
			CancelInvoke ("moveLeft");
		} else {
			bool success = currentPlayerPiece.shiftHorizontal (-1);
			checkBottomMovement (success);
			drawFlag = true;
		}
	}
	
	// If the player piece cannot go down any more, but they move L/R, they gain some time
	private void checkBottomMovement (bool successfullyMoved) {
		if (atBottom && successfullyMoved) {
			lastShiftTime = timeElapsed;
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
			canHold = true;
			rowTest ();
		} else if (draw)
			drawFlag = true;
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
			ShakyCam.singleton.shake (Mathf.Max (1f, rowsDestroyed * 1f/3f));
	}

	private int rowToDestroyNext = 0;
	// Ends the game by blowing up each row from the top down
	private void endGame () {
		gameEnded = true;
		drawPieces ();
		InvokeRepeating ("endDestroyRow", 0f, .1f);
	}
	private void endDestroyRow () {
		if (rowToDestroyNext >= grid.GetLength (0)) {
			CancelInvoke ("endDestroyRow");
			Application.LoadLevel ("MainMenu");
		} else {
			for (int i = 0; i < width; i++)
				if (grid[rowToDestroyNext, i] != null)
					Destroy(grid[rowToDestroyNext, i].gameObject);
			rowToDestroyNext++;
		}
	}

	int timesCalled = 0;
	// draws the top n rows
	private void drawPieces (int n = -1) {
		//Debug.Log ("drawPieces call #" + ++timesCalled);
		if (n == -1)
			n = height;
		for (int r = 0; r < n; r++) {
			for (int c = 0; c < width; c++) {
				Block b = grid[r,c];
				if (b != null) {
					//if (b.transform.position.x != c || b.transform.position.y != -r)
						b.transform.position = new Vector3 (c, -r);
				}
			}
		}
		//currentPlayerPiece.drawGhostBlock (ghostBlockPieces);
	}

	public PieceStorage[] nextPieces;

	private void initialSpawnNextPieces () {
		for (int i = 0; i < nextPieces.Length; i++) {
			nextPieces[i].set (Instantiate(piece).GetComponent<Piece> ());
		}
	}

	// spawns the new currentPlayerPiece
	private bool spawnPiece (bool fromHeldPiece = false) {		
		// shift all the next pieces up and spawn the next piece
		if (!fromHeldPiece) {
			currentPlayerPiece = nextPieces[0].piece;
			for (int i = 0; i < nextPieces.Length - 1; i++) {
				nextPieces[i].set (nextPieces[i + 1].piece);
			}
			nextPieces[nextPieces.Length - 1].set (Instantiate (piece).GetComponent<Piece> ());
		} else { // we're spawning from the held piece
			Piece newPiece = heldPiece.piece;
			heldPiece.set (currentPlayerPiece);
			currentPlayerPiece = newPiece;
			if (currentPlayerPiece == null) {
				return spawnPiece ();
			}
		}
		GridCoord gridTopLeft = new GridCoord (0, width/2 - (currentPlayerPiece.width + 1)/2 + (currentPlayerPiece.width == 3 ? Random.Range(0, 2) : 0));
		bool result = currentPlayerPiece.addToGrid (grid, gridTopLeft);
		drawPieces ();
		return result;
	}
	
	private void spawnGhostPieces () {
		ghostBlockPieces = new Transform[4];
		for (int i = 0; i < ghostBlockPieces.Length; i++) {
			ghostBlockPieces[i] = (Instantiate (ghostBlockPrefab) as GameObject).transform;
		}
	}
	
	// Writes out all the GUI words in 3d voxels
	private void write3dText () {
		Alphabet.singleton.write ("next", new Vector3 (13.75f, .25f, 0), Quaternion.identity.eulerAngles, Vector3.one * .5f);
		Alphabet.singleton.write ("hold", new Vector3 (-13.25f, .25f, 0), Quaternion.identity.eulerAngles, Vector3.one * .5f);
	}
}
