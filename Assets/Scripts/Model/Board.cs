    public class Board {
        public Tile[,] Tiles;
        public Piece[,] Pieces;

        public PlayerPosition FirstPlayerPosition;
        
        public Board(Tile[,] tiles, Piece[,] pieces, PlayerPosition firstPlayerPosition) {
            this.Tiles = tiles;
            this.Pieces = pieces;
            this.FirstPlayerPosition = firstPlayerPosition;
        }

    }
