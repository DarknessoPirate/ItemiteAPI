namespace Domain.Entities;

public class DisputeEvidence
{
    public int Id { get; set; }
    public int DisputeId { get; set; }
    public Dispute Dispute { get; set; }

    public int PhotoId { get; set; }
    public Photo Photo { get; set; }
    
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
}