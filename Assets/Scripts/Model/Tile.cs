using System;

public class Tile {
    public int X;
    public int Y;

    public Tile(int x, int y) {
        this.X = x;
        this.Y = y;
    }


    public string ToFigurineAlgebraicNotation(Board board) {
        int a = (int) 'a';
        char x = Convert.ToChar(a + X);
        
        int zero = '0';
        char y = board.WhiteFacingUp ? Convert.ToChar(zero + Y + 1) : Convert.ToChar((zero+board.Tiles.GetLength(1))- Y);
        
        return x + "" + y;
    }
}