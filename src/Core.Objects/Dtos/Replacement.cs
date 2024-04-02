using System;

namespace Core.Objects.Dtos
{
    public class Replacement
    {
        readonly string newString = null;

        public string Old { get; }
        public string New { get { return newString ?? ReplaceFunction(Old); } }

        public Func<string, string> ReplaceFunction { get; }

        public Replacement(string old, string _new)
        {
            Old = old;
            newString = _new;
            ReplaceFunction = null;
        }

        public Replacement(string old, Func<string, string> replacer)
        {
            Old = old;
            newString = null;
            ReplaceFunction = replacer;

            if(replacer is null)
                ReplaceFunction = (s) => s;
        }
    }
}