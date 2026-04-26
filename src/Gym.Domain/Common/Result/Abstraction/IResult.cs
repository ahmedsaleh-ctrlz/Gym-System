namespace Gym.Domain.Common.Result.Abstraction;

public interface IResult
{
    List<Error> Errors { get; }
    bool IsSuccess { get; }
}


public interface IResult<out TValue> : IResult
{
    TValue Value { get; }
}