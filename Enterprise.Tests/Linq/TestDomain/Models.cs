using Enterprise.Core.Linq;

namespace Enterprise.Tests.Linq.TestDomain
{
    public interface IStudent
    {
        long ID { get; }

        string FirstName { get; }

        string LastName { get; }

        GradeLevel Year { get; }

        IAsyncEnumerable<int> ExamScores { get; }
    }

    public enum GradeLevel
    {
        Unknown = 0,
        FirstYear = 1,
        SecondYear,
        ThirdYear,
        FourthYear
    }
}
