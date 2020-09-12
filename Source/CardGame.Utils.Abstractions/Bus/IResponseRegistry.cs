using System.Collections.Generic;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface IResponseRegistry
    {
        IReadOnlyDictionary<string, ResponseRegistration> ResponseRegistry { get; }
    }
}