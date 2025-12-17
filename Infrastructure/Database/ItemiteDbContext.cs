using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Dispute = Domain.Entities.Dispute;
using DisputeEvidence = Domain.Entities.DisputeEvidence;

namespace Infrastructure.Database;

public class ItemiteDbContext(DbContextOptions<ItemiteDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<ListingBase> Listings { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<ListingPhoto> ListingPhotos { get; set; }
    public DbSet<MessagePhoto> MessagePhotos { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<AuctionBid> AuctionBids { get; set; }
    public DbSet<ListingView> ListingViews { get; set; }
    public DbSet<FollowedListing> FollowedListings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<DisputeEvidence> DisputeEvidences { get; set; }
    
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationUser> NotificationUsers { get; set; }
    
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportPhoto> ReportPhotos { get; set; }
    
    public DbSet<UserListingPrice> UserListingPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.ProfilePhoto)
            .WithOne() // no navigation back to user (access only from user)
            .HasForeignKey<User>(u => u.ProfilePhotoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.BackgroundPhoto)
            .WithOne() // no nav back to user (access only from user side)
            .HasForeignKey<User>(u => u.BackgroundPhotoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .Property(u => u.AuthProvider)
            .HasConversion<string>();

        modelBuilder.Entity<ListingPhoto>()
            .HasOne(lp => lp.Listing)
            .WithMany(l => l.ListingPhotos)
            .HasForeignKey(lp => lp.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingPhoto>()
            .HasOne(lp => lp.Photo)
            .WithMany()
            .HasForeignKey(lp => lp.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MessagePhoto>()
            .HasOne(mp => mp.Message)
            .WithMany(m => m.MessagePhotos)
            .HasForeignKey(mp => mp.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MessagePhoto>()
            .HasOne(mp => mp.Photo)
            .WithMany()
            .HasForeignKey(mp => mp.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(s => s.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Recipient)
            .WithMany(r => r.ReceivedMessages)
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Listing)
            .WithMany(l => l.ListingMessages)
            .HasForeignKey(m => m.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

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

        modelBuilder.Entity<ListingView>()
            .HasOne(lv => lv.Listing)
            .WithMany(l => l.ListingViews)
            .HasForeignKey(lv => lv.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingView>()
            .HasOne(lv => lv.User)
            .WithMany(u => u.ViewedListings)
            .HasForeignKey(lv => lv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingView>()
            .HasOne(lv => lv.Category)
            .WithMany()
            .HasForeignKey(lv => lv.RootCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingBase>()
            .HasOne(l => l.Owner)
            .WithMany(u => u.OwnedListings)
            .HasForeignKey(l => l.OwnerId)
            .OnDelete(DeleteBehavior.Cascade); // when user is deleted, all their listings are deleted too

        modelBuilder.Entity<ProductListing>()
            .HasOne(p => p.Payment)
            .WithOne()
            .HasForeignKey<ProductListing>(p => p.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuctionBid>()
            .HasOne(b => b.Auction)
            .WithMany(a => a.Bids)
            .HasForeignKey(a => a.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuctionBid>()
            .HasOne(b => b.Bidder)
            .WithMany(u => u.Bids)
            .HasForeignKey(a => a.BidderId)
            .OnDelete(DeleteBehavior.Cascade);

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
        
        modelBuilder.Entity<Notification>()
            .Property(n => n.ResourceType)
            .HasConversion<string>();
        
        modelBuilder.Entity<Report>()
            .Property(r => r.ResourceType)
            .HasConversion<string>();
        
        modelBuilder.Entity<ReportPhoto>()
            .HasOne(rp => rp.Report)
            .WithMany(r => r.ReportPhotos)
            .HasForeignKey(rp => rp.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ReportPhoto>()
            .HasOne(rp => rp.Photo)
            .WithMany()
            .HasForeignKey(rp => rp.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reports)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // payments 
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Listing)
            .WithMany()
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Buyer)
            .WithMany()
            .HasForeignKey(p => p.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Seller)
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.TransferTrigger)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.ActualTransferMethod)
            .HasConversion<string>();

        modelBuilder.Entity<Dispute>()
            .HasOne(d => d.Payment)
            .WithOne(p => p.Dispute)
            .HasForeignKey<Dispute>(d => d.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Dispute>()
            .HasOne(d => d.InitiatedBy)
            .WithMany()
            .HasForeignKey(d => d.InitiatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Dispute>()
            .HasOne(d => d.ResolvedBy)
            .WithMany()
            .HasForeignKey(d => d.ResolvedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Dispute>()
            .Property(d => d.Reason)
            .HasConversion<string>();

        modelBuilder.Entity<Dispute>()
            .Property(d => d.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Dispute>()
            .Property(d => d.Resolution)
            .HasConversion<string>();

        modelBuilder.Entity<DisputeEvidence>()
            .HasOne(de => de.Dispute)
            .WithMany(d => d.Evidence)
            .HasForeignKey(de => de.DisputeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DisputeEvidence>()
            .HasOne(de => de.Photo)
            .WithMany()
            .HasForeignKey(de => de.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);


        base.OnModelCreating(modelBuilder);
    }
}