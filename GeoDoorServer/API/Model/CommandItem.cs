namespace GeoDoorServer.API.Model
{
    public class CommandItem
    {
        public AuthModel Authentication { get; set; }
        public CommandValue CommandValue { get; set; }
        public Command Command { get; set; }
    }

    public enum Command
    {
        OpenDoor,
        OpenGate,
        OpenGateAuto
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
