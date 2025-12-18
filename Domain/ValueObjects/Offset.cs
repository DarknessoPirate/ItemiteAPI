using Microsoft.EntityFrameworkCore;

namespace Domain.ValueObjects;

[Owned]
public class Offset
{
    public int X { get; set; }
    public int Y { get; set; }
}