using System.ComponentModel.DataAnnotations;

namespace SharedModel;

// Simple journal
public class JournalOperation
{
    [Key] public int OperationId { get; set; }

    public Operation Operation { get; set; }
    public string? Zone { get; set; }
    public string? RecordName { get; set; }
    public string? RecordValue { get; set; }
    public DateTime CreatedAt { get; set; }
}