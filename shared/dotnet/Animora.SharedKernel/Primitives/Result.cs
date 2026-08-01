namespace Animora.SharedKernel.Primitives;

/// <summary>
/// The outcome of an operation that can fail for an expected, domain reason: success, or an
/// <see cref="Primitives.Error"/> carrying a stable code. Exceptions stay reserved for bugs and
/// infrastructure faults.
/// </summary>
/// <remarks>
/// This is the in-process companion to the wire contract: a handler returns a code-bearing failure
/// exactly as the API returns one in a problem-details body, so a desktop screen running offline
/// branches on the same <c>ERR-{MODULE}-{NNN}</c> value it will branch on once the call goes to the
/// server (INV-07).
/// <para>
/// The type is not externally derivable (its constructor is <c>private protected</c>): the only two
/// shapes are this one and <see cref="Result{TValue}"/>, so consumers can treat
/// <see cref="IsSuccess"/> as exhaustive.
/// </para>
/// </remarks>
public class Result
{
    // The valueless success carries no state, so one instance serves every call site.
    private static readonly Result SuccessResult = new(error: null);

    private readonly Error? _error;

    private protected Result(Error? error) => _error = error;

    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess => _error is null;

    /// <summary>Whether the operation failed; <see cref="Error"/> is readable only in this state.</summary>
    public bool IsFailure => _error is not null;

    /// <summary>The failure reason.</summary>
    /// <exception cref="InvalidOperationException">The result is a success.</exception>
    public Error Error =>
        _error ?? throw new InvalidOperationException("A successful result has no error; check IsFailure first.");

    /// <summary>A success with no value.</summary>
    public static Result Success() => SuccessResult;

    /// <summary>A success carrying <paramref name="value"/>.</summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, error: null);

    /// <summary>A failure with no value.</summary>
    public static Result Failure(Error error) => new(Required(error));

    /// <summary>A failure for an operation that would otherwise have produced a <typeparamref name="TValue"/>.</summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default!, Required(error));

    private static Error Required(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return error;
    }
}

/// <summary>
/// A <see cref="Result"/> that carries a value when it succeeds.
/// </summary>
/// <typeparam name="TValue">The produced value's type.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue _value;

    internal Result(TValue value, Error? error)
        : base(error) => _value = value;

    /// <summary>The produced value.</summary>
    /// <exception cref="InvalidOperationException">
    /// The result is a failure. A failed result holds no value, so reading one is a caller bug
    /// rather than a state to handle — the alternative, handing back <c>default</c>, turns a
    /// handled failure into a null further downstream.
    /// </exception>
    public TValue Value =>
        IsSuccess
            ? _value
            : throw new InvalidOperationException($"A failed result ({Error.Code}) has no value; check IsSuccess first.");
}
