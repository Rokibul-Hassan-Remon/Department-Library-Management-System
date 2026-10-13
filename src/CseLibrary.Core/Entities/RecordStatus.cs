namespace CseLibrary.Core.Entities;

/// <summary>Row lifecycle (D-51). Stored as tinyint; 0 is invalid; new values are appended, never renumbered.</summary>
public enum RecordStatus : byte
{
    Active = 1,
    Inactive = 2,
    Deleted = 3
}
