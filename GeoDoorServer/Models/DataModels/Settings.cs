namespace GeoDoorServer.Models.DataModels
{
    public class Settings
    {
        public int Id { get; set; }
        public int MaxErrorLogRows { get; set; }
        public string StatusOpenHabLink { get; set; }
        public string GateOpenHabLink { get; set; }
        public string DoorOpenHabLink { get; set; }
        public int GateTimeout { get; set; }
        public int AutoGateTimeout { get; set; }
    }
}
