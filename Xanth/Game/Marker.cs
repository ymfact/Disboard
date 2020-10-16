namespace Xanth
{
    class Marker
    {
        public int Row { get; }
        public int Column { get; }
        public int BoardSize { get; }

        public Marker(int row, int column, int boardSize)
        {
            if (row < 0 || boardSize <= row || column < 0 || boardSize <= column)
                throw new OutOfBoardException();
            Row = row;
            Column = column;
            BoardSize = boardSize;
        }

        public Marker Move(int deltaRow, int deltaColumn)
            => new Marker(Row + deltaRow, Column + deltaColumn, BoardSize);
    }
}
