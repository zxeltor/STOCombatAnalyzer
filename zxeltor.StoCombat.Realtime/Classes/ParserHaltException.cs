namespace zxeltor.StoCombat.Realtime.Classes
{
    public class ParserHaltException : Exception
    {
        public ParserHaltException() : base() { }
        public ParserHaltException(string message) : base(message) { }
        public ParserHaltException(string message, Exception e) : base(message, e) { }
    }
}
