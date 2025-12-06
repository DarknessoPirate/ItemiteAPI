using Application.Features.Auth.EmailConfirmation;
using Application.Features.Auth.Login;
using Application.Features.Auth.Register;
using Application.Features.Auth.ResetPassword;
using Application.Features.Categories.CreateCategory;
using Application.Features.Categories.DeleteCategory;
using Application.Features.Categories.GetCategoryTree;
using Application.Features.Categories.UpdateCategory;
using Application.Features.Listings.AuctionListings.CreateAuctionListing;
using Application.Features.Listings.AuctionListings.GetBidHistory;
using Application.Features.Listings.AuctionListings.PlaceBid;
using Application.Features.Listings.AuctionListings.UpdateAuctionListing;
using Application.Features.Listings.ProductListings.CreateProductListing;
using Application.Features.Listings.ProductListings.UpdateProductListing;
using Application.Features.Listings.Shared.GetPaginatedFollowedListings;
using Application.Features.Listings.Shared.GetPaginatedListings;
using Application.Features.Messages.DeleteMessage;
using Application.Features.Messages.GetChatPage;
using Application.Features.Messages.GetListingChats;
using Application.Features.Messages.GetUserChats;
using Application.Features.Messages.SendMessage;
using Application.Features.Messages.UpdateMessage;
using Application.Features.Payments.GetPaymentsByStatus;
using Application.Features.Payments.PurchaseProduct;
using Application.Features.Notifications.GetPaginatedUserNotifications;
using Application.Features.Payments.ConfirmDelivery;
using Application.Features.Payments.DisputePurchase;
using Application.Features.Payments.GetLatestPayments;
using Application.Features.Payments.GetUserPurchases;
using Application.Features.Payments.GetUserSales;
using Application.Features.Payments.ResolveDispute;
using Application.Features.Users.ChangeEmail;
using Application.Features.Users.ChangeLocation;
using Application.Features.Users.ChangePassword;
using Application.Features.Users.ChangePhoneNumber;
using Application.Features.Users.ChangeUsername;
using Application.Features.Users.ConfirmEmailChange;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ValidatorExtensions
{
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryValidator>();
        services.AddScoped<IValidator<RegisterCommand>, RegisterValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginValidator>();
        services.AddScoped<IValidator<EmailConfirmationCommand>, EmailConfirmationValidator>();
        services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordValidator>();
        services.AddScoped<IValidator<DeleteCategoryCommand>, DeleteCategoryValidator>();
        services.AddScoped<IValidator<GetCategoryTreeCommand>, GetCategoryTreeValidator>();
        services.AddScoped<IValidator<UpdateCategoryCommand>, UpdateCategoryValidator>();
        services.AddScoped<IValidator<CreateProductListingCommand>, CreateProductListingValidator>();
        services.AddScoped<IValidator<UpdateProductListingCommand>, UpdateProductListingValidator>();
        services.AddScoped<IValidator<ChangeEmailCommand>, ChangeEmailValidator>();
        services.AddScoped<IValidator<ConfirmEmailChangeCommand>, ConfirmEmailChangeValidator>();
        services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordValidator>();
        services.AddScoped<IValidator<SendMessageCommand>, SendMessageValidator>();
        services.AddScoped<IValidator<UpdateMessageCommand>, UpdateMessageValidator>();
        services.AddScoped<IValidator<DeleteMessageCommand>, DeleteMessageValidator>();
        services.AddScoped<IValidator<CreateAuctionListingCommand>, CreateAuctionListingValidator>();
        services.AddScoped<IValidator<GetPaginatedListingsQuery>, GetPaginatedListingsValidator>();
        services.AddScoped<IValidator<GetListingChatsQuery>, GetListingChatsValidator>();
        services.AddScoped<IValidator<GetChatPageQuery>, GetChatPageValidator>();
        services.AddScoped<IValidator<PlaceBidCommand>, PlaceBidValidator>();
        services.AddScoped<IValidator<GetBidHistoryQuery>, GetBidHistoryValidator>();
        services.AddScoped<IValidator<ChangePhoneNumberCommand>, ChangePhoneNumberValidator>();
        services.AddScoped<IValidator<ChangeLocationCommand>, ChangeLocationValidator>();
        services.AddScoped<IValidator<ChangeUsernameCommand>, ChangeUsernameValidator>();
        services.AddScoped<IValidator<GetPaginatedFollowedListingsQuery>, GetPaginatedFollowListingsValidator>();
        services.AddScoped<IValidator<GetPaginatedUserNotificationsQuery>, GetPaginatedUserNotificationsValidator>();
        services.AddScoped<IValidator<GetUserChatsQuery>, GetUserChatsValidator>();
        services.AddScoped<IValidator<UpdateAuctionListingCommand>, UpdateAuctionListingValidator>();
        services.AddScoped<IValidator<PurchaseProductCommand>, PurchaseProductValidator>();
        services.AddScoped<IValidator<GetLatestPaymentsQuery>, GetLatestPaymentsValidator>();
        services.AddScoped<IValidator<GetPaymentsByStatusQuery>, GetPaymentsByStatusValidator>();
        services.AddScoped<IValidator<DisputePurchaseCommand>, DisputePurchaseValidator>();
        services.AddScoped<IValidator<ConfirmDeliveryCommand>, ConfirmDeliveryValidator>();
        services.AddScoped<IValidator<ResolveDisputeCommand>, ResolveDisputeValidator>();
        services.AddScoped<IValidator<GetUserSalesQuery>, GetUserSalesValidator>();
        services.AddScoped<IValidator<GetUserPurchasesQuery>, GetUserPurchasesValidator>();
    }
}