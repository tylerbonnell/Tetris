public class GridCoord {

	public int row;
	public int col;

	// Creates a new GridCoord at the given row and column
	public GridCoord (int row, int col) {
		this.row = row;
		this.col = col;
	}

	// Returns a string like "[5, 7]" indicating row and column
	public override string ToString () {
		return "[" + row + ", " + col + "]";
	}
}
