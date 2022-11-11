namespace search.Shared
{
    public class WaffleText
    {
        public string? GUID { get; set; }
        public string? WaffleHead { get; set; }
        public string? WaffleBody { get; set; }

        public WaffleText() {}
        public WaffleText(string _guid, string _waffleHead, string _waffleBody)
        {
            GUID = _guid;
            WaffleHead = _waffleHead;
            WaffleBody = _waffleBody;
        }
    }
}