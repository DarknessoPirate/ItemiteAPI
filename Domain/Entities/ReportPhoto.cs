namespace Domain.Entities;

public class ReportPhoto
{
    public int Id { get; set; }
    
    public int PhotoId { get; set; }
    public Photo Photo { get; set; }
    
    public int ReportId { get; set; }
    public Report Report { get; set; }
}