namespace cCoder.Core.Objects.Dtos;

public class Replacement
{
    private readonly string newString = null;

    public string Old { get; }
    public string New => newString ?? ReplaceFunction(Old);

    public Func<string, string> ReplaceFunction { get; } = (s) => s;

    public Replacement(string old, string _new)
    {
        Old = old;
        newString = _new;
    }

    public Replacement(string old, Func<string, string> replacer)
    {
        Old = old;

        if(replacer is not null)
            ReplaceFunction = replacer;
    }
}