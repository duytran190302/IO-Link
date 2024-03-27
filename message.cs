namespace IO_Link
{
    public class Message
    {
        public string Code { get; set; }
        public int Cid { get; set; }
        public string Adr { get; set; }
        public Data Data { get; set; }

    }
    public class Data
    {
        public string Eventno { get; set; }
        public string Srcurl { get; set; }
        public Dictionary<string, PayloadData> Payload { get; set; }
    }
    public class PayloadData
    {
        public int Code { get; set; }
        public string Data { get; set; }
    }
}
