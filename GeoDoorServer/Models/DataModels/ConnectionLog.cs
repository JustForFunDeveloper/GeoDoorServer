using System;

namespace GeoDoorServer.Models.DataModels
{
    public class ConnectionLog
    {
        public int Id { get; set; }
        public DateTime MsgDateTime { get; set; }
        public string Message { get; set; }
    }
}