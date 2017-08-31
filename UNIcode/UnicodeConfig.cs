namespace UNIcode
{
    public class UnicodeConfig
    {
        public double ColumnCount { get; set; } = 10D;
        public string HoverBackground { get; set; } = "#0066CC";
        public string HoverForeground { get; set; } = "#FFFFFF";
        public bool Ignore { get; set; } = false;
        public double RowCount { get; set; } = 5D;
        public string SelectedFamily { get; set; }
        public string SelectedTypeface { get; set; }
        public int TileSize { get; set; } = 70;
        public double WindowHeight { get; set; } = 500D;
        public double WindowWidth { get; set; } = 760D;
    }
}
