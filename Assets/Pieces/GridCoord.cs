public class GridCoord {

	public int row;
	public int col;

	public GridCoord (int row, int col) {
		this.row = row;
		this.col = col;
	}

	public override string ToString () {
		return "[" + row + ", " + col + "]";
	}
}
