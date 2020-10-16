namespace Xanth
{
    class Marker
    {
        public int Row { get; }
        public int Column { get; }

        public Marker(int row, int column)
        {
            if (row < 0 || 4 <= row || column < 0 || 4 <= column)
                throw new OutOfBoardException();
            Row = row;
            Column = column;
        }

        public Marker Move(int deltaRow, int deltaColumn)
            => new Marker(Row + deltaRow, Column + deltaColumn);
    }
}
