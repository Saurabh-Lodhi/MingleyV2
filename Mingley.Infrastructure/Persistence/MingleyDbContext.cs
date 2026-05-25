using Microsoft.EntityFrameworkCore;
using Mingley.Domain.Entities;

namespace Mingley.Infrastructure.Persistence;

public class MingleyDbContext : DbContext
{
    // ── Coin economy constants ──────────────────────────────────────────
    public const int AudioCallCoinPerMin = 10;
    public const int VideoCallCoinPerMin = 100;
    public const int VerificationBonus = 50;
    public const int WelcomeBonus = 100;
    public const int SuperLikeCost = 50;
    public const int SuperChatCost = 500;
    public const double CoinToInrRate = 0.10;
    public const double GirlCommissionPct = 0.50;
    public const double FemaleWithdrawPct = 0.70;
    public const int MaleCostPerMessage = 10;
    public const int MalePremiumCostPerMsg = 5;
    public const int FemaleFreeMessages = 3;
    public const int FemaleMessageCost = 5;

    public MingleyDbContext(DbContextOptions<MingleyDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserLocation> UserLocations => Set<UserLocation>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<UserImage> UserImages => Set<UserImage>();
    public DbSet<Interest> Interests => Set<Interest>();
    public DbSet<UserInterest> UserInterests => Set<UserInterest>();
    public DbSet<Swipe> Swipes => Set<Swipe>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<CallSession> CallSessions => Set<CallSession>();
    public DbSet<SuperChat> SuperChats => Set<SuperChat>();
    public DbSet<CoinTransaction> CoinTransactions => Set<CoinTransaction>();
    public DbSet<DepositRequest> DepositRequests => Set<DepositRequest>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<Gift> Gifts => Set<Gift>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<PrivacyAgreement> PrivacyAgreements => Set<PrivacyAgreement>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<Match>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<Message>().HasQueryFilter(e => !e.IsDeleted);

        mb.Entity<User>().HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
        mb.Entity<User>().HasIndex(u => u.Phone).IsUnique().HasFilter("\"Phone\" IS NOT NULL");
        mb.Entity<Block>().HasIndex(b => new { b.BlockerId, b.BlockedUserId }).IsUnique();
        mb.Entity<Swipe>().HasIndex(s => new { s.SwiperId, s.TargetId }).IsUnique();
        mb.Entity<UserInterest>().HasKey(ui => new { ui.UserId, ui.InterestId });

        mb.Entity<UserPreference>().HasOne(p => p.User).WithOne(u => u.Preference).HasForeignKey<UserPreference>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<UserLocation>().HasOne(l => l.User).WithOne(u => u.Location).HasForeignKey<UserLocation>(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<UserSubscription>().HasOne(s => s.User).WithOne(u => u.Subscription).HasForeignKey<UserSubscription>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Chat>().HasOne(c => c.Match).WithOne(m => m.Chat).HasForeignKey<Chat>(c => c.MatchId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Match>().HasOne(m => m.User1).WithMany().HasForeignKey(m => m.User1Id).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Match>().HasOne(m => m.User2).WithMany().HasForeignKey(m => m.User2Id).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Message>().HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Message>().HasOne(m => m.ReplyToMessage).WithMany().HasForeignKey(m => m.ReplyToMessageId).OnDelete(DeleteBehavior.SetNull);
        mb.Entity<Swipe>().HasOne(s => s.Swiper).WithMany().HasForeignKey(s => s.SwiperId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Swipe>().HasOne(s => s.Target).WithMany().HasForeignKey(s => s.TargetId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<CallSession>().HasOne(c => c.Caller).WithMany().HasForeignKey(c => c.CallerId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<CallSession>().HasOne(c => c.Receiver).WithMany().HasForeignKey(c => c.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<CallSession>().HasOne(c => c.Match).WithMany().HasForeignKey(c => c.MatchId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<SuperChat>().HasOne(s => s.FromUser).WithMany().HasForeignKey(s => s.FromUserId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<SuperChat>().HasOne(s => s.ToUser).WithMany().HasForeignKey(s => s.ToUserId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<SuperChat>().HasOne(s => s.MatchCreated).WithMany().HasForeignKey(s => s.MatchCreatedId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
        mb.Entity<Block>().HasOne(b => b.Blocker).WithMany().HasForeignKey(b => b.BlockerId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Block>().HasOne(b => b.BlockedUser).WithMany().HasForeignKey(b => b.BlockedUserId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Report>().HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Report>().HasOne(r => r.ReportedUser).WithMany().HasForeignKey(r => r.ReportedUserId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<SubscriptionPlan>().Property(p => p.Price).HasPrecision(18, 2);
        mb.Entity<RefreshToken>(e => { e.HasKey(t => t.Id); e.Property(t => t.Token).IsRequired().HasMaxLength(512); e.HasIndex(t => t.Token).IsUnique(); e.HasOne(t => t.User).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade); });

        SeedData(mb);
    }

    // ════════════════════════════════════════════════════════════════════
    // SEED DATA — 50 users · 20 interests · 4 plans · 28 gifts · 15 matches
    // Password for ALL users: Mingley@123
    // ════════════════════════════════════════════════════════════════════
    private static void SeedData(ModelBuilder mb)
    {
        const string hash = "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq"; // Mingley@123

        // ── Interests ────────────────────────────────────────────────────
        mb.Entity<Interest>().HasData(
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000001"), Name = "Music", Icon = "musical-notes-outline", Emoji = "🎵" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000002"), Name = "Travel", Icon = "airplane-outline", Emoji = "✈️" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000003"), Name = "Gym", Icon = "barbell-outline", Emoji = "💪" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000004"), Name = "Movies", Icon = "film-outline", Emoji = "🎬" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000005"), Name = "Reading", Icon = "book-outline", Emoji = "📚" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000006"), Name = "Cooking", Icon = "restaurant-outline", Emoji = "🍳" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000007"), Name = "Art", Icon = "color-palette-outline", Emoji = "🎨" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000008"), Name = "Dancing", Icon = "body-outline", Emoji = "💃" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000009"), Name = "Photography", Icon = "camera-outline", Emoji = "📸" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000010"), Name = "Yoga", Icon = "body-outline", Emoji = "🧘" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000011"), Name = "Cricket", Icon = "baseball-outline", Emoji = "🏏" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000012"), Name = "Gaming", Icon = "game-controller-outline", Emoji = "🎮" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000013"), Name = "Shopping", Icon = "bag-handle-outline", Emoji = "🛍️" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000014"), Name = "Foodie", Icon = "pizza-outline", Emoji = "🍕" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000015"), Name = "Hiking", Icon = "walk-outline", Emoji = "🥾" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000016"), Name = "Coding", Icon = "code-slash-outline", Emoji = "💻" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000017"), Name = "Pets", Icon = "paw-outline", Emoji = "🐾" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000018"), Name = "Coffee", Icon = "cafe-outline", Emoji = "☕" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000019"), Name = "Meditation", Icon = "leaf-outline", Emoji = "🧠" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-0000000000020"), Name = "Football", Icon = "football-outline", Emoji = "⚽" }
        );

        // ── Subscription Plans ───────────────────────────────────────────
        mb.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"), Name = "Silver", Price = 299, DurationDays = 30, Features = "[\"Unlimited likes\",\"No ads\",\"5 Super Likes/day\",\"See who liked you\"]", IsPopular = false, SuperLikesPerDay = 5, BoostsPerMonth = 0, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = false },
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000002"), Name = "Gold", Price = 599, DurationDays = 30, Features = "[\"All Silver\",\"Video calls\",\"10 Super Likes/day\",\"2 Profile boosts\",\"5 coins/msg\"]", IsPopular = true, SuperLikesPerDay = 10, BoostsPerMonth = 2, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true },
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000003"), Name = "Platinum", Price = 999, DurationDays = 30, Features = "[\"All Gold\",\"Top picks daily\",\"Unlimited Super Likes\",\"5 boosts/month\",\"Priority support\"]", IsPopular = false, SuperLikesPerDay = -1, BoostsPerMonth = 5, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true },
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000004"), Name = "VIP", Price = 1999, DurationDays = 90, Features = "[\"All Platinum\",\"VIP badge\",\"Global search\",\"Dedicated support\",\"Early features\"]", IsPopular = false, SuperLikesPerDay = -1, BoostsPerMonth = 15, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true }
        );

        // ── Gifts (6 categories, 28 gifts) ──────────────────────────────
        mb.Entity<Gift>().HasData(
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"), Name = "Heart", Emoji = "❤️", Icon = "heart-outline", CoinCost = 10, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000002"), Name = "Rose", Emoji = "🌹", Icon = "rose-outline", CoinCost = 20, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000003"), Name = "Gift Box", Emoji = "🎁", Icon = "gift-outline", CoinCost = 50, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000004"), Name = "Coffee Date", Emoji = "☕", Icon = "cafe-outline", CoinCost = 100, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000005"), Name = "Diamond Ring", Emoji = "💍", Icon = "diamond-outline", CoinCost = 500, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000001"), Name = "Bouquet", Emoji = "💐", Icon = "flower-outline", CoinCost = 50, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000002"), Name = "Chocolate Box", Emoji = "🍫", Icon = "heart-outline", CoinCost = 75, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000003"), Name = "Love Letter", Emoji = "💌", Icon = "mail-outline", CoinCost = 30, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000004"), Name = "Candlelight", Emoji = "🕯️", Icon = "flame-outline", CoinCost = 150, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000005"), Name = "Teddy Bear", Emoji = "🧸", Icon = "gift-outline", CoinCost = 200, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000001"), Name = "Cake", Emoji = "🎂", Icon = "cake-outline", CoinCost = 30, Category = "fun", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000002"), Name = "Party Popper", Emoji = "🎉", Icon = "sparkles-outline", CoinCost = 40, Category = "fun", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000003"), Name = "Trophy", Emoji = "🏆", Icon = "trophy-outline", CoinCost = 80, Category = "fun", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000004"), Name = "Balloon", Emoji = "🎈", Icon = "balloon-outline", CoinCost = 25, Category = "fun", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000005"), Name = "Confetti", Emoji = "🎊", Icon = "sparkles-outline", CoinCost = 35, Category = "fun", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000004-0000-0000-0000-000000000001"), Name = "Fireworks", Emoji = "🎆", Icon = "sparkles-outline", CoinCost = 150, Category = "animated", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000004-0000-0000-0000-000000000002"), Name = "Shooting Star", Emoji = "🌠", Icon = "star-outline", CoinCost = 200, Category = "animated", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000004-0000-0000-0000-000000000003"), Name = "Rainbow", Emoji = "🌈", Icon = "color-fill-outline", CoinCost = 300, Category = "animated", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000004-0000-0000-0000-000000000004"), Name = "Magic Wand", Emoji = "🪄", Icon = "sparkles-outline", CoinCost = 250, Category = "animated", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000005-0000-0000-0000-000000000001"), Name = "Crown", Emoji = "👑", Icon = "diamond-outline", CoinCost = 500, Category = "luxury", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000005-0000-0000-0000-000000000002"), Name = "Sports Car", Emoji = "🚗", Icon = "car-outline", CoinCost = 800, Category = "luxury", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000005-0000-0000-0000-000000000003"), Name = "Private Jet", Emoji = "✈️", Icon = "airplane-outline", CoinCost = 1500, Category = "luxury", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000005-0000-0000-0000-000000000004"), Name = "Yacht", Emoji = "⛵", Icon = "boat-outline", CoinCost = 2000, Category = "luxury", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000006-0000-0000-0000-000000000001"), Name = "Golden Rose", Emoji = "🌹", Icon = "rose-outline", CoinCost = 1000, Category = "vip", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000006-0000-0000-0000-000000000002"), Name = "Diamond Heart", Emoji = "💎", Icon = "diamond-outline", CoinCost = 3000, Category = "vip", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000006-0000-0000-0000-000000000003"), Name = "King Package", Emoji = "🎰", Icon = "trophy-outline", CoinCost = 5000, Category = "vip", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000006-0000-0000-0000-000000000004"), Name = "Luxury Suite", Emoji = "🏰", Icon = "star-outline", CoinCost = 8000, Category = "vip", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000006-0000-0000-0000-000000000005"), Name = "Universe", Emoji = "🌌", Icon = "telescope-outline", CoinCost = 10000, Category = "vip", IsAnimated = true }
        );

        // ── User IDs ─────────────────────────────────────────────────────
        var superId = Guid.Parse("d0000001-0000-0000-0000-000000000001");
        var priyaId = Guid.Parse("d0000002-0000-0000-0000-000000000002");
        var nehaId = Guid.Parse("d0000003-0000-0000-0000-000000000003");
        var ankitaId = Guid.Parse("d0000004-0000-0000-0000-000000000004");
        var aishaId = Guid.Parse("d0000005-0000-0000-0000-000000000005");
        var shreyaId = Guid.Parse("d0000006-0000-0000-0000-000000000006");
        var meenaId = Guid.Parse("d0000007-0000-0000-0000-000000000007");
        var poojaId = Guid.Parse("d0000008-0000-0000-0000-000000000008");
        var kritikaId = Guid.Parse("d0000009-0000-0000-0000-000000000009");
        var ritaId = Guid.Parse("d0000010-0000-0000-0000-000000000010");
        var simranId = Guid.Parse("d0000011-0000-0000-0000-000000000011");
        var divyaId = Guid.Parse("d0000012-0000-0000-0000-000000000012");
        var kavyaId = Guid.Parse("d0000013-0000-0000-0000-000000000013");
        var tanviId = Guid.Parse("d0000014-0000-0000-0000-000000000014");
        var ishitaId = Guid.Parse("d0000015-0000-0000-0000-000000000015");
        var riyaId = Guid.Parse("d0000016-0000-0000-0000-000000000016");
        var zaraId = Guid.Parse("d0000017-0000-0000-0000-000000000017");
        var nainaId = Guid.Parse("d0000018-0000-0000-0000-000000000018");
        var preethiId = Guid.Parse("d0000019-0000-0000-0000-000000000019");
        var ananyaId = Guid.Parse("d0000020-0000-0000-0000-000000000020");
        var sonalId = Guid.Parse("d0000021-0000-0000-0000-000000000021");
        var alishaId = Guid.Parse("d0000022-0000-0000-0000-000000000022");
        var ayeshaId = Guid.Parse("d0000023-0000-0000-0000-000000000023");
        var taraId = Guid.Parse("d0000024-0000-0000-0000-000000000024");
        var naliniId = Guid.Parse("d0000025-0000-0000-0000-000000000025");
        var arjunId = Guid.Parse("d0000026-0000-0000-0000-000000000026");
        var rahulId = Guid.Parse("d0000027-0000-0000-0000-000000000027");
        var vikramId = Guid.Parse("d0000028-0000-0000-0000-000000000028");
        var deepakId = Guid.Parse("d0000029-0000-0000-0000-000000000029");
        var rohitId = Guid.Parse("d0000030-0000-0000-0000-000000000030");
        var karthikId = Guid.Parse("d0000031-0000-0000-0000-000000000031");
        var rajeshId = Guid.Parse("d0000032-0000-0000-0000-000000000032");
        var amanId = Guid.Parse("d0000033-0000-0000-0000-000000000033");
        var adityaId = Guid.Parse("d0000034-0000-0000-0000-000000000034");
        var nikhilId = Guid.Parse("d0000035-0000-0000-0000-000000000035");
        var sureshId = Guid.Parse("d0000036-0000-0000-0000-000000000036");
        var aakashId = Guid.Parse("d0000037-0000-0000-0000-000000000037");
        var kabirId = Guid.Parse("d0000038-0000-0000-0000-000000000038");
        var aryanId = Guid.Parse("d0000039-0000-0000-0000-000000000039");
        var devId = Guid.Parse("d0000040-0000-0000-0000-000000000040");
        var mihirId = Guid.Parse("d0000041-0000-0000-0000-000000000041");
        var rohanId = Guid.Parse("d0000042-0000-0000-0000-000000000042");
        var vivekId = Guid.Parse("d0000043-0000-0000-0000-000000000043");
        var priyankId = Guid.Parse("d0000044-0000-0000-0000-000000000044");
        var ankitId = Guid.Parse("d0000045-0000-0000-0000-000000000045");
        var jayId = Guid.Parse("d0000046-0000-0000-0000-000000000046");
        var shivId = Guid.Parse("d0000047-0000-0000-0000-000000000047");
        var saurabhId = Guid.Parse("d0000048-0000-0000-0000-000000000048");
        var mohanId = Guid.Parse("d0000049-0000-0000-0000-000000000049");
        var testId = Guid.Parse("d0000050-0000-0000-0000-000000000050");

        // ── Users (50 total · password: Mingley@123) ─────────────────────
        mb.Entity<User>().HasData(
            new User
            {
                Id = superId,
                FullName = "Super Admin",
                Email = "admin@mingley.app",
                PasswordHash = hash,
                Gender = "male",
                Role = "admin",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 99999,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Platform administrator 🔧",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = priyaId,
                FullName = "Priya Sharma",
                Email = "priya@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 2500,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1998, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Kathak dancer & yoga instructor 🌺 | Delhi girl | Love chai mornings ✨",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = nehaId,
                FullName = "Neha Kapoor",
                Email = "neha@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 800,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1999, 7, 20, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Playback singer 🎵 | Travel addict ✈️ | Mumbai | Chai > coffee ☕",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = ankitaId,
                FullName = "Ankita Singh",
                Email = "ankita@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1200,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(2000, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Foodie & travel photographer 📸🍕 | Pune | Obsessed with sunsets 🌅",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1488426862026-3ee34a7d66df?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = aishaId,
                FullName = "Aisha Khan",
                Email = "aisha@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1800,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1999, 2, 14, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Fashion designer 👗 | Sketch artist 🎨 | Hyderabad | Building my empire 💅",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1529626455594-4ff0802cfb7e?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = shreyaId,
                FullName = "Shreya Patel",
                Email = "shreya@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 3500,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1997, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Doctor by day, dancer by night 💃 | Ahmedabad | Books + Beaches 📖🏖️",
                Profession = "Doctor",
                Avatar = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = meenaId,
                FullName = "Meena Reddy",
                Email = "meena@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 700,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(2001, 3, 22, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Engineering student 📚 | Sketch artist | Hyderabad | 21 & figuring it out 😄",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1542206395-9feb3edaa68d?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = poojaId,
                FullName = "Pooja Gupta",
                Email = "pooja@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 950,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1998, 9, 7, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Marketing lead 📈 | Bookworm 📖 | Jaipur | Pink city girl 🌸",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1489424731084-a5d8b219a5bb?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = kritikaId,
                FullName = "Kritika Bose",
                Email = "kritika@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1100,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1996, 7, 14, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Senior journalist ✍️ | World traveller ✈️ | Kolkata | City of joy forever 🎭",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = ritaId,
                FullName = "Rita Desai",
                Email = "rita@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 4200,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1993, 11, 25, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Finance head 💼 | Yoga guru 🧘 | Surat | Manifesting greatness ✨",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = simranId,
                FullName = "Simran Kaur",
                Email = "simran@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 2800,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1997, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Corporate lawyer ⚖️ | Kathak dancer 💃 | Amritsar | Golden temple sunrise hits different 🌅",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = divyaId,
                FullName = "Divya Menon",
                Email = "divya@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 600,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(2000, 6, 18, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Architecture student 🏛️ | Coffee addict ☕ | Kochi | Designing my future 📐",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1502685104226-ee32379fefbe?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = kavyaId,
                FullName = "Kavya Nair",
                Email = "kavya@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 400,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(2002, 4, 5, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Pre-med | Poet 🖋️ | Trivandrum | Words are my superpower 🌙",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1515077678510-ce3bdf418862?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = tanviId,
                FullName = "Tanvi Joshi",
                Email = "tanvi@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1350,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1995, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Senior UI/UX Designer 🎨 | Plant mom 🌿 | Pune | Making things beautiful ✨",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1520813792240-56fc4a3765a7?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = ishitaId,
                FullName = "Ishita Sharma",
                Email = "ishita@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 5100,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1994, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Tech startup founder 🚀 | TEDx speaker | Delhi | Hustle + heart ❤️‍🔥",
                Profession = "Business Owner",
                Avatar = "https://images.unsplash.com/photo-1509967419530-da38b4704bc6?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = riyaId,
                FullName = "Riya Singh",
                Email = "riya@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 880,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1999, 10, 3, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Classical singer 🎶 | Bookworm 📚 | Varanasi | Old soul in a modern world 🕌",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1531123897727-240d604e3dc3?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = zaraId,
                FullName = "Zara Ahmed",
                Email = "zara@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1500,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1998, 5, 18, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Model & content creator 📸 | Mumbai | Living my best life 🌟",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = nainaId,
                FullName = "Naina Verma",
                Email = "naina@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 2200,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1996, 9, 12, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Cardiologist ❤️‍🩺 | Runner 🏃 | Delhi | Saving hearts in and out of hospital 😄",
                Profession = "Doctor",
                Avatar = "https://images.unsplash.com/photo-1544717305-2782549b5136?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = preethiId,
                FullName = "Preethi Rao",
                Email = "preethi@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 650,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(2001, 2, 28, 0, 0, 0, DateTimeKind.Utc),
                Bio = "CS undergrad 💻 | Hackathon champ | Bengaluru | Ctrl+Z my way through life 😂",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = ananyaId,
                FullName = "Ananya Chatterjee",
                Email = "ananya@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1900,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1997, 11, 15, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Wildlife photographer 🦁 | Bengali foodie 🍛 | Kolkata | Mountains & monsoons 🌧️",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = sonalId,
                FullName = "Sonal Mehta",
                Email = "sonal@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 3100,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1995, 3, 30, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Investment banker 💰 | Marathoner 🏃‍♀️ | Mumbai | Finance & trails ⛰️",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1546961342-ea5f62d4d0b0?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = alishaId,
                FullName = "Alisha D'Souza",
                Email = "alisha@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 750,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(2000, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Marine biologist 🐠 | Beach bum 🏖️ | Goa | Ocean is my therapy 🌊",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1601412436967-a70659db97b5?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = ayeshaId,
                FullName = "Ayesha Mirza",
                Email = "ayesha@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1100,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1998, 12, 5, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Classical musician 🎻 | Urdu poetry lover | Lucknow | Tehzeeb & charm 💫",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1586297135537-94bc81ba6254?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = taraId,
                FullName = "Tara Pillai",
                Email = "tara@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 4800,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1993, 6, 20, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Neurosurgeon 🧠 | Bharatanatyam dancer 💃 | Chennai | Brains AND moves 😉",
                Profession = "Doctor",
                Avatar = "https://images.unsplash.com/photo-1614632537197-38a17061c2bd?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = naliniId,
                FullName = "Nalini Krishnan",
                Email = "nalini@demo.com",
                PasswordHash = hash,
                Gender = "female",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 2000,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1996, 4, 8, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Product Manager @BigTech 📊 | Traveller 🗺️ | Bengaluru | Building products people love ❤️",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1619946794135-5bc917a27793?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = arjunId,
                FullName = "Arjun Singh",
                Email = "arjun@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 10000,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1993, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Fitness freak 💪 | Landscape photographer 📸 | Gurgaon | Mountains > malls 🏔️",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = rahulId,
                FullName = "Rahul Mehta",
                Email = "rahul@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 5000,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1995, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Music lover 🎸 | Solo traveller ✈️ | Software Engineer | Noida | Guitar + code = life",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1552058544-f2b08422138a?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = vikramId,
                FullName = "Vikram Nair",
                Email = "vikram@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 3000,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1996, 4, 12, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Serial entrepreneur ⚡ | Coffee addict ☕ | Delhi | Building the next big thing 🚀",
                Profession = "Business Owner",
                Avatar = "https://images.unsplash.com/photo-1568602471122-7832951cc4c5?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = deepakId,
                FullName = "Deepak Verma",
                Email = "deepak@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 500,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1997, 9, 30, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Gym freak 🏋️ | Cricket fanatic 🏏 | Noida | IPL > everything 😂",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = rohitId,
                FullName = "Rohit Sharma",
                Email = "rohit@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 2000,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1994, 6, 25, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Head chef 👨‍🍳 | Food blogger | Bengaluru | Will cook for you if you laugh at my puns 😄",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = karthikId,
                FullName = "Karthik Menon",
                Email = "karthik@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 4500,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1992, 8, 18, 0, 0, 0, DateTimeKind.Utc),
                Bio = "IIT Madras grad 🎓 | Startup founder 🚀 | Chennai | 0→1 builder ⚙️",
                Profession = "Business Owner",
                Avatar = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = rajeshId,
                FullName = "Rajesh Kumar",
                Email = "rajesh@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1500,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1990, 12, 3, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Principal engineer 💻 | Gaming legend 🎮 | Kolkata | 10 yrs of bugs still counting 😅",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = amanId,
                FullName = "Aman Joshi",
                Email = "aman@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 6000,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1994, 2, 28, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Principal architect 🏛️ | Art collector 🎨 | Chandigarh | Designing spaces, chasing light 🌤️",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1548449112-96a38a643324?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = adityaId,
                FullName = "Aditya Kumar",
                Email = "aditya@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 3200,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1996, 3, 11, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Data scientist 📊 | Bike tourer 🏍️ | Pune | Numbers by day, highways by night 🌙",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = nikhilId,
                FullName = "Nikhil Sharma",
                Email = "nikhil@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 7500,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1993, 9, 19, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Investment banker 💰 | World explorer 🗺️ | Mumbai | 42 countries and counting 🌍",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1488161628813-04466f872be2?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = sureshId,
                FullName = "Suresh Iyer",
                Email = "suresh@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 180,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(2000, 2, 14, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Commerce undergrad 📊 | Meme lord 😂 | Coimbatore | Vibing on good music 🎧",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1504257432389-52343af06ae3?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = aakashId,
                FullName = "Aakash Verma",
                Email = "aakash@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 2200,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1995, 5, 27, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Commercial pilot ✈️ | Astronomy nerd 🔭 | Delhi | Up in the clouds, literally 😄",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1531891437562-4301cf35b7e4?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = kabirId,
                FullName = "Kabir Singh",
                Email = "kabir@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 3800,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1994, 8, 14, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Orthopedic surgeon 🦴 | Classical guitarist 🎸 | Jaipur | Healing bodies & minds 🙏",
                Profession = "Doctor",
                Avatar = "https://images.unsplash.com/photo-1522556189639-b786d812d5ae?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = aryanId,
                FullName = "Aryan Kapoor",
                Email = "aryan@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1200,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1998, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Bollywood choreographer 💃 | Fitness coach 💪 | Mumbai | Dance is my language 🕺",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1521119989659-a83eee488004?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = devId,
                FullName = "Dev Malhotra",
                Email = "dev@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 8500,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1992, 5, 5, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Tech co-founder 🚀 | Angel investor | Delhi | Disrupting industries before breakfast ⚡",
                Profession = "Business Owner",
                Avatar = "https://images.unsplash.com/photo-1534030347209-467a573065b7?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = mihirId,
                FullName = "Mihir Shah",
                Email = "mihir@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 900,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1997, 12, 18, 0, 0, 0, DateTimeKind.Utc),
                Bio = "CA & tax consultant 📋 | Cricket player 🏏 | Surat | Numbers make sense, people don't 😄",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = rohanId,
                FullName = "Rohan Bose",
                Email = "rohan@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 2600,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1996, 7, 4, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Documentary filmmaker 🎬 | Street photographer | Kolkata | Storytelling through lens 📸",
                Profession = "Freelancer",
                Avatar = "https://images.unsplash.com/photo-1530268729831-4b0b9e170600?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = vivekId,
                FullName = "Vivek Pandey",
                Email = "vivek@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 700,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1999, 3, 25, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Law student ⚖️ | Debate champion | Lucknow | Arguing is my cardio 😂",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1496345875838-ff236a81d931?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = priyankId,
                FullName = "Priyank Agarwal",
                Email = "priyank@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 1800,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1995, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Product manager 📱 | Foodie 🍕 | Indore | Poha > everything 😋",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1524598191073-ee72ca03e29a?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = ankitId,
                FullName = "Ankit Tiwari",
                Email = "ankit@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 450,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1998, 6, 30, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Engineering student 🔧 | Gamer 🎮 | Bhopal | BGMI Conqueror 🏆",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = jayId,
                FullName = "Jay Patel",
                Email = "jay@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 5200,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1993, 4, 17, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Real estate mogul 🏠 | Gym addict 💪 | Ahmedabad | Building empires one property at a time 🏗️",
                Profession = "Business Owner",
                Avatar = "https://images.unsplash.com/photo-1504593811423-6dd665756598?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = shivId,
                FullName = "Shiv Kumar",
                Email = "shiv@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = true,
                IsPremium = true,
                CoinBalance = 12000,
                ProfileComplete = true,
                IsOnline = true,
                DateOfBirth = new DateTime(1991, 9, 8, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Retired army officer 🎖️ | Mountaineer 🏔️ | Chandigarh | Adventure is my middle name ⛰️",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1463453091185-61582044d556?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = saurabhId,
                FullName = "Saurabh Mishra",
                Email = "saurabh@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 250,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(1999, 4, 16, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Cricketer ⚡ | Final year student | Lucknow | Future RCB player 😂🏏",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = mohanId,
                FullName = "Mohan Pillai",
                Email = "mohan@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = true,
                IsActive = false,
                IsPremium = false,
                CoinBalance = 100,
                ProfileComplete = false,
                IsOnline = false,
                DateOfBirth = new DateTime(1988, 6, 8, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Retired athlete 🥇 | Fitness coach | Thiruvananthapuram | Chasing a second wind 💨",
                Profession = "Working Professional",
                Avatar = "https://images.unsplash.com/photo-1519345182560-3f2917c472ef?w=1200&q=95&fit=crop&crop=faces&auto=format"
            },
            new User
            {
                Id = testId,
                FullName = "Test User Male",
                Email = "test_male@demo.com",
                PasswordHash = hash,
                Gender = "male",
                Role = "user",
                IsVerified = false,
                IsActive = true,
                IsPremium = false,
                CoinBalance = 50,
                ProfileComplete = true,
                IsOnline = false,
                DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Just testing 🧪",
                Profession = "Student",
                Avatar = "https://images.unsplash.com/photo-1463453091185-61582044d556?w=1200&q=95&fit=crop&crop=faces&auto=format"
            }
        );

        // ── User Locations ───────────────────────────────────────────────
        mb.Entity<UserLocation>().HasData(
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"), UserId = superId, City = "Delhi", Country = "India", Lat = 28.6139, Lng = 77.209 },
            new UserLocation { Id = Guid.Parse("f0000002-0000-0000-0000-000000000002"), UserId = priyaId, City = "Delhi", Country = "India", Lat = 28.6139, Lng = 77.209 },
            new UserLocation { Id = Guid.Parse("f0000003-0000-0000-0000-000000000003"), UserId = nehaId, City = "Mumbai", Country = "India", Lat = 19.076, Lng = 72.8777 },
            new UserLocation { Id = Guid.Parse("f0000004-0000-0000-0000-000000000004"), UserId = ankitaId, City = "Pune", Country = "India", Lat = 18.5204, Lng = 73.8567 },
            new UserLocation { Id = Guid.Parse("f0000005-0000-0000-0000-000000000005"), UserId = aishaId, City = "Hyderabad", Country = "India", Lat = 17.385, Lng = 78.4867 },
            new UserLocation { Id = Guid.Parse("f0000006-0000-0000-0000-000000000006"), UserId = shreyaId, City = "Ahmedabad", Country = "India", Lat = 23.0225, Lng = 72.5714 },
            new UserLocation { Id = Guid.Parse("f0000007-0000-0000-0000-000000000007"), UserId = meenaId, City = "Hyderabad", Country = "India", Lat = 17.4, Lng = 78.5 },
            new UserLocation { Id = Guid.Parse("f0000008-0000-0000-0000-000000000008"), UserId = poojaId, City = "Jaipur", Country = "India", Lat = 26.9124, Lng = 75.7873 },
            new UserLocation { Id = Guid.Parse("f0000009-0000-0000-0000-000000000009"), UserId = kritikaId, City = "Kolkata", Country = "India", Lat = 22.5726, Lng = 88.3639 },
            new UserLocation { Id = Guid.Parse("f0000010-0000-0000-0000-000000000010"), UserId = ritaId, City = "Surat", Country = "India", Lat = 21.1702, Lng = 72.8311 },
            new UserLocation { Id = Guid.Parse("f0000011-0000-0000-0000-000000000011"), UserId = simranId, City = "Amritsar", Country = "India", Lat = 31.634, Lng = 74.8723 },
            new UserLocation { Id = Guid.Parse("f0000012-0000-0000-0000-000000000012"), UserId = divyaId, City = "Kochi", Country = "India", Lat = 9.9312, Lng = 76.2673 },
            new UserLocation { Id = Guid.Parse("f0000013-0000-0000-0000-000000000013"), UserId = kavyaId, City = "Trivandrum", Country = "India", Lat = 8.5241, Lng = 76.9366 },
            new UserLocation { Id = Guid.Parse("f0000014-0000-0000-0000-000000000014"), UserId = tanviId, City = "Pune", Country = "India", Lat = 18.53, Lng = 73.87 },
            new UserLocation { Id = Guid.Parse("f0000015-0000-0000-0000-000000000015"), UserId = ishitaId, City = "Delhi", Country = "India", Lat = 28.63, Lng = 77.22 },
            new UserLocation { Id = Guid.Parse("f0000016-0000-0000-0000-000000000016"), UserId = riyaId, City = "Varanasi", Country = "India", Lat = 25.3176, Lng = 82.9739 },
            new UserLocation { Id = Guid.Parse("f0000017-0000-0000-0000-000000000017"), UserId = zaraId, City = "Mumbai", Country = "India", Lat = 19.09, Lng = 72.86 },
            new UserLocation { Id = Guid.Parse("f0000018-0000-0000-0000-000000000018"), UserId = nainaId, City = "Delhi", Country = "India", Lat = 28.65, Lng = 77.23 },
            new UserLocation { Id = Guid.Parse("f0000019-0000-0000-0000-000000000019"), UserId = preethiId, City = "Bengaluru", Country = "India", Lat = 12.9716, Lng = 77.5946 },
            new UserLocation { Id = Guid.Parse("f0000020-0000-0000-0000-000000000020"), UserId = ananyaId, City = "Kolkata", Country = "India", Lat = 22.58, Lng = 88.36 },
            new UserLocation { Id = Guid.Parse("f0000021-0000-0000-0000-000000000021"), UserId = sonalId, City = "Mumbai", Country = "India", Lat = 19.08, Lng = 72.89 },
            new UserLocation { Id = Guid.Parse("f0000022-0000-0000-0000-000000000022"), UserId = alishaId, City = "Goa", Country = "India", Lat = 15.4909, Lng = 73.8278 },
            new UserLocation { Id = Guid.Parse("f0000023-0000-0000-0000-000000000023"), UserId = ayeshaId, City = "Lucknow", Country = "India", Lat = 26.8467, Lng = 80.9462 },
            new UserLocation { Id = Guid.Parse("f0000024-0000-0000-0000-000000000024"), UserId = taraId, City = "Chennai", Country = "India", Lat = 13.0827, Lng = 80.2707 },
            new UserLocation { Id = Guid.Parse("f0000025-0000-0000-0000-000000000025"), UserId = naliniId, City = "Bengaluru", Country = "India", Lat = 12.98, Lng = 77.6 },
            new UserLocation { Id = Guid.Parse("f0000026-0000-0000-0000-000000000026"), UserId = arjunId, City = "Gurgaon", Country = "India", Lat = 28.4595, Lng = 77.0266 },
            new UserLocation { Id = Guid.Parse("f0000027-0000-0000-0000-000000000027"), UserId = rahulId, City = "Noida", Country = "India", Lat = 28.5355, Lng = 77.391 },
            new UserLocation { Id = Guid.Parse("f0000028-0000-0000-0000-000000000028"), UserId = vikramId, City = "Delhi", Country = "India", Lat = 28.7041, Lng = 77.1025 },
            new UserLocation { Id = Guid.Parse("f0000029-0000-0000-0000-000000000029"), UserId = deepakId, City = "Noida", Country = "India", Lat = 28.54, Lng = 77.4 },
            new UserLocation { Id = Guid.Parse("f0000030-0000-0000-0000-000000000030"), UserId = rohitId, City = "Bengaluru", Country = "India", Lat = 12.9716, Lng = 77.5946 },
            new UserLocation { Id = Guid.Parse("f0000031-0000-0000-0000-000000000031"), UserId = karthikId, City = "Chennai", Country = "India", Lat = 13.0827, Lng = 80.2707 },
            new UserLocation { Id = Guid.Parse("f0000032-0000-0000-0000-000000000032"), UserId = rajeshId, City = "Kolkata", Country = "India", Lat = 22.5726, Lng = 88.3639 },
            new UserLocation { Id = Guid.Parse("f0000033-0000-0000-0000-000000000033"), UserId = amanId, City = "Chandigarh", Country = "India", Lat = 30.7333, Lng = 76.7794 },
            new UserLocation { Id = Guid.Parse("f0000034-0000-0000-0000-000000000034"), UserId = adityaId, City = "Pune", Country = "India", Lat = 18.52, Lng = 73.86 },
            new UserLocation { Id = Guid.Parse("f0000035-0000-0000-0000-000000000035"), UserId = nikhilId, City = "Mumbai", Country = "India", Lat = 19.08, Lng = 72.88 },
            new UserLocation { Id = Guid.Parse("f0000036-0000-0000-0000-000000000036"), UserId = sureshId, City = "Coimbatore", Country = "India", Lat = 11.0168, Lng = 76.9558 },
            new UserLocation { Id = Guid.Parse("f0000037-0000-0000-0000-000000000037"), UserId = aakashId, City = "Delhi", Country = "India", Lat = 28.62, Lng = 77.21 },
            new UserLocation { Id = Guid.Parse("f0000038-0000-0000-0000-000000000038"), UserId = kabirId, City = "Jaipur", Country = "India", Lat = 26.9124, Lng = 75.7873 },
            new UserLocation { Id = Guid.Parse("f0000039-0000-0000-0000-000000000039"), UserId = aryanId, City = "Mumbai", Country = "India", Lat = 19.07, Lng = 72.88 },
            new UserLocation { Id = Guid.Parse("f0000040-0000-0000-0000-000000000040"), UserId = devId, City = "Delhi", Country = "India", Lat = 28.64, Lng = 77.2 },
            new UserLocation { Id = Guid.Parse("f0000041-0000-0000-0000-000000000041"), UserId = mihirId, City = "Surat", Country = "India", Lat = 21.1702, Lng = 72.8311 },
            new UserLocation { Id = Guid.Parse("f0000042-0000-0000-0000-000000000042"), UserId = rohanId, City = "Kolkata", Country = "India", Lat = 22.59, Lng = 88.37 },
            new UserLocation { Id = Guid.Parse("f0000043-0000-0000-0000-000000000043"), UserId = vivekId, City = "Lucknow", Country = "India", Lat = 26.85, Lng = 80.95 },
            new UserLocation { Id = Guid.Parse("f0000044-0000-0000-0000-000000000044"), UserId = priyankId, City = "Indore", Country = "India", Lat = 22.7196, Lng = 75.8577 },
            new UserLocation { Id = Guid.Parse("f0000045-0000-0000-0000-000000000045"), UserId = ankitId, City = "Bhopal", Country = "India", Lat = 23.2599, Lng = 77.4126 },
            new UserLocation { Id = Guid.Parse("f0000046-0000-0000-0000-000000000046"), UserId = jayId, City = "Ahmedabad", Country = "India", Lat = 23.03, Lng = 72.58 },
            new UserLocation { Id = Guid.Parse("f0000047-0000-0000-0000-000000000047"), UserId = shivId, City = "Chandigarh", Country = "India", Lat = 30.74, Lng = 76.79 },
            new UserLocation { Id = Guid.Parse("f0000048-0000-0000-0000-000000000048"), UserId = saurabhId, City = "Lucknow", Country = "India", Lat = 26.8467, Lng = 80.9462 },
            new UserLocation { Id = Guid.Parse("f0000049-0000-0000-0000-000000000049"), UserId = mohanId, City = "Thiruvananthapuram", Country = "India", Lat = 8.5241, Lng = 76.9366 },
            new UserLocation { Id = Guid.Parse("f0000050-0000-0000-0000-000000000050"), UserId = testId, City = "Delhi", Country = "India", Lat = 28.62, Lng = 77.2 }
        );

        // ── User Preferences ─────────────────────────────────────────────
        mb.Entity<UserPreference>().HasData(
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000001"), UserId = superId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 150, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000002-0000-0000-0000-000000000002"), UserId = priyaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 200, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000003-0000-0000-0000-000000000003"), UserId = nehaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 250, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000004-0000-0000-0000-000000000004"), UserId = ankitaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 300, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000005-0000-0000-0000-000000000005"), UserId = aishaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000006-0000-0000-0000-000000000006"), UserId = shreyaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 150, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000007-0000-0000-0000-000000000007"), UserId = meenaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 200, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000008-0000-0000-0000-000000000008"), UserId = poojaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 250, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000009-0000-0000-0000-000000000009"), UserId = kritikaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 300, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000010-0000-0000-0000-000000000010"), UserId = ritaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000011-0000-0000-0000-000000000011"), UserId = simranId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 150, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000012-0000-0000-0000-000000000012"), UserId = divyaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 200, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000013-0000-0000-0000-000000000013"), UserId = kavyaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 250, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000014-0000-0000-0000-000000000014"), UserId = tanviId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 300, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000015-0000-0000-0000-000000000015"), UserId = ishitaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000016-0000-0000-0000-000000000016"), UserId = riyaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 150, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000017-0000-0000-0000-000000000017"), UserId = zaraId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 200, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000018-0000-0000-0000-000000000018"), UserId = nainaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 250, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000019-0000-0000-0000-000000000019"), UserId = preethiId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 300, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000020-0000-0000-0000-000000000020"), UserId = ananyaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 100, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000021-0000-0000-0000-000000000021"), UserId = sonalId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 150, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000022-0000-0000-0000-000000000022"), UserId = alishaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 200, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000023-0000-0000-0000-000000000023"), UserId = ayeshaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 250, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000024-0000-0000-0000-000000000024"), UserId = taraId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 300, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000025-0000-0000-0000-000000000025"), UserId = naliniId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000026-0000-0000-0000-000000000026"), UserId = arjunId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 150, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000027-0000-0000-0000-000000000027"), UserId = rahulId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 200, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000028-0000-0000-0000-000000000028"), UserId = vikramId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 250, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000029-0000-0000-0000-000000000029"), UserId = deepakId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 300, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000030-0000-0000-0000-000000000030"), UserId = rohitId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000031-0000-0000-0000-000000000031"), UserId = karthikId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 150, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000032-0000-0000-0000-000000000032"), UserId = rajeshId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 200, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000033-0000-0000-0000-000000000033"), UserId = amanId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 250, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000034-0000-0000-0000-000000000034"), UserId = adityaId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 300, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000035-0000-0000-0000-000000000035"), UserId = nikhilId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000036-0000-0000-0000-000000000036"), UserId = sureshId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 150, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000037-0000-0000-0000-000000000037"), UserId = aakashId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 200, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000038-0000-0000-0000-000000000038"), UserId = kabirId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 250, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000039-0000-0000-0000-000000000039"), UserId = aryanId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 300, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000040-0000-0000-0000-000000000040"), UserId = devId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000041-0000-0000-0000-000000000041"), UserId = mihirId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 150, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000042-0000-0000-0000-000000000042"), UserId = rohanId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 200, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000043-0000-0000-0000-000000000043"), UserId = vivekId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 250, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000044-0000-0000-0000-000000000044"), UserId = priyankId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 300, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000045-0000-0000-0000-000000000045"), UserId = ankitId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000046-0000-0000-0000-000000000046"), UserId = jayId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 150, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000047-0000-0000-0000-000000000047"), UserId = shivId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 200, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000048-0000-0000-0000-000000000048"), UserId = saurabhId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 250, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000049-0000-0000-0000-000000000049"), UserId = mohanId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 300, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000050-0000-0000-0000-000000000050"), UserId = testId, InterestedIn = "girls", MinAge = 20, MaxAge = 32, MaxDistance = 100, RelationshipType = "casual" }
        );

        // ── User Interests ───────────────────────────────────────────────
        mb.Entity<UserInterest>().HasData(
            new UserInterest { UserId = superId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = superId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = priyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = priyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = priyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = priyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
            new UserInterest { UserId = nehaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = nehaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = nehaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = ankitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
            new UserInterest { UserId = ankitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = ankitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000014") },
            new UserInterest { UserId = aishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = aishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000013") },
            new UserInterest { UserId = aishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = shreyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = shreyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = shreyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = shreyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = meenaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = meenaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = meenaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = poojaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = poojaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000013") },
            new UserInterest { UserId = poojaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = kritikaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = kritikaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = kritikaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = ritaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = ritaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = ritaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = ritaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = simranId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = simranId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = simranId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = divyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = divyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = divyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = kavyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = kavyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = kavyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = tanviId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = tanviId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000017") },
            new UserInterest { UserId = tanviId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = tanviId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = ishitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = ishitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = ishitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = riyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = riyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = riyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = zaraId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = zaraId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000013") },
            new UserInterest { UserId = zaraId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = zaraId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = nainaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = nainaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = nainaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = nainaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = preethiId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = preethiId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = preethiId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = preethiId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = ananyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = ananyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = ananyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000014") },
            new UserInterest { UserId = ananyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = sonalId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = sonalId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = sonalId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = sonalId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = alishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000017") },
            new UserInterest { UserId = alishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = alishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = alishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = ayeshaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = ayeshaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = ayeshaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = ayeshaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = taraId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = taraId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = taraId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = taraId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = naliniId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = naliniId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = naliniId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = naliniId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = arjunId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = arjunId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = arjunId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = arjunId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = rahulId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = rahulId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = rahulId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = rahulId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = vikramId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = vikramId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = vikramId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = vikramId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = deepakId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = deepakId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
            new UserInterest { UserId = deepakId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = rohitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
            new UserInterest { UserId = rohitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000014") },
            new UserInterest { UserId = rohitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = rohitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = karthikId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = karthikId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = karthikId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = karthikId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = rajeshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = rajeshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = rajeshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = rajeshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = amanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = amanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = amanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = amanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = adityaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = adityaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = adityaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = adityaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = nikhilId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = nikhilId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = nikhilId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000013") },
            new UserInterest { UserId = nikhilId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = sureshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = sureshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000020") },
            new UserInterest { UserId = sureshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = sureshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = aakashId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = aakashId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = aakashId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = aakashId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = kabirId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = kabirId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = kabirId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = kabirId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = aryanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = aryanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = aryanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = aryanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = devId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = devId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = devId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = devId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = mihirId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
            new UserInterest { UserId = mihirId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = mihirId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = mihirId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = rohanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = rohanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = rohanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = rohanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = vivekId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = vivekId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = vivekId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = vivekId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = priyankId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000014") },
            new UserInterest { UserId = priyankId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = priyankId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = priyankId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = ankitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = ankitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = ankitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = ankitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
            new UserInterest { UserId = jayId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = jayId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = jayId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = jayId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = shivId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = shivId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = shivId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = shivId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = saurabhId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
            new UserInterest { UserId = saurabhId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = saurabhId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = mohanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = mohanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = mohanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = testId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") }
        );

        // ── User Subscriptions ───────────────────────────────────────────
        mb.Entity<UserSubscription>().HasData(
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000001"), UserId = priyaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000002-0000-0000-0000-000000000002"), UserId = shreyaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000003"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000003-0000-0000-0000-000000000003"), UserId = ritaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000004-0000-0000-0000-000000000004"), UserId = simranId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000003"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000005-0000-0000-0000-000000000005"), UserId = ishitaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000004"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000006-0000-0000-0000-000000000006"), UserId = nainaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000007-0000-0000-0000-000000000007"), UserId = sonalId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000001"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000008-0000-0000-0000-000000000008"), UserId = taraId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000004"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000009-0000-0000-0000-000000000009"), UserId = arjunId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000010-0000-0000-0000-000000000010"), UserId = amanId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000001"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000011-0000-0000-0000-000000000011"), UserId = nikhilId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000004"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000012-0000-0000-0000-000000000012"), UserId = kabirId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000013-0000-0000-0000-000000000013"), UserId = devId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000004"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000014-0000-0000-0000-000000000014"), UserId = jayId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000003"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000015-0000-0000-0000-000000000015"), UserId = shivId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000004"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true }
        );

        // ── Swipes (matched pairs + unmatched) ──────────────────────────
        mb.Entity<Swipe>().HasData(
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000001"), SwiperId = arjunId, TargetId = aishaId, Action = "like", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000002-0000-0000-0000-000000000002"), SwiperId = aishaId, TargetId = arjunId, Action = "like", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000003-0000-0000-0000-000000000003"), SwiperId = rahulId, TargetId = priyaId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000004-0000-0000-0000-000000000004"), SwiperId = priyaId, TargetId = rahulId, Action = "like", CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000005-0000-0000-0000-000000000005"), SwiperId = vikramId, TargetId = shreyaId, Action = "like", CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000006-0000-0000-0000-000000000006"), SwiperId = shreyaId, TargetId = vikramId, Action = "like", CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000007-0000-0000-0000-000000000007"), SwiperId = nikhilId, TargetId = simranId, Action = "like", CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000008-0000-0000-0000-000000000008"), SwiperId = simranId, TargetId = nikhilId, Action = "like", CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000009-0000-0000-0000-000000000009"), SwiperId = amanId, TargetId = ishitaId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000010-0000-0000-0000-000000000010"), SwiperId = ishitaId, TargetId = amanId, Action = "like", CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000011-0000-0000-0000-000000000011"), SwiperId = adityaId, TargetId = tanviId, Action = "like", CreatedAt = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000012-0000-0000-0000-000000000012"), SwiperId = tanviId, TargetId = adityaId, Action = "like", CreatedAt = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000013-0000-0000-0000-000000000013"), SwiperId = kabirId, TargetId = ritaId, Action = "like", CreatedAt = new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000014-0000-0000-0000-000000000014"), SwiperId = ritaId, TargetId = kabirId, Action = "like", CreatedAt = new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000015-0000-0000-0000-000000000015"), SwiperId = devId, TargetId = taraId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000016-0000-0000-0000-000000000016"), SwiperId = taraId, TargetId = devId, Action = "like", CreatedAt = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000017-0000-0000-0000-000000000017"), SwiperId = shivId, TargetId = naliniId, Action = "like", CreatedAt = new DateTime(2024, 1, 18, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000018-0000-0000-0000-000000000018"), SwiperId = naliniId, TargetId = shivId, Action = "like", CreatedAt = new DateTime(2024, 1, 18, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000019-0000-0000-0000-000000000019"), SwiperId = jayId, TargetId = sonalId, Action = "like", CreatedAt = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000020-0000-0000-0000-000000000020"), SwiperId = sonalId, TargetId = jayId, Action = "like", CreatedAt = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000021-0000-0000-0000-000000000021"), SwiperId = rohitId, TargetId = zaraId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 22, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000022-0000-0000-0000-000000000022"), SwiperId = zaraId, TargetId = rohitId, Action = "like", CreatedAt = new DateTime(2024, 1, 22, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000023-0000-0000-0000-000000000023"), SwiperId = karthikId, TargetId = ananyaId, Action = "like", CreatedAt = new DateTime(2024, 1, 24, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000024-0000-0000-0000-000000000024"), SwiperId = ananyaId, TargetId = karthikId, Action = "like", CreatedAt = new DateTime(2024, 1, 24, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000025-0000-0000-0000-000000000025"), SwiperId = rohanId, TargetId = kritikaId, Action = "like", CreatedAt = new DateTime(2024, 1, 26, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000026-0000-0000-0000-000000000026"), SwiperId = kritikaId, TargetId = rohanId, Action = "like", CreatedAt = new DateTime(2024, 1, 26, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000027-0000-0000-0000-000000000027"), SwiperId = mihirId, TargetId = poojaId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 28, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000028-0000-0000-0000-000000000028"), SwiperId = poojaId, TargetId = mihirId, Action = "like", CreatedAt = new DateTime(2024, 1, 28, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000029-0000-0000-0000-000000000029"), SwiperId = aakashId, TargetId = nainaId, Action = "like", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000030-0000-0000-0000-000000000030"), SwiperId = nainaId, TargetId = aakashId, Action = "like", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000031-0000-0000-0000-000000000031"), SwiperId = deepakId, TargetId = nehaId, Action = "like", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000032-0000-0000-0000-000000000032"), SwiperId = sureshId, TargetId = kavyaId, Action = "like", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000033-0000-0000-0000-000000000033"), SwiperId = saurabhId, TargetId = meenaId, Action = "like", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000034-0000-0000-0000-000000000034"), SwiperId = vivekId, TargetId = divyaId, Action = "dislike", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000035-0000-0000-0000-000000000035"), SwiperId = priyankId, TargetId = ankitaId, Action = "like", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000036-0000-0000-0000-000000000036"), SwiperId = ankitId, TargetId = riyaId, Action = "like", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000037-0000-0000-0000-000000000037"), SwiperId = aryanId, TargetId = alishaId, Action = "like", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000038-0000-0000-0000-000000000038"), SwiperId = rajeshId, TargetId = ayeshaId, Action = "dislike", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // ── Matches (15 pairs) ───────────────────────────────────────────
        mb.Entity<Match>().HasData(
            new Match { Id = Guid.Parse("a1000001-0000-0000-0000-000000000001"), User1Id = arjunId, User2Id = aishaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000002-0000-0000-0000-000000000002"), User1Id = rahulId, User2Id = priyaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000003-0000-0000-0000-000000000003"), User1Id = vikramId, User2Id = shreyaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000004-0000-0000-0000-000000000004"), User1Id = nikhilId, User2Id = simranId, IsActive = true, CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000005-0000-0000-0000-000000000005"), User1Id = amanId, User2Id = ishitaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000006-0000-0000-0000-000000000006"), User1Id = adityaId, User2Id = tanviId, IsActive = true, CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000007-0000-0000-0000-000000000007"), User1Id = kabirId, User2Id = ritaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000008-0000-0000-0000-000000000008"), User1Id = devId, User2Id = taraId, IsActive = true, CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000009-0000-0000-0000-000000000009"), User1Id = shivId, User2Id = naliniId, IsActive = true, CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000010-0000-0000-0000-000000000010"), User1Id = jayId, User2Id = sonalId, IsActive = true, CreatedAt = new DateTime(2024, 1, 11, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000011-0000-0000-0000-000000000011"), User1Id = rohitId, User2Id = zaraId, IsActive = true, CreatedAt = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000012-0000-0000-0000-000000000012"), User1Id = karthikId, User2Id = ananyaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 13, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000013-0000-0000-0000-000000000013"), User1Id = rohanId, User2Id = kritikaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000014-0000-0000-0000-000000000014"), User1Id = mihirId, User2Id = poojaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = Guid.Parse("a1000015-0000-0000-0000-000000000015"), User1Id = aakashId, User2Id = nainaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc) }
        );

        // ── Chats ────────────────────────────────────────────────────────
        mb.Entity<Chat>().HasData(
            new Chat { Id = Guid.Parse("a2000001-0000-0000-0000-000000000001"), MatchId = Guid.Parse("a1000001-0000-0000-0000-000000000001"), CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000002-0000-0000-0000-000000000002"), MatchId = Guid.Parse("a1000002-0000-0000-0000-000000000002"), CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000003-0000-0000-0000-000000000003"), MatchId = Guid.Parse("a1000003-0000-0000-0000-000000000003"), CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000004-0000-0000-0000-000000000004"), MatchId = Guid.Parse("a1000004-0000-0000-0000-000000000004"), CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000005-0000-0000-0000-000000000005"), MatchId = Guid.Parse("a1000005-0000-0000-0000-000000000005"), CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000006-0000-0000-0000-000000000006"), MatchId = Guid.Parse("a1000006-0000-0000-0000-000000000006"), CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000007-0000-0000-0000-000000000007"), MatchId = Guid.Parse("a1000007-0000-0000-0000-000000000007"), CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000008-0000-0000-0000-000000000008"), MatchId = Guid.Parse("a1000008-0000-0000-0000-000000000008"), CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000009-0000-0000-0000-000000000009"), MatchId = Guid.Parse("a1000009-0000-0000-0000-000000000009"), CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000010-0000-0000-0000-000000000010"), MatchId = Guid.Parse("a1000010-0000-0000-0000-000000000010"), CreatedAt = new DateTime(2024, 1, 11, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000011-0000-0000-0000-000000000011"), MatchId = Guid.Parse("a1000011-0000-0000-0000-000000000011"), CreatedAt = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000012-0000-0000-0000-000000000012"), MatchId = Guid.Parse("a1000012-0000-0000-0000-000000000012"), CreatedAt = new DateTime(2024, 1, 13, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000013-0000-0000-0000-000000000013"), MatchId = Guid.Parse("a1000013-0000-0000-0000-000000000013"), CreatedAt = new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000014-0000-0000-0000-000000000014"), MatchId = Guid.Parse("a1000014-0000-0000-0000-000000000014"), CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = Guid.Parse("a2000015-0000-0000-0000-000000000015"), MatchId = Guid.Parse("a1000015-0000-0000-0000-000000000015"), CreatedAt = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc) }
        );

        // ── Messages ─────────────────────────────────────────────────────
        mb.Entity<Message>().HasData(
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000001"), ChatId = Guid.Parse("a2000001-0000-0000-0000-000000000001"), SenderId = arjunId, Text = "Hi Aisha! Saw your art profile — absolutely stunning 🎨", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 2, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 2, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000002"), ChatId = Guid.Parse("a2000001-0000-0000-0000-000000000001"), SenderId = aishaId, Text = "Wow thank you so much! Your photography is incredible too 📸", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 2, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 2, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000003"), ChatId = Guid.Parse("a2000001-0000-0000-0000-000000000001"), SenderId = arjunId, Text = "Would love to know more about your art style! Do you paint?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 2, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 2, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000004-0000-0000-0000-000000000004"), ChatId = Guid.Parse("a2000001-0000-0000-0000-000000000001"), SenderId = aishaId, Text = "Yes! Mostly abstract & watercolors 🌊 What's your favorite subject to photograph?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 2, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 2, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000005-0000-0000-0000-000000000005"), ChatId = Guid.Parse("a2000001-0000-0000-0000-000000000001"), SenderId = arjunId, Text = "Landscapes and golden hour portraits 🌅 You'd be a great subject btw 😊", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 2, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 2, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000006-0000-0000-0000-000000000006"), ChatId = Guid.Parse("a2000001-0000-0000-0000-000000000001"), SenderId = aishaId, Text = "Haha smooth! ☺️ I'd love to see your portfolio sometime", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 2, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000007-0000-0000-0000-000000000007"), ChatId = Guid.Parse("a2000001-0000-0000-0000-000000000001"), SenderId = arjunId, Text = "Deal! Coffee date + portfolio exchange? ☕", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 2, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000008-0000-0000-0000-000000000008"), ChatId = Guid.Parse("a2000001-0000-0000-0000-000000000001"), SenderId = aishaId, Text = "Perfect plan! Hyderabad has some amazing cafés 🌸", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 2, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000009-0000-0000-0000-000000000009"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = rahulId, Text = "Hey Priya! We matched 🎉 Music lover here — you dance, I play guitar 🎸", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 3, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 3, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000010-0000-0000-0000-000000000010"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = priyaId, Text = "Hi Rahul! Oh wow guitar + dance = perfect collab! 💃🎵", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 3, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 3, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000011-0000-0000-0000-000000000011"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = rahulId, Text = "Exactly what I was thinking! Hindustani or Western dance?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 3, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 3, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000012-0000-0000-0000-000000000012"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = priyaId, Text = "Kathak ❤️ 15 years of practice! What genres do you play?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 3, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 3, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000013-0000-0000-0000-000000000013"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = rahulId, Text = "Blues & Indie mostly. Would love to see Kathak live someday!", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 3, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 3, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000014-0000-0000-0000-000000000014"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = priyaId, Text = "Come to my next recital 😊 Delhi Habitat Centre next month", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 3, 6, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 3, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000015-0000-0000-0000-000000000015"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = rahulId, Text = "Absolutely! Also — chai date before that? ☕", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 3, 7, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 3, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000016-0000-0000-0000-000000000016"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = priyaId, Text = "I thought you'd never ask! Yes please 🥰", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 3, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000017-0000-0000-0000-000000000017"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = rahulId, Text = "Saturday 3pm? Connaught Place has this tiny amazing chai spot", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 3, 9, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000018-0000-0000-0000-000000000018"), ChatId = Guid.Parse("a2000002-0000-0000-0000-000000000002"), SenderId = priyaId, Text = "It's a date! 💕", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000019-0000-0000-0000-000000000019"), ChatId = Guid.Parse("a2000003-0000-0000-0000-000000000003"), SenderId = vikramId, Text = "Hey Shreya! Entrepreneur meets Doctor — now that's a power match ⚡", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 4, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 4, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000020-0000-0000-0000-000000000020"), ChatId = Guid.Parse("a2000003-0000-0000-0000-000000000003"), SenderId = shreyaId, Text = "Haha love the energy! What kind of startup? 🚀", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 4, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 4, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000021-0000-0000-0000-000000000021"), ChatId = Guid.Parse("a2000003-0000-0000-0000-000000000003"), SenderId = vikramId, Text = "EdTech — making quality education accessible across India 📚", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 4, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 4, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000022-0000-0000-0000-000000000022"), ChatId = Guid.Parse("a2000003-0000-0000-0000-000000000003"), SenderId = shreyaId, Text = "That's genuinely amazing. I'm passionate about healthcare access too 🏥", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 4, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 4, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000023-0000-0000-0000-000000000023"), ChatId = Guid.Parse("a2000003-0000-0000-0000-000000000003"), SenderId = vikramId, Text = "We should talk! Combining EdTech + HealthTech could be huge", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 4, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 4, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000024-0000-0000-0000-000000000024"), ChatId = Guid.Parse("a2000003-0000-0000-0000-000000000003"), SenderId = shreyaId, Text = "You had me at 'making impact' 😄 Tell me more over chai?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 4, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000025-0000-0000-0000-000000000025"), ChatId = Guid.Parse("a2000003-0000-0000-0000-000000000003"), SenderId = vikramId, Text = "Ahmedabad or Delhi? I travel between both", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 4, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000026-0000-0000-0000-000000000026"), ChatId = Guid.Parse("a2000003-0000-0000-0000-000000000003"), SenderId = shreyaId, Text = "Ahmedabad this weekend works! I know the perfect spot 🌸", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 4, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000027-0000-0000-0000-000000000027"), ChatId = Guid.Parse("a2000004-0000-0000-0000-000000000004"), SenderId = nikhilId, Text = "Simran Kaur — lawyer + dancer = most dangerous combo I've ever swiped right on 😄", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 5, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 5, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000028-0000-0000-0000-000000000028"), ChatId = Guid.Parse("a2000004-0000-0000-0000-000000000004"), SenderId = simranId, Text = "Haha I plead guilty 😂 Investment banker + world traveller — not bad either 😏", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 5, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 5, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000029-0000-0000-0000-000000000029"), ChatId = Guid.Parse("a2000004-0000-0000-0000-000000000004"), SenderId = nikhilId, Text = "42 countries down, still searching for the best chai 🍵", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 5, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 5, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000030-0000-0000-0000-000000000030"), ChatId = Guid.Parse("a2000004-0000-0000-0000-000000000004"), SenderId = simranId, Text = "THE AUDACITY — Amritsar has the world's best chai and you know it 😤", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 5, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 5, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000031-0000-0000-0000-000000000031"), ChatId = Guid.Parse("a2000004-0000-0000-0000-000000000004"), SenderId = nikhilId, Text = "Prove it. I'll fly in this weekend 😄", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 5, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 5, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000032-0000-0000-0000-000000000032"), ChatId = Guid.Parse("a2000004-0000-0000-0000-000000000004"), SenderId = simranId, Text = "Challenge accepted. Golden Temple at sunrise first, then chai 🌅", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 5, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000033-0000-0000-0000-000000000033"), ChatId = Guid.Parse("a2000004-0000-0000-0000-000000000004"), SenderId = nikhilId, Text = "You just planned the best morning of my life", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 5, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000034-0000-0000-0000-000000000034"), ChatId = Guid.Parse("a2000004-0000-0000-0000-000000000004"), SenderId = simranId, Text = "Wait till you see the Langar 🙏 Life-changing", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 5, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000035-0000-0000-0000-000000000035"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = amanId, Text = "Ishita! TEDx speaker + startup founder = most impressive profile I've seen 🙌", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 6, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 6, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000036-0000-0000-0000-000000000036"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = ishitaId, Text = "An architect who collects art? I need to see your apartment 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 6, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 6, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000037-0000-0000-0000-000000000037"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = amanId, Text = "Fair warning — it's half gallery half home 🎨🏛️", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 6, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 6, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000038-0000-0000-0000-000000000038"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = ishitaId, Text = "I'm already in love with it 😍 What's your most prized piece?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 6, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 6, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000039-0000-0000-0000-000000000039"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = amanId, Text = "A Hussain original. Took me 3 years to save for it", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 6, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 6, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000040-0000-0000-0000-000000000040"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = ishitaId, Text = "MF Hussain?! That's incredible. I spoke about his legacy at TEDx!", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 6, 6, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 6, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000041-0000-0000-0000-000000000041"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = amanId, Text = "No way! Which TED? I might have watched that talk!", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 6, 7, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 6, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000042-0000-0000-0000-000000000042"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = ishitaId, Text = "TEDxDelhi 2023 — Art as a mirror of society", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 6, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000043-0000-0000-0000-000000000043"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = amanId, Text = "I DID watch that! The part about street art was phenomenal", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 6, 9, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000044-0000-0000-0000-000000000044"), ChatId = Guid.Parse("a2000005-0000-0000-0000-000000000005"), SenderId = ishitaId, Text = "Oh my god! Coffee. Now. We have so much to talk about ☕", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 6, 10, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000045-0000-0000-0000-000000000045"), ChatId = Guid.Parse("a2000006-0000-0000-0000-000000000006"), SenderId = adityaId, Text = "Hey Tanvi! Designer + plant mom = my two favorite things 🌿🎨", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 7, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 7, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000046-0000-0000-0000-000000000046"), ChatId = Guid.Parse("a2000006-0000-0000-0000-000000000006"), SenderId = tanviId, Text = "A data scientist who rides? Bold combo 🏍️📊", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 7, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 7, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000047-0000-0000-0000-000000000047"), ChatId = Guid.Parse("a2000006-0000-0000-0000-000000000006"), SenderId = adityaId, Text = "Numbers by day, open roads by night 🌙 Any design philosophy you live by?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 7, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 7, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000048-0000-0000-0000-000000000048"), ChatId = Guid.Parse("a2000006-0000-0000-0000-000000000006"), SenderId = tanviId, Text = "'Good design is invisible.' — You feel it without thinking about it", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 7, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 7, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000049-0000-0000-0000-000000000049"), ChatId = Guid.Parse("a2000006-0000-0000-0000-000000000006"), SenderId = adityaId, Text = "That's exactly how data stories should work — you get it without effort", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 7, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 7, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000050-0000-0000-0000-000000000050"), ChatId = Guid.Parse("a2000006-0000-0000-0000-000000000006"), SenderId = tanviId, Text = "We're the same kind of nerd 😄 I like you", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 7, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000051-0000-0000-0000-000000000051"), ChatId = Guid.Parse("a2000006-0000-0000-0000-000000000006"), SenderId = adityaId, Text = "Pune roads on a bike this Sunday? I'll show you the ghats", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 7, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000052-0000-0000-0000-000000000052"), ChatId = Guid.Parse("a2000006-0000-0000-0000-000000000006"), SenderId = tanviId, Text = "YESSS! I've been wanting to do the Sinhagad trail 🌄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 7, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000053-0000-0000-0000-000000000053"), ChatId = Guid.Parse("a2000007-0000-0000-0000-000000000007"), SenderId = kabirId, Text = "Rita! Finance + Yoga — the most balanced person alive 😄", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 8, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 8, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000054-0000-0000-0000-000000000054"), ChatId = Guid.Parse("a2000007-0000-0000-0000-000000000007"), SenderId = ritaId, Text = "Haha healing bodies through yoga and portfolios through finance 😂", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 8, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 8, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000055-0000-0000-0000-000000000055"), ChatId = Guid.Parse("a2000007-0000-0000-0000-000000000007"), SenderId = kabirId, Text = "I'm an ortho surgeon — we're both in the fixing business!", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 8, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 8, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000056-0000-0000-0000-000000000056"), ChatId = Guid.Parse("a2000007-0000-0000-0000-000000000007"), SenderId = ritaId, Text = "Ha! True! Though I suspect you enjoy the dramatic before-after more 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 8, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 8, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000057-0000-0000-0000-000000000057"), ChatId = Guid.Parse("a2000007-0000-0000-0000-000000000007"), SenderId = kabirId, Text = "Guilty 😂 How's Surat treating you?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 8, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 8, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000058-0000-0000-0000-000000000058"), ChatId = Guid.Parse("a2000007-0000-0000-0000-000000000007"), SenderId = ritaId, Text = "Wonderful! Diamond city has hidden gems. You should visit", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 8, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000059-0000-0000-0000-000000000059"), ChatId = Guid.Parse("a2000007-0000-0000-0000-000000000007"), SenderId = kabirId, Text = "Book me a yoga class and I'll bring the chai ☕", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 8, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000060-0000-0000-0000-000000000060"), ChatId = Guid.Parse("a2000007-0000-0000-0000-000000000007"), SenderId = ritaId, Text = "Deal! Sunrise session? My rooftop shala has the best views 🌅", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 8, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000061-0000-0000-0000-000000000061"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = devId, Text = "Tara! Neurosurgeon + Bharatanatyam dancer — I'm genuinely in awe 🧠💃", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 9, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 9, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000062-0000-0000-0000-000000000062"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = taraId, Text = "Tech founder + angel investor — not exactly a slouch yourself 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 9, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 9, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000063-0000-0000-0000-000000000063"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = devId, Text = "Haha flattery noted. How do you balance neurosurgery and dance?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 9, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 9, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000064-0000-0000-0000-000000000064"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = taraId, Text = "Discipline. Both require absolute precision and full presence 🙏", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 9, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 9, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000065-0000-0000-0000-000000000065"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = devId, Text = "That's profound. Do you see similarities in the movements?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 9, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 9, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000066-0000-0000-0000-000000000066"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = taraId, Text = "Interesting you ask — I wrote a paper on neuroplasticity and classical dance", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 9, 6, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 9, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000067-0000-0000-0000-000000000067"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = devId, Text = "I would genuinely love to read that", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 9, 7, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 9, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000068-0000-0000-0000-000000000068"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = taraId, Text = "I'll email it if you promise to actually read it 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 9, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000069-0000-0000-0000-000000000069"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = devId, Text = "Scout's honour. Chennai this month? I have a board meeting there", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 9, 9, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000070-0000-0000-0000-000000000070"), ChatId = Guid.Parse("a2000008-0000-0000-0000-000000000008"), SenderId = taraId, Text = "Perfect timing! My next recital is the 25th 💃", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 9, 10, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000071-0000-0000-0000-000000000071"), ChatId = Guid.Parse("a2000009-0000-0000-0000-000000000009"), SenderId = shivId, Text = "Nalini! PM at BigTech + traveller — someone who builds AND explores 🚀", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 10, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 10, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000072-0000-0000-0000-000000000072"), ChatId = Guid.Parse("a2000009-0000-0000-0000-000000000009"), SenderId = naliniId, Text = "Retired army officer turned mountaineer — the most disciplined swipe I've gotten 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 10, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 10, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000073-0000-0000-0000-000000000073"), ChatId = Guid.Parse("a2000009-0000-0000-0000-000000000009"), SenderId = shivId, Text = "15 years of service teaches you that mountains and deadlines are equally unforgiving 😂", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 10, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 10, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000074-0000-0000-0000-000000000074"), ChatId = Guid.Parse("a2000009-0000-0000-0000-000000000009"), SenderId = naliniId, Text = "Haha that's the most army thing I've heard today! Which peaks?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 10, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 10, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000075-0000-0000-0000-000000000075"), ChatId = Guid.Parse("a2000009-0000-0000-0000-000000000009"), SenderId = shivId, Text = "Stok Kangri, Friendship Peak, Kang Yatze. Everest BC is next 🏔️", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 10, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 10, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000076-0000-0000-0000-000000000076"), ChatId = Guid.Parse("a2000009-0000-0000-0000-000000000009"), SenderId = naliniId, Text = "BASE CAMP?! I've always wanted to do EBC! Have you done Roopkund?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 10, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000077-0000-0000-0000-000000000077"), ChatId = Guid.Parse("a2000009-0000-0000-0000-000000000009"), SenderId = shivId, Text = "3 times. Let me know when you want company 💪", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 10, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000078-0000-0000-0000-000000000078"), ChatId = Guid.Parse("a2000009-0000-0000-0000-000000000009"), SenderId = naliniId, Text = "Is this the most epic 'let's hang out' I've ever received? Yes 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 10, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000079-0000-0000-0000-000000000079"), ChatId = Guid.Parse("a2000010-0000-0000-0000-000000000010"), SenderId = jayId, Text = "Sonal! IB + marathoner — making money AND miles 😄", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 11, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 11, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000080-0000-0000-0000-000000000080"), ChatId = Guid.Parse("a2000010-0000-0000-0000-000000000010"), SenderId = sonalId, Text = "Real estate mogul + gym addict — building empires inside and out 😂", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 11, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 11, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000081-0000-0000-0000-000000000081"), ChatId = Guid.Parse("a2000010-0000-0000-0000-000000000010"), SenderId = jayId, Text = "Mumbai vs Ahmedabad — the eternal rivalry continues 😄", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 11, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 11, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000082-0000-0000-0000-000000000082"), ChatId = Guid.Parse("a2000010-0000-0000-0000-000000000010"), SenderId = sonalId, Text = "No contest — Mumbai's pace, Ahmedabad's food 🍛", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 11, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 11, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000083-0000-0000-0000-000000000083"), ChatId = Guid.Parse("a2000010-0000-0000-0000-000000000010"), SenderId = jayId, Text = "I'll accept that compromise. What's your marathon PR?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 11, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 11, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000084-0000-0000-0000-000000000084"), ChatId = Guid.Parse("a2000010-0000-0000-0000-000000000010"), SenderId = sonalId, Text = "3:42 at Mumbai Marathon 2024! You?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 11, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000085-0000-0000-0000-000000000085"), ChatId = Guid.Parse("a2000010-0000-0000-0000-000000000010"), SenderId = jayId, Text = "4:01. You'd destroy me 😂 Train me?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 11, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000086-0000-0000-0000-000000000086"), ChatId = Guid.Parse("a2000010-0000-0000-0000-000000000010"), SenderId = sonalId, Text = "Only if you show me Ahmedabad's best properties in return 🏠", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 11, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000087-0000-0000-0000-000000000087"), ChatId = Guid.Parse("a2000011-0000-0000-0000-000000000011"), SenderId = rohitId, Text = "Zara! A model who loves photography — do you ever end up on both sides of the lens?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 12, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 12, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000088-0000-0000-0000-000000000088"), ChatId = Guid.Parse("a2000011-0000-0000-0000-000000000011"), SenderId = zaraId, Text = "ALL the time 😄 Chef who food-blogs — tell me you make restaurant-quality food at home!", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 12, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 12, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000089-0000-0000-0000-000000000089"), ChatId = Guid.Parse("a2000011-0000-0000-0000-000000000011"), SenderId = rohitId, Text = "Michelin-star aspirations, home kitchen reality 😂 I'll cook for you anytime", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 12, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 12, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000090-0000-0000-0000-000000000090"), ChatId = Guid.Parse("a2000011-0000-0000-0000-000000000011"), SenderId = zaraId, Text = "This is the best offer I've received on this app 🍽️", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 12, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 12, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000091-0000-0000-0000-000000000091"), ChatId = Guid.Parse("a2000011-0000-0000-0000-000000000011"), SenderId = rohitId, Text = "What's your favorite cuisine? I'll recreate it", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 12, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 12, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000092-0000-0000-0000-000000000092"), ChatId = Guid.Parse("a2000011-0000-0000-0000-000000000011"), SenderId = zaraId, Text = "Japanese! I'm obsessed with omakase 🍣", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 12, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000093-0000-0000-0000-000000000093"), ChatId = Guid.Parse("a2000011-0000-0000-0000-000000000011"), SenderId = rohitId, Text = "Challenge accepted. Saturday? I'll do a 7-course omakase at home", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 12, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000094-0000-0000-0000-000000000094"), ChatId = Guid.Parse("a2000011-0000-0000-0000-000000000011"), SenderId = zaraId, Text = "You're either insane or incredible. Either way, I'm in 🙌", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 12, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000095-0000-0000-0000-000000000095"), ChatId = Guid.Parse("a2000012-0000-0000-0000-000000000012"), SenderId = karthikId, Text = "Ananya! Wildlife photographer — the patience that must require! 🦁", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 13, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 13, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000096-0000-0000-0000-000000000096"), ChatId = Guid.Parse("a2000012-0000-0000-0000-000000000012"), SenderId = ananyaId, Text = "IIT Madras + startup founder — you built something from scratch, I know patience 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 13, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 13, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000097-0000-0000-0000-000000000097"), ChatId = Guid.Parse("a2000012-0000-0000-0000-000000000012"), SenderId = karthikId, Text = "Haha fair point! Have you done the Sundarbans?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 13, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 13, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000098-0000-0000-0000-000000000098"), ChatId = Guid.Parse("a2000012-0000-0000-0000-000000000012"), SenderId = ananyaId, Text = "3 times! The Bengal tigers are unreal 🐯 Have you?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 13, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 13, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000099-0000-0000-0000-000000000099"), ChatId = Guid.Parse("a2000012-0000-0000-0000-000000000012"), SenderId = karthikId, Text = "Never! That needs to change. Would you guide me?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 13, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 13, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000100-0000-0000-0000-000000000100"), ChatId = Guid.Parse("a2000012-0000-0000-0000-000000000012"), SenderId = ananyaId, Text = "Only if you tell me what startup you built 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 13, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000101-0000-0000-0000-000000000101"), ChatId = Guid.Parse("a2000012-0000-0000-0000-000000000012"), SenderId = karthikId, Text = "EdTech platform — 2M students now. But tigers > everything 🐯", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 13, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000102-0000-0000-0000-000000000102"), ChatId = Guid.Parse("a2000012-0000-0000-0000-000000000012"), SenderId = ananyaId, Text = "2 MILLION?! Okay you're impressive. Sundarbans next month, deal? 🤝", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 13, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000103-0000-0000-0000-000000000103"), ChatId = Guid.Parse("a2000013-0000-0000-0000-000000000013"), SenderId = rohanId, Text = "Kritika! Journalist + filmmaker — you're literally a storytelling machine 📽️", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 14, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 14, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000104-0000-0000-0000-000000000104"), ChatId = Guid.Parse("a2000013-0000-0000-0000-000000000013"), SenderId = kritikaId, Text = "A documentary filmmaker who shoots street photography — kindred spirit! 📸", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 14, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 14, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000105-0000-0000-0000-000000000105"), ChatId = Guid.Parse("a2000013-0000-0000-0000-000000000013"), SenderId = rohanId, Text = "What's your beat? I cover social issues mostly", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 14, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 14, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000106-0000-0000-0000-000000000106"), ChatId = Guid.Parse("a2000013-0000-0000-0000-000000000013"), SenderId = kritikaId, Text = "Human interest stories. The invisible lives of visible cities", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 14, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 14, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000107-0000-0000-0000-000000000107"), ChatId = Guid.Parse("a2000013-0000-0000-0000-000000000013"), SenderId = rohanId, Text = "That's EXACTLY what I film. Have you covered Dharavi?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 14, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 14, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000108-0000-0000-0000-000000000108"), ChatId = Guid.Parse("a2000013-0000-0000-0000-000000000013"), SenderId = kritikaId, Text = "It was my first published piece 5 years ago! Changed everything for me", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 14, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000109-0000-0000-0000-000000000109"), ChatId = Guid.Parse("a2000013-0000-0000-0000-000000000013"), SenderId = rohanId, Text = "Mine too — that's wild! Kolkata coffee + story exchange?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 14, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000110-0000-0000-0000-000000000110"), ChatId = Guid.Parse("a2000013-0000-0000-0000-000000000013"), SenderId = kritikaId, Text = "Blue Poppy Café, Saturday 4pm. Don't be late 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 14, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000111-0000-0000-0000-000000000111"), ChatId = Guid.Parse("a2000014-0000-0000-0000-000000000014"), SenderId = mihirId, Text = "Pooja! Marketing + bookworm — the rarest, most underrated combo 📚", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 15, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 15, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000112-0000-0000-0000-000000000112"), ChatId = Guid.Parse("a2000014-0000-0000-0000-000000000014"), SenderId = poojaId, Text = "CA + cricketer — numbers AND wickets?! Respect 🏏", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 15, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 15, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000113-0000-0000-0000-000000000113"), ChatId = Guid.Parse("a2000014-0000-0000-0000-000000000014"), SenderId = mihirId, Text = "I'm better at accounts than cricket tbh 😂 What are you reading?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 15, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 15, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000114-0000-0000-0000-000000000114"), ChatId = Guid.Parse("a2000014-0000-0000-0000-000000000014"), SenderId = poojaId, Text = "'Atomic Habits' for the 3rd time 📖 Each read hits different", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 15, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 15, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000115-0000-0000-0000-000000000115"), ChatId = Guid.Parse("a2000014-0000-0000-0000-000000000014"), SenderId = mihirId, Text = "That book genuinely changed my practice. The 1% rule is everything", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 15, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 15, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000116-0000-0000-0000-000000000116"), ChatId = Guid.Parse("a2000014-0000-0000-0000-000000000014"), SenderId = poojaId, Text = "YES! I applied it to my morning routine and productivity tripled", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 15, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000117-0000-0000-0000-000000000117"), ChatId = Guid.Parse("a2000014-0000-0000-0000-000000000014"), SenderId = mihirId, Text = "We need to do a book club. Jaipur has amazing bookshops", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 15, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000118-0000-0000-0000-000000000118"), ChatId = Guid.Parse("a2000014-0000-0000-0000-000000000014"), SenderId = poojaId, Text = "Anokhi Café has a reading corner — next Sunday? 🌸", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000119-0000-0000-0000-000000000119"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = aakashId, Text = "Naina! Cardiologist + marathoner — literally the healthiest person alive 🏃❤️", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 16, 1, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 16, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000120-0000-0000-0000-000000000120"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = nainaId, Text = "Pilot + astronomer — you're both above the clouds AND studying them 😄", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 16, 2, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 16, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000121-0000-0000-0000-000000000121"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = aakashId, Text = "Haha occupational perk! Have you ever done cardiac surgery at altitude?", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 16, 3, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 16, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000122-0000-0000-0000-000000000122"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = nainaId, Text = "Not yet but HAPE is a real risk I've studied for high-altitude treks 🏔️", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 16, 4, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 16, 5, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000123-0000-0000-0000-000000000123"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = aakashId, Text = "I have a patient who did Everest after a stent — most inspiring story", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 16, 5, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 16, 6, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000124-0000-0000-0000-000000000124"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = nainaId, Text = "That's incredible. Medicine has the best stories", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 16, 6, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 16, 7, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000125-0000-0000-0000-000000000125"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = aakashId, Text = "Agreed. Cockpit date? I can show you Delhi from 1000ft AGL 🌃", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 16, 7, 0, 0, DateTimeKind.Utc), ReadAt = new DateTime(2024, 1, 16, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000126-0000-0000-0000-000000000126"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = nainaId, Text = "Is that even legal? 😂", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 16, 8, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000127-0000-0000-0000-000000000127"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = aakashId, Text = "Simulator only 😄 But the view is just as good", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 16, 9, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000128-0000-0000-0000-000000000128"), ChatId = Guid.Parse("a2000015-0000-0000-0000-000000000015"), SenderId = nainaId, Text = "You had me at cockpit 😄 Yes please!", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 16, 10, 0, 0, DateTimeKind.Utc) }
        );

        // ── Call Sessions (20 calls — all statuses) ─────────────────────
        mb.Entity<CallSession>().HasData(
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000001"), CallerId = arjunId, ReceiverId = aishaId, MatchId = Guid.Parse("a1000001-0000-0000-0000-000000000001"), CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 3, 10, 7, 0, DateTimeKind.Utc), DurationSeconds = 420, CoinsDeducted = 700, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 3, 10, 7, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000002-0000-0000-0000-000000000002"), CallerId = rahulId, ReceiverId = priyaId, MatchId = Guid.Parse("a1000002-0000-0000-0000-000000000002"), CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024, 1, 3, 14, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 3, 14, 5, 0, DateTimeKind.Utc), DurationSeconds = 300, CoinsDeducted = 50, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 3, 14, 5, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000003-0000-0000-0000-000000000003"), CallerId = vikramId, ReceiverId = shreyaId, MatchId = Guid.Parse("a1000003-0000-0000-0000-000000000003"), CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024, 1, 4, 11, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 4, 11, 3, 0, DateTimeKind.Utc), DurationSeconds = 180, CoinsDeducted = 30, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 4, 11, 3, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000004-0000-0000-0000-000000000004"), CallerId = nikhilId, ReceiverId = simranId, MatchId = Guid.Parse("a1000004-0000-0000-0000-000000000004"), CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 5, 19, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 5, 19, 15, 0, DateTimeKind.Utc), DurationSeconds = 900, CoinsDeducted = 1500, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 5, 19, 15, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000005-0000-0000-0000-000000000005"), CallerId = amanId, ReceiverId = ishitaId, MatchId = Guid.Parse("a1000005-0000-0000-0000-000000000005"), CallType = "audio", Status = "declined", EndedAt = new DateTime(2024, 1, 6, 20, 0, 0, DateTimeKind.Utc), DurationSeconds = 0, CoinsDeducted = 0, EndReason = "declined", CreatedAt = new DateTime(2024, 1, 6, 20, 0, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000006-0000-0000-0000-000000000006"), CallerId = adityaId, ReceiverId = tanviId, MatchId = Guid.Parse("a1000006-0000-0000-0000-000000000006"), CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024, 1, 7, 18, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 7, 18, 8, 0, DateTimeKind.Utc), DurationSeconds = 480, CoinsDeducted = 80, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 7, 18, 8, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000007-0000-0000-0000-000000000007"), CallerId = kabirId, ReceiverId = ritaId, MatchId = Guid.Parse("a1000007-0000-0000-0000-000000000007"), CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 8, 21, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 8, 21, 12, 0, DateTimeKind.Utc), DurationSeconds = 720, CoinsDeducted = 1200, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 8, 21, 12, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000008-0000-0000-0000-000000000008"), CallerId = devId, ReceiverId = taraId, MatchId = Guid.Parse("a1000008-0000-0000-0000-000000000008"), CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 9, 20, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 9, 20, 25, 0, DateTimeKind.Utc), DurationSeconds = 1500, CoinsDeducted = 2500, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 9, 20, 25, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000009-0000-0000-0000-000000000009"), CallerId = shivId, ReceiverId = naliniId, MatchId = Guid.Parse("a1000009-0000-0000-0000-000000000009"), CallType = "audio", Status = "timeout", EndedAt = new DateTime(2024, 1, 10, 9, 0, 0, DateTimeKind.Utc), DurationSeconds = 0, CoinsDeducted = 0, EndReason = "no_answer", CreatedAt = new DateTime(2024, 1, 10, 9, 0, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000010-0000-0000-0000-000000000010"), CallerId = jayId, ReceiverId = sonalId, MatchId = Guid.Parse("a1000010-0000-0000-0000-000000000010"), CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024, 1, 11, 17, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 11, 17, 4, 0, DateTimeKind.Utc), DurationSeconds = 240, CoinsDeducted = 40, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 11, 17, 4, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000011-0000-0000-0000-000000000011"), CallerId = rohitId, ReceiverId = zaraId, MatchId = Guid.Parse("a1000011-0000-0000-0000-000000000011"), CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 12, 20, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 12, 20, 6, 0, DateTimeKind.Utc), DurationSeconds = 360, CoinsDeducted = 600, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 12, 20, 6, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000012-0000-0000-0000-000000000012"), CallerId = karthikId, ReceiverId = ananyaId, MatchId = Guid.Parse("a1000012-0000-0000-0000-000000000012"), CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024, 1, 13, 15, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 13, 15, 10, 0, DateTimeKind.Utc), DurationSeconds = 600, CoinsDeducted = 100, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 13, 15, 10, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000013-0000-0000-0000-000000000013"), CallerId = rohanId, ReceiverId = kritikaId, MatchId = Guid.Parse("a1000013-0000-0000-0000-000000000013"), CallType = "audio", Status = "cancelled", EndedAt = new DateTime(2024, 1, 14, 12, 0, 0, DateTimeKind.Utc), DurationSeconds = 0, CoinsDeducted = 0, EndReason = "cancelled", CreatedAt = new DateTime(2024, 1, 14, 12, 0, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000014-0000-0000-0000-000000000014"), CallerId = mihirId, ReceiverId = poojaId, MatchId = Guid.Parse("a1000014-0000-0000-0000-000000000014"), CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024, 1, 15, 19, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 15, 19, 6, 0, DateTimeKind.Utc), DurationSeconds = 360, CoinsDeducted = 60, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 15, 19, 6, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000015-0000-0000-0000-000000000015"), CallerId = aakashId, ReceiverId = nainaId, MatchId = Guid.Parse("a1000015-0000-0000-0000-000000000015"), CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 16, 21, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 16, 21, 10, 0, DateTimeKind.Utc), DurationSeconds = 600, CoinsDeducted = 1000, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 16, 21, 10, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000016-0000-0000-0000-000000000016"), CallerId = priyaId, ReceiverId = rahulId, MatchId = Guid.Parse("a1000002-0000-0000-0000-000000000002"), CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024, 1, 17, 15, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 17, 15, 7, 0, DateTimeKind.Utc), DurationSeconds = 420, CoinsDeducted = 70, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 17, 15, 7, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000017-0000-0000-0000-000000000017"), CallerId = aishaId, ReceiverId = arjunId, MatchId = Guid.Parse("a1000001-0000-0000-0000-000000000001"), CallType = "video", Status = "declined", EndedAt = new DateTime(2024, 1, 18, 10, 0, 0, DateTimeKind.Utc), DurationSeconds = 0, CoinsDeducted = 0, EndReason = "declined", CreatedAt = new DateTime(2024, 1, 18, 10, 0, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000018-0000-0000-0000-000000000018"), CallerId = simranId, ReceiverId = nikhilId, MatchId = Guid.Parse("a1000004-0000-0000-0000-000000000004"), CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 19, 20, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 19, 20, 20, 0, DateTimeKind.Utc), DurationSeconds = 1200, CoinsDeducted = 2000, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 19, 20, 20, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000019-0000-0000-0000-000000000019"), CallerId = shreyaId, ReceiverId = vikramId, MatchId = Guid.Parse("a1000003-0000-0000-0000-000000000003"), CallType = "audio", Status = "timeout", EndedAt = new DateTime(2024, 1, 20, 11, 0, 0, DateTimeKind.Utc), DurationSeconds = 0, CoinsDeducted = 0, EndReason = "no_answer", CreatedAt = new DateTime(2024, 1, 20, 11, 0, 0, DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000020-0000-0000-0000-000000000020"), CallerId = ishitaId, ReceiverId = amanId, MatchId = Guid.Parse("a1000005-0000-0000-0000-000000000005"), CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 21, 19, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 21, 19, 30, 0, DateTimeKind.Utc), DurationSeconds = 1800, CoinsDeducted = 3000, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 21, 19, 30, 0, DateTimeKind.Utc) }
        );

        // ── Notifications ─────────────────────────────────────────────────
        mb.Entity<Notification>().HasData(
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000001"), UserId = arjunId, Title = "New Match! 🎉", Body = "You matched with Aisha Khan!", Type = "match", IsRead = false, ReferenceId = "a1000001-0000-0000-0000-000000000001", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000002-0000-0000-0000-000000000002"), UserId = aishaId, Title = "New Match! 🎉", Body = "You matched with Arjun Singh!", Type = "match", IsRead = true, ReferenceId = "a1000001-0000-0000-0000-000000000001", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000003-0000-0000-0000-000000000003"), UserId = rahulId, Title = "New Match! 🎉", Body = "You matched with Priya Sharma!", Type = "match", IsRead = false, ReferenceId = "a1000002-0000-0000-0000-000000000002", CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000004-0000-0000-0000-000000000004"), UserId = priyaId, Title = "New Match! 🎉", Body = "You matched with Rahul Mehta!", Type = "match", IsRead = false, ReferenceId = "a1000002-0000-0000-0000-000000000002", CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000005-0000-0000-0000-000000000005"), UserId = vikramId, Title = "New Match! 🎉", Body = "You matched with Shreya Patel!", Type = "match", IsRead = false, ReferenceId = "a1000003-0000-0000-0000-000000000003", CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000006-0000-0000-0000-000000000006"), UserId = shreyaId, Title = "New Match! 🎉", Body = "You matched with Vikram Nair!", Type = "match", IsRead = true, ReferenceId = "a1000003-0000-0000-0000-000000000003", CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000007-0000-0000-0000-000000000007"), UserId = nikhilId, Title = "New Match! 🎉", Body = "You matched with Simran Kaur!", Type = "match", IsRead = false, ReferenceId = "a1000004-0000-0000-0000-000000000004", CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000008-0000-0000-0000-000000000008"), UserId = simranId, Title = "New Match! 🎉", Body = "You matched with Nikhil Sharma!", Type = "match", IsRead = false, ReferenceId = "a1000004-0000-0000-0000-000000000004", CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000009-0000-0000-0000-000000000009"), UserId = amanId, Title = "New Match! 🎉", Body = "You matched with Ishita Sharma!", Type = "match", IsRead = false, ReferenceId = "a1000005-0000-0000-0000-000000000005", CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000010-0000-0000-0000-000000000010"), UserId = ishitaId, Title = "New Match! 🎉", Body = "You matched with Aman Joshi!", Type = "match", IsRead = true, ReferenceId = "a1000005-0000-0000-0000-000000000005", CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000011-0000-0000-0000-000000000011"), UserId = adityaId, Title = "New Match! 🎉", Body = "You matched with Tanvi Joshi!", Type = "match", IsRead = false, ReferenceId = "a1000006-0000-0000-0000-000000000006", CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000012-0000-0000-0000-000000000012"), UserId = tanviId, Title = "New Match! 🎉", Body = "You matched with Aditya Kumar!", Type = "match", IsRead = false, ReferenceId = "a1000006-0000-0000-0000-000000000006", CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000013-0000-0000-0000-000000000013"), UserId = kabirId, Title = "New Match! 🎉", Body = "You matched with Rita Desai!", Type = "match", IsRead = false, ReferenceId = "a1000007-0000-0000-0000-000000000007", CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000014-0000-0000-0000-000000000014"), UserId = ritaId, Title = "New Match! 🎉", Body = "You matched with Kabir Singh!", Type = "match", IsRead = true, ReferenceId = "a1000007-0000-0000-0000-000000000007", CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000015-0000-0000-0000-000000000015"), UserId = devId, Title = "New Match! 🎉", Body = "You matched with Tara Pillai!", Type = "match", IsRead = false, ReferenceId = "a1000008-0000-0000-0000-000000000008", CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000016-0000-0000-0000-000000000016"), UserId = taraId, Title = "New Match! 🎉", Body = "You matched with Dev Malhotra!", Type = "match", IsRead = false, ReferenceId = "a1000008-0000-0000-0000-000000000008", CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000017-0000-0000-0000-000000000017"), UserId = shivId, Title = "New Match! 🎉", Body = "You matched with Nalini Krishnan!", Type = "match", IsRead = false, ReferenceId = "a1000009-0000-0000-0000-000000000009", CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000018-0000-0000-0000-000000000018"), UserId = naliniId, Title = "New Match! 🎉", Body = "You matched with Shiv Kumar!", Type = "match", IsRead = true, ReferenceId = "a1000009-0000-0000-0000-000000000009", CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000019-0000-0000-0000-000000000019"), UserId = jayId, Title = "New Match! 🎉", Body = "You matched with Sonal Mehta!", Type = "match", IsRead = false, ReferenceId = "a1000010-0000-0000-0000-000000000010", CreatedAt = new DateTime(2024, 1, 11, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000020-0000-0000-0000-000000000020"), UserId = sonalId, Title = "New Match! 🎉", Body = "You matched with Jay Patel!", Type = "match", IsRead = false, ReferenceId = "a1000010-0000-0000-0000-000000000010", CreatedAt = new DateTime(2024, 1, 11, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000021-0000-0000-0000-000000000021"), UserId = rohitId, Title = "New Match! 🎉", Body = "You matched with Zara Ahmed!", Type = "match", IsRead = false, ReferenceId = "a1000011-0000-0000-0000-000000000011", CreatedAt = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000022-0000-0000-0000-000000000022"), UserId = zaraId, Title = "New Match! 🎉", Body = "You matched with Rohit Sharma!", Type = "match", IsRead = true, ReferenceId = "a1000011-0000-0000-0000-000000000011", CreatedAt = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000023-0000-0000-0000-000000000023"), UserId = karthikId, Title = "New Match! 🎉", Body = "You matched with Ananya Chatterjee!", Type = "match", IsRead = false, ReferenceId = "a1000012-0000-0000-0000-000000000012", CreatedAt = new DateTime(2024, 1, 13, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000024-0000-0000-0000-000000000024"), UserId = ananyaId, Title = "New Match! 🎉", Body = "You matched with Karthik Menon!", Type = "match", IsRead = false, ReferenceId = "a1000012-0000-0000-0000-000000000012", CreatedAt = new DateTime(2024, 1, 13, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000025-0000-0000-0000-000000000025"), UserId = rohanId, Title = "New Match! 🎉", Body = "You matched with Kritika Bose!", Type = "match", IsRead = false, ReferenceId = "a1000013-0000-0000-0000-000000000013", CreatedAt = new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000026-0000-0000-0000-000000000026"), UserId = kritikaId, Title = "New Match! 🎉", Body = "You matched with Rohan Bose!", Type = "match", IsRead = true, ReferenceId = "a1000013-0000-0000-0000-000000000013", CreatedAt = new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000027-0000-0000-0000-000000000027"), UserId = mihirId, Title = "New Match! 🎉", Body = "You matched with Pooja Gupta!", Type = "match", IsRead = false, ReferenceId = "a1000014-0000-0000-0000-000000000014", CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000028-0000-0000-0000-000000000028"), UserId = poojaId, Title = "New Match! 🎉", Body = "You matched with Mihir Shah!", Type = "match", IsRead = false, ReferenceId = "a1000014-0000-0000-0000-000000000014", CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000029-0000-0000-0000-000000000029"), UserId = aakashId, Title = "New Match! 🎉", Body = "You matched with Naina Verma!", Type = "match", IsRead = false, ReferenceId = "a1000015-0000-0000-0000-000000000015", CreatedAt = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000030-0000-0000-0000-000000000030"), UserId = nainaId, Title = "New Match! 🎉", Body = "You matched with Aakash Verma!", Type = "match", IsRead = true, ReferenceId = "a1000015-0000-0000-0000-000000000015", CreatedAt = new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000031-0000-0000-0000-000000000031"), UserId = rahulId, Title = "New Message 💬", Body = "Priya sent you a message 💌", Type = "message", IsRead = false, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000032-0000-0000-0000-000000000032"), UserId = priyaId, Title = "Welcome Bonus 🪙", Body = "+100 coins added to your wallet", Type = "coins", IsRead = false, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000033-0000-0000-0000-000000000033"), UserId = nikhilId, Title = "Low Balance ⚠️", Body = "Your coin balance is below 500 coins", Type = "system", IsRead = false, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000034-0000-0000-0000-000000000034"), UserId = devId, Title = "Profile Verified ✅", Body = "Your identity has been verified", Type = "system", IsRead = false, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000035-0000-0000-0000-000000000035"), UserId = deepakId, Title = "Profile Incomplete ⚠️", Body = "Add a bio to attract more matches!", Type = "system", IsRead = false, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000036-0000-0000-0000-000000000036"), UserId = shivId, Title = "Super Like Received ⭐", Body = "Someone sent you a super like!", Type = "like", IsRead = false, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // ── Coin Transactions ────────────────────────────────────────────
        mb.Entity<CoinTransaction>().HasData(
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000001"), UserId = rahulId, Coins = 100, Direction = "credit", Description = "Welcome bonus", TransactionType = "welcome", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000002-0000-0000-0000-000000000002"), UserId = rahulId, Coins = 5000, Direction = "credit", Description = "Deposit — 5000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 1, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000003-0000-0000-0000-000000000003"), UserId = rahulId, Coins = 50, Direction = "debit", Description = "Audio call · 5 min", TransactionType = "call", CreatedAt = new DateTime(2024, 1, 3, 14, 5, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000004-0000-0000-0000-000000000004"), UserId = arjunId, Coins = 10000, Direction = "credit", Description = "Deposit — 10000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000005-0000-0000-0000-000000000005"), UserId = arjunId, Coins = 700, Direction = "debit", Description = "Video call · 7 min", TransactionType = "call", CreatedAt = new DateTime(2024, 1, 3, 10, 7, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000006-0000-0000-0000-000000000006"), UserId = nikhilId, Coins = 10000, Direction = "credit", Description = "Deposit — 10000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000007-0000-0000-0000-000000000007"), UserId = nikhilId, Coins = 1500, Direction = "debit", Description = "Video call · 15 min", TransactionType = "call", CreatedAt = new DateTime(2024, 1, 5, 19, 15, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000008-0000-0000-0000-000000000008"), UserId = devId, Coins = 100, Direction = "credit", Description = "Welcome bonus", TransactionType = "welcome", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000009-0000-0000-0000-000000000009"), UserId = devId, Coins = 10000, Direction = "credit", Description = "Deposit — 10000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000010-0000-0000-0000-000000000010"), UserId = priyaId, Coins = 100, Direction = "credit", Description = "Welcome bonus", TransactionType = "welcome", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000011-0000-0000-0000-000000000011"), UserId = priyaId, Coins = 50, Direction = "credit", Description = "Verification bonus", TransactionType = "verification", CreatedAt = new DateTime(2024, 1, 1, 2, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000012-0000-0000-0000-000000000012"), UserId = aishaId, Coins = 100, Direction = "credit", Description = "Welcome bonus", TransactionType = "welcome", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000013-0000-0000-0000-000000000013"), UserId = aishaId, Coins = 50, Direction = "credit", Description = "Verification bonus", TransactionType = "verification", CreatedAt = new DateTime(2024, 1, 1, 2, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000014-0000-0000-0000-000000000014"), UserId = shivId, Coins = 12000, Direction = "credit", Description = "Deposit — 12000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000015-0000-0000-0000-000000000015"), UserId = ishitaId, Coins = 100, Direction = "credit", Description = "Welcome bonus", TransactionType = "welcome", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000016-0000-0000-0000-000000000016"), UserId = ishitaId, Coins = 5000, Direction = "credit", Description = "Deposit — 5000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000017-0000-0000-0000-000000000017"), UserId = amanId, Coins = 6000, Direction = "credit", Description = "Deposit — 6000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000018-0000-0000-0000-000000000018"), UserId = kabirId, Coins = 3800, Direction = "credit", Description = "Deposit — 3800 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // ── Reports (safety testing) ──────────────────────────────────────
        mb.Entity<Report>().HasData(
            new Report { Id = Guid.Parse("h1000001-0000-0000-0000-000000000001"), ReporterId = priyaId, ReportedUserId = deepakId, Reason = "spam", Description = "Sending repeated unsolicited messages", Status = "pending", CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Report { Id = Guid.Parse("h1000002-0000-0000-0000-000000000002"), ReporterId = aishaId, ReportedUserId = sureshId, Reason = "harassment", Description = "Inappropriate language in DMs", Status = "reviewed", CreatedAt = new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Report { Id = Guid.Parse("h1000003-0000-0000-0000-000000000003"), ReporterId = poojaId, ReportedUserId = vivekId, Reason = "inappropriate_content", Description = "Shared offensive content", Status = "action_taken", CreatedAt = new DateTime(2024, 2, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Report { Id = Guid.Parse("h1000004-0000-0000-0000-000000000004"), ReporterId = tanviId, ReportedUserId = ankitId, Reason = "fake_profile", Description = "Profile pictures appear to be stolen", Status = "pending", CreatedAt = new DateTime(2024, 2, 4, 0, 0, 0, DateTimeKind.Utc) }
        );

        // ── Blocks ───────────────────────────────────────────────────────
        mb.Entity<Block>().HasData(
            new Block { Id = Guid.Parse("i1000001-0000-0000-0000-000000000001"), BlockerId = priyaId, BlockedUserId = deepakId, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Block { Id = Guid.Parse("i1000002-0000-0000-0000-000000000002"), BlockerId = aishaId, BlockedUserId = sureshId, CreatedAt = new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Block { Id = Guid.Parse("i1000003-0000-0000-0000-000000000003"), BlockerId = poojaId, BlockedUserId = vivekId, CreatedAt = new DateTime(2024, 2, 3, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}