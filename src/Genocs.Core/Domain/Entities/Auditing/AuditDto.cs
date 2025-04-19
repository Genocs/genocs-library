namespace Genocs.Core.Domain.Entities.Auditing;

public class AuditDto
{
    public DefaultIdType Id { get; set; }
    public DefaultIdType UserId { get; set; }
    public string? Type { get; set; }
    public string? TableName { get; set; }
    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
}