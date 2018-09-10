using System;
using System.Collections.Generic;
using System.Text;

namespace SpreadShare.ZeroMQ
{
    class SubCommand : Command
    {
        public static string Hoi = "hoi";
        public string Hoid = "hoid";
        protected override string CommandString => "commandX";
        public string PublicString { get; set; }
        public static string PublicStaticString { get; set; }
        protected static string ProtectedStaticString { get; set; }
        protected string ProtectedString { get; set; }
        protected override string[] RequiredArguments { get; }

        public override void Parse()
        {
                
            throw new NotImplementedException();
        }

        public override void Action()
        {
            throw new NotImplementedException();
        }
    }
}
