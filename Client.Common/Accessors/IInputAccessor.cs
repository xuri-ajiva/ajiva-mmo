using Silk.NET.Input;

namespace Client.Common.Accessors;

public interface IInputAccessor
{
    public IInputContext InputContext { get; }
}