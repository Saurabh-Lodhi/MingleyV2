using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mingley.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverPhotoToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Emoji = table.Column<string>(type: "text", nullable: true),
                    CoinCost = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsAnimated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Emoji = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Features = table.Column<string>(type: "text", nullable: true),
                    IsPopular = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SuperLikesPerDay = table.Column<int>(type: "integer", nullable: false),
                    BoostsPerMonth = table.Column<int>(type: "integer", nullable: false),
                    UnlimitedLikes = table.Column<bool>(type: "boolean", nullable: false),
                    CanSeeWhoLiked = table.Column<bool>(type: "boolean", nullable: false),
                    VideoCallEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    Profession = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    CoverPhoto = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false),
                    CoinBalance = table.Column<int>(type: "integer", nullable: false),
                    TotalEarned = table.Column<double>(type: "double precision", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorSecret = table.Column<string>(type: "text", nullable: true),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    OtpCode = table.Column<string>(type: "text", nullable: true),
                    OtpExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OtpPurpose = table.Column<string>(type: "text", nullable: true),
                    FcmToken = table.Column<string>(type: "text", nullable: true),
                    ProfileComplete = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocationLocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsTravelMode = table.Column<bool>(type: "boolean", nullable: false),
                    TravelCity = table.Column<string>(type: "text", nullable: true),
                    TravelLat = table.Column<double>(type: "double precision", nullable: true),
                    TravelLng = table.Column<double>(type: "double precision", nullable: true),
                    IsCreatedByAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuspended = table.Column<bool>(type: "boolean", nullable: false),
                    SuspendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspendReason = table.Column<string>(type: "text", nullable: true),
                    SuspendedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Users_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Blocks_Users_BlockerId",
                        column: x => x.BlockerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoinTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Coins = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TransactionType = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtrId = table.Column<string>(type: "text", nullable: true),
                    ScreenshotUrl = table.Column<string>(type: "text", nullable: true),
                    RequestedCoins = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AdminNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepositRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Users_User1Id",
                        column: x => x.User1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Users_User2Id",
                        column: x => x.User2Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReferenceId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivacyAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacyAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivacyAgreements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AdminNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Swipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SwiperId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Swipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Swipes_Users_SwiperId",
                        column: x => x.SwiperId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Swipes_Users_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserImages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserInterests",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => new { x.UserId, x.InterestId });
                    table.ForeignKey(
                        name: "FK_UserInterests_Interests_InterestId",
                        column: x => x.InterestId,
                        principalTable: "Interests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInterests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: true),
                    Lng = table.Column<double>(type: "double precision", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLocations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterestedIn = table.Column<string>(type: "text", nullable: false),
                    MinAge = table.Column<int>(type: "integer", nullable: false),
                    MaxAge = table.Column<int>(type: "integer", nullable: false),
                    MaxDistance = table.Column<int>(type: "integer", nullable: false),
                    RelationshipType = table.Column<string>(type: "text", nullable: false),
                    NearbyOnly = table.Column<bool>(type: "boolean", nullable: false),
                    OnlineOnly = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedOnly = table.Column<bool>(type: "boolean", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                    CancelReason = table.Column<string>(type: "text", nullable: true),
                    GrantedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Coins = table.Column<int>(type: "integer", nullable: false),
                    BankOrUpi = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AdminNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CallSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CallType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    CoinsDeducted = table.Column<int>(type: "integer", nullable: false),
                    EndReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallSessions_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallSessions_Users_CallerId",
                        column: x => x.CallerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallSessions_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chats_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SuperChats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CoinAmount = table.Column<int>(type: "integer", nullable: false),
                    GirlCommission = table.Column<double>(type: "double precision", nullable: false),
                    CompanyRevenue = table.Column<double>(type: "double precision", nullable: false),
                    IsResponded = table.Column<bool>(type: "boolean", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MatchCreatedId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuperChats_Matches_MatchCreatedId",
                        column: x => x.MatchCreatedId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SuperChats_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuperChats_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    GiftName = table.Column<string>(type: "text", nullable: true),
                    GiftCost = table.Column<int>(type: "integer", nullable: true),
                    CoinAmount = table.Column<int>(type: "integer", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CoinsDeducted = table.Column<int>(type: "integer", nullable: false),
                    ReplyToMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Gifts",
                columns: new[] { "Id", "Category", "CoinCost", "CreatedAt", "DeletedAt", "Emoji", "Icon", "ImageUrl", "IsActive", "IsAnimated", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("c0000001-0000-0000-0000-000000000001"), "standard", 10, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3493), null, "❤️", "heart-outline", null, true, false, false, "Heart", null },
                    { new Guid("c0000001-0000-0000-0000-000000000002"), "standard", 20, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3501), null, "🌹", "rose-outline", null, true, false, false, "Rose", null },
                    { new Guid("c0000001-0000-0000-0000-000000000003"), "standard", 50, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3514), null, "🎁", "gift-outline", null, true, false, false, "Gift Box", null },
                    { new Guid("c0000001-0000-0000-0000-000000000004"), "standard", 100, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3521), null, "☕", "cafe-outline", null, true, false, false, "Coffee Date", null },
                    { new Guid("c0000001-0000-0000-0000-000000000005"), "standard", 500, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3529), null, "💍", "diamond-outline", null, true, false, false, "Diamond Ring", null },
                    { new Guid("c0000002-0000-0000-0000-000000000001"), "romantic", 50, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3532), null, "💐", "flower-outline", null, true, false, false, "Bouquet", null },
                    { new Guid("c0000002-0000-0000-0000-000000000002"), "romantic", 75, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3552), null, "🍫", "heart-outline", null, true, false, false, "Chocolate Box", null },
                    { new Guid("c0000002-0000-0000-0000-000000000003"), "romantic", 30, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3555), null, "💌", "mail-outline", null, true, false, false, "Love Letter", null },
                    { new Guid("c0000002-0000-0000-0000-000000000004"), "romantic", 150, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3558), null, "🕯️", "flame-outline", null, true, false, false, "Candlelight", null },
                    { new Guid("c0000002-0000-0000-0000-000000000005"), "romantic", 200, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3561), null, "🧸", "gift-outline", null, true, false, false, "Teddy Bear", null },
                    { new Guid("c0000003-0000-0000-0000-000000000001"), "fun", 30, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3565), null, "🎂", "cake-outline", null, true, false, false, "Cake", null },
                    { new Guid("c0000003-0000-0000-0000-000000000002"), "fun", 40, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3568), null, "🎉", "sparkles-outline", null, true, false, false, "Party Popper", null },
                    { new Guid("c0000003-0000-0000-0000-000000000003"), "fun", 80, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3571), null, "🏆", "trophy-outline", null, true, false, false, "Trophy", null },
                    { new Guid("c0000003-0000-0000-0000-000000000004"), "fun", 25, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3574), null, "🎈", "balloon-outline", null, true, false, false, "Balloon", null },
                    { new Guid("c0000003-0000-0000-0000-000000000005"), "fun", 35, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3580), null, "🎊", "sparkles-outline", null, true, true, false, "Confetti", null },
                    { new Guid("c0000004-0000-0000-0000-000000000001"), "animated", 150, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3637), null, "🎆", "sparkles-outline", null, true, true, false, "Fireworks", null },
                    { new Guid("c0000004-0000-0000-0000-000000000002"), "animated", 200, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3644), null, "🌠", "star-outline", null, true, true, false, "Shooting Star", null },
                    { new Guid("c0000004-0000-0000-0000-000000000003"), "animated", 300, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3647), null, "🌈", "color-fill-outline", null, true, true, false, "Rainbow", null },
                    { new Guid("c0000004-0000-0000-0000-000000000004"), "animated", 250, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3650), null, "🪄", "sparkles-outline", null, true, true, false, "Magic Wand", null },
                    { new Guid("c0000005-0000-0000-0000-000000000001"), "luxury", 500, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3654), null, "👑", "diamond-outline", null, true, false, false, "Crown", null },
                    { new Guid("c0000005-0000-0000-0000-000000000002"), "luxury", 800, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3660), null, "🚗", "car-outline", null, true, false, false, "Sports Car", null },
                    { new Guid("c0000005-0000-0000-0000-000000000003"), "luxury", 1500, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3663), null, "✈️", "airplane-outline", null, true, false, false, "Private Jet", null },
                    { new Guid("c0000005-0000-0000-0000-000000000004"), "luxury", 2000, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3669), null, "⛵", "boat-outline", null, true, false, false, "Yacht", null },
                    { new Guid("c0000006-0000-0000-0000-000000000001"), "vip", 1000, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3672), null, "🌹", "rose-outline", null, true, true, false, "Golden Rose", null },
                    { new Guid("c0000006-0000-0000-0000-000000000002"), "vip", 3000, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3675), null, "💎", "diamond-outline", null, true, true, false, "Diamond Heart", null },
                    { new Guid("c0000006-0000-0000-0000-000000000003"), "vip", 5000, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3678), null, "🎰", "trophy-outline", null, true, true, false, "King Package", null },
                    { new Guid("c0000006-0000-0000-0000-000000000004"), "vip", 8000, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3681), null, "🏰", "star-outline", null, true, true, false, "Luxury Suite", null },
                    { new Guid("c0000006-0000-0000-0000-000000000005"), "vip", 10000, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3683), null, "🌌", "telescope-outline", null, true, true, false, "Universe", null }
                });

            migrationBuilder.InsertData(
                table: "Interests",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Emoji", "Icon", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3026), null, "🎵", "musical-notes-outline", false, "Music", null },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3055), null, "✈️", "airplane-outline", false, "Travel", null },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3059), null, "💪", "barbell-outline", false, "Gym", null },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3064), null, "🎬", "film-outline", false, "Movies", null },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3067), null, "📚", "book-outline", false, "Reading", null },
                    { new Guid("a0000001-0000-0000-0000-000000000006"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3069), null, "🍳", "restaurant-outline", false, "Cooking", null },
                    { new Guid("a0000001-0000-0000-0000-000000000007"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3091), null, "🎨", "color-palette-outline", false, "Art", null },
                    { new Guid("a0000001-0000-0000-0000-000000000008"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3095), null, "💃", "body-outline", false, "Dancing", null },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3105), null, "📸", "camera-outline", false, "Photography", null },
                    { new Guid("a0000001-0000-0000-0000-000000000010"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3108), null, "🧘", "body-outline", false, "Yoga", null },
                    { new Guid("a0000001-0000-0000-0000-000000000011"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3112), null, "🏏", "baseball-outline", false, "Cricket", null },
                    { new Guid("a0000001-0000-0000-0000-000000000012"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3115), null, "🎮", "game-controller-outline", false, "Gaming", null },
                    { new Guid("a0000001-0000-0000-0000-000000000013"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3118), null, "🛍️", "bag-handle-outline", false, "Shopping", null },
                    { new Guid("a0000001-0000-0000-0000-000000000014"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3120), null, "🍕", "pizza-outline", false, "Foodie", null },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3127), null, "🥾", "walk-outline", false, "Hiking", null },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3130), null, "💻", "code-slash-outline", false, "Coding", null },
                    { new Guid("a0000001-0000-0000-0000-000000000017"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3136), null, "🐾", "paw-outline", false, "Pets", null },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3138), null, "☕", "cafe-outline", false, "Coffee", null },
                    { new Guid("a0000001-0000-0000-0000-000000000019"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3141), null, "🧠", "leaf-outline", false, "Meditation", null },
                    { new Guid("a0000001-0000-0000-0000-000000000020"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3144), null, "⚽", "football-outline", false, "Football", null }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "BoostsPerMonth", "CanSeeWhoLiked", "CreatedAt", "DeletedAt", "DurationDays", "Features", "IsActive", "IsDeleted", "IsPopular", "Name", "Price", "SuperLikesPerDay", "UnlimitedLikes", "UpdatedAt", "VideoCallEnabled" },
                values: new object[,]
                {
                    { new Guid("b0000001-0000-0000-0000-000000000001"), 0, true, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3419), null, 30, "[\"Unlimited likes\",\"No ads\",\"5 Super Likes/day\",\"See who liked you\"]", true, false, false, "Silver", 299m, 5, true, null, false },
                    { new Guid("b0000001-0000-0000-0000-000000000002"), 2, true, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3434), null, 30, "[\"All Silver\",\"Video calls\",\"10 Super Likes/day\",\"2 Profile boosts\",\"5 coins/msg\"]", true, false, true, "Gold", 599m, 10, true, null, true },
                    { new Guid("b0000001-0000-0000-0000-000000000003"), 5, true, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3443), null, 30, "[\"All Gold\",\"Top picks daily\",\"Unlimited Super Likes\",\"5 boosts/month\",\"Priority support\"]", true, false, false, "Platinum", 999m, -1, true, null, true },
                    { new Guid("b0000001-0000-0000-0000-000000000004"), 15, true, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3447), null, 90, "[\"All Platinum\",\"VIP badge\",\"Global search\",\"Dedicated support\",\"Early features\"]", true, false, false, "VIP", 1999m, -1, true, null, true }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "Bio", "CoinBalance", "CoverPhoto", "CreatedAt", "DateOfBirth", "DeletedAt", "Email", "FcmToken", "FullName", "Gender", "IsActive", "IsCreatedByAdmin", "IsDeleted", "IsLocationLocked", "IsOnline", "IsPremium", "IsSuspended", "IsTravelMode", "IsVerified", "LastActiveAt", "OtpCode", "OtpExpiry", "OtpPurpose", "PasswordHash", "Phone", "Profession", "ProfileComplete", "Role", "SuspendReason", "SuspendedAt", "SuspendedBy", "TotalEarned", "TravelCity", "TravelLat", "TravelLng", "TwoFactorEnabled", "TwoFactorSecret", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("d0000001-0000-0000-0000-000000000001"), "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=1200&q=95&fit=crop&crop=faces&auto=format", "Platform administrator 🔧", 99999, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3791), new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@mingley.app", null, "Super Admin", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "admin", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000002-0000-0000-0000-000000000002"), "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?w=1200&q=95&fit=crop&crop=faces&auto=format", "Kathak dancer & yoga instructor 🌺 | Delhi girl | Love chai mornings ✨", 2500, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3825), new DateTime(1998, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "priya@demo.com", null, "Priya Sharma", "female", true, false, false, true, true, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000003-0000-0000-0000-000000000003"), "https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?w=1200&q=95&fit=crop&crop=faces&auto=format", "Playback singer 🎵 | Travel addict ✈️ | Mumbai | Chai > coffee ☕", 800, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3833), new DateTime(1999, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, "neha@demo.com", null, "Neha Kapoor", "female", true, false, false, true, true, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000004-0000-0000-0000-000000000004"), "https://images.unsplash.com/photo-1488426862026-3ee34a7d66df?w=1200&q=95&fit=crop&crop=faces&auto=format", "Foodie & travel photographer 📸🍕 | Pune | Obsessed with sunsets 🌅", 1200, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3838), new DateTime(2000, 11, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "ankita@demo.com", null, "Ankita Singh", "female", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000005-0000-0000-0000-000000000005"), "https://images.unsplash.com/photo-1529626455594-4ff0802cfb7e?w=1200&q=95&fit=crop&crop=faces&auto=format", "Fashion designer 👗 | Sketch artist 🎨 | Hyderabad | Building my empire 💅", 1800, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3850), new DateTime(1999, 2, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "aisha@demo.com", null, "Aisha Khan", "female", true, false, false, true, true, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000006-0000-0000-0000-000000000006"), "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=1200&q=95&fit=crop&crop=faces&auto=format", "Doctor by day, dancer by night 💃 | Ahmedabad | Books + Beaches 📖🏖️", 3500, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3854), new DateTime(1997, 5, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, "shreya@demo.com", null, "Shreya Patel", "female", true, false, false, true, true, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Doctor", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000007-0000-0000-0000-000000000007"), "https://images.unsplash.com/photo-1542206395-9feb3edaa68d?w=1200&q=95&fit=crop&crop=faces&auto=format", "Engineering student 📚 | Sketch artist | Hyderabad | 21 & figuring it out 😄", 700, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3858), new DateTime(2001, 3, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "meena@demo.com", null, "Meena Reddy", "female", true, false, false, true, false, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000008-0000-0000-0000-000000000008"), "https://images.unsplash.com/photo-1489424731084-a5d8b219a5bb?w=1200&q=95&fit=crop&crop=faces&auto=format", "Marketing lead 📈 | Bookworm 📖 | Jaipur | Pink city girl 🌸", 950, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3863), new DateTime(1998, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "pooja@demo.com", null, "Pooja Gupta", "female", true, false, false, true, true, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000009-0000-0000-0000-000000000009"), "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=1200&q=95&fit=crop&crop=faces&auto=format", "Senior journalist ✍️ | World traveller ✈️ | Kolkata | City of joy forever 🎭", 1100, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3868), new DateTime(1996, 7, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "kritika@demo.com", null, "Kritika Bose", "female", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000010-0000-0000-0000-000000000010"), "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=1200&q=95&fit=crop&crop=faces&auto=format", "Finance head 💼 | Yoga guru 🧘 | Surat | Manifesting greatness ✨", 4200, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3872), new DateTime(1993, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "rita@demo.com", null, "Rita Desai", "female", true, false, false, true, false, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000011-0000-0000-0000-000000000011"), "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=1200&q=95&fit=crop&crop=faces&auto=format", "Corporate lawyer ⚖️ | Kathak dancer 💃 | Amritsar | Golden temple sunrise hits different 🌅", 2800, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3883), new DateTime(1997, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, "simran@demo.com", null, "Simran Kaur", "female", true, false, false, true, true, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000012-0000-0000-0000-000000000012"), "https://images.unsplash.com/photo-1502685104226-ee32379fefbe?w=1200&q=95&fit=crop&crop=faces&auto=format", "Architecture student 🏛️ | Coffee addict ☕ | Kochi | Designing my future 📐", 600, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3928), new DateTime(2000, 6, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, "divya@demo.com", null, "Divya Menon", "female", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000013-0000-0000-0000-000000000013"), "https://images.unsplash.com/photo-1515077678510-ce3bdf418862?w=1200&q=95&fit=crop&crop=faces&auto=format", "Pre-med | Poet 🖋️ | Trivandrum | Words are my superpower 🌙", 400, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3933), new DateTime(2002, 4, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "kavya@demo.com", null, "Kavya Nair", "female", true, false, false, true, false, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000014-0000-0000-0000-000000000014"), "https://images.unsplash.com/photo-1520813792240-56fc4a3765a7?w=1200&q=95&fit=crop&crop=faces&auto=format", "Senior UI/UX Designer 🎨 | Plant mom 🌿 | Pune | Making things beautiful ✨", 1350, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3938), new DateTime(1995, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "tanvi@demo.com", null, "Tanvi Joshi", "female", true, false, false, true, true, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000015-0000-0000-0000-000000000015"), "https://images.unsplash.com/photo-1509967419530-da38b4704bc6?w=1200&q=95&fit=crop&crop=faces&auto=format", "Tech startup founder 🚀 | TEDx speaker | Delhi | Hustle + heart ❤️‍🔥", 5100, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3942), new DateTime(1994, 12, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, "ishita@demo.com", null, "Ishita Sharma", "female", true, false, false, true, false, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Business Owner", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000016-0000-0000-0000-000000000016"), "https://images.unsplash.com/photo-1531123897727-240d604e3dc3?w=1200&q=95&fit=crop&crop=faces&auto=format", "Classical singer 🎶 | Bookworm 📚 | Varanasi | Old soul in a modern world 🕌", 880, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3947), new DateTime(1999, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, "riya@demo.com", null, "Riya Singh", "female", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000017-0000-0000-0000-000000000017"), "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=1200&q=95&fit=crop&crop=faces&auto=format", "Model & content creator 📸 | Mumbai | Living my best life 🌟", 1500, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3952), new DateTime(1998, 5, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, "zara@demo.com", null, "Zara Ahmed", "female", true, false, false, true, true, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000018-0000-0000-0000-000000000018"), "https://images.unsplash.com/photo-1544717305-2782549b5136?w=1200&q=95&fit=crop&crop=faces&auto=format", "Cardiologist ❤️‍🩺 | Runner 🏃 | Delhi | Saving hearts in and out of hospital 😄", 2200, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3962), new DateTime(1996, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, "naina@demo.com", null, "Naina Verma", "female", true, false, false, true, false, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Doctor", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000019-0000-0000-0000-000000000019"), "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=1200&q=95&fit=crop&crop=faces&auto=format", "CS undergrad 💻 | Hackathon champ | Bengaluru | Ctrl+Z my way through life 😂", 650, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3970), new DateTime(2001, 2, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, "preethi@demo.com", null, "Preethi Rao", "female", true, false, false, true, true, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000020-0000-0000-0000-000000000020"), "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=1200&q=95&fit=crop&crop=faces&auto=format", "Wildlife photographer 🦁 | Bengali foodie 🍛 | Kolkata | Mountains & monsoons 🌧️", 1900, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3975), new DateTime(1997, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "ananya@demo.com", null, "Ananya Chatterjee", "female", true, false, false, true, true, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000021-0000-0000-0000-000000000021"), "https://images.unsplash.com/photo-1546961342-ea5f62d4d0b0?w=1200&q=95&fit=crop&crop=faces&auto=format", "Investment banker 💰 | Marathoner 🏃‍♀️ | Mumbai | Finance & trails ⛰️", 3100, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3979), new DateTime(1995, 3, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, "sonal@demo.com", null, "Sonal Mehta", "female", true, false, false, true, false, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000022-0000-0000-0000-000000000022"), "https://images.unsplash.com/photo-1601412436967-a70659db97b5?w=1200&q=95&fit=crop&crop=faces&auto=format", "Marine biologist 🐠 | Beach bum 🏖️ | Goa | Ocean is my therapy 🌊", 750, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3984), new DateTime(2000, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "alisha@demo.com", null, "Alisha D'Souza", "female", true, false, false, true, true, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000023-0000-0000-0000-000000000023"), "https://images.unsplash.com/photo-1586297135537-94bc81ba6254?w=1200&q=95&fit=crop&crop=faces&auto=format", "Classical musician 🎻 | Urdu poetry lover | Lucknow | Tehzeeb & charm 💫", 1100, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3989), new DateTime(1998, 12, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "ayesha@demo.com", null, "Ayesha Mirza", "female", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000024-0000-0000-0000-000000000024"), "https://images.unsplash.com/photo-1614632537197-38a17061c2bd?w=1200&q=95&fit=crop&crop=faces&auto=format", "Neurosurgeon 🧠 | Bharatanatyam dancer 💃 | Chennai | Brains AND moves 😉", 4800, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(3994), new DateTime(1993, 6, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, "tara@demo.com", null, "Tara Pillai", "female", true, false, false, true, true, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Doctor", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000025-0000-0000-0000-000000000025"), "https://images.unsplash.com/photo-1619946794135-5bc917a27793?w=1200&q=95&fit=crop&crop=faces&auto=format", "Product Manager @BigTech 📊 | Traveller 🗺️ | Bengaluru | Building products people love ❤️", 2000, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4001), new DateTime(1996, 4, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, "nalini@demo.com", null, "Nalini Krishnan", "female", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000026-0000-0000-0000-000000000026"), "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=1200&q=95&fit=crop&crop=faces&auto=format", "Fitness freak 💪 | Landscape photographer 📸 | Gurgaon | Mountains > malls 🏔️", 10000, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4006), new DateTime(1993, 11, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "arjun@demo.com", null, "Arjun Singh", "male", true, false, false, true, false, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000027-0000-0000-0000-000000000027"), "https://images.unsplash.com/photo-1552058544-f2b08422138a?w=1200&q=95&fit=crop&crop=faces&auto=format", "Music lover 🎸 | Solo traveller ✈️ | Software Engineer | Noida | Guitar + code = life", 5000, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4013), new DateTime(1995, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "rahul@demo.com", null, "Rahul Mehta", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000028-0000-0000-0000-000000000028"), "https://images.unsplash.com/photo-1568602471122-7832951cc4c5?w=1200&q=95&fit=crop&crop=faces&auto=format", "Serial entrepreneur ⚡ | Coffee addict ☕ | Delhi | Building the next big thing 🚀", 3000, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4017), new DateTime(1996, 4, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, "vikram@demo.com", null, "Vikram Nair", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Business Owner", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000029-0000-0000-0000-000000000029"), "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=1200&q=95&fit=crop&crop=faces&auto=format", "Gym freak 🏋️ | Cricket fanatic 🏏 | Noida | IPL > everything 😂", 500, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4115), new DateTime(1997, 9, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, "deepak@demo.com", null, "Deepak Verma", "male", true, false, false, true, false, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000030-0000-0000-0000-000000000030"), "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=1200&q=95&fit=crop&crop=faces&auto=format", "Head chef 👨‍🍳 | Food blogger | Bengaluru | Will cook for you if you laugh at my puns 😄", 2000, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4120), new DateTime(1994, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "rohit@demo.com", null, "Rohit Sharma", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000031-0000-0000-0000-000000000031"), "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=1200&q=95&fit=crop&crop=faces&auto=format", "IIT Madras grad 🎓 | Startup founder 🚀 | Chennai | 0→1 builder ⚙️", 4500, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4137), new DateTime(1992, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, "karthik@demo.com", null, "Karthik Menon", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Business Owner", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000032-0000-0000-0000-000000000032"), "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=1200&q=95&fit=crop&crop=faces&auto=format", "Principal engineer 💻 | Gaming legend 🎮 | Kolkata | 10 yrs of bugs still counting 😅", 1500, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4141), new DateTime(1990, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, "rajesh@demo.com", null, "Rajesh Kumar", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000033-0000-0000-0000-000000000033"), "https://images.unsplash.com/photo-1548449112-96a38a643324?w=1200&q=95&fit=crop&crop=faces&auto=format", "Principal architect 🏛️ | Art collector 🎨 | Chandigarh | Designing spaces, chasing light 🌤️", 6000, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4146), new DateTime(1994, 2, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, "aman@demo.com", null, "Aman Joshi", "male", true, false, false, true, false, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000034-0000-0000-0000-000000000034"), "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?w=1200&q=95&fit=crop&crop=faces&auto=format", "Data scientist 📊 | Bike tourer 🏍️ | Pune | Numbers by day, highways by night 🌙", 3200, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4150), new DateTime(1996, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, "aditya@demo.com", null, "Aditya Kumar", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000035-0000-0000-0000-000000000035"), "https://images.unsplash.com/photo-1488161628813-04466f872be2?w=1200&q=95&fit=crop&crop=faces&auto=format", "Investment banker 💰 | World explorer 🗺️ | Mumbai | 42 countries and counting 🌍", 7500, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4158), new DateTime(1993, 9, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, "nikhil@demo.com", null, "Nikhil Sharma", "male", true, false, false, true, true, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000036-0000-0000-0000-000000000036"), "https://images.unsplash.com/photo-1504257432389-52343af06ae3?w=1200&q=95&fit=crop&crop=faces&auto=format", "Commerce undergrad 📊 | Meme lord 😂 | Coimbatore | Vibing on good music 🎧", 180, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4162), new DateTime(2000, 2, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "suresh@demo.com", null, "Suresh Iyer", "male", true, false, false, true, false, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000037-0000-0000-0000-000000000037"), "https://images.unsplash.com/photo-1531891437562-4301cf35b7e4?w=1200&q=95&fit=crop&crop=faces&auto=format", "Commercial pilot ✈️ | Astronomy nerd 🔭 | Delhi | Up in the clouds, literally 😄", 2200, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4167), new DateTime(1995, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "aakash@demo.com", null, "Aakash Verma", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000038-0000-0000-0000-000000000038"), "https://images.unsplash.com/photo-1522556189639-b786d812d5ae?w=1200&q=95&fit=crop&crop=faces&auto=format", "Orthopedic surgeon 🦴 | Classical guitarist 🎸 | Jaipur | Healing bodies & minds 🙏", 3800, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4175), new DateTime(1994, 8, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "kabir@demo.com", null, "Kabir Singh", "male", true, false, false, true, true, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Doctor", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000039-0000-0000-0000-000000000039"), "https://images.unsplash.com/photo-1521119989659-a83eee488004?w=1200&q=95&fit=crop&crop=faces&auto=format", "Bollywood choreographer 💃 | Fitness coach 💪 | Mumbai | Dance is my language 🕺", 1200, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4179), new DateTime(1998, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, "aryan@demo.com", null, "Aryan Kapoor", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000040-0000-0000-0000-000000000040"), "https://images.unsplash.com/photo-1534030347209-467a573065b7?w=1200&q=95&fit=crop&crop=faces&auto=format", "Tech co-founder 🚀 | Angel investor | Delhi | Disrupting industries before breakfast ⚡", 8500, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4184), new DateTime(1992, 5, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "dev@demo.com", null, "Dev Malhotra", "male", true, false, false, true, true, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Business Owner", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000041-0000-0000-0000-000000000041"), "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=1200&q=95&fit=crop&crop=faces&auto=format", "CA & tax consultant 📋 | Cricket player 🏏 | Surat | Numbers make sense, people don't 😄", 900, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4188), new DateTime(1997, 12, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, "mihir@demo.com", null, "Mihir Shah", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000042-0000-0000-0000-000000000042"), "https://images.unsplash.com/photo-1530268729831-4b0b9e170600?w=1200&q=95&fit=crop&crop=faces&auto=format", "Documentary filmmaker 🎬 | Street photographer | Kolkata | Storytelling through lens 📸", 2600, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4192), new DateTime(1996, 7, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, "rohan@demo.com", null, "Rohan Bose", "male", true, false, false, true, true, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Freelancer", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000043-0000-0000-0000-000000000043"), "https://images.unsplash.com/photo-1496345875838-ff236a81d931?w=1200&q=95&fit=crop&crop=faces&auto=format", "Law student ⚖️ | Debate champion | Lucknow | Arguing is my cardio 😂", 700, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4199), new DateTime(1999, 3, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "vivek@demo.com", null, "Vivek Pandey", "male", true, false, false, true, false, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000044-0000-0000-0000-000000000044"), "https://images.unsplash.com/photo-1524598191073-ee72ca03e29a?w=1200&q=95&fit=crop&crop=faces&auto=format", "Product manager 📱 | Foodie 🍕 | Indore | Poha > everything 😋", 1800, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4207), new DateTime(1995, 10, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, "priyank@demo.com", null, "Priyank Agarwal", "male", true, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000045-0000-0000-0000-000000000045"), "https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=1200&q=95&fit=crop&crop=faces&auto=format", "Engineering student 🔧 | Gamer 🎮 | Bhopal | BGMI Conqueror 🏆", 450, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4212), new DateTime(1998, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, "ankit@demo.com", null, "Ankit Tiwari", "male", true, false, false, true, false, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000046-0000-0000-0000-000000000046"), "https://images.unsplash.com/photo-1504593811423-6dd665756598?w=1200&q=95&fit=crop&crop=faces&auto=format", "Real estate mogul 🏠 | Gym addict 💪 | Ahmedabad | Building empires one property at a time 🏗️", 5200, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4220), new DateTime(1993, 4, 17, 0, 0, 0, 0, DateTimeKind.Utc), null, "jay@demo.com", null, "Jay Patel", "male", true, false, false, true, false, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Business Owner", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000047-0000-0000-0000-000000000047"), "https://images.unsplash.com/photo-1463453091185-61582044d556?w=1200&q=95&fit=crop&crop=faces&auto=format", "Retired army officer 🎖️ | Mountaineer 🏔️ | Chandigarh | Adventure is my middle name ⛰️", 12000, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4225), new DateTime(1991, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, "shiv@demo.com", null, "Shiv Kumar", "male", true, false, false, true, true, true, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000048-0000-0000-0000-000000000048"), "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?w=1200&q=95&fit=crop&crop=faces&auto=format", "Cricketer ⚡ | Final year student | Lucknow | Future RCB player 😂🏏", 250, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4229), new DateTime(1999, 4, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, "saurabh@demo.com", null, "Saurabh Mishra", "male", true, false, false, true, false, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000049-0000-0000-0000-000000000049"), "https://images.unsplash.com/photo-1519345182560-3f2917c472ef?w=1200&q=95&fit=crop&crop=faces&auto=format", "Retired athlete 🥇 | Fitness coach | Thiruvananthapuram | Chasing a second wind 💨", 100, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4236), new DateTime(1988, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, "mohan@demo.com", null, "Mohan Pillai", "male", false, false, false, true, false, false, false, false, true, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Working Professional", false, "user", null, null, null, 0.0, null, null, null, false, null, null },
                    { new Guid("d0000050-0000-0000-0000-000000000050"), "https://images.unsplash.com/photo-1463453091185-61582044d556?w=1200&q=95&fit=crop&crop=faces&auto=format", "Just testing 🧪", 50, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4244), new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "test_male@demo.com", null, "Test User Male", "male", true, false, false, true, false, false, false, false, false, null, null, null, null, "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq", null, "Student", true, "user", null, null, null, 0.0, null, null, null, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "Blocks",
                columns: new[] { "Id", "BlockedUserId", "BlockerId", "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a5000001-0000-0000-0000-000000000001"), new Guid("d0000029-0000-0000-0000-000000000029"), new Guid("d0000002-0000-0000-0000-000000000002"), new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { new Guid("a5000002-0000-0000-0000-000000000002"), new Guid("d0000036-0000-0000-0000-000000000036"), new Guid("d0000005-0000-0000-0000-000000000005"), new DateTime(2024, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { new Guid("a5000003-0000-0000-0000-000000000003"), new Guid("d0000043-0000-0000-0000-000000000043"), new Guid("d0000008-0000-0000-0000-000000000008"), new DateTime(2024, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null }
                });

            migrationBuilder.InsertData(
                table: "CoinTransactions",
                columns: new[] { "Id", "Coins", "CreatedAt", "DeletedAt", "Description", "Direction", "IsDeleted", "ReferenceId", "TransactionType", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("a3000001-0000-0000-0000-000000000001"), 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Welcome bonus", "credit", false, null, "welcome", null, new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("a3000002-0000-0000-0000-000000000002"), 5000, new DateTime(2024, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, "Deposit — 5000 coins", "credit", false, null, "deposit", null, new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("a3000003-0000-0000-0000-000000000003"), 50, new DateTime(2024, 1, 3, 14, 5, 0, 0, DateTimeKind.Utc), null, "Audio call · 5 min", "debit", false, null, "call", null, new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("a3000004-0000-0000-0000-000000000004"), 10000, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Deposit — 10000 coins", "credit", false, null, "deposit", null, new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("a3000005-0000-0000-0000-000000000005"), 700, new DateTime(2024, 1, 3, 10, 7, 0, 0, DateTimeKind.Utc), null, "Video call · 7 min", "debit", false, null, "call", null, new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("a3000006-0000-0000-0000-000000000006"), 10000, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Deposit — 10000 coins", "credit", false, null, "deposit", null, new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("a3000007-0000-0000-0000-000000000007"), 1500, new DateTime(2024, 1, 5, 19, 15, 0, 0, DateTimeKind.Utc), null, "Video call · 15 min", "debit", false, null, "call", null, new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("a3000008-0000-0000-0000-000000000008"), 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Welcome bonus", "credit", false, null, "welcome", null, new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("a3000009-0000-0000-0000-000000000009"), 10000, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Deposit — 10000 coins", "credit", false, null, "deposit", null, new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("a3000010-0000-0000-0000-000000000010"), 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Welcome bonus", "credit", false, null, "welcome", null, new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("a3000011-0000-0000-0000-000000000011"), 50, new DateTime(2024, 1, 1, 2, 0, 0, 0, DateTimeKind.Utc), null, "Verification bonus", "credit", false, null, "verification", null, new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("a3000012-0000-0000-0000-000000000012"), 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Welcome bonus", "credit", false, null, "welcome", null, new Guid("d0000005-0000-0000-0000-000000000005") },
                    { new Guid("a3000013-0000-0000-0000-000000000013"), 50, new DateTime(2024, 1, 1, 2, 0, 0, 0, DateTimeKind.Utc), null, "Verification bonus", "credit", false, null, "verification", null, new Guid("d0000005-0000-0000-0000-000000000005") },
                    { new Guid("a3000014-0000-0000-0000-000000000014"), 12000, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Deposit — 12000 coins", "credit", false, null, "deposit", null, new Guid("d0000047-0000-0000-0000-000000000047") },
                    { new Guid("a3000015-0000-0000-0000-000000000015"), 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Welcome bonus", "credit", false, null, "welcome", null, new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("a3000016-0000-0000-0000-000000000016"), 5000, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Deposit — 5000 coins", "credit", false, null, "deposit", null, new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("a3000017-0000-0000-0000-000000000017"), 6000, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Deposit — 6000 coins", "credit", false, null, "deposit", null, new Guid("d0000033-0000-0000-0000-000000000033") },
                    { new Guid("a3000018-0000-0000-0000-000000000018"), 3800, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Deposit — 3800 coins", "credit", false, null, "deposit", null, new Guid("d0000038-0000-0000-0000-000000000038") }
                });

            migrationBuilder.InsertData(
                table: "Matches",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsDeleted", "UpdatedAt", "User1Id", "User2Id" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000026-0000-0000-0000-000000000026"), new Guid("d0000005-0000-0000-0000-000000000005") },
                    { new Guid("a1000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000027-0000-0000-0000-000000000027"), new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("a1000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000028-0000-0000-0000-000000000028"), new Guid("d0000006-0000-0000-0000-000000000006") },
                    { new Guid("a1000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000035-0000-0000-0000-000000000035"), new Guid("d0000011-0000-0000-0000-000000000011") },
                    { new Guid("a1000005-0000-0000-0000-000000000005"), new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000033-0000-0000-0000-000000000033"), new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("a1000006-0000-0000-0000-000000000006"), new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000034-0000-0000-0000-000000000034"), new Guid("d0000014-0000-0000-0000-000000000014") },
                    { new Guid("a1000007-0000-0000-0000-000000000007"), new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000038-0000-0000-0000-000000000038"), new Guid("d0000010-0000-0000-0000-000000000010") },
                    { new Guid("a1000008-0000-0000-0000-000000000008"), new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000040-0000-0000-0000-000000000040"), new Guid("d0000024-0000-0000-0000-000000000024") },
                    { new Guid("a1000009-0000-0000-0000-000000000009"), new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000047-0000-0000-0000-000000000047"), new Guid("d0000025-0000-0000-0000-000000000025") },
                    { new Guid("a1000010-0000-0000-0000-000000000010"), new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000046-0000-0000-0000-000000000046"), new Guid("d0000021-0000-0000-0000-000000000021") },
                    { new Guid("a1000011-0000-0000-0000-000000000011"), new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000030-0000-0000-0000-000000000030"), new Guid("d0000017-0000-0000-0000-000000000017") },
                    { new Guid("a1000012-0000-0000-0000-000000000012"), new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000031-0000-0000-0000-000000000031"), new Guid("d0000020-0000-0000-0000-000000000020") },
                    { new Guid("a1000013-0000-0000-0000-000000000013"), new DateTime(2024, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000042-0000-0000-0000-000000000042"), new Guid("d0000009-0000-0000-0000-000000000009") },
                    { new Guid("a1000014-0000-0000-0000-000000000014"), new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000041-0000-0000-0000-000000000041"), new Guid("d0000008-0000-0000-0000-000000000008") },
                    { new Guid("a1000015-0000-0000-0000-000000000015"), new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, null, new Guid("d0000037-0000-0000-0000-000000000037"), new Guid("d0000018-0000-0000-0000-000000000018") }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "Body", "CreatedAt", "DeletedAt", "IsDeleted", "IsRead", "ReferenceId", "Title", "Type", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("e1000001-0000-0000-0000-000000000001"), "You matched with Aisha Khan!", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000001-0000-0000-0000-000000000001", "New Match! 🎉", "match", null, new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("e1000002-0000-0000-0000-000000000002"), "You matched with Arjun Singh!", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, "a1000001-0000-0000-0000-000000000001", "New Match! 🎉", "match", null, new Guid("d0000005-0000-0000-0000-000000000005") },
                    { new Guid("e1000003-0000-0000-0000-000000000003"), "You matched with Priya Sharma!", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000002-0000-0000-0000-000000000002", "New Match! 🎉", "match", null, new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("e1000004-0000-0000-0000-000000000004"), "You matched with Rahul Mehta!", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000002-0000-0000-0000-000000000002", "New Match! 🎉", "match", null, new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("e1000005-0000-0000-0000-000000000005"), "You matched with Shreya Patel!", new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000003-0000-0000-0000-000000000003", "New Match! 🎉", "match", null, new Guid("d0000028-0000-0000-0000-000000000028") },
                    { new Guid("e1000006-0000-0000-0000-000000000006"), "You matched with Vikram Nair!", new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, "a1000003-0000-0000-0000-000000000003", "New Match! 🎉", "match", null, new Guid("d0000006-0000-0000-0000-000000000006") },
                    { new Guid("e1000007-0000-0000-0000-000000000007"), "You matched with Simran Kaur!", new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000004-0000-0000-0000-000000000004", "New Match! 🎉", "match", null, new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("e1000008-0000-0000-0000-000000000008"), "You matched with Nikhil Sharma!", new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000004-0000-0000-0000-000000000004", "New Match! 🎉", "match", null, new Guid("d0000011-0000-0000-0000-000000000011") },
                    { new Guid("e1000009-0000-0000-0000-000000000009"), "You matched with Ishita Sharma!", new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000005-0000-0000-0000-000000000005", "New Match! 🎉", "match", null, new Guid("d0000033-0000-0000-0000-000000000033") },
                    { new Guid("e1000010-0000-0000-0000-000000000010"), "You matched with Aman Joshi!", new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, "a1000005-0000-0000-0000-000000000005", "New Match! 🎉", "match", null, new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("e1000011-0000-0000-0000-000000000011"), "You matched with Tanvi Joshi!", new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000006-0000-0000-0000-000000000006", "New Match! 🎉", "match", null, new Guid("d0000034-0000-0000-0000-000000000034") },
                    { new Guid("e1000012-0000-0000-0000-000000000012"), "You matched with Aditya Kumar!", new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000006-0000-0000-0000-000000000006", "New Match! 🎉", "match", null, new Guid("d0000014-0000-0000-0000-000000000014") },
                    { new Guid("e1000013-0000-0000-0000-000000000013"), "You matched with Rita Desai!", new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000007-0000-0000-0000-000000000007", "New Match! 🎉", "match", null, new Guid("d0000038-0000-0000-0000-000000000038") },
                    { new Guid("e1000014-0000-0000-0000-000000000014"), "You matched with Kabir Singh!", new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, "a1000007-0000-0000-0000-000000000007", "New Match! 🎉", "match", null, new Guid("d0000010-0000-0000-0000-000000000010") },
                    { new Guid("e1000015-0000-0000-0000-000000000015"), "You matched with Tara Pillai!", new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000008-0000-0000-0000-000000000008", "New Match! 🎉", "match", null, new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("e1000016-0000-0000-0000-000000000016"), "You matched with Dev Malhotra!", new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000008-0000-0000-0000-000000000008", "New Match! 🎉", "match", null, new Guid("d0000024-0000-0000-0000-000000000024") },
                    { new Guid("e1000017-0000-0000-0000-000000000017"), "You matched with Nalini Krishnan!", new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000009-0000-0000-0000-000000000009", "New Match! 🎉", "match", null, new Guid("d0000047-0000-0000-0000-000000000047") },
                    { new Guid("e1000018-0000-0000-0000-000000000018"), "You matched with Shiv Kumar!", new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, "a1000009-0000-0000-0000-000000000009", "New Match! 🎉", "match", null, new Guid("d0000025-0000-0000-0000-000000000025") },
                    { new Guid("e1000019-0000-0000-0000-000000000019"), "You matched with Sonal Mehta!", new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000010-0000-0000-0000-000000000010", "New Match! 🎉", "match", null, new Guid("d0000046-0000-0000-0000-000000000046") },
                    { new Guid("e1000020-0000-0000-0000-000000000020"), "You matched with Jay Patel!", new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000010-0000-0000-0000-000000000010", "New Match! 🎉", "match", null, new Guid("d0000021-0000-0000-0000-000000000021") },
                    { new Guid("e1000021-0000-0000-0000-000000000021"), "You matched with Zara Ahmed!", new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000011-0000-0000-0000-000000000011", "New Match! 🎉", "match", null, new Guid("d0000030-0000-0000-0000-000000000030") },
                    { new Guid("e1000022-0000-0000-0000-000000000022"), "You matched with Rohit Sharma!", new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, "a1000011-0000-0000-0000-000000000011", "New Match! 🎉", "match", null, new Guid("d0000017-0000-0000-0000-000000000017") },
                    { new Guid("e1000023-0000-0000-0000-000000000023"), "You matched with Ananya Chatterjee!", new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000012-0000-0000-0000-000000000012", "New Match! 🎉", "match", null, new Guid("d0000031-0000-0000-0000-000000000031") },
                    { new Guid("e1000024-0000-0000-0000-000000000024"), "You matched with Karthik Menon!", new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000012-0000-0000-0000-000000000012", "New Match! 🎉", "match", null, new Guid("d0000020-0000-0000-0000-000000000020") },
                    { new Guid("e1000025-0000-0000-0000-000000000025"), "You matched with Kritika Bose!", new DateTime(2024, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000013-0000-0000-0000-000000000013", "New Match! 🎉", "match", null, new Guid("d0000042-0000-0000-0000-000000000042") },
                    { new Guid("e1000026-0000-0000-0000-000000000026"), "You matched with Rohan Bose!", new DateTime(2024, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, "a1000013-0000-0000-0000-000000000013", "New Match! 🎉", "match", null, new Guid("d0000009-0000-0000-0000-000000000009") },
                    { new Guid("e1000027-0000-0000-0000-000000000027"), "You matched with Pooja Gupta!", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000014-0000-0000-0000-000000000014", "New Match! 🎉", "match", null, new Guid("d0000041-0000-0000-0000-000000000041") },
                    { new Guid("e1000028-0000-0000-0000-000000000028"), "You matched with Mihir Shah!", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000014-0000-0000-0000-000000000014", "New Match! 🎉", "match", null, new Guid("d0000008-0000-0000-0000-000000000008") },
                    { new Guid("e1000029-0000-0000-0000-000000000029"), "You matched with Naina Verma!", new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, "a1000015-0000-0000-0000-000000000015", "New Match! 🎉", "match", null, new Guid("d0000037-0000-0000-0000-000000000037") },
                    { new Guid("e1000030-0000-0000-0000-000000000030"), "You matched with Aakash Verma!", new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, "a1000015-0000-0000-0000-000000000015", "New Match! 🎉", "match", null, new Guid("d0000018-0000-0000-0000-000000000018") },
                    { new Guid("e1000031-0000-0000-0000-000000000031"), "Priya sent you a message 💌", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "New Message 💬", "message", null, new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("e1000032-0000-0000-0000-000000000032"), "+100 coins added to your wallet", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "Welcome Bonus 🪙", "coins", null, new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("e1000033-0000-0000-0000-000000000033"), "Your coin balance is below 500 coins", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "Low Balance ⚠️", "system", null, new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("e1000034-0000-0000-0000-000000000034"), "Your identity has been verified", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "Profile Verified ✅", "system", null, new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("e1000035-0000-0000-0000-000000000035"), "Add a bio to attract more matches!", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "Profile Incomplete ⚠️", "system", null, new Guid("d0000029-0000-0000-0000-000000000029") },
                    { new Guid("e1000036-0000-0000-0000-000000000036"), "Someone sent you a super like!", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, "Super Like Received ⭐", "like", null, new Guid("d0000047-0000-0000-0000-000000000047") }
                });

            migrationBuilder.InsertData(
                table: "Reports",
                columns: new[] { "Id", "AdminNote", "CreatedAt", "DeletedAt", "Description", "IsDeleted", "Reason", "ReportedUserId", "ReporterId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a4000001-0000-0000-0000-000000000001"), null, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sending repeated unsolicited messages", false, "spam", new Guid("d0000029-0000-0000-0000-000000000029"), new Guid("d0000002-0000-0000-0000-000000000002"), "pending", null },
                    { new Guid("a4000002-0000-0000-0000-000000000002"), null, new DateTime(2024, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inappropriate language in DMs", false, "harassment", new Guid("d0000036-0000-0000-0000-000000000036"), new Guid("d0000005-0000-0000-0000-000000000005"), "reviewed", null },
                    { new Guid("a4000003-0000-0000-0000-000000000003"), null, new DateTime(2024, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, "Shared offensive content", false, "inappropriate_content", new Guid("d0000043-0000-0000-0000-000000000043"), new Guid("d0000008-0000-0000-0000-000000000008"), "action_taken", null },
                    { new Guid("a4000004-0000-0000-0000-000000000004"), null, new DateTime(2024, 2, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, "Profile pictures appear to be stolen", false, "fake_profile", new Guid("d0000045-0000-0000-0000-000000000045"), new Guid("d0000014-0000-0000-0000-000000000014"), "pending", null }
                });

            migrationBuilder.InsertData(
                table: "Swipes",
                columns: new[] { "Id", "Action", "CreatedAt", "DeletedAt", "IsDeleted", "SwiperId", "TargetId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("b1000001-0000-0000-0000-000000000001"), "like", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000026-0000-0000-0000-000000000026"), new Guid("d0000005-0000-0000-0000-000000000005"), null },
                    { new Guid("b1000002-0000-0000-0000-000000000002"), "like", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000005-0000-0000-0000-000000000005"), new Guid("d0000026-0000-0000-0000-000000000026"), null },
                    { new Guid("b1000003-0000-0000-0000-000000000003"), "superlike", new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000027-0000-0000-0000-000000000027"), new Guid("d0000002-0000-0000-0000-000000000002"), null },
                    { new Guid("b1000004-0000-0000-0000-000000000004"), "like", new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000002-0000-0000-0000-000000000002"), new Guid("d0000027-0000-0000-0000-000000000027"), null },
                    { new Guid("b1000005-0000-0000-0000-000000000005"), "like", new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000028-0000-0000-0000-000000000028"), new Guid("d0000006-0000-0000-0000-000000000006"), null },
                    { new Guid("b1000006-0000-0000-0000-000000000006"), "like", new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000006-0000-0000-0000-000000000006"), new Guid("d0000028-0000-0000-0000-000000000028"), null },
                    { new Guid("b1000007-0000-0000-0000-000000000007"), "like", new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000035-0000-0000-0000-000000000035"), new Guid("d0000011-0000-0000-0000-000000000011"), null },
                    { new Guid("b1000008-0000-0000-0000-000000000008"), "like", new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000011-0000-0000-0000-000000000011"), new Guid("d0000035-0000-0000-0000-000000000035"), null },
                    { new Guid("b1000009-0000-0000-0000-000000000009"), "superlike", new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000033-0000-0000-0000-000000000033"), new Guid("d0000015-0000-0000-0000-000000000015"), null },
                    { new Guid("b1000010-0000-0000-0000-000000000010"), "like", new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000015-0000-0000-0000-000000000015"), new Guid("d0000033-0000-0000-0000-000000000033"), null },
                    { new Guid("b1000011-0000-0000-0000-000000000011"), "like", new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000034-0000-0000-0000-000000000034"), new Guid("d0000014-0000-0000-0000-000000000014"), null },
                    { new Guid("b1000012-0000-0000-0000-000000000012"), "like", new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000014-0000-0000-0000-000000000014"), new Guid("d0000034-0000-0000-0000-000000000034"), null },
                    { new Guid("b1000013-0000-0000-0000-000000000013"), "like", new DateTime(2024, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000038-0000-0000-0000-000000000038"), new Guid("d0000010-0000-0000-0000-000000000010"), null },
                    { new Guid("b1000014-0000-0000-0000-000000000014"), "like", new DateTime(2024, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000010-0000-0000-0000-000000000010"), new Guid("d0000038-0000-0000-0000-000000000038"), null },
                    { new Guid("b1000015-0000-0000-0000-000000000015"), "superlike", new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000040-0000-0000-0000-000000000040"), new Guid("d0000024-0000-0000-0000-000000000024"), null },
                    { new Guid("b1000016-0000-0000-0000-000000000016"), "like", new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000024-0000-0000-0000-000000000024"), new Guid("d0000040-0000-0000-0000-000000000040"), null },
                    { new Guid("b1000017-0000-0000-0000-000000000017"), "like", new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000047-0000-0000-0000-000000000047"), new Guid("d0000025-0000-0000-0000-000000000025"), null },
                    { new Guid("b1000018-0000-0000-0000-000000000018"), "like", new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000025-0000-0000-0000-000000000025"), new Guid("d0000047-0000-0000-0000-000000000047"), null },
                    { new Guid("b1000019-0000-0000-0000-000000000019"), "like", new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000046-0000-0000-0000-000000000046"), new Guid("d0000021-0000-0000-0000-000000000021"), null },
                    { new Guid("b1000020-0000-0000-0000-000000000020"), "like", new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000021-0000-0000-0000-000000000021"), new Guid("d0000046-0000-0000-0000-000000000046"), null },
                    { new Guid("b1000021-0000-0000-0000-000000000021"), "superlike", new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000030-0000-0000-0000-000000000030"), new Guid("d0000017-0000-0000-0000-000000000017"), null },
                    { new Guid("b1000022-0000-0000-0000-000000000022"), "like", new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000017-0000-0000-0000-000000000017"), new Guid("d0000030-0000-0000-0000-000000000030"), null },
                    { new Guid("b1000023-0000-0000-0000-000000000023"), "like", new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000031-0000-0000-0000-000000000031"), new Guid("d0000020-0000-0000-0000-000000000020"), null },
                    { new Guid("b1000024-0000-0000-0000-000000000024"), "like", new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000020-0000-0000-0000-000000000020"), new Guid("d0000031-0000-0000-0000-000000000031"), null },
                    { new Guid("b1000025-0000-0000-0000-000000000025"), "like", new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000042-0000-0000-0000-000000000042"), new Guid("d0000009-0000-0000-0000-000000000009"), null },
                    { new Guid("b1000026-0000-0000-0000-000000000026"), "like", new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000009-0000-0000-0000-000000000009"), new Guid("d0000042-0000-0000-0000-000000000042"), null },
                    { new Guid("b1000027-0000-0000-0000-000000000027"), "superlike", new DateTime(2024, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000041-0000-0000-0000-000000000041"), new Guid("d0000008-0000-0000-0000-000000000008"), null },
                    { new Guid("b1000028-0000-0000-0000-000000000028"), "like", new DateTime(2024, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000008-0000-0000-0000-000000000008"), new Guid("d0000041-0000-0000-0000-000000000041"), null },
                    { new Guid("b1000029-0000-0000-0000-000000000029"), "like", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000037-0000-0000-0000-000000000037"), new Guid("d0000018-0000-0000-0000-000000000018"), null },
                    { new Guid("b1000030-0000-0000-0000-000000000030"), "like", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000018-0000-0000-0000-000000000018"), new Guid("d0000037-0000-0000-0000-000000000037"), null },
                    { new Guid("b1000031-0000-0000-0000-000000000031"), "like", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000029-0000-0000-0000-000000000029"), new Guid("d0000003-0000-0000-0000-000000000003"), null },
                    { new Guid("b1000032-0000-0000-0000-000000000032"), "like", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000036-0000-0000-0000-000000000036"), new Guid("d0000013-0000-0000-0000-000000000013"), null },
                    { new Guid("b1000033-0000-0000-0000-000000000033"), "like", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000048-0000-0000-0000-000000000048"), new Guid("d0000007-0000-0000-0000-000000000007"), null },
                    { new Guid("b1000034-0000-0000-0000-000000000034"), "dislike", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000043-0000-0000-0000-000000000043"), new Guid("d0000012-0000-0000-0000-000000000012"), null },
                    { new Guid("b1000035-0000-0000-0000-000000000035"), "like", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000044-0000-0000-0000-000000000044"), new Guid("d0000004-0000-0000-0000-000000000004"), null },
                    { new Guid("b1000036-0000-0000-0000-000000000036"), "like", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000045-0000-0000-0000-000000000045"), new Guid("d0000016-0000-0000-0000-000000000016"), null },
                    { new Guid("b1000037-0000-0000-0000-000000000037"), "like", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000039-0000-0000-0000-000000000039"), new Guid("d0000022-0000-0000-0000-000000000022"), null },
                    { new Guid("b1000038-0000-0000-0000-000000000038"), "dislike", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("d0000032-0000-0000-0000-000000000032"), new Guid("d0000023-0000-0000-0000-000000000023"), null }
                });

            migrationBuilder.InsertData(
                table: "UserInterests",
                columns: new[] { "InterestId", "UserId" },
                values: new object[,]
                {
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000001-0000-0000-0000-000000000001") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000001-0000-0000-0000-000000000001") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("a0000001-0000-0000-0000-000000000006"), new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("a0000001-0000-0000-0000-000000000008"), new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("a0000001-0000-0000-0000-000000000010"), new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000003-0000-0000-0000-000000000003") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000003-0000-0000-0000-000000000003") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000003-0000-0000-0000-000000000003") },
                    { new Guid("a0000001-0000-0000-0000-000000000006"), new Guid("d0000004-0000-0000-0000-000000000004") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000004-0000-0000-0000-000000000004") },
                    { new Guid("a0000001-0000-0000-0000-000000000014"), new Guid("d0000004-0000-0000-0000-000000000004") },
                    { new Guid("a0000001-0000-0000-0000-000000000007"), new Guid("d0000005-0000-0000-0000-000000000005") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000005-0000-0000-0000-000000000005") },
                    { new Guid("a0000001-0000-0000-0000-000000000013"), new Guid("d0000005-0000-0000-0000-000000000005") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000006-0000-0000-0000-000000000006") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000006-0000-0000-0000-000000000006") },
                    { new Guid("a0000001-0000-0000-0000-000000000008"), new Guid("d0000006-0000-0000-0000-000000000006") },
                    { new Guid("a0000001-0000-0000-0000-000000000010"), new Guid("d0000006-0000-0000-0000-000000000006") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000007-0000-0000-0000-000000000007") },
                    { new Guid("a0000001-0000-0000-0000-000000000007"), new Guid("d0000007-0000-0000-0000-000000000007") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000007-0000-0000-0000-000000000007") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000008-0000-0000-0000-000000000008") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000008-0000-0000-0000-000000000008") },
                    { new Guid("a0000001-0000-0000-0000-000000000013"), new Guid("d0000008-0000-0000-0000-000000000008") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000009-0000-0000-0000-000000000009") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000009-0000-0000-0000-000000000009") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000009-0000-0000-0000-000000000009") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000010-0000-0000-0000-000000000010") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000010-0000-0000-0000-000000000010") },
                    { new Guid("a0000001-0000-0000-0000-000000000010"), new Guid("d0000010-0000-0000-0000-000000000010") },
                    { new Guid("a0000001-0000-0000-0000-000000000019"), new Guid("d0000010-0000-0000-0000-000000000010") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000011-0000-0000-0000-000000000011") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000011-0000-0000-0000-000000000011") },
                    { new Guid("a0000001-0000-0000-0000-000000000008"), new Guid("d0000011-0000-0000-0000-000000000011") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000012-0000-0000-0000-000000000012") },
                    { new Guid("a0000001-0000-0000-0000-000000000007"), new Guid("d0000012-0000-0000-0000-000000000012") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000012-0000-0000-0000-000000000012") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000013-0000-0000-0000-000000000013") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000013-0000-0000-0000-000000000013") },
                    { new Guid("a0000001-0000-0000-0000-000000000019"), new Guid("d0000013-0000-0000-0000-000000000013") },
                    { new Guid("a0000001-0000-0000-0000-000000000007"), new Guid("d0000014-0000-0000-0000-000000000014") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000014-0000-0000-0000-000000000014") },
                    { new Guid("a0000001-0000-0000-0000-000000000017"), new Guid("d0000014-0000-0000-0000-000000000014") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000014-0000-0000-0000-000000000014") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("a0000001-0000-0000-0000-000000000007"), new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000016-0000-0000-0000-000000000016") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000016-0000-0000-0000-000000000016") },
                    { new Guid("a0000001-0000-0000-0000-000000000019"), new Guid("d0000016-0000-0000-0000-000000000016") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000017-0000-0000-0000-000000000017") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000017-0000-0000-0000-000000000017") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000017-0000-0000-0000-000000000017") },
                    { new Guid("a0000001-0000-0000-0000-000000000013"), new Guid("d0000017-0000-0000-0000-000000000017") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000018-0000-0000-0000-000000000018") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000018-0000-0000-0000-000000000018") },
                    { new Guid("a0000001-0000-0000-0000-000000000010"), new Guid("d0000018-0000-0000-0000-000000000018") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000018-0000-0000-0000-000000000018") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000019-0000-0000-0000-000000000019") },
                    { new Guid("a0000001-0000-0000-0000-000000000012"), new Guid("d0000019-0000-0000-0000-000000000019") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000019-0000-0000-0000-000000000019") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000019-0000-0000-0000-000000000019") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000020-0000-0000-0000-000000000020") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000020-0000-0000-0000-000000000020") },
                    { new Guid("a0000001-0000-0000-0000-000000000014"), new Guid("d0000020-0000-0000-0000-000000000020") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000020-0000-0000-0000-000000000020") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000021-0000-0000-0000-000000000021") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000021-0000-0000-0000-000000000021") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000021-0000-0000-0000-000000000021") },
                    { new Guid("a0000001-0000-0000-0000-000000000019"), new Guid("d0000021-0000-0000-0000-000000000021") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000022-0000-0000-0000-000000000022") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000022-0000-0000-0000-000000000022") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000022-0000-0000-0000-000000000022") },
                    { new Guid("a0000001-0000-0000-0000-000000000017"), new Guid("d0000022-0000-0000-0000-000000000022") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000023-0000-0000-0000-000000000023") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000023-0000-0000-0000-000000000023") },
                    { new Guid("a0000001-0000-0000-0000-000000000008"), new Guid("d0000023-0000-0000-0000-000000000023") },
                    { new Guid("a0000001-0000-0000-0000-000000000019"), new Guid("d0000023-0000-0000-0000-000000000023") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000024-0000-0000-0000-000000000024") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000024-0000-0000-0000-000000000024") },
                    { new Guid("a0000001-0000-0000-0000-000000000008"), new Guid("d0000024-0000-0000-0000-000000000024") },
                    { new Guid("a0000001-0000-0000-0000-000000000010"), new Guid("d0000024-0000-0000-0000-000000000024") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000025-0000-0000-0000-000000000025") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000025-0000-0000-0000-000000000025") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000025-0000-0000-0000-000000000025") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000025-0000-0000-0000-000000000025") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000028-0000-0000-0000-000000000028") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000028-0000-0000-0000-000000000028") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000028-0000-0000-0000-000000000028") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000028-0000-0000-0000-000000000028") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000029-0000-0000-0000-000000000029") },
                    { new Guid("a0000001-0000-0000-0000-000000000011"), new Guid("d0000029-0000-0000-0000-000000000029") },
                    { new Guid("a0000001-0000-0000-0000-000000000012"), new Guid("d0000029-0000-0000-0000-000000000029") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000030-0000-0000-0000-000000000030") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000030-0000-0000-0000-000000000030") },
                    { new Guid("a0000001-0000-0000-0000-000000000006"), new Guid("d0000030-0000-0000-0000-000000000030") },
                    { new Guid("a0000001-0000-0000-0000-000000000014"), new Guid("d0000030-0000-0000-0000-000000000030") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000031-0000-0000-0000-000000000031") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000031-0000-0000-0000-000000000031") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000031-0000-0000-0000-000000000031") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000031-0000-0000-0000-000000000031") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000032-0000-0000-0000-000000000032") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000032-0000-0000-0000-000000000032") },
                    { new Guid("a0000001-0000-0000-0000-000000000012"), new Guid("d0000032-0000-0000-0000-000000000032") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000032-0000-0000-0000-000000000032") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000033-0000-0000-0000-000000000033") },
                    { new Guid("a0000001-0000-0000-0000-000000000007"), new Guid("d0000033-0000-0000-0000-000000000033") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000033-0000-0000-0000-000000000033") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000033-0000-0000-0000-000000000033") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000034-0000-0000-0000-000000000034") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000034-0000-0000-0000-000000000034") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000034-0000-0000-0000-000000000034") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000034-0000-0000-0000-000000000034") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("a0000001-0000-0000-0000-000000000013"), new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000036-0000-0000-0000-000000000036") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000036-0000-0000-0000-000000000036") },
                    { new Guid("a0000001-0000-0000-0000-000000000012"), new Guid("d0000036-0000-0000-0000-000000000036") },
                    { new Guid("a0000001-0000-0000-0000-000000000020"), new Guid("d0000036-0000-0000-0000-000000000036") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000037-0000-0000-0000-000000000037") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000037-0000-0000-0000-000000000037") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000037-0000-0000-0000-000000000037") },
                    { new Guid("a0000001-0000-0000-0000-000000000019"), new Guid("d0000037-0000-0000-0000-000000000037") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000038-0000-0000-0000-000000000038") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000038-0000-0000-0000-000000000038") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000038-0000-0000-0000-000000000038") },
                    { new Guid("a0000001-0000-0000-0000-000000000010"), new Guid("d0000038-0000-0000-0000-000000000038") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000039-0000-0000-0000-000000000039") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000039-0000-0000-0000-000000000039") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000039-0000-0000-0000-000000000039") },
                    { new Guid("a0000001-0000-0000-0000-000000000008"), new Guid("d0000039-0000-0000-0000-000000000039") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000041-0000-0000-0000-000000000041") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000041-0000-0000-0000-000000000041") },
                    { new Guid("a0000001-0000-0000-0000-000000000011"), new Guid("d0000041-0000-0000-0000-000000000041") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000041-0000-0000-0000-000000000041") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000042-0000-0000-0000-000000000042") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000042-0000-0000-0000-000000000042") },
                    { new Guid("a0000001-0000-0000-0000-000000000007"), new Guid("d0000042-0000-0000-0000-000000000042") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000042-0000-0000-0000-000000000042") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000043-0000-0000-0000-000000000043") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000043-0000-0000-0000-000000000043") },
                    { new Guid("a0000001-0000-0000-0000-000000000004"), new Guid("d0000043-0000-0000-0000-000000000043") },
                    { new Guid("a0000001-0000-0000-0000-000000000005"), new Guid("d0000043-0000-0000-0000-000000000043") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000044-0000-0000-0000-000000000044") },
                    { new Guid("a0000001-0000-0000-0000-000000000014"), new Guid("d0000044-0000-0000-0000-000000000044") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000044-0000-0000-0000-000000000044") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000044-0000-0000-0000-000000000044") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000045-0000-0000-0000-000000000045") },
                    { new Guid("a0000001-0000-0000-0000-000000000011"), new Guid("d0000045-0000-0000-0000-000000000045") },
                    { new Guid("a0000001-0000-0000-0000-000000000012"), new Guid("d0000045-0000-0000-0000-000000000045") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000045-0000-0000-0000-000000000045") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000046-0000-0000-0000-000000000046") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000046-0000-0000-0000-000000000046") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000046-0000-0000-0000-000000000046") },
                    { new Guid("a0000001-0000-0000-0000-000000000018"), new Guid("d0000046-0000-0000-0000-000000000046") },
                    { new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("d0000047-0000-0000-0000-000000000047") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000047-0000-0000-0000-000000000047") },
                    { new Guid("a0000001-0000-0000-0000-000000000009"), new Guid("d0000047-0000-0000-0000-000000000047") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000047-0000-0000-0000-000000000047") },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("d0000048-0000-0000-0000-000000000048") },
                    { new Guid("a0000001-0000-0000-0000-000000000011"), new Guid("d0000048-0000-0000-0000-000000000048") },
                    { new Guid("a0000001-0000-0000-0000-000000000012"), new Guid("d0000048-0000-0000-0000-000000000048") },
                    { new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("d0000049-0000-0000-0000-000000000049") },
                    { new Guid("a0000001-0000-0000-0000-000000000010"), new Guid("d0000049-0000-0000-0000-000000000049") },
                    { new Guid("a0000001-0000-0000-0000-000000000015"), new Guid("d0000049-0000-0000-0000-000000000049") },
                    { new Guid("a0000001-0000-0000-0000-000000000016"), new Guid("d0000050-0000-0000-0000-000000000050") }
                });

            migrationBuilder.InsertData(
                table: "UserLocations",
                columns: new[] { "Id", "City", "Country", "CreatedAt", "DeletedAt", "IsDeleted", "Lat", "Lng", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("f0000001-0000-0000-0000-000000000001"), "Delhi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4390), null, false, 28.613900000000001, 77.209000000000003, null, new Guid("d0000001-0000-0000-0000-000000000001") },
                    { new Guid("f0000002-0000-0000-0000-000000000002"), "Delhi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4405), null, false, 28.613900000000001, 77.209000000000003, null, new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("f0000003-0000-0000-0000-000000000003"), "Mumbai", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4408), null, false, 19.076000000000001, 72.877700000000004, null, new Guid("d0000003-0000-0000-0000-000000000003") },
                    { new Guid("f0000004-0000-0000-0000-000000000004"), "Pune", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4411), null, false, 18.520399999999999, 73.856700000000004, null, new Guid("d0000004-0000-0000-0000-000000000004") },
                    { new Guid("f0000005-0000-0000-0000-000000000005"), "Hyderabad", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4414), null, false, 17.385000000000002, 78.486699999999999, null, new Guid("d0000005-0000-0000-0000-000000000005") },
                    { new Guid("f0000006-0000-0000-0000-000000000006"), "Ahmedabad", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4417), null, false, 23.022500000000001, 72.571399999999997, null, new Guid("d0000006-0000-0000-0000-000000000006") },
                    { new Guid("f0000007-0000-0000-0000-000000000007"), "Hyderabad", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4420), null, false, 17.399999999999999, 78.5, null, new Guid("d0000007-0000-0000-0000-000000000007") },
                    { new Guid("f0000008-0000-0000-0000-000000000008"), "Jaipur", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4423), null, false, 26.912400000000002, 75.787300000000002, null, new Guid("d0000008-0000-0000-0000-000000000008") },
                    { new Guid("f0000009-0000-0000-0000-000000000009"), "Kolkata", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4432), null, false, 22.572600000000001, 88.363900000000001, null, new Guid("d0000009-0000-0000-0000-000000000009") },
                    { new Guid("f0000010-0000-0000-0000-000000000010"), "Surat", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4438), null, false, 21.170200000000001, 72.831100000000006, null, new Guid("d0000010-0000-0000-0000-000000000010") },
                    { new Guid("f0000011-0000-0000-0000-000000000011"), "Amritsar", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4441), null, false, 31.634, 74.872299999999996, null, new Guid("d0000011-0000-0000-0000-000000000011") },
                    { new Guid("f0000012-0000-0000-0000-000000000012"), "Kochi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4444), null, false, 9.9312000000000005, 76.267300000000006, null, new Guid("d0000012-0000-0000-0000-000000000012") },
                    { new Guid("f0000013-0000-0000-0000-000000000013"), "Trivandrum", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4447), null, false, 8.5241000000000007, 76.936599999999999, null, new Guid("d0000013-0000-0000-0000-000000000013") },
                    { new Guid("f0000014-0000-0000-0000-000000000014"), "Pune", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4450), null, false, 18.530000000000001, 73.870000000000005, null, new Guid("d0000014-0000-0000-0000-000000000014") },
                    { new Guid("f0000015-0000-0000-0000-000000000015"), "Delhi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4452), null, false, 28.629999999999999, 77.219999999999999, null, new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("f0000016-0000-0000-0000-000000000016"), "Varanasi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4462), null, false, 25.317599999999999, 82.9739, null, new Guid("d0000016-0000-0000-0000-000000000016") },
                    { new Guid("f0000017-0000-0000-0000-000000000017"), "Mumbai", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4467), null, false, 19.09, 72.859999999999999, null, new Guid("d0000017-0000-0000-0000-000000000017") },
                    { new Guid("f0000018-0000-0000-0000-000000000018"), "Delhi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4470), null, false, 28.649999999999999, 77.230000000000004, null, new Guid("d0000018-0000-0000-0000-000000000018") },
                    { new Guid("f0000019-0000-0000-0000-000000000019"), "Bengaluru", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4477), null, false, 12.9716, 77.5946, null, new Guid("d0000019-0000-0000-0000-000000000019") },
                    { new Guid("f0000020-0000-0000-0000-000000000020"), "Kolkata", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4480), null, false, 22.579999999999998, 88.359999999999999, null, new Guid("d0000020-0000-0000-0000-000000000020") },
                    { new Guid("f0000021-0000-0000-0000-000000000021"), "Mumbai", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4483), null, false, 19.079999999999998, 72.890000000000001, null, new Guid("d0000021-0000-0000-0000-000000000021") },
                    { new Guid("f0000022-0000-0000-0000-000000000022"), "Goa", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4486), null, false, 15.4909, 73.827799999999996, null, new Guid("d0000022-0000-0000-0000-000000000022") },
                    { new Guid("f0000023-0000-0000-0000-000000000023"), "Lucknow", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4489), null, false, 26.846699999999998, 80.946200000000005, null, new Guid("d0000023-0000-0000-0000-000000000023") },
                    { new Guid("f0000024-0000-0000-0000-000000000024"), "Chennai", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4563), null, false, 13.082700000000001, 80.270700000000005, null, new Guid("d0000024-0000-0000-0000-000000000024") },
                    { new Guid("f0000025-0000-0000-0000-000000000025"), "Bengaluru", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4570), null, false, 12.98, 77.599999999999994, null, new Guid("d0000025-0000-0000-0000-000000000025") },
                    { new Guid("f0000026-0000-0000-0000-000000000026"), "Gurgaon", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4573), null, false, 28.459499999999998, 77.026600000000002, null, new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("f0000027-0000-0000-0000-000000000027"), "Noida", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4576), null, false, 28.535499999999999, 77.391000000000005, null, new Guid("d0000027-0000-0000-0000-000000000027") },
                    { new Guid("f0000028-0000-0000-0000-000000000028"), "Delhi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4582), null, false, 28.7041, 77.102500000000006, null, new Guid("d0000028-0000-0000-0000-000000000028") },
                    { new Guid("f0000029-0000-0000-0000-000000000029"), "Noida", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4585), null, false, 28.539999999999999, 77.400000000000006, null, new Guid("d0000029-0000-0000-0000-000000000029") },
                    { new Guid("f0000030-0000-0000-0000-000000000030"), "Bengaluru", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4588), null, false, 12.9716, 77.5946, null, new Guid("d0000030-0000-0000-0000-000000000030") },
                    { new Guid("f0000031-0000-0000-0000-000000000031"), "Chennai", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4591), null, false, 13.082700000000001, 80.270700000000005, null, new Guid("d0000031-0000-0000-0000-000000000031") },
                    { new Guid("f0000032-0000-0000-0000-000000000032"), "Kolkata", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4594), null, false, 22.572600000000001, 88.363900000000001, null, new Guid("d0000032-0000-0000-0000-000000000032") },
                    { new Guid("f0000033-0000-0000-0000-000000000033"), "Chandigarh", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4600), null, false, 30.7333, 76.779399999999995, null, new Guid("d0000033-0000-0000-0000-000000000033") },
                    { new Guid("f0000034-0000-0000-0000-000000000034"), "Pune", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4602), null, false, 18.52, 73.859999999999999, null, new Guid("d0000034-0000-0000-0000-000000000034") },
                    { new Guid("f0000035-0000-0000-0000-000000000035"), "Mumbai", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4605), null, false, 19.079999999999998, 72.879999999999995, null, new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("f0000036-0000-0000-0000-000000000036"), "Coimbatore", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4609), null, false, 11.0168, 76.955799999999996, null, new Guid("d0000036-0000-0000-0000-000000000036") },
                    { new Guid("f0000037-0000-0000-0000-000000000037"), "Delhi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4619), null, false, 28.620000000000001, 77.209999999999994, null, new Guid("d0000037-0000-0000-0000-000000000037") },
                    { new Guid("f0000038-0000-0000-0000-000000000038"), "Jaipur", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4622), null, false, 26.912400000000002, 75.787300000000002, null, new Guid("d0000038-0000-0000-0000-000000000038") },
                    { new Guid("f0000039-0000-0000-0000-000000000039"), "Mumbai", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4625), null, false, 19.07, 72.879999999999995, null, new Guid("d0000039-0000-0000-0000-000000000039") },
                    { new Guid("f0000040-0000-0000-0000-000000000040"), "Delhi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4628), null, false, 28.640000000000001, 77.200000000000003, null, new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("f0000041-0000-0000-0000-000000000041"), "Surat", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4633), null, false, 21.170200000000001, 72.831100000000006, null, new Guid("d0000041-0000-0000-0000-000000000041") },
                    { new Guid("f0000042-0000-0000-0000-000000000042"), "Kolkata", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4636), null, false, 22.59, 88.370000000000005, null, new Guid("d0000042-0000-0000-0000-000000000042") },
                    { new Guid("f0000043-0000-0000-0000-000000000043"), "Lucknow", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4639), null, false, 26.850000000000001, 80.950000000000003, null, new Guid("d0000043-0000-0000-0000-000000000043") },
                    { new Guid("f0000044-0000-0000-0000-000000000044"), "Indore", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4642), null, false, 22.7196, 75.857699999999994, null, new Guid("d0000044-0000-0000-0000-000000000044") },
                    { new Guid("f0000045-0000-0000-0000-000000000045"), "Bhopal", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4649), null, false, 23.259899999999998, 77.412599999999998, null, new Guid("d0000045-0000-0000-0000-000000000045") },
                    { new Guid("f0000046-0000-0000-0000-000000000046"), "Ahmedabad", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4655), null, false, 23.030000000000001, 72.579999999999998, null, new Guid("d0000046-0000-0000-0000-000000000046") },
                    { new Guid("f0000047-0000-0000-0000-000000000047"), "Chandigarh", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4658), null, false, 30.739999999999998, 76.790000000000006, null, new Guid("d0000047-0000-0000-0000-000000000047") },
                    { new Guid("f0000048-0000-0000-0000-000000000048"), "Lucknow", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4661), null, false, 26.846699999999998, 80.946200000000005, null, new Guid("d0000048-0000-0000-0000-000000000048") },
                    { new Guid("f0000049-0000-0000-0000-000000000049"), "Thiruvananthapuram", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4667), null, false, 8.5241000000000007, 76.936599999999999, null, new Guid("d0000049-0000-0000-0000-000000000049") },
                    { new Guid("f0000050-0000-0000-0000-000000000050"), "Delhi", "India", new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4670), null, false, 28.620000000000001, 77.200000000000003, null, new Guid("d0000050-0000-0000-0000-000000000050") }
                });

            migrationBuilder.InsertData(
                table: "UserPreferences",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "InterestedIn", "IsDeleted", "Location", "MaxAge", "MaxDistance", "MinAge", "NearbyOnly", "OnlineOnly", "RelationshipType", "UpdatedAt", "UserId", "VerifiedOnly" },
                values: new object[,]
                {
                    { new Guid("e0000001-0000-0000-0000-000000000001"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4758), null, "girls", false, null, 32, 150, 20, false, false, "both", null, new Guid("d0000001-0000-0000-0000-000000000001"), false },
                    { new Guid("e0000002-0000-0000-0000-000000000002"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4765), null, "boys", false, null, 35, 200, 22, false, false, "casual", null, new Guid("d0000002-0000-0000-0000-000000000002"), false },
                    { new Guid("e0000003-0000-0000-0000-000000000003"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4768), null, "boys", false, null, 35, 250, 22, false, false, "both", null, new Guid("d0000003-0000-0000-0000-000000000003"), false },
                    { new Guid("e0000004-0000-0000-0000-000000000004"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4771), null, "boys", false, null, 35, 300, 22, false, false, "serious", null, new Guid("d0000004-0000-0000-0000-000000000004"), false },
                    { new Guid("e0000005-0000-0000-0000-000000000005"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4774), null, "boys", false, null, 35, 100, 22, false, false, "both", null, new Guid("d0000005-0000-0000-0000-000000000005"), false },
                    { new Guid("e0000006-0000-0000-0000-000000000006"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4781), null, "boys", false, null, 35, 150, 22, false, false, "serious", null, new Guid("d0000006-0000-0000-0000-000000000006"), false },
                    { new Guid("e0000007-0000-0000-0000-000000000007"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4787), null, "boys", false, null, 35, 200, 22, false, false, "both", null, new Guid("d0000007-0000-0000-0000-000000000007"), false },
                    { new Guid("e0000008-0000-0000-0000-000000000008"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4790), null, "boys", false, null, 35, 250, 22, false, false, "casual", null, new Guid("d0000008-0000-0000-0000-000000000008"), false },
                    { new Guid("e0000009-0000-0000-0000-000000000009"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4793), null, "boys", false, null, 35, 300, 22, false, false, "both", null, new Guid("d0000009-0000-0000-0000-000000000009"), false },
                    { new Guid("e0000010-0000-0000-0000-000000000010"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4796), null, "boys", false, null, 35, 100, 22, false, false, "serious", null, new Guid("d0000010-0000-0000-0000-000000000010"), false },
                    { new Guid("e0000011-0000-0000-0000-000000000011"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4799), null, "boys", false, null, 35, 150, 22, false, false, "both", null, new Guid("d0000011-0000-0000-0000-000000000011"), false },
                    { new Guid("e0000012-0000-0000-0000-000000000012"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4802), null, "boys", false, null, 35, 200, 22, false, false, "serious", null, new Guid("d0000012-0000-0000-0000-000000000012"), false },
                    { new Guid("e0000013-0000-0000-0000-000000000013"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4805), null, "boys", false, null, 35, 250, 22, false, false, "both", null, new Guid("d0000013-0000-0000-0000-000000000013"), false },
                    { new Guid("e0000014-0000-0000-0000-000000000014"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4808), null, "boys", false, null, 35, 300, 22, false, false, "casual", null, new Guid("d0000014-0000-0000-0000-000000000014"), false },
                    { new Guid("e0000015-0000-0000-0000-000000000015"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4814), null, "boys", false, null, 35, 100, 22, false, false, "both", null, new Guid("d0000015-0000-0000-0000-000000000015"), false },
                    { new Guid("e0000016-0000-0000-0000-000000000016"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4817), null, "boys", false, null, 35, 150, 22, false, false, "serious", null, new Guid("d0000016-0000-0000-0000-000000000016"), false },
                    { new Guid("e0000017-0000-0000-0000-000000000017"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4910), null, "boys", false, null, 35, 200, 22, false, false, "both", null, new Guid("d0000017-0000-0000-0000-000000000017"), false },
                    { new Guid("e0000018-0000-0000-0000-000000000018"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4917), null, "boys", false, null, 35, 250, 22, false, false, "serious", null, new Guid("d0000018-0000-0000-0000-000000000018"), false },
                    { new Guid("e0000019-0000-0000-0000-000000000019"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4920), null, "boys", false, null, 35, 300, 22, false, false, "both", null, new Guid("d0000019-0000-0000-0000-000000000019"), false },
                    { new Guid("e0000020-0000-0000-0000-000000000020"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4923), null, "boys", false, null, 35, 100, 22, false, false, "casual", null, new Guid("d0000020-0000-0000-0000-000000000020"), false },
                    { new Guid("e0000021-0000-0000-0000-000000000021"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4926), null, "boys", false, null, 35, 150, 22, false, false, "both", null, new Guid("d0000021-0000-0000-0000-000000000021"), false },
                    { new Guid("e0000022-0000-0000-0000-000000000022"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4928), null, "boys", false, null, 35, 200, 22, false, false, "serious", null, new Guid("d0000022-0000-0000-0000-000000000022"), false },
                    { new Guid("e0000023-0000-0000-0000-000000000023"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4934), null, "boys", false, null, 35, 250, 22, false, false, "both", null, new Guid("d0000023-0000-0000-0000-000000000023"), false },
                    { new Guid("e0000024-0000-0000-0000-000000000024"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4937), null, "boys", false, null, 35, 300, 22, false, false, "serious", null, new Guid("d0000024-0000-0000-0000-000000000024"), false },
                    { new Guid("e0000025-0000-0000-0000-000000000025"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4940), null, "boys", false, null, 35, 100, 22, false, false, "both", null, new Guid("d0000025-0000-0000-0000-000000000025"), false },
                    { new Guid("e0000026-0000-0000-0000-000000000026"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4943), null, "girls", false, null, 32, 150, 20, false, false, "casual", null, new Guid("d0000026-0000-0000-0000-000000000026"), false },
                    { new Guid("e0000027-0000-0000-0000-000000000027"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4946), null, "girls", false, null, 32, 200, 20, false, false, "both", null, new Guid("d0000027-0000-0000-0000-000000000027"), false },
                    { new Guid("e0000028-0000-0000-0000-000000000028"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4949), null, "girls", false, null, 32, 250, 20, false, false, "serious", null, new Guid("d0000028-0000-0000-0000-000000000028"), false },
                    { new Guid("e0000029-0000-0000-0000-000000000029"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4955), null, "girls", false, null, 32, 300, 20, false, false, "both", null, new Guid("d0000029-0000-0000-0000-000000000029"), false },
                    { new Guid("e0000030-0000-0000-0000-000000000030"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4958), null, "girls", false, null, 32, 100, 20, false, false, "serious", null, new Guid("d0000030-0000-0000-0000-000000000030"), false },
                    { new Guid("e0000031-0000-0000-0000-000000000031"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4963), null, "girls", false, null, 32, 150, 20, false, false, "both", null, new Guid("d0000031-0000-0000-0000-000000000031"), false },
                    { new Guid("e0000032-0000-0000-0000-000000000032"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4966), null, "girls", false, null, 32, 200, 20, false, false, "casual", null, new Guid("d0000032-0000-0000-0000-000000000032"), false },
                    { new Guid("e0000033-0000-0000-0000-000000000033"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4969), null, "girls", false, null, 32, 250, 20, false, false, "both", null, new Guid("d0000033-0000-0000-0000-000000000033"), false },
                    { new Guid("e0000034-0000-0000-0000-000000000034"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4972), null, "girls", false, null, 32, 300, 20, false, false, "serious", null, new Guid("d0000034-0000-0000-0000-000000000034"), false },
                    { new Guid("e0000035-0000-0000-0000-000000000035"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4979), null, "girls", false, null, 32, 100, 20, false, false, "both", null, new Guid("d0000035-0000-0000-0000-000000000035"), false },
                    { new Guid("e0000036-0000-0000-0000-000000000036"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4981), null, "girls", false, null, 32, 150, 20, false, false, "serious", null, new Guid("d0000036-0000-0000-0000-000000000036"), false },
                    { new Guid("e0000037-0000-0000-0000-000000000037"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4984), null, "girls", false, null, 32, 200, 20, false, false, "both", null, new Guid("d0000037-0000-0000-0000-000000000037"), false },
                    { new Guid("e0000038-0000-0000-0000-000000000038"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4987), null, "girls", false, null, 32, 250, 20, false, false, "casual", null, new Guid("d0000038-0000-0000-0000-000000000038"), false },
                    { new Guid("e0000039-0000-0000-0000-000000000039"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4993), null, "girls", false, null, 32, 300, 20, false, false, "both", null, new Guid("d0000039-0000-0000-0000-000000000039"), false },
                    { new Guid("e0000040-0000-0000-0000-000000000040"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(4996), null, "girls", false, null, 32, 100, 20, false, false, "serious", null, new Guid("d0000040-0000-0000-0000-000000000040"), false },
                    { new Guid("e0000041-0000-0000-0000-000000000041"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5002), null, "girls", false, null, 32, 150, 20, false, false, "both", null, new Guid("d0000041-0000-0000-0000-000000000041"), false },
                    { new Guid("e0000042-0000-0000-0000-000000000042"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5005), null, "girls", false, null, 32, 200, 20, false, false, "serious", null, new Guid("d0000042-0000-0000-0000-000000000042"), false },
                    { new Guid("e0000043-0000-0000-0000-000000000043"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5008), null, "girls", false, null, 32, 250, 20, false, false, "both", null, new Guid("d0000043-0000-0000-0000-000000000043"), false },
                    { new Guid("e0000044-0000-0000-0000-000000000044"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5010), null, "girls", false, null, 32, 300, 20, false, false, "casual", null, new Guid("d0000044-0000-0000-0000-000000000044"), false },
                    { new Guid("e0000045-0000-0000-0000-000000000045"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5013), null, "girls", false, null, 32, 100, 20, false, false, "both", null, new Guid("d0000045-0000-0000-0000-000000000045"), false },
                    { new Guid("e0000046-0000-0000-0000-000000000046"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5016), null, "girls", false, null, 32, 150, 20, false, false, "serious", null, new Guid("d0000046-0000-0000-0000-000000000046"), false },
                    { new Guid("e0000047-0000-0000-0000-000000000047"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5022), null, "girls", false, null, 32, 200, 20, false, false, "both", null, new Guid("d0000047-0000-0000-0000-000000000047"), false },
                    { new Guid("e0000048-0000-0000-0000-000000000048"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5025), null, "girls", false, null, 32, 250, 20, false, false, "serious", null, new Guid("d0000048-0000-0000-0000-000000000048"), false },
                    { new Guid("e0000049-0000-0000-0000-000000000049"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5028), null, "girls", false, null, 32, 300, 20, false, false, "both", null, new Guid("d0000049-0000-0000-0000-000000000049"), false },
                    { new Guid("e0000050-0000-0000-0000-000000000050"), new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5030), null, "girls", false, null, 32, 100, 20, false, false, "casual", null, new Guid("d0000050-0000-0000-0000-000000000050"), false }
                });

            migrationBuilder.InsertData(
                table: "UserSubscriptions",
                columns: new[] { "Id", "AutoRenew", "CancelReason", "CreatedAt", "DeletedAt", "EndDate", "GrantedBy", "IsActive", "IsDeleted", "PlanId", "StartDate", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("f1000001-0000-0000-0000-000000000001"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5611), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000002-0000-0000-0000-000000000002") },
                    { new Guid("f1000002-0000-0000-0000-000000000002"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5618), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000006-0000-0000-0000-000000000006") },
                    { new Guid("f1000003-0000-0000-0000-000000000003"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5623), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000010-0000-0000-0000-000000000010") },
                    { new Guid("f1000004-0000-0000-0000-000000000004"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5627), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000011-0000-0000-0000-000000000011") },
                    { new Guid("f1000005-0000-0000-0000-000000000005"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5675), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000015-0000-0000-0000-000000000015") },
                    { new Guid("f1000006-0000-0000-0000-000000000006"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5680), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000018-0000-0000-0000-000000000018") },
                    { new Guid("f1000007-0000-0000-0000-000000000007"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5684), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000021-0000-0000-0000-000000000021") },
                    { new Guid("f1000008-0000-0000-0000-000000000008"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5689), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000024-0000-0000-0000-000000000024") },
                    { new Guid("f1000009-0000-0000-0000-000000000009"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5696), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000026-0000-0000-0000-000000000026") },
                    { new Guid("f1000010-0000-0000-0000-000000000010"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5701), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000033-0000-0000-0000-000000000033") },
                    { new Guid("f1000011-0000-0000-0000-000000000011"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5705), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000035-0000-0000-0000-000000000035") },
                    { new Guid("f1000012-0000-0000-0000-000000000012"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5709), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000038-0000-0000-0000-000000000038") },
                    { new Guid("f1000013-0000-0000-0000-000000000013"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5716), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000040-0000-0000-0000-000000000040") },
                    { new Guid("f1000014-0000-0000-0000-000000000014"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5721), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000046-0000-0000-0000-000000000046") },
                    { new Guid("f1000015-0000-0000-0000-000000000015"), true, null, new DateTime(2026, 6, 5, 17, 4, 16, 217, DateTimeKind.Utc).AddTicks(5725), null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("b0000001-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000047-0000-0000-0000-000000000047") }
                });

            migrationBuilder.InsertData(
                table: "CallSessions",
                columns: new[] { "Id", "AnsweredAt", "CallType", "CallerId", "CoinsDeducted", "CreatedAt", "DeletedAt", "DurationSeconds", "EndReason", "EndedAt", "IsDeleted", "MatchId", "ReceiverId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("d1000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 3, 10, 0, 0, 0, DateTimeKind.Utc), "video", new Guid("d0000026-0000-0000-0000-000000000026"), 700, new DateTime(2024, 1, 3, 10, 7, 0, 0, DateTimeKind.Utc), null, 420, "user_ended", new DateTime(2024, 1, 3, 10, 7, 0, 0, DateTimeKind.Utc), false, new Guid("a1000001-0000-0000-0000-000000000001"), new Guid("d0000005-0000-0000-0000-000000000005"), "ended", null },
                    { new Guid("d1000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 3, 14, 0, 0, 0, DateTimeKind.Utc), "audio", new Guid("d0000027-0000-0000-0000-000000000027"), 50, new DateTime(2024, 1, 3, 14, 5, 0, 0, DateTimeKind.Utc), null, 300, "user_ended", new DateTime(2024, 1, 3, 14, 5, 0, 0, DateTimeKind.Utc), false, new Guid("a1000002-0000-0000-0000-000000000002"), new Guid("d0000002-0000-0000-0000-000000000002"), "ended", null },
                    { new Guid("d1000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 4, 11, 0, 0, 0, DateTimeKind.Utc), "audio", new Guid("d0000028-0000-0000-0000-000000000028"), 30, new DateTime(2024, 1, 4, 11, 3, 0, 0, DateTimeKind.Utc), null, 180, "user_ended", new DateTime(2024, 1, 4, 11, 3, 0, 0, DateTimeKind.Utc), false, new Guid("a1000003-0000-0000-0000-000000000003"), new Guid("d0000006-0000-0000-0000-000000000006"), "ended", null },
                    { new Guid("d1000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 5, 19, 0, 0, 0, DateTimeKind.Utc), "video", new Guid("d0000035-0000-0000-0000-000000000035"), 1500, new DateTime(2024, 1, 5, 19, 15, 0, 0, DateTimeKind.Utc), null, 900, "user_ended", new DateTime(2024, 1, 5, 19, 15, 0, 0, DateTimeKind.Utc), false, new Guid("a1000004-0000-0000-0000-000000000004"), new Guid("d0000011-0000-0000-0000-000000000011"), "ended", null },
                    { new Guid("d1000005-0000-0000-0000-000000000005"), null, "audio", new Guid("d0000033-0000-0000-0000-000000000033"), 0, new DateTime(2024, 1, 6, 20, 0, 0, 0, DateTimeKind.Utc), null, 0, "declined", new DateTime(2024, 1, 6, 20, 0, 0, 0, DateTimeKind.Utc), false, new Guid("a1000005-0000-0000-0000-000000000005"), new Guid("d0000015-0000-0000-0000-000000000015"), "declined", null },
                    { new Guid("d1000006-0000-0000-0000-000000000006"), new DateTime(2024, 1, 7, 18, 0, 0, 0, DateTimeKind.Utc), "audio", new Guid("d0000034-0000-0000-0000-000000000034"), 80, new DateTime(2024, 1, 7, 18, 8, 0, 0, DateTimeKind.Utc), null, 480, "user_ended", new DateTime(2024, 1, 7, 18, 8, 0, 0, DateTimeKind.Utc), false, new Guid("a1000006-0000-0000-0000-000000000006"), new Guid("d0000014-0000-0000-0000-000000000014"), "ended", null },
                    { new Guid("d1000007-0000-0000-0000-000000000007"), new DateTime(2024, 1, 8, 21, 0, 0, 0, DateTimeKind.Utc), "video", new Guid("d0000038-0000-0000-0000-000000000038"), 1200, new DateTime(2024, 1, 8, 21, 12, 0, 0, DateTimeKind.Utc), null, 720, "user_ended", new DateTime(2024, 1, 8, 21, 12, 0, 0, DateTimeKind.Utc), false, new Guid("a1000007-0000-0000-0000-000000000007"), new Guid("d0000010-0000-0000-0000-000000000010"), "ended", null },
                    { new Guid("d1000008-0000-0000-0000-000000000008"), new DateTime(2024, 1, 9, 20, 0, 0, 0, DateTimeKind.Utc), "video", new Guid("d0000040-0000-0000-0000-000000000040"), 2500, new DateTime(2024, 1, 9, 20, 25, 0, 0, DateTimeKind.Utc), null, 1500, "user_ended", new DateTime(2024, 1, 9, 20, 25, 0, 0, DateTimeKind.Utc), false, new Guid("a1000008-0000-0000-0000-000000000008"), new Guid("d0000024-0000-0000-0000-000000000024"), "ended", null },
                    { new Guid("d1000009-0000-0000-0000-000000000009"), null, "audio", new Guid("d0000047-0000-0000-0000-000000000047"), 0, new DateTime(2024, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), null, 0, "no_answer", new DateTime(2024, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), false, new Guid("a1000009-0000-0000-0000-000000000009"), new Guid("d0000025-0000-0000-0000-000000000025"), "timeout", null },
                    { new Guid("d1000010-0000-0000-0000-000000000010"), new DateTime(2024, 1, 11, 17, 0, 0, 0, DateTimeKind.Utc), "audio", new Guid("d0000046-0000-0000-0000-000000000046"), 40, new DateTime(2024, 1, 11, 17, 4, 0, 0, DateTimeKind.Utc), null, 240, "user_ended", new DateTime(2024, 1, 11, 17, 4, 0, 0, DateTimeKind.Utc), false, new Guid("a1000010-0000-0000-0000-000000000010"), new Guid("d0000021-0000-0000-0000-000000000021"), "ended", null },
                    { new Guid("d1000011-0000-0000-0000-000000000011"), new DateTime(2024, 1, 12, 20, 0, 0, 0, DateTimeKind.Utc), "video", new Guid("d0000030-0000-0000-0000-000000000030"), 600, new DateTime(2024, 1, 12, 20, 6, 0, 0, DateTimeKind.Utc), null, 360, "user_ended", new DateTime(2024, 1, 12, 20, 6, 0, 0, DateTimeKind.Utc), false, new Guid("a1000011-0000-0000-0000-000000000011"), new Guid("d0000017-0000-0000-0000-000000000017"), "ended", null },
                    { new Guid("d1000012-0000-0000-0000-000000000012"), new DateTime(2024, 1, 13, 15, 0, 0, 0, DateTimeKind.Utc), "audio", new Guid("d0000031-0000-0000-0000-000000000031"), 100, new DateTime(2024, 1, 13, 15, 10, 0, 0, DateTimeKind.Utc), null, 600, "user_ended", new DateTime(2024, 1, 13, 15, 10, 0, 0, DateTimeKind.Utc), false, new Guid("a1000012-0000-0000-0000-000000000012"), new Guid("d0000020-0000-0000-0000-000000000020"), "ended", null },
                    { new Guid("d1000013-0000-0000-0000-000000000013"), null, "audio", new Guid("d0000042-0000-0000-0000-000000000042"), 0, new DateTime(2024, 1, 14, 12, 0, 0, 0, DateTimeKind.Utc), null, 0, "cancelled", new DateTime(2024, 1, 14, 12, 0, 0, 0, DateTimeKind.Utc), false, new Guid("a1000013-0000-0000-0000-000000000013"), new Guid("d0000009-0000-0000-0000-000000000009"), "cancelled", null },
                    { new Guid("d1000014-0000-0000-0000-000000000014"), new DateTime(2024, 1, 15, 19, 0, 0, 0, DateTimeKind.Utc), "audio", new Guid("d0000041-0000-0000-0000-000000000041"), 60, new DateTime(2024, 1, 15, 19, 6, 0, 0, DateTimeKind.Utc), null, 360, "user_ended", new DateTime(2024, 1, 15, 19, 6, 0, 0, DateTimeKind.Utc), false, new Guid("a1000014-0000-0000-0000-000000000014"), new Guid("d0000008-0000-0000-0000-000000000008"), "ended", null },
                    { new Guid("d1000015-0000-0000-0000-000000000015"), new DateTime(2024, 1, 16, 21, 0, 0, 0, DateTimeKind.Utc), "video", new Guid("d0000037-0000-0000-0000-000000000037"), 1000, new DateTime(2024, 1, 16, 21, 10, 0, 0, DateTimeKind.Utc), null, 600, "user_ended", new DateTime(2024, 1, 16, 21, 10, 0, 0, DateTimeKind.Utc), false, new Guid("a1000015-0000-0000-0000-000000000015"), new Guid("d0000018-0000-0000-0000-000000000018"), "ended", null },
                    { new Guid("d1000016-0000-0000-0000-000000000016"), new DateTime(2024, 1, 17, 15, 0, 0, 0, DateTimeKind.Utc), "audio", new Guid("d0000002-0000-0000-0000-000000000002"), 70, new DateTime(2024, 1, 17, 15, 7, 0, 0, DateTimeKind.Utc), null, 420, "user_ended", new DateTime(2024, 1, 17, 15, 7, 0, 0, DateTimeKind.Utc), false, new Guid("a1000002-0000-0000-0000-000000000002"), new Guid("d0000027-0000-0000-0000-000000000027"), "ended", null },
                    { new Guid("d1000017-0000-0000-0000-000000000017"), null, "video", new Guid("d0000005-0000-0000-0000-000000000005"), 0, new DateTime(2024, 1, 18, 10, 0, 0, 0, DateTimeKind.Utc), null, 0, "declined", new DateTime(2024, 1, 18, 10, 0, 0, 0, DateTimeKind.Utc), false, new Guid("a1000001-0000-0000-0000-000000000001"), new Guid("d0000026-0000-0000-0000-000000000026"), "declined", null },
                    { new Guid("d1000018-0000-0000-0000-000000000018"), new DateTime(2024, 1, 19, 20, 0, 0, 0, DateTimeKind.Utc), "video", new Guid("d0000011-0000-0000-0000-000000000011"), 2000, new DateTime(2024, 1, 19, 20, 20, 0, 0, DateTimeKind.Utc), null, 1200, "user_ended", new DateTime(2024, 1, 19, 20, 20, 0, 0, DateTimeKind.Utc), false, new Guid("a1000004-0000-0000-0000-000000000004"), new Guid("d0000035-0000-0000-0000-000000000035"), "ended", null },
                    { new Guid("d1000019-0000-0000-0000-000000000019"), null, "audio", new Guid("d0000006-0000-0000-0000-000000000006"), 0, new DateTime(2024, 1, 20, 11, 0, 0, 0, DateTimeKind.Utc), null, 0, "no_answer", new DateTime(2024, 1, 20, 11, 0, 0, 0, DateTimeKind.Utc), false, new Guid("a1000003-0000-0000-0000-000000000003"), new Guid("d0000028-0000-0000-0000-000000000028"), "timeout", null },
                    { new Guid("d1000020-0000-0000-0000-000000000020"), new DateTime(2024, 1, 21, 19, 0, 0, 0, DateTimeKind.Utc), "video", new Guid("d0000015-0000-0000-0000-000000000015"), 3000, new DateTime(2024, 1, 21, 19, 30, 0, 0, DateTimeKind.Utc), null, 1800, "user_ended", new DateTime(2024, 1, 21, 19, 30, 0, 0, DateTimeKind.Utc), false, new Guid("a1000005-0000-0000-0000-000000000005"), new Guid("d0000033-0000-0000-0000-000000000033"), "ended", null }
                });

            migrationBuilder.InsertData(
                table: "Chats",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsDeleted", "MatchId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a2000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000001-0000-0000-0000-000000000001"), null },
                    { new Guid("a2000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000002-0000-0000-0000-000000000002"), null },
                    { new Guid("a2000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000003-0000-0000-0000-000000000003"), null },
                    { new Guid("a2000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000004-0000-0000-0000-000000000004"), null },
                    { new Guid("a2000005-0000-0000-0000-000000000005"), new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000005-0000-0000-0000-000000000005"), null },
                    { new Guid("a2000006-0000-0000-0000-000000000006"), new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000006-0000-0000-0000-000000000006"), null },
                    { new Guid("a2000007-0000-0000-0000-000000000007"), new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000007-0000-0000-0000-000000000007"), null },
                    { new Guid("a2000008-0000-0000-0000-000000000008"), new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000008-0000-0000-0000-000000000008"), null },
                    { new Guid("a2000009-0000-0000-0000-000000000009"), new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000009-0000-0000-0000-000000000009"), null },
                    { new Guid("a2000010-0000-0000-0000-000000000010"), new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000010-0000-0000-0000-000000000010"), null },
                    { new Guid("a2000011-0000-0000-0000-000000000011"), new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000011-0000-0000-0000-000000000011"), null },
                    { new Guid("a2000012-0000-0000-0000-000000000012"), new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000012-0000-0000-0000-000000000012"), null },
                    { new Guid("a2000013-0000-0000-0000-000000000013"), new DateTime(2024, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000013-0000-0000-0000-000000000013"), null },
                    { new Guid("a2000014-0000-0000-0000-000000000014"), new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000014-0000-0000-0000-000000000014"), null },
                    { new Guid("a2000015-0000-0000-0000-000000000015"), new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new Guid("a1000015-0000-0000-0000-000000000015"), null }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "ChatId", "CoinAmount", "CoinsDeducted", "CreatedAt", "DeletedAt", "GiftCost", "GiftName", "ImageUrl", "IsDeleted", "ReadAt", "ReplyToMessageId", "SenderId", "Text", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("c1000001-0000-0000-0000-000000000001"), new Guid("a2000001-0000-0000-0000-000000000001"), null, 10, new DateTime(2024, 1, 2, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 2, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000026-0000-0000-0000-000000000026"), "Hi Aisha! Saw your art profile — absolutely stunning 🎨", "text", null },
                    { new Guid("c1000002-0000-0000-0000-000000000002"), new Guid("a2000001-0000-0000-0000-000000000001"), null, 0, new DateTime(2024, 1, 2, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 2, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000005-0000-0000-0000-000000000005"), "Wow thank you so much! Your photography is incredible too 📸", "text", null },
                    { new Guid("c1000003-0000-0000-0000-000000000003"), new Guid("a2000001-0000-0000-0000-000000000001"), null, 10, new DateTime(2024, 1, 2, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 2, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000026-0000-0000-0000-000000000026"), "Would love to know more about your art style! Do you paint?", "text", null },
                    { new Guid("c1000004-0000-0000-0000-000000000004"), new Guid("a2000001-0000-0000-0000-000000000001"), null, 0, new DateTime(2024, 1, 2, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 2, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000005-0000-0000-0000-000000000005"), "Yes! Mostly abstract & watercolors 🌊 What's your favorite subject to photograph?", "text", null },
                    { new Guid("c1000005-0000-0000-0000-000000000005"), new Guid("a2000001-0000-0000-0000-000000000001"), null, 10, new DateTime(2024, 1, 2, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 2, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000026-0000-0000-0000-000000000026"), "Landscapes and golden hour portraits 🌅 You'd be a great subject btw 😊", "text", null },
                    { new Guid("c1000006-0000-0000-0000-000000000006"), new Guid("a2000001-0000-0000-0000-000000000001"), null, 0, new DateTime(2024, 1, 2, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000005-0000-0000-0000-000000000005"), "Haha smooth! ☺️ I'd love to see your portfolio sometime", "text", null },
                    { new Guid("c1000007-0000-0000-0000-000000000007"), new Guid("a2000001-0000-0000-0000-000000000001"), null, 10, new DateTime(2024, 1, 2, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000026-0000-0000-0000-000000000026"), "Deal! Coffee date + portfolio exchange? ☕", "text", null },
                    { new Guid("c1000008-0000-0000-0000-000000000008"), new Guid("a2000001-0000-0000-0000-000000000001"), null, 0, new DateTime(2024, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000005-0000-0000-0000-000000000005"), "Perfect plan! Hyderabad has some amazing cafés 🌸", "text", null },
                    { new Guid("c1000009-0000-0000-0000-000000000009"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 10, new DateTime(2024, 1, 3, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 3, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000027-0000-0000-0000-000000000027"), "Hey Priya! We matched 🎉 Music lover here — you dance, I play guitar 🎸", "text", null },
                    { new Guid("c1000010-0000-0000-0000-000000000010"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 0, new DateTime(2024, 1, 3, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 3, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000002-0000-0000-0000-000000000002"), "Hi Rahul! Oh wow guitar + dance = perfect collab! 💃🎵", "text", null },
                    { new Guid("c1000011-0000-0000-0000-000000000011"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 10, new DateTime(2024, 1, 3, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 3, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000027-0000-0000-0000-000000000027"), "Exactly what I was thinking! Hindustani or Western dance?", "text", null },
                    { new Guid("c1000012-0000-0000-0000-000000000012"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 0, new DateTime(2024, 1, 3, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 3, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000002-0000-0000-0000-000000000002"), "Kathak ❤️ 15 years of practice! What genres do you play?", "text", null },
                    { new Guid("c1000013-0000-0000-0000-000000000013"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 10, new DateTime(2024, 1, 3, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 3, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000027-0000-0000-0000-000000000027"), "Blues & Indie mostly. Would love to see Kathak live someday!", "text", null },
                    { new Guid("c1000014-0000-0000-0000-000000000014"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 0, new DateTime(2024, 1, 3, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 3, 7, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000002-0000-0000-0000-000000000002"), "Come to my next recital 😊 Delhi Habitat Centre next month", "text", null },
                    { new Guid("c1000015-0000-0000-0000-000000000015"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 10, new DateTime(2024, 1, 3, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 3, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000027-0000-0000-0000-000000000027"), "Absolutely! Also — chai date before that? ☕", "text", null },
                    { new Guid("c1000016-0000-0000-0000-000000000016"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 0, new DateTime(2024, 1, 3, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000002-0000-0000-0000-000000000002"), "I thought you'd never ask! Yes please 🥰", "text", null },
                    { new Guid("c1000017-0000-0000-0000-000000000017"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 10, new DateTime(2024, 1, 3, 9, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000027-0000-0000-0000-000000000027"), "Saturday 3pm? Connaught Place has this tiny amazing chai spot", "text", null },
                    { new Guid("c1000018-0000-0000-0000-000000000018"), new Guid("a2000002-0000-0000-0000-000000000002"), null, 0, new DateTime(2024, 1, 3, 10, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000002-0000-0000-0000-000000000002"), "It's a date! 💕", "text", null },
                    { new Guid("c1000019-0000-0000-0000-000000000019"), new Guid("a2000003-0000-0000-0000-000000000003"), null, 10, new DateTime(2024, 1, 4, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 4, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000028-0000-0000-0000-000000000028"), "Hey Shreya! Entrepreneur meets Doctor — now that's a power match ⚡", "text", null },
                    { new Guid("c1000020-0000-0000-0000-000000000020"), new Guid("a2000003-0000-0000-0000-000000000003"), null, 0, new DateTime(2024, 1, 4, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 4, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000006-0000-0000-0000-000000000006"), "Haha love the energy! What kind of startup? 🚀", "text", null },
                    { new Guid("c1000021-0000-0000-0000-000000000021"), new Guid("a2000003-0000-0000-0000-000000000003"), null, 10, new DateTime(2024, 1, 4, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 4, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000028-0000-0000-0000-000000000028"), "EdTech — making quality education accessible across India 📚", "text", null },
                    { new Guid("c1000022-0000-0000-0000-000000000022"), new Guid("a2000003-0000-0000-0000-000000000003"), null, 0, new DateTime(2024, 1, 4, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 4, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000006-0000-0000-0000-000000000006"), "That's genuinely amazing. I'm passionate about healthcare access too 🏥", "text", null },
                    { new Guid("c1000023-0000-0000-0000-000000000023"), new Guid("a2000003-0000-0000-0000-000000000003"), null, 10, new DateTime(2024, 1, 4, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 4, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000028-0000-0000-0000-000000000028"), "We should talk! Combining EdTech + HealthTech could be huge", "text", null },
                    { new Guid("c1000024-0000-0000-0000-000000000024"), new Guid("a2000003-0000-0000-0000-000000000003"), null, 0, new DateTime(2024, 1, 4, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000006-0000-0000-0000-000000000006"), "You had me at 'making impact' 😄 Tell me more over chai?", "text", null },
                    { new Guid("c1000025-0000-0000-0000-000000000025"), new Guid("a2000003-0000-0000-0000-000000000003"), null, 10, new DateTime(2024, 1, 4, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000028-0000-0000-0000-000000000028"), "Ahmedabad or Delhi? I travel between both", "text", null },
                    { new Guid("c1000026-0000-0000-0000-000000000026"), new Guid("a2000003-0000-0000-0000-000000000003"), null, 0, new DateTime(2024, 1, 4, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000006-0000-0000-0000-000000000006"), "Ahmedabad this weekend works! I know the perfect spot 🌸", "text", null },
                    { new Guid("c1000027-0000-0000-0000-000000000027"), new Guid("a2000004-0000-0000-0000-000000000004"), null, 10, new DateTime(2024, 1, 5, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 5, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000035-0000-0000-0000-000000000035"), "Simran Kaur — lawyer + dancer = most dangerous combo I've ever swiped right on 😄", "text", null },
                    { new Guid("c1000028-0000-0000-0000-000000000028"), new Guid("a2000004-0000-0000-0000-000000000004"), null, 0, new DateTime(2024, 1, 5, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 5, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000011-0000-0000-0000-000000000011"), "Haha I plead guilty 😂 Investment banker + world traveller — not bad either 😏", "text", null },
                    { new Guid("c1000029-0000-0000-0000-000000000029"), new Guid("a2000004-0000-0000-0000-000000000004"), null, 10, new DateTime(2024, 1, 5, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 5, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000035-0000-0000-0000-000000000035"), "42 countries down, still searching for the best chai 🍵", "text", null },
                    { new Guid("c1000030-0000-0000-0000-000000000030"), new Guid("a2000004-0000-0000-0000-000000000004"), null, 0, new DateTime(2024, 1, 5, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 5, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000011-0000-0000-0000-000000000011"), "THE AUDACITY — Amritsar has the world's best chai and you know it 😤", "text", null },
                    { new Guid("c1000031-0000-0000-0000-000000000031"), new Guid("a2000004-0000-0000-0000-000000000004"), null, 10, new DateTime(2024, 1, 5, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 5, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000035-0000-0000-0000-000000000035"), "Prove it. I'll fly in this weekend 😄", "text", null },
                    { new Guid("c1000032-0000-0000-0000-000000000032"), new Guid("a2000004-0000-0000-0000-000000000004"), null, 0, new DateTime(2024, 1, 5, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000011-0000-0000-0000-000000000011"), "Challenge accepted. Golden Temple at sunrise first, then chai 🌅", "text", null },
                    { new Guid("c1000033-0000-0000-0000-000000000033"), new Guid("a2000004-0000-0000-0000-000000000004"), null, 10, new DateTime(2024, 1, 5, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000035-0000-0000-0000-000000000035"), "You just planned the best morning of my life", "text", null },
                    { new Guid("c1000034-0000-0000-0000-000000000034"), new Guid("a2000004-0000-0000-0000-000000000004"), null, 0, new DateTime(2024, 1, 5, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000011-0000-0000-0000-000000000011"), "Wait till you see the Langar 🙏 Life-changing", "text", null },
                    { new Guid("c1000035-0000-0000-0000-000000000035"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 10, new DateTime(2024, 1, 6, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 6, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000033-0000-0000-0000-000000000033"), "Ishita! TEDx speaker + startup founder = most impressive profile I've seen 🙌", "text", null },
                    { new Guid("c1000036-0000-0000-0000-000000000036"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 0, new DateTime(2024, 1, 6, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 6, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000015-0000-0000-0000-000000000015"), "An architect who collects art? I need to see your apartment 😄", "text", null },
                    { new Guid("c1000037-0000-0000-0000-000000000037"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 10, new DateTime(2024, 1, 6, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 6, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000033-0000-0000-0000-000000000033"), "Fair warning — it's half gallery half home 🎨🏛️", "text", null },
                    { new Guid("c1000038-0000-0000-0000-000000000038"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 0, new DateTime(2024, 1, 6, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 6, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000015-0000-0000-0000-000000000015"), "I'm already in love with it 😍 What's your most prized piece?", "text", null },
                    { new Guid("c1000039-0000-0000-0000-000000000039"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 10, new DateTime(2024, 1, 6, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 6, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000033-0000-0000-0000-000000000033"), "A Hussain original. Took me 3 years to save for it", "text", null },
                    { new Guid("c1000040-0000-0000-0000-000000000040"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 0, new DateTime(2024, 1, 6, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 6, 7, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000015-0000-0000-0000-000000000015"), "MF Hussain?! That's incredible. I spoke about his legacy at TEDx!", "text", null },
                    { new Guid("c1000041-0000-0000-0000-000000000041"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 10, new DateTime(2024, 1, 6, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 6, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000033-0000-0000-0000-000000000033"), "No way! Which TED? I might have watched that talk!", "text", null },
                    { new Guid("c1000042-0000-0000-0000-000000000042"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 0, new DateTime(2024, 1, 6, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000015-0000-0000-0000-000000000015"), "TEDxDelhi 2023 — Art as a mirror of society", "text", null },
                    { new Guid("c1000043-0000-0000-0000-000000000043"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 10, new DateTime(2024, 1, 6, 9, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000033-0000-0000-0000-000000000033"), "I DID watch that! The part about street art was phenomenal", "text", null },
                    { new Guid("c1000044-0000-0000-0000-000000000044"), new Guid("a2000005-0000-0000-0000-000000000005"), null, 0, new DateTime(2024, 1, 6, 10, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000015-0000-0000-0000-000000000015"), "Oh my god! Coffee. Now. We have so much to talk about ☕", "text", null },
                    { new Guid("c1000045-0000-0000-0000-000000000045"), new Guid("a2000006-0000-0000-0000-000000000006"), null, 10, new DateTime(2024, 1, 7, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 7, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000034-0000-0000-0000-000000000034"), "Hey Tanvi! Designer + plant mom = my two favorite things 🌿🎨", "text", null },
                    { new Guid("c1000046-0000-0000-0000-000000000046"), new Guid("a2000006-0000-0000-0000-000000000006"), null, 0, new DateTime(2024, 1, 7, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 7, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000014-0000-0000-0000-000000000014"), "A data scientist who rides? Bold combo 🏍️📊", "text", null },
                    { new Guid("c1000047-0000-0000-0000-000000000047"), new Guid("a2000006-0000-0000-0000-000000000006"), null, 10, new DateTime(2024, 1, 7, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 7, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000034-0000-0000-0000-000000000034"), "Numbers by day, open roads by night 🌙 Any design philosophy you live by?", "text", null },
                    { new Guid("c1000048-0000-0000-0000-000000000048"), new Guid("a2000006-0000-0000-0000-000000000006"), null, 0, new DateTime(2024, 1, 7, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 7, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000014-0000-0000-0000-000000000014"), "'Good design is invisible.' — You feel it without thinking about it", "text", null },
                    { new Guid("c1000049-0000-0000-0000-000000000049"), new Guid("a2000006-0000-0000-0000-000000000006"), null, 10, new DateTime(2024, 1, 7, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 7, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000034-0000-0000-0000-000000000034"), "That's exactly how data stories should work — you get it without effort", "text", null },
                    { new Guid("c1000050-0000-0000-0000-000000000050"), new Guid("a2000006-0000-0000-0000-000000000006"), null, 0, new DateTime(2024, 1, 7, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000014-0000-0000-0000-000000000014"), "We're the same kind of nerd 😄 I like you", "text", null },
                    { new Guid("c1000051-0000-0000-0000-000000000051"), new Guid("a2000006-0000-0000-0000-000000000006"), null, 10, new DateTime(2024, 1, 7, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000034-0000-0000-0000-000000000034"), "Pune roads on a bike this Sunday? I'll show you the ghats", "text", null },
                    { new Guid("c1000052-0000-0000-0000-000000000052"), new Guid("a2000006-0000-0000-0000-000000000006"), null, 0, new DateTime(2024, 1, 7, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000014-0000-0000-0000-000000000014"), "YESSS! I've been wanting to do the Sinhagad trail 🌄", "text", null },
                    { new Guid("c1000053-0000-0000-0000-000000000053"), new Guid("a2000007-0000-0000-0000-000000000007"), null, 10, new DateTime(2024, 1, 8, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 8, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000038-0000-0000-0000-000000000038"), "Rita! Finance + Yoga — the most balanced person alive 😄", "text", null },
                    { new Guid("c1000054-0000-0000-0000-000000000054"), new Guid("a2000007-0000-0000-0000-000000000007"), null, 0, new DateTime(2024, 1, 8, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 8, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000010-0000-0000-0000-000000000010"), "Haha healing bodies through yoga and portfolios through finance 😂", "text", null },
                    { new Guid("c1000055-0000-0000-0000-000000000055"), new Guid("a2000007-0000-0000-0000-000000000007"), null, 10, new DateTime(2024, 1, 8, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 8, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000038-0000-0000-0000-000000000038"), "I'm an ortho surgeon — we're both in the fixing business!", "text", null },
                    { new Guid("c1000056-0000-0000-0000-000000000056"), new Guid("a2000007-0000-0000-0000-000000000007"), null, 0, new DateTime(2024, 1, 8, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 8, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000010-0000-0000-0000-000000000010"), "Ha! True! Though I suspect you enjoy the dramatic before-after more 😄", "text", null },
                    { new Guid("c1000057-0000-0000-0000-000000000057"), new Guid("a2000007-0000-0000-0000-000000000007"), null, 10, new DateTime(2024, 1, 8, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 8, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000038-0000-0000-0000-000000000038"), "Guilty 😂 How's Surat treating you?", "text", null },
                    { new Guid("c1000058-0000-0000-0000-000000000058"), new Guid("a2000007-0000-0000-0000-000000000007"), null, 0, new DateTime(2024, 1, 8, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000010-0000-0000-0000-000000000010"), "Wonderful! Diamond city has hidden gems. You should visit", "text", null },
                    { new Guid("c1000059-0000-0000-0000-000000000059"), new Guid("a2000007-0000-0000-0000-000000000007"), null, 10, new DateTime(2024, 1, 8, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000038-0000-0000-0000-000000000038"), "Book me a yoga class and I'll bring the chai ☕", "text", null },
                    { new Guid("c1000060-0000-0000-0000-000000000060"), new Guid("a2000007-0000-0000-0000-000000000007"), null, 0, new DateTime(2024, 1, 8, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000010-0000-0000-0000-000000000010"), "Deal! Sunrise session? My rooftop shala has the best views 🌅", "text", null },
                    { new Guid("c1000061-0000-0000-0000-000000000061"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 10, new DateTime(2024, 1, 9, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 9, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000040-0000-0000-0000-000000000040"), "Tara! Neurosurgeon + Bharatanatyam dancer — I'm genuinely in awe 🧠💃", "text", null },
                    { new Guid("c1000062-0000-0000-0000-000000000062"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 0, new DateTime(2024, 1, 9, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 9, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000024-0000-0000-0000-000000000024"), "Tech founder + angel investor — not exactly a slouch yourself 😄", "text", null },
                    { new Guid("c1000063-0000-0000-0000-000000000063"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 10, new DateTime(2024, 1, 9, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 9, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000040-0000-0000-0000-000000000040"), "Haha flattery noted. How do you balance neurosurgery and dance?", "text", null },
                    { new Guid("c1000064-0000-0000-0000-000000000064"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 0, new DateTime(2024, 1, 9, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 9, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000024-0000-0000-0000-000000000024"), "Discipline. Both require absolute precision and full presence 🙏", "text", null },
                    { new Guid("c1000065-0000-0000-0000-000000000065"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 10, new DateTime(2024, 1, 9, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 9, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000040-0000-0000-0000-000000000040"), "That's profound. Do you see similarities in the movements?", "text", null },
                    { new Guid("c1000066-0000-0000-0000-000000000066"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 0, new DateTime(2024, 1, 9, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 9, 7, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000024-0000-0000-0000-000000000024"), "Interesting you ask — I wrote a paper on neuroplasticity and classical dance", "text", null },
                    { new Guid("c1000067-0000-0000-0000-000000000067"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 10, new DateTime(2024, 1, 9, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 9, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000040-0000-0000-0000-000000000040"), "I would genuinely love to read that", "text", null },
                    { new Guid("c1000068-0000-0000-0000-000000000068"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 0, new DateTime(2024, 1, 9, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000024-0000-0000-0000-000000000024"), "I'll email it if you promise to actually read it 😄", "text", null },
                    { new Guid("c1000069-0000-0000-0000-000000000069"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 10, new DateTime(2024, 1, 9, 9, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000040-0000-0000-0000-000000000040"), "Scout's honour. Chennai this month? I have a board meeting there", "text", null },
                    { new Guid("c1000070-0000-0000-0000-000000000070"), new Guid("a2000008-0000-0000-0000-000000000008"), null, 0, new DateTime(2024, 1, 9, 10, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000024-0000-0000-0000-000000000024"), "Perfect timing! My next recital is the 25th 💃", "text", null },
                    { new Guid("c1000071-0000-0000-0000-000000000071"), new Guid("a2000009-0000-0000-0000-000000000009"), null, 10, new DateTime(2024, 1, 10, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 10, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000047-0000-0000-0000-000000000047"), "Nalini! PM at BigTech + traveller — someone who builds AND explores 🚀", "text", null },
                    { new Guid("c1000072-0000-0000-0000-000000000072"), new Guid("a2000009-0000-0000-0000-000000000009"), null, 0, new DateTime(2024, 1, 10, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 10, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000025-0000-0000-0000-000000000025"), "Retired army officer turned mountaineer — the most disciplined swipe I've gotten 😄", "text", null },
                    { new Guid("c1000073-0000-0000-0000-000000000073"), new Guid("a2000009-0000-0000-0000-000000000009"), null, 10, new DateTime(2024, 1, 10, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 10, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000047-0000-0000-0000-000000000047"), "15 years of service teaches you that mountains and deadlines are equally unforgiving 😂", "text", null },
                    { new Guid("c1000074-0000-0000-0000-000000000074"), new Guid("a2000009-0000-0000-0000-000000000009"), null, 0, new DateTime(2024, 1, 10, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 10, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000025-0000-0000-0000-000000000025"), "Haha that's the most army thing I've heard today! Which peaks?", "text", null },
                    { new Guid("c1000075-0000-0000-0000-000000000075"), new Guid("a2000009-0000-0000-0000-000000000009"), null, 10, new DateTime(2024, 1, 10, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 10, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000047-0000-0000-0000-000000000047"), "Stok Kangri, Friendship Peak, Kang Yatze. Everest BC is next 🏔️", "text", null },
                    { new Guid("c1000076-0000-0000-0000-000000000076"), new Guid("a2000009-0000-0000-0000-000000000009"), null, 0, new DateTime(2024, 1, 10, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000025-0000-0000-0000-000000000025"), "BASE CAMP?! I've always wanted to do EBC! Have you done Roopkund?", "text", null },
                    { new Guid("c1000077-0000-0000-0000-000000000077"), new Guid("a2000009-0000-0000-0000-000000000009"), null, 10, new DateTime(2024, 1, 10, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000047-0000-0000-0000-000000000047"), "3 times. Let me know when you want company 💪", "text", null },
                    { new Guid("c1000078-0000-0000-0000-000000000078"), new Guid("a2000009-0000-0000-0000-000000000009"), null, 0, new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000025-0000-0000-0000-000000000025"), "Is this the most epic 'let's hang out' I've ever received? Yes 😄", "text", null },
                    { new Guid("c1000079-0000-0000-0000-000000000079"), new Guid("a2000010-0000-0000-0000-000000000010"), null, 10, new DateTime(2024, 1, 11, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 11, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000046-0000-0000-0000-000000000046"), "Sonal! IB + marathoner — making money AND miles 😄", "text", null },
                    { new Guid("c1000080-0000-0000-0000-000000000080"), new Guid("a2000010-0000-0000-0000-000000000010"), null, 0, new DateTime(2024, 1, 11, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 11, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000021-0000-0000-0000-000000000021"), "Real estate mogul + gym addict — building empires inside and out 😂", "text", null },
                    { new Guid("c1000081-0000-0000-0000-000000000081"), new Guid("a2000010-0000-0000-0000-000000000010"), null, 10, new DateTime(2024, 1, 11, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 11, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000046-0000-0000-0000-000000000046"), "Mumbai vs Ahmedabad — the eternal rivalry continues 😄", "text", null },
                    { new Guid("c1000082-0000-0000-0000-000000000082"), new Guid("a2000010-0000-0000-0000-000000000010"), null, 0, new DateTime(2024, 1, 11, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 11, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000021-0000-0000-0000-000000000021"), "No contest — Mumbai's pace, Ahmedabad's food 🍛", "text", null },
                    { new Guid("c1000083-0000-0000-0000-000000000083"), new Guid("a2000010-0000-0000-0000-000000000010"), null, 10, new DateTime(2024, 1, 11, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 11, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000046-0000-0000-0000-000000000046"), "I'll accept that compromise. What's your marathon PR?", "text", null },
                    { new Guid("c1000084-0000-0000-0000-000000000084"), new Guid("a2000010-0000-0000-0000-000000000010"), null, 0, new DateTime(2024, 1, 11, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000021-0000-0000-0000-000000000021"), "3:42 at Mumbai Marathon 2024! You?", "text", null },
                    { new Guid("c1000085-0000-0000-0000-000000000085"), new Guid("a2000010-0000-0000-0000-000000000010"), null, 10, new DateTime(2024, 1, 11, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000046-0000-0000-0000-000000000046"), "4:01. You'd destroy me 😂 Train me?", "text", null },
                    { new Guid("c1000086-0000-0000-0000-000000000086"), new Guid("a2000010-0000-0000-0000-000000000010"), null, 0, new DateTime(2024, 1, 11, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000021-0000-0000-0000-000000000021"), "Only if you show me Ahmedabad's best properties in return 🏠", "text", null },
                    { new Guid("c1000087-0000-0000-0000-000000000087"), new Guid("a2000011-0000-0000-0000-000000000011"), null, 10, new DateTime(2024, 1, 12, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 12, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000030-0000-0000-0000-000000000030"), "Zara! A model who loves photography — do you ever end up on both sides of the lens?", "text", null },
                    { new Guid("c1000088-0000-0000-0000-000000000088"), new Guid("a2000011-0000-0000-0000-000000000011"), null, 0, new DateTime(2024, 1, 12, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 12, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000017-0000-0000-0000-000000000017"), "ALL the time 😄 Chef who food-blogs — tell me you make restaurant-quality food at home!", "text", null },
                    { new Guid("c1000089-0000-0000-0000-000000000089"), new Guid("a2000011-0000-0000-0000-000000000011"), null, 10, new DateTime(2024, 1, 12, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 12, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000030-0000-0000-0000-000000000030"), "Michelin-star aspirations, home kitchen reality 😂 I'll cook for you anytime", "text", null },
                    { new Guid("c1000090-0000-0000-0000-000000000090"), new Guid("a2000011-0000-0000-0000-000000000011"), null, 0, new DateTime(2024, 1, 12, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 12, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000017-0000-0000-0000-000000000017"), "This is the best offer I've received on this app 🍽️", "text", null },
                    { new Guid("c1000091-0000-0000-0000-000000000091"), new Guid("a2000011-0000-0000-0000-000000000011"), null, 10, new DateTime(2024, 1, 12, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 12, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000030-0000-0000-0000-000000000030"), "What's your favorite cuisine? I'll recreate it", "text", null },
                    { new Guid("c1000092-0000-0000-0000-000000000092"), new Guid("a2000011-0000-0000-0000-000000000011"), null, 0, new DateTime(2024, 1, 12, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000017-0000-0000-0000-000000000017"), "Japanese! I'm obsessed with omakase 🍣", "text", null },
                    { new Guid("c1000093-0000-0000-0000-000000000093"), new Guid("a2000011-0000-0000-0000-000000000011"), null, 10, new DateTime(2024, 1, 12, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000030-0000-0000-0000-000000000030"), "Challenge accepted. Saturday? I'll do a 7-course omakase at home", "text", null },
                    { new Guid("c1000094-0000-0000-0000-000000000094"), new Guid("a2000011-0000-0000-0000-000000000011"), null, 0, new DateTime(2024, 1, 12, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000017-0000-0000-0000-000000000017"), "You're either insane or incredible. Either way, I'm in 🙌", "text", null },
                    { new Guid("c1000095-0000-0000-0000-000000000095"), new Guid("a2000012-0000-0000-0000-000000000012"), null, 10, new DateTime(2024, 1, 13, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 13, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000031-0000-0000-0000-000000000031"), "Ananya! Wildlife photographer — the patience that must require! 🦁", "text", null },
                    { new Guid("c1000096-0000-0000-0000-000000000096"), new Guid("a2000012-0000-0000-0000-000000000012"), null, 0, new DateTime(2024, 1, 13, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 13, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000020-0000-0000-0000-000000000020"), "IIT Madras + startup founder — you built something from scratch, I know patience 😄", "text", null },
                    { new Guid("c1000097-0000-0000-0000-000000000097"), new Guid("a2000012-0000-0000-0000-000000000012"), null, 10, new DateTime(2024, 1, 13, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 13, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000031-0000-0000-0000-000000000031"), "Haha fair point! Have you done the Sundarbans?", "text", null },
                    { new Guid("c1000098-0000-0000-0000-000000000098"), new Guid("a2000012-0000-0000-0000-000000000012"), null, 0, new DateTime(2024, 1, 13, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 13, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000020-0000-0000-0000-000000000020"), "3 times! The Bengal tigers are unreal 🐯 Have you?", "text", null },
                    { new Guid("c1000099-0000-0000-0000-000000000099"), new Guid("a2000012-0000-0000-0000-000000000012"), null, 10, new DateTime(2024, 1, 13, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 13, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000031-0000-0000-0000-000000000031"), "Never! That needs to change. Would you guide me?", "text", null },
                    { new Guid("c1000100-0000-0000-0000-000000000100"), new Guid("a2000012-0000-0000-0000-000000000012"), null, 0, new DateTime(2024, 1, 13, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000020-0000-0000-0000-000000000020"), "Only if you tell me what startup you built 😄", "text", null },
                    { new Guid("c1000101-0000-0000-0000-000000000101"), new Guid("a2000012-0000-0000-0000-000000000012"), null, 10, new DateTime(2024, 1, 13, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000031-0000-0000-0000-000000000031"), "EdTech platform — 2M students now. But tigers > everything 🐯", "text", null },
                    { new Guid("c1000102-0000-0000-0000-000000000102"), new Guid("a2000012-0000-0000-0000-000000000012"), null, 0, new DateTime(2024, 1, 13, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000020-0000-0000-0000-000000000020"), "2 MILLION?! Okay you're impressive. Sundarbans next month, deal? 🤝", "text", null },
                    { new Guid("c1000103-0000-0000-0000-000000000103"), new Guid("a2000013-0000-0000-0000-000000000013"), null, 10, new DateTime(2024, 1, 14, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 14, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000042-0000-0000-0000-000000000042"), "Kritika! Journalist + filmmaker — you're literally a storytelling machine 📽️", "text", null },
                    { new Guid("c1000104-0000-0000-0000-000000000104"), new Guid("a2000013-0000-0000-0000-000000000013"), null, 0, new DateTime(2024, 1, 14, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 14, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000009-0000-0000-0000-000000000009"), "A documentary filmmaker who shoots street photography — kindred spirit! 📸", "text", null },
                    { new Guid("c1000105-0000-0000-0000-000000000105"), new Guid("a2000013-0000-0000-0000-000000000013"), null, 10, new DateTime(2024, 1, 14, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 14, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000042-0000-0000-0000-000000000042"), "What's your beat? I cover social issues mostly", "text", null },
                    { new Guid("c1000106-0000-0000-0000-000000000106"), new Guid("a2000013-0000-0000-0000-000000000013"), null, 0, new DateTime(2024, 1, 14, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 14, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000009-0000-0000-0000-000000000009"), "Human interest stories. The invisible lives of visible cities", "text", null },
                    { new Guid("c1000107-0000-0000-0000-000000000107"), new Guid("a2000013-0000-0000-0000-000000000013"), null, 10, new DateTime(2024, 1, 14, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 14, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000042-0000-0000-0000-000000000042"), "That's EXACTLY what I film. Have you covered Dharavi?", "text", null },
                    { new Guid("c1000108-0000-0000-0000-000000000108"), new Guid("a2000013-0000-0000-0000-000000000013"), null, 0, new DateTime(2024, 1, 14, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000009-0000-0000-0000-000000000009"), "It was my first published piece 5 years ago! Changed everything for me", "text", null },
                    { new Guid("c1000109-0000-0000-0000-000000000109"), new Guid("a2000013-0000-0000-0000-000000000013"), null, 10, new DateTime(2024, 1, 14, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000042-0000-0000-0000-000000000042"), "Mine too — that's wild! Kolkata coffee + story exchange?", "text", null },
                    { new Guid("c1000110-0000-0000-0000-000000000110"), new Guid("a2000013-0000-0000-0000-000000000013"), null, 0, new DateTime(2024, 1, 14, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000009-0000-0000-0000-000000000009"), "Blue Poppy Café, Saturday 4pm. Don't be late 😄", "text", null },
                    { new Guid("c1000111-0000-0000-0000-000000000111"), new Guid("a2000014-0000-0000-0000-000000000014"), null, 10, new DateTime(2024, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 15, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000041-0000-0000-0000-000000000041"), "Pooja! Marketing + bookworm — the rarest, most underrated combo 📚", "text", null },
                    { new Guid("c1000112-0000-0000-0000-000000000112"), new Guid("a2000014-0000-0000-0000-000000000014"), null, 0, new DateTime(2024, 1, 15, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 15, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000008-0000-0000-0000-000000000008"), "CA + cricketer — numbers AND wickets?! Respect 🏏", "text", null },
                    { new Guid("c1000113-0000-0000-0000-000000000113"), new Guid("a2000014-0000-0000-0000-000000000014"), null, 10, new DateTime(2024, 1, 15, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 15, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000041-0000-0000-0000-000000000041"), "I'm better at accounts than cricket tbh 😂 What are you reading?", "text", null },
                    { new Guid("c1000114-0000-0000-0000-000000000114"), new Guid("a2000014-0000-0000-0000-000000000014"), null, 0, new DateTime(2024, 1, 15, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 15, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000008-0000-0000-0000-000000000008"), "'Atomic Habits' for the 3rd time 📖 Each read hits different", "text", null },
                    { new Guid("c1000115-0000-0000-0000-000000000115"), new Guid("a2000014-0000-0000-0000-000000000014"), null, 10, new DateTime(2024, 1, 15, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 15, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000041-0000-0000-0000-000000000041"), "That book genuinely changed my practice. The 1% rule is everything", "text", null },
                    { new Guid("c1000116-0000-0000-0000-000000000116"), new Guid("a2000014-0000-0000-0000-000000000014"), null, 0, new DateTime(2024, 1, 15, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000008-0000-0000-0000-000000000008"), "YES! I applied it to my morning routine and productivity tripled", "text", null },
                    { new Guid("c1000117-0000-0000-0000-000000000117"), new Guid("a2000014-0000-0000-0000-000000000014"), null, 10, new DateTime(2024, 1, 15, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000041-0000-0000-0000-000000000041"), "We need to do a book club. Jaipur has amazing bookshops", "text", null },
                    { new Guid("c1000118-0000-0000-0000-000000000118"), new Guid("a2000014-0000-0000-0000-000000000014"), null, 0, new DateTime(2024, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000008-0000-0000-0000-000000000008"), "Anokhi Café has a reading corner — next Sunday? 🌸", "text", null },
                    { new Guid("c1000119-0000-0000-0000-000000000119"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 10, new DateTime(2024, 1, 16, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 16, 2, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000037-0000-0000-0000-000000000037"), "Naina! Cardiologist + marathoner — literally the healthiest person alive 🏃❤️", "text", null },
                    { new Guid("c1000120-0000-0000-0000-000000000120"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 0, new DateTime(2024, 1, 16, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 16, 3, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000018-0000-0000-0000-000000000018"), "Pilot + astronomer — you're both above the clouds AND studying them 😄", "text", null },
                    { new Guid("c1000121-0000-0000-0000-000000000121"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 10, new DateTime(2024, 1, 16, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 16, 4, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000037-0000-0000-0000-000000000037"), "Haha occupational perk! Have you ever done cardiac surgery at altitude?", "text", null },
                    { new Guid("c1000122-0000-0000-0000-000000000122"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 0, new DateTime(2024, 1, 16, 4, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 16, 5, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000018-0000-0000-0000-000000000018"), "Not yet but HAPE is a real risk I've studied for high-altitude treks 🏔️", "text", null },
                    { new Guid("c1000123-0000-0000-0000-000000000123"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 10, new DateTime(2024, 1, 16, 5, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 16, 6, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000037-0000-0000-0000-000000000037"), "I have a patient who did Everest after a stent — most inspiring story", "text", null },
                    { new Guid("c1000124-0000-0000-0000-000000000124"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 0, new DateTime(2024, 1, 16, 6, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 16, 7, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000018-0000-0000-0000-000000000018"), "That's incredible. Medicine has the best stories", "text", null },
                    { new Guid("c1000125-0000-0000-0000-000000000125"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 10, new DateTime(2024, 1, 16, 7, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, new DateTime(2024, 1, 16, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d0000037-0000-0000-0000-000000000037"), "Agreed. Cockpit date? I can show you Delhi from 1000ft AGL 🌃", "text", null },
                    { new Guid("c1000126-0000-0000-0000-000000000126"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 0, new DateTime(2024, 1, 16, 8, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000018-0000-0000-0000-000000000018"), "Is that even legal? 😂", "text", null },
                    { new Guid("c1000127-0000-0000-0000-000000000127"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 10, new DateTime(2024, 1, 16, 9, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000037-0000-0000-0000-000000000037"), "Simulator only 😄 But the view is just as good", "text", null },
                    { new Guid("c1000128-0000-0000-0000-000000000128"), new Guid("a2000015-0000-0000-0000-000000000015"), null, 0, new DateTime(2024, 1, 16, 10, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, null, new Guid("d0000018-0000-0000-0000-000000000018"), "You had me at cockpit 😄 Yes please!", "text", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockedUserId",
                table: "Blocks",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockerId_BlockedUserId",
                table: "Blocks",
                columns: new[] { "BlockerId", "BlockedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_CallerId",
                table: "CallSessions",
                column: "CallerId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_MatchId",
                table: "CallSessions",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ReceiverId",
                table: "CallSessions",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_MatchId",
                table: "Chats",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransactions_UserId",
                table: "CoinTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositRequests_UserId",
                table: "DepositRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_User1Id_User2Id",
                table: "Matches",
                columns: new[] { "User1Id", "User2Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_User2Id",
                table: "Matches",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId",
                table: "Messages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReplyToMessageId",
                table: "Messages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyAgreements_UserId",
                table: "PrivacyAgreements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedUserId",
                table: "Reports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_SuperChats_FromUserId",
                table: "SuperChats",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SuperChats_MatchCreatedId",
                table: "SuperChats",
                column: "MatchCreatedId");

            migrationBuilder.CreateIndex(
                name: "IX_SuperChats_ToUserId",
                table: "SuperChats",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Swipes_SwiperId_TargetId",
                table: "Swipes",
                columns: new[] { "SwiperId", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Swipes_TargetId",
                table: "Swipes",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_UserImages_UserId",
                table: "UserImages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_InterestId",
                table: "UserInterests",
                column: "InterestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocations_UserId",
                table: "UserLocations",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                table: "Users",
                column: "Phone",
                unique: true,
                filter: "\"Phone\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_PlanId",
                table: "UserSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_UserId",
                table: "WithdrawalRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "CallSessions");

            migrationBuilder.DropTable(
                name: "CoinTransactions");

            migrationBuilder.DropTable(
                name: "DepositRequests");

            migrationBuilder.DropTable(
                name: "Gifts");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PrivacyAgreements");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "SuperChats");

            migrationBuilder.DropTable(
                name: "Swipes");

            migrationBuilder.DropTable(
                name: "UserImages");

            migrationBuilder.DropTable(
                name: "UserInterests");

            migrationBuilder.DropTable(
                name: "UserLocations");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "WithdrawalRequests");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "Interests");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
