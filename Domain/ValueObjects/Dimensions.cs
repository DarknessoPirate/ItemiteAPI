using Microsoft.EntityFrameworkCore;

namespace Domain.ValueObjects;

[Owned]
public class Dimensions
{
    public int Width { get; set; }
    public int Height { get; set; }
}