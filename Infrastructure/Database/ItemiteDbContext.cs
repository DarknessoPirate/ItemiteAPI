using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class ItemiteDbContext(DbContextOptions<ItemiteDbContext> options) : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<ListingBase> Listings {get;set;}
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ListingBase>()
            .HasDiscriminator<string>("ListingType")
            .HasValue<AuctionListing>("Auction")
            .HasValue<ProductListing>("Product");

        modelBuilder.Entity<ListingBase>()
            .HasMany(l => l.Categories)
            .WithMany(c => c.Listings);

        modelBuilder.Entity<ListingBase>()
            .HasOne(l => l.Owner)
            .WithMany(u => u.OwnedListings)
            .HasForeignKey(l => l.OwnerId)
            .OnDelete(DeleteBehavior.Cascade); // when user is deleted, all their listings are deleted too

        modelBuilder.Entity<AuctionListing>()
            .HasOne(a => a.HighestBidder)
            .WithMany(u => u.HighestBids)
            .HasForeignKey(a => a.HighestBidderId)
            .OnDelete(DeleteBehavior.SetNull); // when highest bidder deletes account, set the highest bidder to null
        
        modelBuilder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.ReplacedByToken)
            .WithOne(rt => rt.ReplacedThisToken)
            .HasForeignKey<RefreshToken>(rt => rt.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);
        
        base.OnModelCreating(modelBuilder);
    }
    
}