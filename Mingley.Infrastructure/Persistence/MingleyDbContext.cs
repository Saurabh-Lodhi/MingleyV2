using Microsoft.EntityFrameworkCore;
using Mingley.Domain.Entities;

namespace Mingley.Infrastructure.Persistence;

public class MingleyDbContext : DbContext
{
    // ── Coin economy constants ─────────────────────────────────────────────
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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();  // ← NEW

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── Global soft-delete filters ─────────────────────────────────
        mb.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<Match>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<Message>().HasQueryFilter(e => !e.IsDeleted);

        // ── Unique constraints ─────────────────────────────────────────
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
        mb.Entity<User>().HasIndex(u => u.Phone).IsUnique().HasFilter("\"Phone\" IS NOT NULL");
        mb.Entity<Block>().HasIndex(b => new { b.BlockerId, b.BlockedUserId }).IsUnique();
        mb.Entity<Swipe>().HasIndex(s => new { s.SwiperId, s.TargetId }).IsUnique();
        mb.Entity<UserInterest>().HasKey(ui => new { ui.UserId, ui.InterestId });

        // ── Relationships ──────────────────────────────────────────────
        mb.Entity<UserPreference>()
            .HasOne(p => p.User).WithOne(u => u.Preference)
            .HasForeignKey<UserPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<UserLocation>()
            .HasOne(l => l.User).WithOne(u => u.Location)
            .HasForeignKey<UserLocation>(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<UserSubscription>()
            .HasOne(s => s.User).WithOne(u => u.Subscription)
            .HasForeignKey<UserSubscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Chat>()
            .HasOne(c => c.Match).WithOne(m => m.Chat)
            .HasForeignKey<Chat>(c => c.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Match>()
            .HasOne(m => m.User1).WithMany().HasForeignKey(m => m.User1Id).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Match>()
            .HasOne(m => m.User2).WithMany().HasForeignKey(m => m.User2Id).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Message>()
            .HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Message>()
            .HasOne(m => m.ReplyToMessage).WithMany().HasForeignKey(m => m.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        mb.Entity<Swipe>()
            .HasOne(s => s.Swiper).WithMany().HasForeignKey(s => s.SwiperId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Swipe>()
            .HasOne(s => s.Target).WithMany().HasForeignKey(s => s.TargetId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<CallSession>()
            .HasOne(c => c.Caller).WithMany().HasForeignKey(c => c.CallerId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<CallSession>()
            .HasOne(c => c.Receiver).WithMany().HasForeignKey(c => c.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<CallSession>()
            .HasOne(c => c.Match).WithMany().HasForeignKey(c => c.MatchId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<SuperChat>()
            .HasOne(s => s.FromUser).WithMany().HasForeignKey(s => s.FromUserId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<SuperChat>()
            .HasOne(s => s.ToUser).WithMany().HasForeignKey(s => s.ToUserId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<SuperChat>()
            .HasOne(s => s.MatchCreated).WithMany().HasForeignKey(s => s.MatchCreatedId)
            .OnDelete(DeleteBehavior.SetNull).IsRequired(false);

        mb.Entity<Block>()
            .HasOne(b => b.Blocker).WithMany().HasForeignKey(b => b.BlockerId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Block>()
            .HasOne(b => b.BlockedUser).WithMany().HasForeignKey(b => b.BlockedUserId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Report>()
            .HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Report>()
            .HasOne(r => r.ReportedUser).WithMany().HasForeignKey(r => r.ReportedUserId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<SubscriptionPlan>().Property(p => p.Price).HasPrecision(18, 2);

        // ── RefreshToken ───────────────────────────────────────────────
        mb.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Token).IsRequired().HasMaxLength(512);
            e.HasIndex(t => t.Token).IsUnique();
            e.HasOne(t => t.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(mb);
    }

    // ════════════════════════════════════════════════════════════════════
    // SEED DATA
    // ════════════════════════════════════════════════════════════════════
    private static void SeedData(ModelBuilder mb)
    {
        var hash = "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq"; // Mingley@123

        // ── Interests ──────────────────────────────────────────────────
        mb.Entity<Interest>().HasData(
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "Music", Icon = "musical-notes-outline", Emoji = "🎵" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000002"), Name = "Travel", Icon = "airplane-outline", Emoji = "✈️" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000003"), Name = "Gym", Icon = "barbell-outline", Emoji = "💪" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000004"), Name = "Movies", Icon = "film-outline", Emoji = "🎬" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000005"), Name = "Reading", Icon = "book-outline", Emoji = "📚" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000006"), Name = "Cooking", Icon = "restaurant-outline", Emoji = "🍳" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000007"), Name = "Art", Icon = "color-palette-outline", Emoji = "🎨" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000008"), Name = "Dancing", Icon = "body-outline", Emoji = "💃" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000009"), Name = "Photography", Icon = "camera-outline", Emoji = "📸" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000010"), Name = "Yoga", Icon = "body-outline", Emoji = "🧘" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000011"), Name = "Cricket", Icon = "baseball-outline", Emoji = "🏏" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000012"), Name = "Gaming", Icon = "game-controller-outline", Emoji = "🎮" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000013"), Name = "Shopping", Icon = "bag-handle-outline", Emoji = "🛍️" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000014"), Name = "Foodie", Icon = "pizza-outline", Emoji = "🍕" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000015"), Name = "Hiking", Icon = "walk-outline", Emoji = "🥾" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000016"), Name = "Coding", Icon = "code-slash-outline", Emoji = "💻" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000017"), Name = "Pets", Icon = "paw-outline", Emoji = "🐾" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000018"), Name = "Coffee", Icon = "cafe-outline", Emoji = "☕" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000019"), Name = "Meditation", Icon = "leaf-outline", Emoji = "🧠" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000020"), Name = "Football", Icon = "football-outline", Emoji = "⚽" }
        );

        // ── Subscription Plans ─────────────────────────────────────────
        mb.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"), Name = "Silver", Price = 299, DurationDays = 30, Features = "[\"Unlimited likes\",\"No ads\",\"5 Super Likes/day\",\"See who liked you\"]", IsPopular = false, SuperLikesPerDay = 5, BoostsPerMonth = 0, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = false },
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000002"), Name = "Gold", Price = 599, DurationDays = 30, Features = "[\"All Silver\",\"Video calls\",\"10 Super Likes/day\",\"2 Profile boosts\",\"5 coins/msg\"]", IsPopular = true, SuperLikesPerDay = 10, BoostsPerMonth = 2, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true },
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000003"), Name = "Platinum", Price = 999, DurationDays = 30, Features = "[\"All Gold\",\"Top picks daily\",\"Unlimited Super Likes\",\"5 boosts/month\",\"Priority support\"]", IsPopular = false, SuperLikesPerDay = -1, BoostsPerMonth = 5, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true },
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000004"), Name = "VIP", Price = 1999, DurationDays = 90, Features = "[\"All Platinum\",\"VIP badge\",\"Global search\",\"Dedicated support\",\"Early features\"]", IsPopular = false, SuperLikesPerDay = -1, BoostsPerMonth = 15, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true }
        );

        // ── Gifts (6 categories, 28 gifts) ─────────────────────────────
        mb.Entity<Gift>().HasData(
            // Standard
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"), Name = "Heart", Emoji = "❤️", Icon = "heart-outline", CoinCost = 10, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000002"), Name = "Rose", Emoji = "🌹", Icon = "rose-outline", CoinCost = 20, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000003"), Name = "Gift Box", Emoji = "🎁", Icon = "gift-outline", CoinCost = 50, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000004"), Name = "Coffee Date", Emoji = "☕", Icon = "cafe-outline", CoinCost = 100, Category = "standard", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000005"), Name = "Diamond Ring", Emoji = "💍", Icon = "diamond-outline", CoinCost = 500, Category = "standard", IsAnimated = false },
            // Romantic
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000001"), Name = "Bouquet", Emoji = "💐", Icon = "flower-outline", CoinCost = 50, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000002"), Name = "Chocolate Box", Emoji = "🍫", Icon = "heart-outline", CoinCost = 75, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000003"), Name = "Love Letter", Emoji = "💌", Icon = "mail-outline", CoinCost = 30, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000004"), Name = "Candlelight", Emoji = "🕯️", Icon = "flame-outline", CoinCost = 150, Category = "romantic", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000002-0000-0000-0000-000000000005"), Name = "Teddy Bear", Emoji = "🧸", Icon = "gift-outline", CoinCost = 200, Category = "romantic", IsAnimated = false },
            // Fun
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000001"), Name = "Cake", Emoji = "🎂", Icon = "cake-outline", CoinCost = 30, Category = "fun", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000002"), Name = "Party Popper", Emoji = "🎉", Icon = "sparkles-outline", CoinCost = 40, Category = "fun", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000003"), Name = "Trophy", Emoji = "🏆", Icon = "trophy-outline", CoinCost = 80, Category = "fun", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000004"), Name = "Balloon", Emoji = "🎈", Icon = "balloon-outline", CoinCost = 25, Category = "fun", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000003-0000-0000-0000-000000000005"), Name = "Confetti", Emoji = "🎊", Icon = "sparkles-outline", CoinCost = 35, Category = "fun", IsAnimated = true },
            // Animated
            new Gift { Id = Guid.Parse("c0000004-0000-0000-0000-000000000001"), Name = "Fireworks", Emoji = "🎆", Icon = "sparkles-outline", CoinCost = 150, Category = "animated", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000004-0000-0000-0000-000000000002"), Name = "Shooting Star", Emoji = "🌠", Icon = "star-outline", CoinCost = 200, Category = "animated", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000004-0000-0000-0000-000000000003"), Name = "Rainbow", Emoji = "🌈", Icon = "color-fill-outline", CoinCost = 300, Category = "animated", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000004-0000-0000-0000-000000000004"), Name = "Magic Wand", Emoji = "🪄", Icon = "sparkles-outline", CoinCost = 250, Category = "animated", IsAnimated = true },
            // Luxury
            new Gift { Id = Guid.Parse("c0000005-0000-0000-0000-000000000001"), Name = "Crown", Emoji = "👑", Icon = "diamond-outline", CoinCost = 500, Category = "luxury", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000005-0000-0000-0000-000000000002"), Name = "Sports Car", Emoji = "🚗", Icon = "car-outline", CoinCost = 800, Category = "luxury", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000005-0000-0000-0000-000000000003"), Name = "Private Jet", Emoji = "✈️", Icon = "airplane-outline", CoinCost = 1500, Category = "luxury", IsAnimated = false },
            new Gift { Id = Guid.Parse("c0000005-0000-0000-0000-000000000004"), Name = "Yacht", Emoji = "⛵", Icon = "boat-outline", CoinCost = 2000, Category = "luxury", IsAnimated = false },
            // VIP
            new Gift { Id = Guid.Parse("c0000006-0000-0000-0000-000000000001"), Name = "Golden Rose", Emoji = "🌹", Icon = "rose-outline", CoinCost = 1000, Category = "vip", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000006-0000-0000-0000-000000000002"), Name = "Diamond Heart", Emoji = "💎", Icon = "diamond-outline", CoinCost = 3000, Category = "vip", IsAnimated = true },
            new Gift { Id = Guid.Parse("c0000006-0000-0000-0000-000000000003"), Name = "King Package", Emoji = "🎰", Icon = "trophy-outline", CoinCost = 5000, Category = "vip", IsAnimated = true }
        );

        // ── User IDs ───────────────────────────────────────────────────
        var adminId = Guid.Parse("d0000001-0000-0000-0000-000000000001");
        var priyaId = Guid.Parse("d0000001-0000-0000-0000-000000000002");
        var rahulId = Guid.Parse("d0000001-0000-0000-0000-000000000003");
        var arjunId = Guid.Parse("d0000001-0000-0000-0000-000000000004");
        var nehaId = Guid.Parse("d0000001-0000-0000-0000-000000000005");
        var vikramId = Guid.Parse("d0000001-0000-0000-0000-000000000006");
        var ankitaId = Guid.Parse("d0000001-0000-0000-0000-000000000007");
        var deepakId = Guid.Parse("d0000001-0000-0000-0000-000000000008");
        var aishaId = Guid.Parse("d0000001-0000-0000-0000-000000000009");
        var rohitId = Guid.Parse("d0000001-0000-0000-0000-000000000010");
        var shreyaId = Guid.Parse("d0000001-0000-0000-0000-000000000011");
        var karthikId = Guid.Parse("d0000001-0000-0000-0000-000000000012");
        var meenaId = Guid.Parse("d0000001-0000-0000-0000-000000000013");
        var rajeshId = Guid.Parse("d0000001-0000-0000-0000-000000000014");
        var poojaId = Guid.Parse("d0000001-0000-0000-0000-000000000015");
        var amanId = Guid.Parse("d0000001-0000-0000-0000-000000000016");
        var kritikaId = Guid.Parse("d0000001-0000-0000-0000-000000000017");
        var saurabhId = Guid.Parse("d0000001-0000-0000-0000-000000000018");
        var ritaId = Guid.Parse("d0000001-0000-0000-0000-000000000019");
        var mohanId = Guid.Parse("d0000001-0000-0000-0000-000000000020");
        // Extra users for edge case coverage
        var simranId = Guid.Parse("d0000001-0000-0000-0000-000000000021");
        var adityaId = Guid.Parse("d0000001-0000-0000-0000-000000000022");
        var divyaId = Guid.Parse("d0000001-0000-0000-0000-000000000023");
        var nikhilId = Guid.Parse("d0000001-0000-0000-0000-000000000024");
        var kavyaId = Guid.Parse("d0000001-0000-0000-0000-000000000025");
        var sureshId = Guid.Parse("d0000001-0000-0000-0000-000000000026");
        var tanviId = Guid.Parse("d0000001-0000-0000-0000-000000000027");
        var aakashId = Guid.Parse("d0000001-0000-0000-0000-000000000028");
        var ishitaId = Guid.Parse("d0000001-0000-0000-0000-000000000029");
        var riyaId = Guid.Parse("d0000001-0000-0000-0000-000000000030");

        // ── Users ──────────────────────────────────────────────────────
        // Avatars: Unsplash CDN — real HD portrait photos, no auth needed
        mb.Entity<User>().HasData(

            // ── Admin ──────────────────────────────────────────────────
            new User
            {
                Id = adminId,
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
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Avatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&q=90&fit=crop&crop=face"
            },

            // ── Verified Premium Girls ─────────────────────────────────
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
                Bio = "Love dancing, yoga and cooking 🌺 | Delhi girl | Let's vibe ✨",
                Avatar = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?w=400&q=90&fit=crop&crop=face"
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
                Bio = "Singer and travel lover 🎵✈️ | Mumbai | Chai over coffee ☕",
                Avatar = "https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(2000, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Foodie and photographer 📸🍕 | Pune | Currently obsessed with sunsets 🌅",
                Avatar = "https://images.unsplash.com/photo-1488426862026-3ee34a7d66df?w=400&q=90&fit=crop&crop=face"
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
                Bio = "Fashion lover 👗 | Sketch artist 🎨 | Hyderabad | Building my empire 💅",
                Avatar = "https://images.unsplash.com/photo-1529626455594-4ff0802cfb7e?w=400&q=90&fit=crop&crop=face"
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
                Avatar = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(2001, 3, 22, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Engineering student 📚 | Sketch artist | Hyderabad | 21 and figuring it out 😄",
                Avatar = "https://images.unsplash.com/photo-1542206395-9feb3edaa68d?w=400&q=90&fit=crop&crop=face"
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
                Bio = "Marketing lead | Loves reading 📖 | Jaipur | Pink city girl 🌸",
                Avatar = "https://images.unsplash.com/photo-1489424731084-a5d8b219a5bb?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1996, 7, 14, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Journalist ✍️ | Avid traveller ✈️ | Kolkata | City of joy forever 🎭",
                Avatar = "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1993, 11, 25, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Finance professional 💼 | Yoga instructor 🧘 | Surat | Manifesting my dreams ✨",
                Avatar = "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=400&q=90&fit=crop&crop=face"
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
                Bio = "Lawyer ⚖️ | Kathak dancer 💃 | Amritsar | Golden temple sunrises hit different 🌅",
                Avatar = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(2000, 6, 18, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Architecture student 🏛️ | Coffee addict ☕ | Kochi | Designing my future 📐",
                Avatar = "https://images.unsplash.com/photo-1502685104226-ee32379fefbe?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(2002, 4, 5, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Pre-med | Poet 🖋️ | Trivandrum | Words are my superpower 🌙",
                Avatar = "https://images.unsplash.com/photo-1515077678510-ce3bdf418862?w=400&q=90&fit=crop&crop=face"
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
                Bio = "UI/UX Designer 🎨 | Plant mom 🌿 | Pune | Making things pretty for a living ✨",
                Avatar = "https://images.unsplash.com/photo-1520813792240-56fc4a3765a7?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1994, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Startup founder 🚀 | TEDx speaker | Delhi | Hustle + heart ❤️‍🔥",
                Avatar = "https://images.unsplash.com/photo-1509967419530-da38b4704bc6?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1999, 10, 3, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Classical singer 🎶 | Bookworm 📚 | Varanasi | Old soul in a modern world 🕌",
                Avatar = "https://images.unsplash.com/photo-1463453091185-61582044d556?w=400&q=90&fit=crop&crop=face"
            },

            // ── Verified Boys ──────────────────────────────────────────
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
                DateOfBirth = new DateTime(1995, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Music lover 🎵 | Solo traveller ✈️ | Software Engineer | Noida | Guitar + code = life 🎸",
                Avatar = "https://images.unsplash.com/photo-1552058544-f2b08422138a?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1993, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Fitness freak 💪 | Landscape photographer 📸 | Gurgaon | Mountains > malls 🏔️",
                Avatar = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1996, 4, 12, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Entrepreneur ⚡ | Coffee addict ☕ | Delhi | Building the next big thing 🚀",
                Avatar = "https://images.unsplash.com/photo-1568602471122-7832951cc4c5?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1997, 9, 30, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Gym bro 🏋️ | Cricket fanatic 🏏 | Noida | IPL > everything 😂",
                Avatar = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1994, 6, 25, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Chef 👨‍🍳 | Food blogger | Bengaluru | Will cook for you if you laugh at my puns 😄",
                Avatar = "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1992, 8, 18, 0, 0, 0, DateTimeKind.Utc),
                Bio = "IIT grad 🎓 | Startup founder 🚀 | Chennai | 0 to 1 builder ⚙️",
                Avatar = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1990, 12, 3, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Senior dev 💻 | Gaming nerd 🎮 | Kolkata | 10 years of bugs fixed, still counting 😅",
                Avatar = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1994, 2, 28, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Architect 🏛️ | Art lover 🎨 | Chandigarh | Designing spaces, chasing light 🌤️",
                Avatar = "https://images.unsplash.com/photo-1548449112-96a38a643324?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1999, 4, 16, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Cricketer ⚡ | Final year student | Lucknow | Future RCB player 😂🏏",
                Avatar = "https://images.unsplash.com/photo-1531891437562-4301cf35b7e4?w=400&q=90&fit=crop&crop=face"
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
                ProfileComplete = true,
                DateOfBirth = new DateTime(1988, 6, 8, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Retired athlete 🥇 | Fitness coach | Thiruvananthapuram | Chasing a second wind 💨",
                Avatar = "https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1996, 3, 11, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Data scientist 📊 | Bike rider 🏍️ | Pune | Numbers by day, roads by night 🌙",
                Avatar = "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?w=400&q=90&fit=crop&crop=face"
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
                Bio = "Investment banker 💰 | Traveller 🗺️ | Mumbai | 40 countries and counting 🌍",
                Avatar = "https://images.unsplash.com/photo-1488161628813-04466f872be2?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(2000, 2, 14, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Commerce grad | Meme lord 😂 | Coimbatore | Currently vibing on good music 🎧",
                Avatar = "https://images.unsplash.com/photo-1504257432389-52343af06ae3?w=400&q=90&fit=crop&crop=face"
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
                DateOfBirth = new DateTime(1995, 5, 27, 0, 0, 0, DateTimeKind.Utc),
                Bio = "Pilot ✈️ | Astronomy nerd 🔭 | Delhi | Up in the clouds, literally 😄",
                Avatar = "https://images.unsplash.com/photo-1463453091185-61582044d556?w=400&q=90&fit=crop&crop=face"
            }
        );

        // ── User Preferences ───────────────────────────────────────────
        mb.Entity<UserPreference>().HasData(
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000002"), UserId = rahulId, InterestedIn = "girls", MinAge = 20, MaxAge = 30, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000003"), UserId = arjunId, InterestedIn = "girls", MinAge = 21, MaxAge = 32, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000004"), UserId = nehaId, InterestedIn = "boys", MinAge = 23, MaxAge = 33, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000005"), UserId = vikramId, InterestedIn = "girls", MinAge = 21, MaxAge = 30, MaxDistance = 100, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000006"), UserId = ankitaId, InterestedIn = "boys", MinAge = 24, MaxAge = 34, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000007"), UserId = deepakId, InterestedIn = "girls", MinAge = 20, MaxAge = 28, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000008"), UserId = aishaId, InterestedIn = "boys", MinAge = 22, MaxAge = 32, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000009"), UserId = rohitId, InterestedIn = "girls", MinAge = 20, MaxAge = 30, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000001"), UserId = priyaId, InterestedIn = "boys", MinAge = 22, MaxAge = 35, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000011"), UserId = shreyaId, InterestedIn = "boys", MinAge = 25, MaxAge = 36, MaxDistance = 150, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000012"), UserId = karthikId, InterestedIn = "girls", MinAge = 22, MaxAge = 30, MaxDistance = 200, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000013"), UserId = meenaId, InterestedIn = "boys", MinAge = 22, MaxAge = 28, MaxDistance = 50, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000014"), UserId = rajeshId, InterestedIn = "girls", MinAge = 24, MaxAge = 32, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000015"), UserId = poojaId, InterestedIn = "boys", MinAge = 24, MaxAge = 32, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000016"), UserId = amanId, InterestedIn = "girls", MinAge = 23, MaxAge = 32, MaxDistance = 150, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000017"), UserId = kritikaId, InterestedIn = "boys", MinAge = 25, MaxAge = 35, MaxDistance = 200, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000018"), UserId = saurabhId, InterestedIn = "girls", MinAge = 18, MaxAge = 26, MaxDistance = 50, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000019"), UserId = ritaId, InterestedIn = "boys", MinAge = 27, MaxAge = 40, MaxDistance = 200, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000020"), UserId = mohanId, InterestedIn = "girls", MinAge = 28, MaxAge = 38, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000021"), UserId = simranId, InterestedIn = "boys", MinAge = 24, MaxAge = 34, MaxDistance = 150, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000022"), UserId = adityaId, InterestedIn = "girls", MinAge = 22, MaxAge = 30, MaxDistance = 100, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000023"), UserId = divyaId, InterestedIn = "boys", MinAge = 21, MaxAge = 28, MaxDistance = 75, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000024"), UserId = nikhilId, InterestedIn = "girls", MinAge = 23, MaxAge = 33, MaxDistance = 300, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000025"), UserId = kavyaId, InterestedIn = "boys", MinAge = 20, MaxAge = 27, MaxDistance = 50, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000026"), UserId = sureshId, InterestedIn = "girls", MinAge = 18, MaxAge = 25, MaxDistance = 50, RelationshipType = "casual" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000027"), UserId = tanviId, InterestedIn = "boys", MinAge = 26, MaxAge = 36, MaxDistance = 100, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000028"), UserId = aakashId, InterestedIn = "girls", MinAge = 22, MaxAge = 30, MaxDistance = 200, RelationshipType = "both" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000029"), UserId = ishitaId, InterestedIn = "boys", MinAge = 27, MaxAge = 38, MaxDistance = 200, RelationshipType = "serious" },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000030"), UserId = riyaId, InterestedIn = "boys", MinAge = 22, MaxAge = 30, MaxDistance = 75, RelationshipType = "both" }
        );

        // ── User Locations ─────────────────────────────────────────────
        mb.Entity<UserLocation>().HasData(
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"), UserId = priyaId, City = "Delhi", Country = "India", Lat = 28.6139, Lng = 77.2090 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000002"), UserId = rahulId, City = "Noida", Country = "India", Lat = 28.5355, Lng = 77.3910 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000003"), UserId = arjunId, City = "Gurgaon", Country = "India", Lat = 28.4595, Lng = 77.0266 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000004"), UserId = nehaId, City = "Mumbai", Country = "India", Lat = 19.0760, Lng = 72.8777 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000005"), UserId = vikramId, City = "Delhi", Country = "India", Lat = 28.7041, Lng = 77.1025 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000006"), UserId = ankitaId, City = "Pune", Country = "India", Lat = 18.5204, Lng = 73.8567 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000007"), UserId = deepakId, City = "Noida", Country = "India", Lat = 28.5400, Lng = 77.4000 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000008"), UserId = aishaId, City = "Hyderabad", Country = "India", Lat = 17.3850, Lng = 78.4867 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000009"), UserId = rohitId, City = "Bengaluru", Country = "India", Lat = 12.9716, Lng = 77.5946 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000011"), UserId = shreyaId, City = "Ahmedabad", Country = "India", Lat = 23.0225, Lng = 72.5714 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000012"), UserId = karthikId, City = "Chennai", Country = "India", Lat = 13.0827, Lng = 80.2707 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000013"), UserId = meenaId, City = "Hyderabad", Country = "India", Lat = 17.4000, Lng = 78.5000 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000014"), UserId = rajeshId, City = "Kolkata", Country = "India", Lat = 22.5726, Lng = 88.3639 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000015"), UserId = poojaId, City = "Jaipur", Country = "India", Lat = 26.9124, Lng = 75.7873 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000016"), UserId = amanId, City = "Chandigarh", Country = "India", Lat = 30.7333, Lng = 76.7794 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000017"), UserId = kritikaId, City = "Kolkata", Country = "India", Lat = 22.5800, Lng = 88.3500 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000018"), UserId = saurabhId, City = "Lucknow", Country = "India", Lat = 26.8467, Lng = 80.9462 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000019"), UserId = ritaId, City = "Surat", Country = "India", Lat = 21.1702, Lng = 72.8311 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000020"), UserId = mohanId, City = "Thiruvananthapuram", Country = "India", Lat = 8.5241, Lng = 76.9366 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000021"), UserId = simranId, City = "Amritsar", Country = "India", Lat = 31.6340, Lng = 74.8723 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000022"), UserId = adityaId, City = "Pune", Country = "India", Lat = 18.5200, Lng = 73.8600 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000023"), UserId = divyaId, City = "Kochi", Country = "India", Lat = 9.9312, Lng = 76.2673 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000024"), UserId = nikhilId, City = "Mumbai", Country = "India", Lat = 19.0800, Lng = 72.8800 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000025"), UserId = kavyaId, City = "Trivandrum", Country = "India", Lat = 8.5000, Lng = 76.9500 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000026"), UserId = sureshId, City = "Coimbatore", Country = "India", Lat = 11.0168, Lng = 76.9558 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000027"), UserId = tanviId, City = "Pune", Country = "India", Lat = 18.5300, Lng = 73.8700 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000028"), UserId = aakashId, City = "Delhi", Country = "India", Lat = 28.6200, Lng = 77.2100 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000029"), UserId = ishitaId, City = "Delhi", Country = "India", Lat = 28.6300, Lng = 77.2200 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000030"), UserId = riyaId, City = "Varanasi", Country = "India", Lat = 25.3176, Lng = 82.9739 }
        );

        // ── User Interests ─────────────────────────────────────────────
        mb.Entity<UserInterest>().HasData(
            new UserInterest { UserId = priyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = priyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = priyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = priyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
            new UserInterest { UserId = rahulId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = rahulId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = rahulId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = rahulId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = arjunId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = arjunId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = arjunId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = nehaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = nehaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = nehaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = ankitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
            new UserInterest { UserId = ankitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = ankitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000014") },
            new UserInterest { UserId = vikramId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = vikramId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = deepakId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = deepakId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
            new UserInterest { UserId = aishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = aishaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000013") },
            new UserInterest { UserId = rohitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
            new UserInterest { UserId = rohitId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000014") },
            new UserInterest { UserId = shreyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = shreyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = shreyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = karthikId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = karthikId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = meenaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = meenaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = rajeshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = rajeshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = poojaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = poojaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000013") },
            new UserInterest { UserId = amanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = amanId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = kritikaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = kritikaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = saurabhId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
            new UserInterest { UserId = saurabhId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = ritaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = ritaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = simranId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = simranId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = adityaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = adityaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000015") },
            new UserInterest { UserId = divyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = divyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000018") },
            new UserInterest { UserId = nikhilId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = nikhilId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000004") },
            new UserInterest { UserId = kavyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = kavyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = sureshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = sureshId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000020") },
            new UserInterest { UserId = tanviId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = tanviId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000017") },
            new UserInterest { UserId = aakashId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = aakashId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000019") },
            new UserInterest { UserId = ishitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000016") },
            new UserInterest { UserId = ishitaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = riyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = riyaId, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") }
        );

        // ── Matches, Chats, Messages ───────────────────────────────────
        var match1Id = Guid.Parse("a1000001-0000-0000-0000-000000000001");
        var match2Id = Guid.Parse("a1000001-0000-0000-0000-000000000002");
        var match3Id = Guid.Parse("a1000001-0000-0000-0000-000000000003");
        var match4Id = Guid.Parse("a1000001-0000-0000-0000-000000000004");
        var match5Id = Guid.Parse("a1000001-0000-0000-0000-000000000005");
        var match6Id = Guid.Parse("a1000001-0000-0000-0000-000000000006");
        var chat1Id = Guid.Parse("a1000002-0000-0000-0000-000000000001");
        var chat2Id = Guid.Parse("a1000002-0000-0000-0000-000000000002");
        var chat3Id = Guid.Parse("a1000002-0000-0000-0000-000000000003");
        var chat4Id = Guid.Parse("a1000002-0000-0000-0000-000000000004");
        var chat5Id = Guid.Parse("a1000002-0000-0000-0000-000000000005");
        var chat6Id = Guid.Parse("a1000002-0000-0000-0000-000000000006");

        mb.Entity<Swipe>().HasData(
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000001"), SwiperId = rahulId, TargetId = priyaId, Action = "like", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000002"), SwiperId = priyaId, TargetId = rahulId, Action = "like", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000003"), SwiperId = arjunId, TargetId = aishaId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000004"), SwiperId = aishaId, TargetId = arjunId, Action = "like", CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000005"), SwiperId = vikramId, TargetId = shreyaId, Action = "like", CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000006"), SwiperId = shreyaId, TargetId = vikramId, Action = "like", CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000007"), SwiperId = nikhilId, TargetId = simranId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000008"), SwiperId = simranId, TargetId = nikhilId, Action = "like", CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000009"), SwiperId = amanId, TargetId = ishitaId, Action = "like", CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000010"), SwiperId = ishitaId, TargetId = amanId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000011"), SwiperId = adityaId, TargetId = tanviId, Action = "like", CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000012"), SwiperId = tanviId, TargetId = adityaId, Action = "like", CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            // Unmatched swipes for edge case coverage
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000013"), SwiperId = deepakId, TargetId = poojaId, Action = "like", CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000014"), SwiperId = sureshId, TargetId = kavyaId, Action = "like", CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000015"), SwiperId = rohitId, TargetId = ritaId, Action = "dislike", CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000016"), SwiperId = saurabhId, TargetId = divyaId, Action = "like", CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000017"), SwiperId = karthikId, TargetId = riyaId, Action = "superlike", CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) }
        );

        mb.Entity<Match>().HasData(
            new Match { Id = match1Id, User1Id = rahulId, User2Id = priyaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = match2Id, User1Id = arjunId, User2Id = aishaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = match3Id, User1Id = vikramId, User2Id = shreyaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = match4Id, User1Id = nikhilId, User2Id = simranId, IsActive = true, CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = match5Id, User1Id = amanId, User2Id = ishitaId, IsActive = true, CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Match { Id = match6Id, User1Id = adityaId, User2Id = tanviId, IsActive = true, CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) }
        );

        mb.Entity<Chat>().HasData(
            new Chat { Id = chat1Id, MatchId = match1Id, CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = chat2Id, MatchId = match2Id, CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = chat3Id, MatchId = match3Id, CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = chat4Id, MatchId = match4Id, CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = chat5Id, MatchId = match5Id, CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Chat { Id = chat6Id, MatchId = match6Id, CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) }
        );

        mb.Entity<Message>().HasData(
            // Chat 1 — Rahul & Priya (active, rich thread)
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000001"), ChatId = chat1Id, SenderId = rahulId, Text = "Hey Priya! We matched 🎉 How are you?", Type = "text", CoinsDeducted = 10, ReadAt = new DateTime(2024, 1, 2, 1, 0, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2024, 1, 2, 0, 30, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000002"), ChatId = chat1Id, SenderId = priyaId, Text = "Hi Rahul! I'm great, thanks for the super cute message 😊", Type = "text", CoinsDeducted = 0, ReadAt = new DateTime(2024, 1, 2, 2, 0, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2024, 1, 2, 1, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000003"), ChatId = chat1Id, SenderId = rahulId, Text = "I saw you love dancing 💃 that's so cool! I play guitar 🎸", Type = "text", CoinsDeducted = 10, ReadAt = new DateTime(2024, 1, 2, 3, 0, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2024, 1, 2, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000004"), ChatId = chat1Id, SenderId = priyaId, Text = "No way! I've been dancing since I was 8 🎵 We should collab!", Type = "text", CoinsDeducted = 0, ReadAt = new DateTime(2024, 1, 2, 3, 30, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2024, 1, 2, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000005"), ChatId = chat1Id, SenderId = rahulId, Text = "Yes! Coffee date first though? ☕", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 2, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000006"), ChatId = chat1Id, SenderId = priyaId, Text = "I'd love that 💕 Connaught Place?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 2, 4, 30, 0, DateTimeKind.Utc) },

            // Chat 2 — Arjun & Aisha (video call history)
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000001"), ChatId = chat2Id, SenderId = arjunId, Text = "Hi Aisha! I sent you a super like 🌟 Your art is stunning!", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 3, 1, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000002"), ChatId = chat2Id, SenderId = aishaId, Text = "Aww thank you so much 😍 I love your photography too!", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 3, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000003"), ChatId = chat2Id, SenderId = arjunId, Text = "What are you up to this weekend? Maybe a video call? 📹", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 3, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000004"), ChatId = chat2Id, SenderId = aishaId, Text = "Sure! Saturday evening works for me 🌸", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 3, 4, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000005"), ChatId = chat2Id, SenderId = arjunId, Text = "📹 video call · 05:00", Type = "system", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 3, 5, 5, 0, DateTimeKind.Utc) },

            // Chat 3 — Vikram & Shreya
            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000001"), ChatId = chat3Id, SenderId = vikramId, Text = "Hey Shreya! Great to match with you ✨", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 4, 1, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000002"), ChatId = chat3Id, SenderId = shreyaId, Text = "Hi Vikram! You seem interesting 😊 What do you do?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 4, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000003"), ChatId = chat3Id, SenderId = vikramId, Text = "I run a startup — chaos every day but loving it 🚀", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 4, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000004"), ChatId = chat3Id, SenderId = shreyaId, Text = "Haha I respect the hustle! I'm a doctor, so same chaos 😂", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 4, 4, 0, 0, DateTimeKind.Utc) },

            // Chat 4 — Nikhil & Simran (premium, high coin spend)
            new Message { Id = Guid.Parse("c1000004-0000-0000-0000-000000000001"), ChatId = chat4Id, SenderId = nikhilId, Text = "Simran! Your super like was on point 😄", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 5, 1, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000004-0000-0000-0000-000000000002"), ChatId = chat4Id, SenderId = simranId, Text = "You sent it first actually 😂 I just reciprocated!", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 5, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000004-0000-0000-0000-000000000003"), ChatId = chat4Id, SenderId = nikhilId, Text = "Fair enough 😄 40 countries and finally matched with someone from my own city 😂", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 5, 3, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000004-0000-0000-0000-000000000004"), ChatId = chat4Id, SenderId = simranId, Text = "Classic 🤣 Tell me about your best trip!", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 5, 4, 0, 0, DateTimeKind.Utc) },

            // Chat 5 — Aman & Ishita (startup vibes)
            new Message { Id = Guid.Parse("c1000005-0000-0000-0000-000000000001"), ChatId = chat5Id, SenderId = amanId, Text = "Ishita! A TEDx speaker and a founder — colour me impressed 🙌", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 6, 1, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000005-0000-0000-0000-000000000002"), ChatId = chat5Id, SenderId = ishitaId, Text = "An architect who appreciates art? Rare find 🎨", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 6, 2, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000005-0000-0000-0000-000000000003"), ChatId = chat5Id, SenderId = amanId, Text = "We should grab coffee and exchange ideas ☕", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 6, 3, 0, 0, DateTimeKind.Utc) },

            // Chat 6 — Aditya & Tanvi
            new Message { Id = Guid.Parse("c1000006-0000-0000-0000-000000000001"), ChatId = chat6Id, SenderId = adityaId, Text = "Hey Tanvi! A designer and a plant mom — dream combo 🌿🎨", Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024, 1, 7, 1, 0, 0, DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000006-0000-0000-0000-000000000002"), ChatId = chat6Id, SenderId = tanviId, Text = "Haha yes! And a data scientist who rides bikes 🏍️ respect!", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024, 1, 7, 2, 0, 0, DateTimeKind.Utc) }
        );

        // ── Call Sessions ──────────────────────────────────────────────
        mb.Entity<CallSession>().HasData(
            // Completed video call
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000001"), CallerId = arjunId, ReceiverId = aishaId, MatchId = match2Id, CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 3, 3, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 3, 3, 5, 0, DateTimeKind.Utc), DurationSeconds = 300, CoinsDeducted = 500, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 3, 3, 0, 0, DateTimeKind.Utc) },
            // Completed audio call
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000002"), CallerId = rahulId, ReceiverId = priyaId, MatchId = match1Id, CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024, 1, 2, 5, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 2, 5, 3, 0, DateTimeKind.Utc), DurationSeconds = 180, CoinsDeducted = 30, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 2, 5, 0, 0, DateTimeKind.Utc) },
            // Missed call
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000003"), CallerId = vikramId, ReceiverId = shreyaId, MatchId = match3Id, CallType = "audio", Status = "timeout", EndedAt = new DateTime(2024, 1, 4, 6, 0, 0, DateTimeKind.Utc), CoinsDeducted = 0, EndReason = "no_answer", CreatedAt = new DateTime(2024, 1, 4, 6, 0, 0, DateTimeKind.Utc) },
            // Declined call
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000004"), CallerId = priyaId, ReceiverId = rahulId, MatchId = match1Id, CallType = "audio", Status = "declined", EndedAt = new DateTime(2024, 1, 5, 8, 0, 0, DateTimeKind.Utc), CoinsDeducted = 0, EndReason = "declined", CreatedAt = new DateTime(2024, 1, 5, 8, 0, 0, DateTimeKind.Utc) },
            // Long video call (premium)
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000005"), CallerId = nikhilId, ReceiverId = simranId, MatchId = match4Id, CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024, 1, 5, 9, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2024, 1, 5, 9, 20, 0, DateTimeKind.Utc), DurationSeconds = 1200, CoinsDeducted = 2000, EndReason = "user_ended", CreatedAt = new DateTime(2024, 1, 5, 9, 0, 0, DateTimeKind.Utc) },
            // Cancelled call
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000006"), CallerId = amanId, ReceiverId = ishitaId, MatchId = match5Id, CallType = "audio", Status = "cancelled", EndedAt = new DateTime(2024, 1, 6, 10, 0, 0, DateTimeKind.Utc), CoinsDeducted = 0, EndReason = "new_call", CreatedAt = new DateTime(2024, 1, 6, 10, 0, 0, DateTimeKind.Utc) }
        );

        // ── Notifications ──────────────────────────────────────────────
        mb.Entity<Notification>().HasData(
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000001"), UserId = rahulId, Title = "New Match! 🎉", Body = "You matched with Priya Sharma!", Type = "match", IsRead = false, ReferenceId = match1Id.ToString(), CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000002"), UserId = priyaId, Title = "New Match! 🎉", Body = "You matched with Rahul Mehta!", Type = "match", IsRead = true, ReferenceId = match1Id.ToString(), CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000003"), UserId = arjunId, Title = "New Match! 🎉", Body = "You matched with Aisha Khan!", Type = "match", IsRead = false, ReferenceId = match2Id.ToString(), CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000004"), UserId = aishaId, Title = "New Match! 🎉", Body = "You matched with Arjun Singh!", Type = "match", IsRead = false, ReferenceId = match2Id.ToString(), CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000005"), UserId = vikramId, Title = "New Match! 🎉", Body = "You matched with Shreya Patel!", Type = "match", IsRead = false, ReferenceId = match3Id.ToString(), CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000006"), UserId = shreyaId, Title = "New Match! 🎉", Body = "You matched with Vikram Nair!", Type = "match", IsRead = true, ReferenceId = match3Id.ToString(), CreatedAt = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000007"), UserId = nikhilId, Title = "New Match! 🎉", Body = "You matched with Simran Kaur!", Type = "match", IsRead = false, ReferenceId = match4Id.ToString(), CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000008"), UserId = simranId, Title = "New Match! 🎉", Body = "You matched with Nikhil Sharma!", Type = "match", IsRead = false, ReferenceId = match4Id.ToString(), CreatedAt = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000009"), UserId = rahulId, Title = "New Message 💬", Body = "Priya sent you a message", Type = "message", IsRead = false, CreatedAt = new DateTime(2024, 1, 2, 1, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000010"), UserId = priyaId, Title = "Welcome Bonus 🪙", Body = "+100 coins added to your wallet", Type = "coins", IsRead = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000011"), UserId = arjunId, Title = "Call Missed 📞", Body = "You missed a call from Aisha", Type = "call", IsRead = false, CreatedAt = new DateTime(2024, 1, 3, 4, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000012"), UserId = aishaId, Title = "Incoming Call 📹", Body = "Arjun is calling — video call", Type = "call", IsRead = true, CreatedAt = new DateTime(2024, 1, 3, 3, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000013"), UserId = vikramId, Title = "Call Missed 📞", Body = "Shreya didn't pick up", Type = "call", IsRead = false, CreatedAt = new DateTime(2024, 1, 4, 6, 5, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000014"), UserId = nikhilId, Title = "Low Balance ⚠️", Body = "Your coin balance is below 500", Type = "system", IsRead = false, CreatedAt = new DateTime(2024, 1, 5, 9, 25, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000015"), UserId = deepakId, Title = "Profile Incomplete ⚠️", Body = "Add a bio to attract more matches!", Type = "system", IsRead = false, CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000016"), UserId = amanId, Title = "New Match! 🎉", Body = "You matched with Ishita Sharma!", Type = "match", IsRead = false, ReferenceId = match5Id.ToString(), CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000017"), UserId = ishitaId, Title = "Super Like Received ⭐", Body = "Aman Joshi sent you a super like!", Type = "like", IsRead = false, CreatedAt = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000018"), UserId = adityaId, Title = "New Match! 🎉", Body = "You matched with Tanvi Joshi!", Type = "match", IsRead = true, ReferenceId = match6Id.ToString(), CreatedAt = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc) }
        );

        // ── Subscriptions ──────────────────────────────────────────────
        mb.Entity<UserSubscription>().HasData(
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000001"), UserId = arjunId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000002"), UserId = priyaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000003"), UserId = shreyaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000003"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000004"), UserId = amanId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000001"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 6, 30, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = false },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000005"), UserId = ritaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000006"), UserId = nikhilId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000004"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000007"), UserId = simranId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000003"), StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000008"), UserId = ishitaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000004"), StartDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true, AutoRenew = true }
        );

        // ── Coin Transactions (wallet history) ─────────────────────────
        mb.Entity<CoinTransaction>().HasData(
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000001"), UserId = rahulId, Coins = 100, Direction = "credit", Description = "Welcome bonus", TransactionType = "admin", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000002"), UserId = rahulId, Coins = 5000, Direction = "credit", Description = "Deposit — 5000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 1, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000003"), UserId = rahulId, Coins = 30, Direction = "debit", Description = "Audio call · 3 min", TransactionType = "call", CreatedAt = new DateTime(2024, 1, 2, 5, 3, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000004"), UserId = arjunId, Coins = 500, Direction = "debit", Description = "Video call · 5 min", TransactionType = "call", CreatedAt = new DateTime(2024, 1, 3, 3, 5, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000005"), UserId = arjunId, Coins = 10000, Direction = "credit", Description = "Deposit — 10000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000006"), UserId = nikhilId, Coins = 2000, Direction = "debit", Description = "Video call · 20 min", TransactionType = "call", CreatedAt = new DateTime(2024, 1, 5, 9, 20, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000007"), UserId = nikhilId, Coins = 10000, Direction = "credit", Description = "Deposit — 10000 coins", TransactionType = "deposit", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000008"), UserId = priyaId, Coins = 100, Direction = "credit", Description = "Welcome bonus", TransactionType = "admin", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000009"), UserId = aishaId, Coins = 100, Direction = "credit", Description = "Welcome bonus", TransactionType = "admin", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CoinTransaction { Id = Guid.Parse("g1000001-0000-0000-0000-000000000010"), UserId = aishaId, Coins = 50, Direction = "credit", Description = "Profile verified bonus", TransactionType = "admin", CreatedAt = new DateTime(2024, 1, 1, 1, 0, 0, DateTimeKind.Utc) }
        );

        // ── Reports (safety edge cases) ────────────────────────────────
        mb.Entity<Report>().HasData(
            new Report { Id = Guid.Parse("a21000001-0000-0000-0000-000000000001"), ReporterId = priyaId, ReportedUserId = deepakId, Reason = "spam", Description = "Sending repeated unsolicited messages", Status = "pending", CreatedAt = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Report { Id = Guid.Parse("a21000001-0000-0000-0000-000000000002"), ReporterId = aishaId, ReportedUserId = sureshId, Reason = "harassment", Description = "Inappropriate language", Status = "reviewed", AdminNote = "Warning issued", CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0, DateTimeKind.Utc) }
        );

        // ── Blocks (safety edge cases) ─────────────────────────────────
        mb.Entity<Block>().HasData(
            new Block { Id = Guid.Parse("a31000001-0000-0000-0000-000000000001"), BlockerId = priyaId, BlockedUserId = deepakId, CreatedAt = new DateTime(2024, 1, 8, 1, 0, 0, DateTimeKind.Utc) }
        );
    }
}

//using Microsoft.EntityFrameworkCore;
//using Mingley.Domain.Entities;

//namespace Mingley.Infrastructure.Persistence;

//public class MingleyDbContext : DbContext
//{
//    // ── Coin economy constants ─────────────────────────────────────────────
//    public const int AudioCallCoinPerMin   = 10;
//    public const int VideoCallCoinPerMin   = 100;
//    public const int VerificationBonus    = 50;
//    public const int WelcomeBonus         = 100;
//    public const int SuperLikeCost        = 50;
//    public const int SuperChatCost        = 500;
//    public const double CoinToInrRate     = 0.10;   // 1 coin = ₹0.10
//    public const double GirlCommissionPct = 0.50;   // 50% to girl
//    public const double FemaleWithdrawPct = 0.70;
//    public const int MaleCostPerMessage   = 10;
//    public const int MalePremiumCostPerMsg= 5;
//    public const int FemaleFreeMessages   = 3;
//    public const int FemaleMessageCost    = 5;

//    public MingleyDbContext(DbContextOptions<MingleyDbContext> options) : base(options) { }

//    public DbSet<User> Users => Set<User>();
//    public DbSet<UserLocation> UserLocations => Set<UserLocation>();
//    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
//    public DbSet<UserImage> UserImages => Set<UserImage>();
//    public DbSet<Interest> Interests => Set<Interest>();
//    public DbSet<UserInterest> UserInterests => Set<UserInterest>();
//    public DbSet<Swipe> Swipes => Set<Swipe>();
//    public DbSet<Match> Matches => Set<Match>();
//    public DbSet<Chat> Chats => Set<Chat>();
//    public DbSet<Message> Messages => Set<Message>();
//    public DbSet<CallSession> CallSessions => Set<CallSession>();
//    public DbSet<SuperChat> SuperChats => Set<SuperChat>();
//    public DbSet<CoinTransaction> CoinTransactions => Set<CoinTransaction>();
//    public DbSet<DepositRequest> DepositRequests => Set<DepositRequest>();
//    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();
//    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
//    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
//    public DbSet<Gift> Gifts => Set<Gift>();
//    public DbSet<Notification> Notifications => Set<Notification>();
//    public DbSet<Report> Reports => Set<Report>();
//    public DbSet<Block> Blocks => Set<Block>();
//    public DbSet<PrivacyAgreement> PrivacyAgreements => Set<PrivacyAgreement>();

//    protected override void OnModelCreating(ModelBuilder mb)
//    {
//        base.OnModelCreating(mb);

//        // ── Global soft-delete filters ──────────────────────────────────
//        mb.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
//        mb.Entity<Match>().HasQueryFilter(e => !e.IsDeleted);
//        mb.Entity<Message>().HasQueryFilter(e => !e.IsDeleted);

//        // ── Unique constraints ──────────────────────────────────────────
//        mb.Entity<User>().HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
//        mb.Entity<User>().HasIndex(u => u.Phone).IsUnique().HasFilter("\"Phone\" IS NOT NULL");
//        mb.Entity<Block>().HasIndex(b => new { b.BlockerId, b.BlockedUserId }).IsUnique();
//        mb.Entity<Swipe>().HasIndex(s => new { s.SwiperId, s.TargetId }).IsUnique();
//        mb.Entity<UserInterest>().HasKey(ui => new { ui.UserId, ui.InterestId });

//        // ── Relationships ───────────────────────────────────────────────
//        mb.Entity<UserPreference>()
//            .HasOne(p => p.User).WithOne(u => u.Preference)
//            .HasForeignKey<UserPreference>(p => p.UserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        mb.Entity<UserLocation>()
//            .HasOne(l => l.User).WithOne(u => u.Location)
//            .HasForeignKey<UserLocation>(l => l.UserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        mb.Entity<UserSubscription>()
//            .HasOne(s => s.User).WithOne(u => u.Subscription)
//            .HasForeignKey<UserSubscription>(s => s.UserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        mb.Entity<Chat>()
//            .HasOne(c => c.Match).WithOne(m => m.Chat)
//            .HasForeignKey<Chat>(c => c.MatchId)
//            .OnDelete(DeleteBehavior.Restrict);

//        mb.Entity<Match>()
//            .HasOne(m => m.User1).WithMany().HasForeignKey(m => m.User1Id).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<Match>()
//            .HasOne(m => m.User2).WithMany().HasForeignKey(m => m.User2Id).OnDelete(DeleteBehavior.Restrict);

//        mb.Entity<Message>()
//            .HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<Message>()
//            .HasOne(m => m.ReplyToMessage).WithMany().HasForeignKey(m => m.ReplyToMessageId)
//            .OnDelete(DeleteBehavior.SetNull);

//        mb.Entity<Swipe>()
//            .HasOne(s => s.Swiper).WithMany().HasForeignKey(s => s.SwiperId).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<Swipe>()
//            .HasOne(s => s.Target).WithMany().HasForeignKey(s => s.TargetId).OnDelete(DeleteBehavior.Restrict);

//        mb.Entity<CallSession>()
//            .HasOne(c => c.Caller).WithMany().HasForeignKey(c => c.CallerId).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<CallSession>()
//            .HasOne(c => c.Receiver).WithMany().HasForeignKey(c => c.ReceiverId).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<CallSession>()
//            .HasOne(c => c.Match).WithMany().HasForeignKey(c => c.MatchId).OnDelete(DeleteBehavior.Restrict);

//        mb.Entity<SuperChat>()
//            .HasOne(s => s.FromUser).WithMany().HasForeignKey(s => s.FromUserId).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<SuperChat>()
//            .HasOne(s => s.ToUser).WithMany().HasForeignKey(s => s.ToUserId).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<SuperChat>()
//            .HasOne(s => s.MatchCreated).WithMany().HasForeignKey(s => s.MatchCreatedId)
//            .OnDelete(DeleteBehavior.SetNull).IsRequired(false);

//        mb.Entity<Block>()
//            .HasOne(b => b.Blocker).WithMany().HasForeignKey(b => b.BlockerId).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<Block>()
//            .HasOne(b => b.BlockedUser).WithMany().HasForeignKey(b => b.BlockedUserId).OnDelete(DeleteBehavior.Restrict);

//        mb.Entity<Report>()
//            .HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterId).OnDelete(DeleteBehavior.Restrict);
//        mb.Entity<Report>()
//            .HasOne(r => r.ReportedUser).WithMany().HasForeignKey(r => r.ReportedUserId).OnDelete(DeleteBehavior.Restrict);

//        mb.Entity<SubscriptionPlan>().Property(p => p.Price).HasPrecision(18, 2);

//        SeedData(mb);
//    }

//    private static void SeedData(ModelBuilder mb)
//    {
//        //var hash = "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq"; // Mingley@123

//        // ── Interests ──────────────────────────────────────────────────
//        var hash = "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq"; // Mingley@123

//        // ── Interests ──────────────────────────────────────────────────
//        mb.Entity<Interest>().HasData(
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "Music",        Icon = "musical-notes-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000002"), Name = "Travel",       Icon = "airplane-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000003"), Name = "Gym",          Icon = "barbell-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000004"), Name = "Movies",       Icon = "film-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000005"), Name = "Reading",      Icon = "book-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000006"), Name = "Cooking",      Icon = "restaurant-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000007"), Name = "Art",          Icon = "color-palette-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000008"), Name = "Dancing",      Icon = "body-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000009"), Name = "Photography",  Icon = "camera-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000010"), Name = "Yoga",         Icon = "body-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000011"), Name = "Cricket",      Icon = "baseball-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000012"), Name = "Gaming",       Icon = "game-controller-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000013"), Name = "Shopping",     Icon = "bag-handle-outline" },
//            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000014"), Name = "Foodie",       Icon = "pizza-outline" }
//        );

//        // ── Subscription Plans ─────────────────────────────────────────
//        mb.Entity<SubscriptionPlan>().HasData(
//            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"), Name = "Silver",   Price = 299,  DurationDays = 30, Features = "[\"Unlimited likes\",\"No ads\",\"5 Super Likes/day\",\"See who liked you\"]",           IsPopular = false, SuperLikesPerDay = 5,  BoostsPerMonth = 0, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = false },
//            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000002"), Name = "Gold",     Price = 599,  DurationDays = 30, Features = "[\"All Silver\",\"Video calls\",\"10 Super Likes/day\",\"Profile boost\",\"5 coin/msg\"]",  IsPopular = true,  SuperLikesPerDay = 10, BoostsPerMonth = 2, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true  },
//            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000003"), Name = "Platinum", Price = 999,  DurationDays = 30, Features = "[\"All Gold\",\"Top picks daily\",\"Unlimited Super Likes\",\"Priority support\"]",          IsPopular = false, SuperLikesPerDay = -1, BoostsPerMonth = 5, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true  }
//        );

//        // ── Gifts ──────────────────────────────────────────────────────
//        mb.Entity<Gift>().HasData(
//            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"), Name = "Heart",        Icon = "heart-outline",   CoinCost = 10,  Emoji = "❤️" },
//            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000002"), Name = "Rose",         Icon = "rose-outline",    CoinCost = 20,  Emoji = "🌹" },
//            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000003"), Name = "Gift Box",     Icon = "gift-outline",    CoinCost = 50,  Emoji = "🎁" },
//            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000004"), Name = "Coffee Date",  Icon = "cafe-outline",    CoinCost = 200, Emoji = "☕" },
//            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000005"), Name = "Diamond Ring", Icon = "diamond-outline", CoinCost = 500, Emoji = "💍" }
//        );

//        // ── Users (20 users for comprehensive testing) ─────────────────
//        var adminId  = Guid.Parse("d0000001-0000-0000-0000-000000000001");
//        var priyaId  = Guid.Parse("d0000001-0000-0000-0000-000000000002");
//        var rahulId  = Guid.Parse("d0000001-0000-0000-0000-000000000003");
//        var arjunId  = Guid.Parse("d0000001-0000-0000-0000-000000000004");
//        var nehaId   = Guid.Parse("d0000001-0000-0000-0000-000000000005");
//        var vikramId = Guid.Parse("d0000001-0000-0000-0000-000000000006");
//        var ankitaId = Guid.Parse("d0000001-0000-0000-0000-000000000007");
//        var deepakId = Guid.Parse("d0000001-0000-0000-0000-000000000008");
//        var aishaId  = Guid.Parse("d0000001-0000-0000-0000-000000000009");
//        var rohitId  = Guid.Parse("d0000001-0000-0000-0000-000000000010");
//        // 10 more users for edge case testing
//        var shreyaId = Guid.Parse("d0000001-0000-0000-0000-000000000011");
//        var karthikId= Guid.Parse("d0000001-0000-0000-0000-000000000012");
//        var meenaId  = Guid.Parse("d0000001-0000-0000-0000-000000000013");
//        var rajeshId = Guid.Parse("d0000001-0000-0000-0000-000000000014");
//        var pooja2Id = Guid.Parse("d0000001-0000-0000-0000-000000000015");
//        var amanId   = Guid.Parse("d0000001-0000-0000-0000-000000000016");
//        var kritika2Id=Guid.Parse("d0000001-0000-0000-0000-000000000017");
//        var saurabhId= Guid.Parse("d0000001-0000-0000-0000-000000000018");
//        var ritaId   = Guid.Parse("d0000001-0000-0000-0000-000000000019");
//        var mohanId  = Guid.Parse("d0000001-0000-0000-0000-000000000020");

//        mb.Entity<User>().HasData(
//            new User { Id = adminId,   FullName = "Super Admin",      Email = "admin@mingley.app",   PasswordHash = hash, Gender = "male",   Role = "admin", IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 9999,  ProfileComplete = true,  DateOfBirth = new DateTime(1990,1,1,0,0,0,DateTimeKind.Utc),   Avatar = "https://randomuser.me/api/portraits/men/1.jpg" },
//            new User { Id = priyaId,   FullName = "Priya Sharma",     Email = "priya@demo.com",      PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 2500,  ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1998,3,15,0,0,0,DateTimeKind.Utc),  Bio = "Love dancing, yoga and cooking 🌺 | Delhi girl",           Avatar = "https://randomuser.me/api/portraits/women/44.jpg" },
//            new User { Id = rahulId,   FullName = "Rahul Mehta",      Email = "rahul@demo.com",      PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 5000,  ProfileComplete = true,  DateOfBirth = new DateTime(1995,7,22,0,0,0,DateTimeKind.Utc),   Bio = "Music lover 🎵 | Traveller | Software Engineer",          Avatar = "https://randomuser.me/api/portraits/men/32.jpg" },
//            new User { Id = arjunId,   FullName = "Arjun Singh",      Email = "arjun@demo.com",      PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 10000, ProfileComplete = true,  DateOfBirth = new DateTime(1993,11,5,0,0,0,DateTimeKind.Utc),   Bio = "Fitness enthusiast 💪 | Photographer | Noida",             Avatar = "https://randomuser.me/api/portraits/men/45.jpg" },
//            new User { Id = nehaId,    FullName = "Neha Kapoor",      Email = "neha@demo.com",       PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 800,   ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1999,7,20,0,0,0,DateTimeKind.Utc),   Bio = "Singer and travel lover 🎵✈️ | Mumbai",                   Avatar = "https://randomuser.me/api/portraits/women/68.jpg" },
//            new User { Id = vikramId,  FullName = "Vikram Nair",      Email = "vikram@demo.com",     PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 3000,  ProfileComplete = true,  DateOfBirth = new DateTime(1996,4,12,0,0,0,DateTimeKind.Utc),   Bio = "Entrepreneur | Coffee addict ☕ | Delhi",                 Avatar = "https://randomuser.me/api/portraits/men/75.jpg" },
//            new User { Id = ankitaId,  FullName = "Ankita Singh",     Email = "ankita@demo.com",     PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 1200,  ProfileComplete = true,  DateOfBirth = new DateTime(2000,11,5,0,0,0,DateTimeKind.Utc),   Bio = "Foodie and photographer 📸🍕 | Pune",                     Avatar = "https://randomuser.me/api/portraits/women/90.jpg" },
//            new User { Id = deepakId,  FullName = "Deepak Verma",     Email = "deepak@demo.com",     PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = false, IsActive = true,  IsPremium = false, CoinBalance = 500,   ProfileComplete = true,  DateOfBirth = new DateTime(1997,9,30,0,0,0,DateTimeKind.Utc),   Bio = "Gym rat 🏋️ | Cricket fan | Noida",                        Avatar = "https://randomuser.me/api/portraits/men/88.jpg" },
//            new User { Id = aishaId,   FullName = "Aisha Khan",       Email = "aisha@demo.com",      PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 1800,  ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1999,2,14,0,0,0,DateTimeKind.Utc),   Bio = "Fashion lover 👗 | Artist | Hyderabad",                   Avatar = "https://randomuser.me/api/portraits/women/55.jpg" },
//            new User { Id = rohitId,   FullName = "Rohit Sharma",     Email = "rohit@demo.com",      PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 2000,  ProfileComplete = true,  DateOfBirth = new DateTime(1994,6,25,0,0,0,DateTimeKind.Utc),   Bio = "Chef 🍳 | Food blogger | Bengaluru",                      Avatar = "https://randomuser.me/api/portraits/men/60.jpg" },
//            // Extended users for edge case coverage
//            new User { Id = shreyaId,  FullName = "Shreya Patel",     Email = "shreya@demo.com",     PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 3500,  ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1997,5,10,0,0,0,DateTimeKind.Utc),   Bio = "Doctor by day, dancer by night 💃 | Ahmedabad",           Avatar = "https://randomuser.me/api/portraits/women/30.jpg" },
//            new User { Id = karthikId, FullName = "Karthik Menon",    Email = "karthik@demo.com",    PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 4500,  ProfileComplete = true,  DateOfBirth = new DateTime(1992,8,18,0,0,0,DateTimeKind.Utc),   Bio = "IIT grad | Startup founder 🚀 | Chennai",                Avatar = "https://randomuser.me/api/portraits/men/20.jpg" },
//            new User { Id = meenaId,   FullName = "Meena Reddy",      Email = "meena@demo.com",      PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = false, IsActive = true,  IsPremium = false, CoinBalance = 700,   ProfileComplete = true,  DateOfBirth = new DateTime(2001,3,22,0,0,0,DateTimeKind.Utc),   Bio = "Engineering student 📚 | Sketch artist | Hyderabad",     Avatar = "https://randomuser.me/api/portraits/women/22.jpg" },
//            new User { Id = rajeshId,  FullName = "Rajesh Kumar",     Email = "rajesh@demo.com",     PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 1500,  ProfileComplete = true,  DateOfBirth = new DateTime(1990,12,3,0,0,0,DateTimeKind.Utc),   Bio = "Senior dev | Gaming enthusiast 🎮 | Kolkata",            Avatar = "https://randomuser.me/api/portraits/men/40.jpg" },
//            new User { Id = pooja2Id,  FullName = "Pooja Gupta",      Email = "pooja@demo.com",      PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 950,   ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1998,9,7,0,0,0,DateTimeKind.Utc),    Bio = "Marketing lead | Loves reading 📖 | Jaipur",             Avatar = "https://randomuser.me/api/portraits/women/15.jpg" },
//            new User { Id = amanId,    FullName = "Aman Joshi",       Email = "aman@demo.com",       PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 6000,  ProfileComplete = true,  DateOfBirth = new DateTime(1994,2,28,0,0,0,DateTimeKind.Utc),   Bio = "Architect | Art lover 🏛️ | Chandigarh",                   Avatar = "https://randomuser.me/api/portraits/men/55.jpg" },
//            new User { Id = kritika2Id,FullName = "Kritika Bose",     Email = "kritika@demo.com",    PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 1100,  ProfileComplete = true,  DateOfBirth = new DateTime(1996,7,14,0,0,0,DateTimeKind.Utc),   Bio = "Journalist | Avid traveller ✈️ | Kolkata",               Avatar = "https://randomuser.me/api/portraits/women/35.jpg" },
//            new User { Id = saurabhId, FullName = "Saurabh Mishra",   Email = "saurabh@demo.com",    PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = false, IsActive = true,  IsPremium = false, CoinBalance = 250,   ProfileComplete = true,  DateOfBirth = new DateTime(1999,4,16,0,0,0,DateTimeKind.Utc),   Bio = "Cricketer ⚡ | College student | Lucknow",               Avatar = "https://randomuser.me/api/portraits/men/70.jpg" },
//            new User { Id = ritaId,    FullName = "Rita Desai",       Email = "rita@demo.com",       PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 4200,  ProfileComplete = true,  DateOfBirth = new DateTime(1993,11,25,0,0,0,DateTimeKind.Utc),  Bio = "Finance professional 💼 | Yoga instructor | Surat",       Avatar = "https://randomuser.me/api/portraits/women/50.jpg" },
//            new User { Id = mohanId,   FullName = "Mohan Pillai",     Email = "mohan@demo.com",      PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = false, IsPremium = false, CoinBalance = 100,   ProfileComplete = true,  DateOfBirth = new DateTime(1988,6,8,0,0,0,DateTimeKind.Utc),    Bio = "Retired athlete | Fitness coach | Thiruvananthapuram",    Avatar = "https://randomuser.me/api/portraits/men/80.jpg" }
//        );

//        // ── User Preferences ───────────────────────────────────────────
//        mb.Entity<UserPreference>().HasData(
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000001"), UserId = priyaId,   InterestedIn = "boys",  MinAge = 22, MaxAge = 35, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000002"), UserId = rahulId,   InterestedIn = "girls", MinAge = 20, MaxAge = 30, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000003"), UserId = arjunId,   InterestedIn = "girls", MinAge = 21, MaxAge = 32, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000004"), UserId = nehaId,    InterestedIn = "boys",  MinAge = 23, MaxAge = 33, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000005"), UserId = vikramId,  InterestedIn = "girls", MinAge = 21, MaxAge = 30, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000006"), UserId = ankitaId,  InterestedIn = "boys",  MinAge = 24, MaxAge = 34, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000007"), UserId = deepakId,  InterestedIn = "girls", MinAge = 20, MaxAge = 28, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000008"), UserId = aishaId,   InterestedIn = "boys",  MinAge = 22, MaxAge = 32, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000009"), UserId = rohitId,   InterestedIn = "girls", MinAge = 20, MaxAge = 30, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000011"), UserId = shreyaId,  InterestedIn = "boys",  MinAge = 25, MaxAge = 36, MaxDistance = 150 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000012"), UserId = karthikId, InterestedIn = "girls", MinAge = 22, MaxAge = 30, MaxDistance = 200 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000013"), UserId = meenaId,   InterestedIn = "boys",  MinAge = 22, MaxAge = 28, MaxDistance = 50  },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000014"), UserId = rajeshId,  InterestedIn = "girls", MinAge = 24, MaxAge = 32, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000015"), UserId = pooja2Id,  InterestedIn = "boys",  MinAge = 24, MaxAge = 32, MaxDistance = 100 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000016"), UserId = amanId,    InterestedIn = "girls", MinAge = 23, MaxAge = 32, MaxDistance = 150 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000017"), UserId = kritika2Id,InterestedIn = "boys",  MinAge = 25, MaxAge = 35, MaxDistance = 200 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000018"), UserId = saurabhId, InterestedIn = "girls", MinAge = 18, MaxAge = 26, MaxDistance = 50  },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000019"), UserId = ritaId,    InterestedIn = "boys",  MinAge = 27, MaxAge = 40, MaxDistance = 200 },
//            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000020"), UserId = mohanId,   InterestedIn = "girls", MinAge = 28, MaxAge = 38, MaxDistance = 100 }
//        );

//        // ── Locations (diverse cities across India) ────────────────────
//        mb.Entity<UserLocation>().HasData(
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"), UserId = priyaId,    City = "Delhi",             Country = "India", Lat = 28.6139, Lng = 77.2090 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000002"), UserId = rahulId,    City = "Noida",             Country = "India", Lat = 28.5355, Lng = 77.3910 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000003"), UserId = arjunId,    City = "Gurgaon",           Country = "India", Lat = 28.4595, Lng = 77.0266 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000004"), UserId = nehaId,     City = "Mumbai",            Country = "India", Lat = 19.0760, Lng = 72.8777 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000005"), UserId = vikramId,   City = "Delhi",             Country = "India", Lat = 28.7041, Lng = 77.1025 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000006"), UserId = ankitaId,   City = "Pune",              Country = "India", Lat = 18.5204, Lng = 73.8567 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000007"), UserId = deepakId,   City = "Noida",             Country = "India", Lat = 28.5400, Lng = 77.4000 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000008"), UserId = aishaId,    City = "Hyderabad",         Country = "India", Lat = 17.3850, Lng = 78.4867 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000009"), UserId = rohitId,    City = "Bengaluru",         Country = "India", Lat = 12.9716, Lng = 77.5946 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000011"), UserId = shreyaId,   City = "Ahmedabad",         Country = "India", Lat = 23.0225, Lng = 72.5714 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000012"), UserId = karthikId,  City = "Chennai",           Country = "India", Lat = 13.0827, Lng = 80.2707 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000013"), UserId = meenaId,    City = "Hyderabad",         Country = "India", Lat = 17.4000, Lng = 78.5000 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000014"), UserId = rajeshId,   City = "Kolkata",           Country = "India", Lat = 22.5726, Lng = 88.3639 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000015"), UserId = pooja2Id,   City = "Jaipur",            Country = "India", Lat = 26.9124, Lng = 75.7873 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000016"), UserId = amanId,     City = "Chandigarh",        Country = "India", Lat = 30.7333, Lng = 76.7794 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000017"), UserId = kritika2Id, City = "Kolkata",           Country = "India", Lat = 22.5800, Lng = 88.3500 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000018"), UserId = saurabhId,  City = "Lucknow",           Country = "India", Lat = 26.8467, Lng = 80.9462 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000019"), UserId = ritaId,     City = "Surat",             Country = "India", Lat = 21.1702, Lng = 72.8311 },
//            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000020"), UserId = mohanId,    City = "Thiruvananthapuram",Country = "India", Lat = 8.5241,  Lng = 76.9366 }
//        );

//        // ── User Interests ─────────────────────────────────────────────
//        mb.Entity<UserInterest>().HasData(
//            new UserInterest { UserId = priyaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
//            new UserInterest { UserId = priyaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
//            new UserInterest { UserId = priyaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
//            new UserInterest { UserId = rahulId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
//            new UserInterest { UserId = rahulId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
//            new UserInterest { UserId = rahulId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
//            new UserInterest { UserId = arjunId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
//            new UserInterest { UserId = arjunId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
//            new UserInterest { UserId = nehaId,     InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
//            new UserInterest { UserId = nehaId,     InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
//            new UserInterest { UserId = ankitaId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
//            new UserInterest { UserId = ankitaId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
//            new UserInterest { UserId = vikramId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
//            new UserInterest { UserId = deepakId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
//            new UserInterest { UserId = deepakId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
//            new UserInterest { UserId = aishaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
//            new UserInterest { UserId = rohitId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
//            new UserInterest { UserId = shreyaId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
//            new UserInterest { UserId = shreyaId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
//            new UserInterest { UserId = karthikId,  InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
//            new UserInterest { UserId = karthikId,  InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
//            new UserInterest { UserId = meenaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
//            new UserInterest { UserId = rajeshId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
//            new UserInterest { UserId = pooja2Id,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
//            new UserInterest { UserId = amanId,     InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
//            new UserInterest { UserId = kritika2Id, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
//            new UserInterest { UserId = saurabhId,  InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
//            new UserInterest { UserId = ritaId,     InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
//            new UserInterest { UserId = mohanId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") }
//        );

//        // ── Pre-seeded Matches ─────────────────────────────────────────
//        var match1Id = Guid.Parse("a1000001-0000-0000-0000-000000000001");
//        var match2Id = Guid.Parse("a1000001-0000-0000-0000-000000000002");
//        var match3Id = Guid.Parse("a1000001-0000-0000-0000-000000000003");
//        var chat1Id  = Guid.Parse("a1000002-0000-0000-0000-000000000001");
//        var chat2Id  = Guid.Parse("a1000002-0000-0000-0000-000000000002");
//        var chat3Id  = Guid.Parse("a1000002-0000-0000-0000-000000000003");

//        mb.Entity<Swipe>().HasData(
//            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000001"), SwiperId = rahulId,   TargetId = priyaId,    Action = "like",      CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
//            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000002"), SwiperId = priyaId,   TargetId = rahulId,    Action = "like",      CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
//            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000003"), SwiperId = arjunId,   TargetId = aishaId,    Action = "superlike", CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
//            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000004"), SwiperId = aishaId,   TargetId = arjunId,    Action = "like",      CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
//            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000005"), SwiperId = vikramId,  TargetId = shreyaId,   Action = "like",      CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) },
//            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000006"), SwiperId = shreyaId,  TargetId = vikramId,   Action = "like",      CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) }
//        );

//        mb.Entity<Match>().HasData(
//            new Match { Id = match1Id, User1Id = rahulId,  User2Id = priyaId,  IsActive = true, CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
//            new Match { Id = match2Id, User1Id = arjunId,  User2Id = aishaId,  IsActive = true, CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
//            new Match { Id = match3Id, User1Id = vikramId, User2Id = shreyaId, IsActive = true, CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) }
//        );

//        mb.Entity<Chat>().HasData(
//            new Chat { Id = chat1Id, MatchId = match1Id, CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
//            new Chat { Id = chat2Id, MatchId = match2Id, CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
//            new Chat { Id = chat3Id, MatchId = match3Id, CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) }
//        );

//        mb.Entity<Message>().HasData(
//            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000001"), ChatId = chat1Id, SenderId = rahulId,  Text = "Hey Priya! We matched 🎉 How are you?",        Type = "text", CoinsDeducted = 10, ReadAt = new DateTime(2024,1,2,1,0,0,DateTimeKind.Utc), CreatedAt = new DateTime(2024,1,2,0,30,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000002"), ChatId = chat1Id, SenderId = priyaId,  Text = "Hi Rahul! I'm great, thanks! 😊",              Type = "text", CoinsDeducted = 0,  ReadAt = new DateTime(2024,1,2,2,0,0,DateTimeKind.Utc), CreatedAt = new DateTime(2024,1,2,1,0,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000003"), ChatId = chat1Id, SenderId = rahulId,  Text = "I saw you love dancing 💃 so cool!",           Type = "text", CoinsDeducted = 10, ReadAt = new DateTime(2024,1,2,3,0,0,DateTimeKind.Utc), CreatedAt = new DateTime(2024,1,2,2,0,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000004"), ChatId = chat1Id, SenderId = priyaId,  Text = "Yes! Been dancing since I was 8 🎵",            Type = "text", CoinsDeducted = 0,  CreatedAt = new DateTime(2024,1,2,3,0,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000005"), ChatId = chat1Id, SenderId = rahulId,  Text = "Amazing! I play guitar 🎸 we should jam!",     Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024,1,2,4,0,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000001"), ChatId = chat2Id, SenderId = arjunId,  Text = "Hi Aisha! I sent you a super like 🌟",          Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024,1,3,1,0,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000002"), ChatId = chat2Id, SenderId = aishaId,  Text = "Aww thank you! I loved your photos 😍",         Type = "text", CoinsDeducted = 0,  CreatedAt = new DateTime(2024,1,3,2,0,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000003"), ChatId = chat2Id, SenderId = arjunId,  Text = "What are you up to this weekend? 🎯",           Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024,1,3,3,0,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000001"), ChatId = chat3Id, SenderId = vikramId, Text = "Hey Shreya! Great to match with you ✨",         Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024,1,4,1,0,0,DateTimeKind.Utc) },
//            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000002"), ChatId = chat3Id, SenderId = shreyaId, Text = "Hi Vikram! You seem interesting 😊 What do you do?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024,1,4,2,0,0,DateTimeKind.Utc) }
//        );

//        mb.Entity<CallSession>().HasData(
//            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000001"), CallerId = arjunId,  ReceiverId = aishaId,  MatchId = match2Id, CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024,1,3,3,0,0,DateTimeKind.Utc), EndedAt = new DateTime(2024,1,3,3,5,0,DateTimeKind.Utc), DurationSeconds = 300, CoinsDeducted = 500, EndReason = "user_ended", CreatedAt = new DateTime(2024,1,3,3,0,0,DateTimeKind.Utc) },
//            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000002"), CallerId = rahulId,  ReceiverId = priyaId,  MatchId = match1Id, CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024,1,2,5,0,0,DateTimeKind.Utc), EndedAt = new DateTime(2024,1,2,5,3,0,DateTimeKind.Utc), DurationSeconds = 180, CoinsDeducted = 30,  EndReason = "user_ended", CreatedAt = new DateTime(2024,1,2,5,0,0,DateTimeKind.Utc) },
//            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000003"), CallerId = vikramId, ReceiverId = shreyaId, MatchId = match3Id, CallType = "audio", Status = "missed", EndedAt = new DateTime(2024,1,4,6,0,0,DateTimeKind.Utc), CoinsDeducted = 0, CreatedAt = new DateTime(2024,1,4,6,0,0,DateTimeKind.Utc) },
//            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000004"), CallerId = priyaId,  ReceiverId = rahulId,  MatchId = match1Id, CallType = "audio", Status = "declined", EndedAt = new DateTime(2024,1,5,8,0,0,DateTimeKind.Utc), CoinsDeducted = 0, CreatedAt = new DateTime(2024,1,5,8,0,0,DateTimeKind.Utc) }
//        );

//        mb.Entity<Notification>().HasData(
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000001"), UserId = rahulId,  Title = "New Match! 🎉",        Body = "You matched with Priya Sharma!",  Type = "match",   IsRead = false, ReferenceId = match1Id.ToString(), CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000002"), UserId = priyaId,  Title = "New Match! 🎉",        Body = "You matched with Rahul Mehta!",   Type = "match",   IsRead = true,  ReferenceId = match1Id.ToString(), CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000003"), UserId = arjunId,  Title = "New Match! 🎉",        Body = "You matched with Aisha Khan!",    Type = "match",   IsRead = false, ReferenceId = match2Id.ToString(), CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000004"), UserId = aishaId,  Title = "New Match! 🎉",        Body = "You matched with Arjun Singh!",   Type = "match",   IsRead = false, ReferenceId = match2Id.ToString(), CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000005"), UserId = vikramId, Title = "New Match! 🎉",        Body = "You matched with Shreya Patel!",  Type = "match",   IsRead = false, ReferenceId = match3Id.ToString(), CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000006"), UserId = shreyaId, Title = "New Match! 🎉",        Body = "You matched with Vikram Nair!",   Type = "match",   IsRead = true,  ReferenceId = match3Id.ToString(), CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000007"), UserId = rahulId,  Title = "New Message 💬",       Body = "Priya sent you a message",        Type = "message", IsRead = false, CreatedAt = new DateTime(2024,1,2,1,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000008"), UserId = priyaId,  Title = "Coins Added 🪙",       Body = "Welcome bonus: +100 coins",       Type = "coins",   IsRead = true,  CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000009"), UserId = arjunId,  Title = "Call Missed 📞",       Body = "You missed a call from Aisha",    Type = "call",    IsRead = false, CreatedAt = new DateTime(2024,1,3,4,0,0,DateTimeKind.Utc) },
//            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000010"), UserId = aishaId,  Title = "Incoming Call 📹",     Body = "Arjun is calling — video call",   Type = "call",    IsRead = true,  CreatedAt = new DateTime(2024,1,3,3,0,0,DateTimeKind.Utc) }
//        );

//        mb.Entity<UserSubscription>().HasData(
//            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000001"), UserId = arjunId,  PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc), IsActive = true, AutoRenew = true },
//            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000002"), UserId = priyaId,  PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc), IsActive = true, AutoRenew = true },
//            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000003"), UserId = shreyaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000003"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc), IsActive = true, AutoRenew = true },
//            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000004"), UserId = amanId,   PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000001"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,6,30,0,0,0,DateTimeKind.Utc),  IsActive = true, AutoRenew = false },
//            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000005"), UserId = ritaId,   PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc), IsActive = true, AutoRenew = true }
//        );
//    }
//}
