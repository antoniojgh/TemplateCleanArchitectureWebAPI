using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;

namespace DientesLimpios.Domain.ValueObjects
{
    public sealed record TimeInterval
    {
        public DateTime Start { get; private init; }
        public DateTime End { get; private init; }

        private TimeInterval() { }   // EF Core

        private TimeInterval(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public static Result<TimeInterval> Create(DateTime start, DateTime end)
        {
            if (start >= end)
                return Result.Failure<TimeInterval>(
                    DomainErrors.TimeInterval.StartGreaterThanOrEqualToEnd);

            return Result.Success(new TimeInterval(start, end));
        }
    }

}
