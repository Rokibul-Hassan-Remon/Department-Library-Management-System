using CseLibrary.Core.Entities;

namespace CseLibrary.Core.Constants;

/// <summary>Lending policy lives in GeneralConfig so the library can change it without a release (Session 23 defaults).</summary>
public static class LendingConfigKeys
{
    public static string MaxLoans(MemberType type) => $"Lending.{type}.MaxLoans";
    public static string LoanDays(MemberType type) => $"Lending.{type}.LoanDays";
    public const string MaxRenewals = "Lending.MaxRenewals";
    public const string ReminderDaysBefore = "Lending.ReminderDaysBefore";

    public static readonly IReadOnlyList<(string Key, string Value)> Defaults =
    [
        (MaxLoans(MemberType.Student), "3"), (LoanDays(MemberType.Student), "14"),
        (MaxLoans(MemberType.Fellow), "5"), (LoanDays(MemberType.Fellow), "30"),
        (MaxRenewals, "1"), (ReminderDaysBefore, "2")
    ];

    public static int DefaultFor(string key) => int.Parse(Defaults.First(d => d.Key == key).Value);
}
