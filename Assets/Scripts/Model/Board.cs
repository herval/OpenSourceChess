    public class Board {
        public Tile[,] Tiles;
        public Piece[,] Pieces;

        public bool WhiteFacingUp;
        
        public Board(Tile[,] tiles, Piece[,] pieces, bool whiteFacingUp) {
            this.Tiles = tiles;
            this.Pieces = pieces;
            this.WhiteFacingUp = whiteFacingUp;
        }

    }
