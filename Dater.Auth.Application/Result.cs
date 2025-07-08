using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application
{
    public class Result<T>
    {
        public T? Value { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static Result<T> OnSuccess(T value, int statusCode = 200) => new Result<T>
            {
                Value = value,
                StatusCode = statusCode
            };

        public static Result<T> OnError(int statusCode, string errorMessage, T value = default) => new Result<T>
            {
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                Value = value
            };
    }
}
