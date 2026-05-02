namespace Genocs.Identities.Application;

/// <summary>
/// Attribute used to mark a class as a contract.
/// This attribute is used to indicate that the class is a contract for a command, query or event.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ContractAttribute : Attribute;