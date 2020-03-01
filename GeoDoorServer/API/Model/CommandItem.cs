namespace GeoDoorServer3.API.Model
{
    public class CommandItem
    {
        public string Id { get; set; }
        public string CommandValue { get; set; }
        public Command Command { get; set; }
    }

    public enum Command
    {
        CheckUser,
        OpenDoor,
        OpenGate
    }

    public enum CommandValue
    {
        Open,
        Close,
        ForceOpen,
        ForceClose
    }

    public enum CommandValueAnswer
    {
        AlreadyOpen,
        AlreadyClosed,
        GateOpening,
        GateClosing,
        AlreadyOpening,
        AlreadyClosing
    }
}
