using System.Collections.Generic;

namespace Projekat.Utils
{
    public class QueueItem
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public List<KeyValuePair<int, int>> Path { get; set; }

        public QueueItem()
        {
            Row = Col = 0;
            Path = new List<KeyValuePair<int, int>>();
        }

        public QueueItem(int row, int col, List<KeyValuePair<int, int>> path)
        {
            Row = row; Col = col; Path = path;
        }
    }
}
