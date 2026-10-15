namespace CseLibrary.Core.Interfaces;

/// <summary>
/// Marks an entity whose changes are audited automatically (D-22 allowlist). Identity entities never implement it;
/// every property of an implementing entity is classified audited / not audited in its configuration (D-23).
/// </summary>
public interface IAuditable;
