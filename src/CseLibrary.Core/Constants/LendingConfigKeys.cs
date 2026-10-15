namespace CseLibrary.Core.Constants;

/// <summary>Lending settings that are not per member type (those live in LibraryPolicies).</summary>
public static class LendingConfigKeys
{
    public const string ReminderDaysBefore = "Lending.ReminderDaysBefore";

    public static readonly IReadOnlyList<(string Key, string Value)> Defaults = [(ReminderDaysBefore, "2")];

    public static int DefaultFor(string key) => int.Parse(Defaults.First(d => d.Key == key).Value);
}
