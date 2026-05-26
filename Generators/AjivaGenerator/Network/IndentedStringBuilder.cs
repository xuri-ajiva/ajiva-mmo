using System.Text;

namespace AjivaGenerator.Network;

internal class IndentedStringBuilder
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel;
    private const string IndentString = "    ";

    public void Indent() => _indentLevel++;

    public void Unindent()
    {
        if (_indentLevel > 0)
        {
            _indentLevel--;
        }
    }

    public void AppendLine(string line = "")
    {
        if (!string.IsNullOrEmpty(line))
        {
            for (int i = 0; i < _indentLevel; i++)
            {
                _builder.Append(IndentString);
            }
        }

        _builder.AppendLine(line);
    }

    public void EnterScope(char open = '{')
    {
        AppendLine(open.ToString());
        Indent();
    }

    public void ExitScope(char close = '}', bool addSemicolon = false)
    {
        Unindent();
        var line = close.ToString();
        if (addSemicolon)
        {
            line += ";";
        }

        AppendLine(line);
    }

    public override string ToString() => _builder.ToString();
}