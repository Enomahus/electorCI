using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Application.Models
{
    [ExcludeFromCodeCoverage]
    public class Result : IResult
    {
        public double Duration { protected set; get; }

        public void SetDuration(double duration) => Duration = duration;

        public static Result Default() => new();
    }

    [ExcludeFromCodeCoverage]
    public class Result<T> : Result
    {
        public T? Data { get; set; }

        protected Result(T? data = default)
        {
            Data = data;
        }

        public static Result<T> From(T? data = default) => new Result<T>(data);
    }
}
