namespace AccessControl.SharedKernel;

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess ? _value! :
        throw new InvalidOperationException("No se puede leer el valor de un resultado fallido.");
}
