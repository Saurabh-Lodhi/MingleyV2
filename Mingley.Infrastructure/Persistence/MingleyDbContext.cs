using Microsoft.EntityFrameworkCore;
using Mingley.Domain.Entities;

namespace Mingley.Infrastructure.Persistence;

public class MingleyDbContext : DbContext
{
    // ── Coin economy constants ─────────────────────────────────────────────
    public const int AudioCallCoinPerMin   = 10;
    public const int VideoCallCoinPerMin   = 100;
    public const int VerificationBonus    = 50;
    public const int WelcomeBonus         = 100;
    public const int SuperLikeCost        = 50;
    public const int SuperChatCost        = 500;
    public const double CoinToInrRate     = 0.10;   // 1 coin = ₹0.10
    public const double GirlCommissionPct = 0.50;   // 50% to girl
    public const double FemaleWithdrawPct = 0.70;
    public const int MaleCostPerMessage   = 10;
    public const int MalePremiumCostPerMsg= 5;
    public const int FemaleFreeMessages   = 3;
    public const int FemaleMessageCost    = 5;

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

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── Global soft-delete filters ──────────────────────────────────
        mb.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<Match>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<Message>().HasQueryFilter(e => !e.IsDeleted);

        // ── Unique constraints ──────────────────────────────────────────
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
        mb.Entity<User>().HasIndex(u => u.Phone).IsUnique().HasFilter("\"Phone\" IS NOT NULL");
        mb.Entity<Block>().HasIndex(b => new { b.BlockerId, b.BlockedUserId }).IsUnique();
        mb.Entity<Swipe>().HasIndex(s => new { s.SwiperId, s.TargetId }).IsUnique();
        mb.Entity<UserInterest>().HasKey(ui => new { ui.UserId, ui.InterestId });

        // ── Relationships ───────────────────────────────────────────────
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

        SeedData(mb);
    }

    private static void SeedData(ModelBuilder mb)
    {
        //var hash = "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq"; // Mingley@123

        // ── Interests ──────────────────────────────────────────────────
        var hash = "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq"; // Mingley@123

        // ── Interests ──────────────────────────────────────────────────
        mb.Entity<Interest>().HasData(
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "Music",        Icon = "musical-notes-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000002"), Name = "Travel",       Icon = "airplane-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000003"), Name = "Gym",          Icon = "barbell-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000004"), Name = "Movies",       Icon = "film-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000005"), Name = "Reading",      Icon = "book-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000006"), Name = "Cooking",      Icon = "restaurant-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000007"), Name = "Art",          Icon = "color-palette-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000008"), Name = "Dancing",      Icon = "body-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000009"), Name = "Photography",  Icon = "camera-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000010"), Name = "Yoga",         Icon = "body-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000011"), Name = "Cricket",      Icon = "baseball-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000012"), Name = "Gaming",       Icon = "game-controller-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000013"), Name = "Shopping",     Icon = "bag-handle-outline" },
            new Interest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000014"), Name = "Foodie",       Icon = "pizza-outline" }
        );

        // ── Subscription Plans ─────────────────────────────────────────
        mb.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"), Name = "Silver",   Price = 299,  DurationDays = 30, Features = "[\"Unlimited likes\",\"No ads\",\"5 Super Likes/day\",\"See who liked you\"]",           IsPopular = false, SuperLikesPerDay = 5,  BoostsPerMonth = 0, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = false },
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000002"), Name = "Gold",     Price = 599,  DurationDays = 30, Features = "[\"All Silver\",\"Video calls\",\"10 Super Likes/day\",\"Profile boost\",\"5 coin/msg\"]",  IsPopular = true,  SuperLikesPerDay = 10, BoostsPerMonth = 2, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true  },
            new SubscriptionPlan { Id = Guid.Parse("b0000001-0000-0000-0000-000000000003"), Name = "Platinum", Price = 999,  DurationDays = 30, Features = "[\"All Gold\",\"Top picks daily\",\"Unlimited Super Likes\",\"Priority support\"]",          IsPopular = false, SuperLikesPerDay = -1, BoostsPerMonth = 5, UnlimitedLikes = true, CanSeeWhoLiked = true, VideoCallEnabled = true  }
        );

        // ── Gifts ──────────────────────────────────────────────────────
        mb.Entity<Gift>().HasData(
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"), Name = "Heart",        Icon = "heart-outline",   CoinCost = 10,  Emoji = "❤️" },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000002"), Name = "Rose",         Icon = "rose-outline",    CoinCost = 20,  Emoji = "🌹" },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000003"), Name = "Gift Box",     Icon = "gift-outline",    CoinCost = 50,  Emoji = "🎁" },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000004"), Name = "Coffee Date",  Icon = "cafe-outline",    CoinCost = 200, Emoji = "☕" },
            new Gift { Id = Guid.Parse("c0000001-0000-0000-0000-000000000005"), Name = "Diamond Ring", Icon = "diamond-outline", CoinCost = 500, Emoji = "💍" }
        );

        // ── Users (20 users for comprehensive testing) ─────────────────
        var adminId  = Guid.Parse("d0000001-0000-0000-0000-000000000001");
        var priyaId  = Guid.Parse("d0000001-0000-0000-0000-000000000002");
        var rahulId  = Guid.Parse("d0000001-0000-0000-0000-000000000003");
        var arjunId  = Guid.Parse("d0000001-0000-0000-0000-000000000004");
        var nehaId   = Guid.Parse("d0000001-0000-0000-0000-000000000005");
        var vikramId = Guid.Parse("d0000001-0000-0000-0000-000000000006");
        var ankitaId = Guid.Parse("d0000001-0000-0000-0000-000000000007");
        var deepakId = Guid.Parse("d0000001-0000-0000-0000-000000000008");
        var aishaId  = Guid.Parse("d0000001-0000-0000-0000-000000000009");
        var rohitId  = Guid.Parse("d0000001-0000-0000-0000-000000000010");
        // 10 more users for edge case testing
        var shreyaId = Guid.Parse("d0000001-0000-0000-0000-000000000011");
        var karthikId= Guid.Parse("d0000001-0000-0000-0000-000000000012");
        var meenaId  = Guid.Parse("d0000001-0000-0000-0000-000000000013");
        var rajeshId = Guid.Parse("d0000001-0000-0000-0000-000000000014");
        var pooja2Id = Guid.Parse("d0000001-0000-0000-0000-000000000015");
        var amanId   = Guid.Parse("d0000001-0000-0000-0000-000000000016");
        var kritika2Id=Guid.Parse("d0000001-0000-0000-0000-000000000017");
        var saurabhId= Guid.Parse("d0000001-0000-0000-0000-000000000018");
        var ritaId   = Guid.Parse("d0000001-0000-0000-0000-000000000019");
        var mohanId  = Guid.Parse("d0000001-0000-0000-0000-000000000020");

        mb.Entity<User>().HasData(
            new User { Id = adminId,   FullName = "Super Admin",      Email = "admin@mingley.app",   PasswordHash = hash, Gender = "male",   Role = "admin", IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 9999,  ProfileComplete = true,  DateOfBirth = new DateTime(1990,1,1,0,0,0,DateTimeKind.Utc),   Avatar = "https://randomuser.me/api/portraits/men/1.jpg" },
            new User { Id = priyaId,   FullName = "Priya Sharma",     Email = "priya@demo.com",      PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 2500,  ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1998,3,15,0,0,0,DateTimeKind.Utc),  Bio = "Love dancing, yoga and cooking 🌺 | Delhi girl",           Avatar = "https://randomuser.me/api/portraits/women/44.jpg" },
            new User { Id = rahulId,   FullName = "Rahul Mehta",      Email = "rahul@demo.com",      PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 5000,  ProfileComplete = true,  DateOfBirth = new DateTime(1995,7,22,0,0,0,DateTimeKind.Utc),   Bio = "Music lover 🎵 | Traveller | Software Engineer",          Avatar = "https://randomuser.me/api/portraits/men/32.jpg" },
            new User { Id = arjunId,   FullName = "Arjun Singh",      Email = "arjun@demo.com",      PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 10000, ProfileComplete = true,  DateOfBirth = new DateTime(1993,11,5,0,0,0,DateTimeKind.Utc),   Bio = "Fitness enthusiast 💪 | Photographer | Noida",             Avatar = "https://randomuser.me/api/portraits/men/45.jpg" },
            new User { Id = nehaId,    FullName = "Neha Kapoor",      Email = "neha@demo.com",       PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 800,   ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1999,7,20,0,0,0,DateTimeKind.Utc),   Bio = "Singer and travel lover 🎵✈️ | Mumbai",                   Avatar = "https://randomuser.me/api/portraits/women/68.jpg" },
            new User { Id = vikramId,  FullName = "Vikram Nair",      Email = "vikram@demo.com",     PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 3000,  ProfileComplete = true,  DateOfBirth = new DateTime(1996,4,12,0,0,0,DateTimeKind.Utc),   Bio = "Entrepreneur | Coffee addict ☕ | Delhi",                 Avatar = "https://randomuser.me/api/portraits/men/75.jpg" },
            new User { Id = ankitaId,  FullName = "Ankita Singh",     Email = "ankita@demo.com",     PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 1200,  ProfileComplete = true,  DateOfBirth = new DateTime(2000,11,5,0,0,0,DateTimeKind.Utc),   Bio = "Foodie and photographer 📸🍕 | Pune",                     Avatar = "https://randomuser.me/api/portraits/women/90.jpg" },
            new User { Id = deepakId,  FullName = "Deepak Verma",     Email = "deepak@demo.com",     PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = false, IsActive = true,  IsPremium = false, CoinBalance = 500,   ProfileComplete = true,  DateOfBirth = new DateTime(1997,9,30,0,0,0,DateTimeKind.Utc),   Bio = "Gym rat 🏋️ | Cricket fan | Noida",                        Avatar = "https://randomuser.me/api/portraits/men/88.jpg" },
            new User { Id = aishaId,   FullName = "Aisha Khan",       Email = "aisha@demo.com",      PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 1800,  ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1999,2,14,0,0,0,DateTimeKind.Utc),   Bio = "Fashion lover 👗 | Artist | Hyderabad",                   Avatar = "https://randomuser.me/api/portraits/women/55.jpg" },
            new User { Id = rohitId,   FullName = "Rohit Sharma",     Email = "rohit@demo.com",      PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 2000,  ProfileComplete = true,  DateOfBirth = new DateTime(1994,6,25,0,0,0,DateTimeKind.Utc),   Bio = "Chef 🍳 | Food blogger | Bengaluru",                      Avatar = "https://randomuser.me/api/portraits/men/60.jpg" },
            // Extended users for edge case coverage
            new User { Id = shreyaId,  FullName = "Shreya Patel",     Email = "shreya@demo.com",     PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 3500,  ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1997,5,10,0,0,0,DateTimeKind.Utc),   Bio = "Doctor by day, dancer by night 💃 | Ahmedabad",           Avatar = "https://randomuser.me/api/portraits/women/30.jpg" },
            new User { Id = karthikId, FullName = "Karthik Menon",    Email = "karthik@demo.com",    PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 4500,  ProfileComplete = true,  DateOfBirth = new DateTime(1992,8,18,0,0,0,DateTimeKind.Utc),   Bio = "IIT grad | Startup founder 🚀 | Chennai",                Avatar = "https://randomuser.me/api/portraits/men/20.jpg" },
            new User { Id = meenaId,   FullName = "Meena Reddy",      Email = "meena@demo.com",      PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = false, IsActive = true,  IsPremium = false, CoinBalance = 700,   ProfileComplete = true,  DateOfBirth = new DateTime(2001,3,22,0,0,0,DateTimeKind.Utc),   Bio = "Engineering student 📚 | Sketch artist | Hyderabad",     Avatar = "https://randomuser.me/api/portraits/women/22.jpg" },
            new User { Id = rajeshId,  FullName = "Rajesh Kumar",     Email = "rajesh@demo.com",     PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 1500,  ProfileComplete = true,  DateOfBirth = new DateTime(1990,12,3,0,0,0,DateTimeKind.Utc),   Bio = "Senior dev | Gaming enthusiast 🎮 | Kolkata",            Avatar = "https://randomuser.me/api/portraits/men/40.jpg" },
            new User { Id = pooja2Id,  FullName = "Pooja Gupta",      Email = "pooja@demo.com",      PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 950,   ProfileComplete = true,  IsOnline = true,  DateOfBirth = new DateTime(1998,9,7,0,0,0,DateTimeKind.Utc),    Bio = "Marketing lead | Loves reading 📖 | Jaipur",             Avatar = "https://randomuser.me/api/portraits/women/15.jpg" },
            new User { Id = amanId,    FullName = "Aman Joshi",       Email = "aman@demo.com",       PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 6000,  ProfileComplete = true,  DateOfBirth = new DateTime(1994,2,28,0,0,0,DateTimeKind.Utc),   Bio = "Architect | Art lover 🏛️ | Chandigarh",                   Avatar = "https://randomuser.me/api/portraits/men/55.jpg" },
            new User { Id = kritika2Id,FullName = "Kritika Bose",     Email = "kritika@demo.com",    PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = false, CoinBalance = 1100,  ProfileComplete = true,  DateOfBirth = new DateTime(1996,7,14,0,0,0,DateTimeKind.Utc),   Bio = "Journalist | Avid traveller ✈️ | Kolkata",               Avatar = "https://randomuser.me/api/portraits/women/35.jpg" },
            new User { Id = saurabhId, FullName = "Saurabh Mishra",   Email = "saurabh@demo.com",    PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = false, IsActive = true,  IsPremium = false, CoinBalance = 250,   ProfileComplete = true,  DateOfBirth = new DateTime(1999,4,16,0,0,0,DateTimeKind.Utc),   Bio = "Cricketer ⚡ | College student | Lucknow",               Avatar = "https://randomuser.me/api/portraits/men/70.jpg" },
            new User { Id = ritaId,    FullName = "Rita Desai",       Email = "rita@demo.com",       PasswordHash = hash, Gender = "female", Role = "user",  IsVerified = true,  IsActive = true,  IsPremium = true,  CoinBalance = 4200,  ProfileComplete = true,  DateOfBirth = new DateTime(1993,11,25,0,0,0,DateTimeKind.Utc),  Bio = "Finance professional 💼 | Yoga instructor | Surat",       Avatar = "https://randomuser.me/api/portraits/women/50.jpg" },
            new User { Id = mohanId,   FullName = "Mohan Pillai",     Email = "mohan@demo.com",      PasswordHash = hash, Gender = "male",   Role = "user",  IsVerified = true,  IsActive = false, IsPremium = false, CoinBalance = 100,   ProfileComplete = true,  DateOfBirth = new DateTime(1988,6,8,0,0,0,DateTimeKind.Utc),    Bio = "Retired athlete | Fitness coach | Thiruvananthapuram",    Avatar = "https://randomuser.me/api/portraits/men/80.jpg" }
        );

        // ── User Preferences ───────────────────────────────────────────
        mb.Entity<UserPreference>().HasData(
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000001"), UserId = priyaId,   InterestedIn = "boys",  MinAge = 22, MaxAge = 35, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000002"), UserId = rahulId,   InterestedIn = "girls", MinAge = 20, MaxAge = 30, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000003"), UserId = arjunId,   InterestedIn = "girls", MinAge = 21, MaxAge = 32, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000004"), UserId = nehaId,    InterestedIn = "boys",  MinAge = 23, MaxAge = 33, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000005"), UserId = vikramId,  InterestedIn = "girls", MinAge = 21, MaxAge = 30, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000006"), UserId = ankitaId,  InterestedIn = "boys",  MinAge = 24, MaxAge = 34, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000007"), UserId = deepakId,  InterestedIn = "girls", MinAge = 20, MaxAge = 28, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000008"), UserId = aishaId,   InterestedIn = "boys",  MinAge = 22, MaxAge = 32, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000009"), UserId = rohitId,   InterestedIn = "girls", MinAge = 20, MaxAge = 30, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000011"), UserId = shreyaId,  InterestedIn = "boys",  MinAge = 25, MaxAge = 36, MaxDistance = 150 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000012"), UserId = karthikId, InterestedIn = "girls", MinAge = 22, MaxAge = 30, MaxDistance = 200 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000013"), UserId = meenaId,   InterestedIn = "boys",  MinAge = 22, MaxAge = 28, MaxDistance = 50  },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000014"), UserId = rajeshId,  InterestedIn = "girls", MinAge = 24, MaxAge = 32, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000015"), UserId = pooja2Id,  InterestedIn = "boys",  MinAge = 24, MaxAge = 32, MaxDistance = 100 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000016"), UserId = amanId,    InterestedIn = "girls", MinAge = 23, MaxAge = 32, MaxDistance = 150 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000017"), UserId = kritika2Id,InterestedIn = "boys",  MinAge = 25, MaxAge = 35, MaxDistance = 200 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000018"), UserId = saurabhId, InterestedIn = "girls", MinAge = 18, MaxAge = 26, MaxDistance = 50  },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000019"), UserId = ritaId,    InterestedIn = "boys",  MinAge = 27, MaxAge = 40, MaxDistance = 200 },
            new UserPreference { Id = Guid.Parse("e0000001-0000-0000-0000-000000000020"), UserId = mohanId,   InterestedIn = "girls", MinAge = 28, MaxAge = 38, MaxDistance = 100 }
        );

        // ── Locations (diverse cities across India) ────────────────────
        mb.Entity<UserLocation>().HasData(
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"), UserId = priyaId,    City = "Delhi",             Country = "India", Lat = 28.6139, Lng = 77.2090 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000002"), UserId = rahulId,    City = "Noida",             Country = "India", Lat = 28.5355, Lng = 77.3910 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000003"), UserId = arjunId,    City = "Gurgaon",           Country = "India", Lat = 28.4595, Lng = 77.0266 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000004"), UserId = nehaId,     City = "Mumbai",            Country = "India", Lat = 19.0760, Lng = 72.8777 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000005"), UserId = vikramId,   City = "Delhi",             Country = "India", Lat = 28.7041, Lng = 77.1025 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000006"), UserId = ankitaId,   City = "Pune",              Country = "India", Lat = 18.5204, Lng = 73.8567 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000007"), UserId = deepakId,   City = "Noida",             Country = "India", Lat = 28.5400, Lng = 77.4000 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000008"), UserId = aishaId,    City = "Hyderabad",         Country = "India", Lat = 17.3850, Lng = 78.4867 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000009"), UserId = rohitId,    City = "Bengaluru",         Country = "India", Lat = 12.9716, Lng = 77.5946 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000011"), UserId = shreyaId,   City = "Ahmedabad",         Country = "India", Lat = 23.0225, Lng = 72.5714 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000012"), UserId = karthikId,  City = "Chennai",           Country = "India", Lat = 13.0827, Lng = 80.2707 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000013"), UserId = meenaId,    City = "Hyderabad",         Country = "India", Lat = 17.4000, Lng = 78.5000 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000014"), UserId = rajeshId,   City = "Kolkata",           Country = "India", Lat = 22.5726, Lng = 88.3639 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000015"), UserId = pooja2Id,   City = "Jaipur",            Country = "India", Lat = 26.9124, Lng = 75.7873 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000016"), UserId = amanId,     City = "Chandigarh",        Country = "India", Lat = 30.7333, Lng = 76.7794 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000017"), UserId = kritika2Id, City = "Kolkata",           Country = "India", Lat = 22.5800, Lng = 88.3500 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000018"), UserId = saurabhId,  City = "Lucknow",           Country = "India", Lat = 26.8467, Lng = 80.9462 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000019"), UserId = ritaId,     City = "Surat",             Country = "India", Lat = 21.1702, Lng = 72.8311 },
            new UserLocation { Id = Guid.Parse("f0000001-0000-0000-0000-000000000020"), UserId = mohanId,    City = "Thiruvananthapuram",Country = "India", Lat = 8.5241,  Lng = 76.9366 }
        );

        // ── User Interests ─────────────────────────────────────────────
        mb.Entity<UserInterest>().HasData(
            new UserInterest { UserId = priyaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = priyaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = priyaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = rahulId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = rahulId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = rahulId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = arjunId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = arjunId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = nehaId,     InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000001") },
            new UserInterest { UserId = nehaId,     InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = ankitaId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
            new UserInterest { UserId = ankitaId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000009") },
            new UserInterest { UserId = vikramId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = deepakId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") },
            new UserInterest { UserId = deepakId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
            new UserInterest { UserId = aishaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = rohitId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000006") },
            new UserInterest { UserId = shreyaId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000008") },
            new UserInterest { UserId = shreyaId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = karthikId,  InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = karthikId,  InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = meenaId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = rajeshId,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000012") },
            new UserInterest { UserId = pooja2Id,   InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000005") },
            new UserInterest { UserId = amanId,     InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000007") },
            new UserInterest { UserId = kritika2Id, InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000002") },
            new UserInterest { UserId = saurabhId,  InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000011") },
            new UserInterest { UserId = ritaId,     InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000010") },
            new UserInterest { UserId = mohanId,    InterestId = Guid.Parse("a0000001-0000-0000-0000-000000000003") }
        );

        // ── Pre-seeded Matches ─────────────────────────────────────────
        var match1Id = Guid.Parse("a1000001-0000-0000-0000-000000000001");
        var match2Id = Guid.Parse("a1000001-0000-0000-0000-000000000002");
        var match3Id = Guid.Parse("a1000001-0000-0000-0000-000000000003");
        var chat1Id  = Guid.Parse("a1000002-0000-0000-0000-000000000001");
        var chat2Id  = Guid.Parse("a1000002-0000-0000-0000-000000000002");
        var chat3Id  = Guid.Parse("a1000002-0000-0000-0000-000000000003");

        mb.Entity<Swipe>().HasData(
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000001"), SwiperId = rahulId,   TargetId = priyaId,    Action = "like",      CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000002"), SwiperId = priyaId,   TargetId = rahulId,    Action = "like",      CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000003"), SwiperId = arjunId,   TargetId = aishaId,    Action = "superlike", CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000004"), SwiperId = aishaId,   TargetId = arjunId,    Action = "like",      CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000005"), SwiperId = vikramId,  TargetId = shreyaId,   Action = "like",      CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) },
            new Swipe { Id = Guid.Parse("b1000001-0000-0000-0000-000000000006"), SwiperId = shreyaId,  TargetId = vikramId,   Action = "like",      CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) }
        );

        mb.Entity<Match>().HasData(
            new Match { Id = match1Id, User1Id = rahulId,  User2Id = priyaId,  IsActive = true, CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
            new Match { Id = match2Id, User1Id = arjunId,  User2Id = aishaId,  IsActive = true, CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
            new Match { Id = match3Id, User1Id = vikramId, User2Id = shreyaId, IsActive = true, CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) }
        );

        mb.Entity<Chat>().HasData(
            new Chat { Id = chat1Id, MatchId = match1Id, CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
            new Chat { Id = chat2Id, MatchId = match2Id, CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
            new Chat { Id = chat3Id, MatchId = match3Id, CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) }
        );

        mb.Entity<Message>().HasData(
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000001"), ChatId = chat1Id, SenderId = rahulId,  Text = "Hey Priya! We matched 🎉 How are you?",        Type = "text", CoinsDeducted = 10, ReadAt = new DateTime(2024,1,2,1,0,0,DateTimeKind.Utc), CreatedAt = new DateTime(2024,1,2,0,30,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000002"), ChatId = chat1Id, SenderId = priyaId,  Text = "Hi Rahul! I'm great, thanks! 😊",              Type = "text", CoinsDeducted = 0,  ReadAt = new DateTime(2024,1,2,2,0,0,DateTimeKind.Utc), CreatedAt = new DateTime(2024,1,2,1,0,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000003"), ChatId = chat1Id, SenderId = rahulId,  Text = "I saw you love dancing 💃 so cool!",           Type = "text", CoinsDeducted = 10, ReadAt = new DateTime(2024,1,2,3,0,0,DateTimeKind.Utc), CreatedAt = new DateTime(2024,1,2,2,0,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000004"), ChatId = chat1Id, SenderId = priyaId,  Text = "Yes! Been dancing since I was 8 🎵",            Type = "text", CoinsDeducted = 0,  CreatedAt = new DateTime(2024,1,2,3,0,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000001-0000-0000-0000-000000000005"), ChatId = chat1Id, SenderId = rahulId,  Text = "Amazing! I play guitar 🎸 we should jam!",     Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024,1,2,4,0,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000001"), ChatId = chat2Id, SenderId = arjunId,  Text = "Hi Aisha! I sent you a super like 🌟",          Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024,1,3,1,0,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000002"), ChatId = chat2Id, SenderId = aishaId,  Text = "Aww thank you! I loved your photos 😍",         Type = "text", CoinsDeducted = 0,  CreatedAt = new DateTime(2024,1,3,2,0,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000002-0000-0000-0000-000000000003"), ChatId = chat2Id, SenderId = arjunId,  Text = "What are you up to this weekend? 🎯",           Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024,1,3,3,0,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000001"), ChatId = chat3Id, SenderId = vikramId, Text = "Hey Shreya! Great to match with you ✨",         Type = "text", CoinsDeducted = 10, CreatedAt = new DateTime(2024,1,4,1,0,0,DateTimeKind.Utc) },
            new Message { Id = Guid.Parse("c1000003-0000-0000-0000-000000000002"), ChatId = chat3Id, SenderId = shreyaId, Text = "Hi Vikram! You seem interesting 😊 What do you do?", Type = "text", CoinsDeducted = 0, CreatedAt = new DateTime(2024,1,4,2,0,0,DateTimeKind.Utc) }
        );

        mb.Entity<CallSession>().HasData(
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000001"), CallerId = arjunId,  ReceiverId = aishaId,  MatchId = match2Id, CallType = "video", Status = "ended", AnsweredAt = new DateTime(2024,1,3,3,0,0,DateTimeKind.Utc), EndedAt = new DateTime(2024,1,3,3,5,0,DateTimeKind.Utc), DurationSeconds = 300, CoinsDeducted = 500, EndReason = "user_ended", CreatedAt = new DateTime(2024,1,3,3,0,0,DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000002"), CallerId = rahulId,  ReceiverId = priyaId,  MatchId = match1Id, CallType = "audio", Status = "ended", AnsweredAt = new DateTime(2024,1,2,5,0,0,DateTimeKind.Utc), EndedAt = new DateTime(2024,1,2,5,3,0,DateTimeKind.Utc), DurationSeconds = 180, CoinsDeducted = 30,  EndReason = "user_ended", CreatedAt = new DateTime(2024,1,2,5,0,0,DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000003"), CallerId = vikramId, ReceiverId = shreyaId, MatchId = match3Id, CallType = "audio", Status = "missed", EndedAt = new DateTime(2024,1,4,6,0,0,DateTimeKind.Utc), CoinsDeducted = 0, CreatedAt = new DateTime(2024,1,4,6,0,0,DateTimeKind.Utc) },
            new CallSession { Id = Guid.Parse("d1000001-0000-0000-0000-000000000004"), CallerId = priyaId,  ReceiverId = rahulId,  MatchId = match1Id, CallType = "audio", Status = "declined", EndedAt = new DateTime(2024,1,5,8,0,0,DateTimeKind.Utc), CoinsDeducted = 0, CreatedAt = new DateTime(2024,1,5,8,0,0,DateTimeKind.Utc) }
        );

        mb.Entity<Notification>().HasData(
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000001"), UserId = rahulId,  Title = "New Match! 🎉",        Body = "You matched with Priya Sharma!",  Type = "match",   IsRead = false, ReferenceId = match1Id.ToString(), CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000002"), UserId = priyaId,  Title = "New Match! 🎉",        Body = "You matched with Rahul Mehta!",   Type = "match",   IsRead = true,  ReferenceId = match1Id.ToString(), CreatedAt = new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000003"), UserId = arjunId,  Title = "New Match! 🎉",        Body = "You matched with Aisha Khan!",    Type = "match",   IsRead = false, ReferenceId = match2Id.ToString(), CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000004"), UserId = aishaId,  Title = "New Match! 🎉",        Body = "You matched with Arjun Singh!",   Type = "match",   IsRead = false, ReferenceId = match2Id.ToString(), CreatedAt = new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000005"), UserId = vikramId, Title = "New Match! 🎉",        Body = "You matched with Shreya Patel!",  Type = "match",   IsRead = false, ReferenceId = match3Id.ToString(), CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000006"), UserId = shreyaId, Title = "New Match! 🎉",        Body = "You matched with Vikram Nair!",   Type = "match",   IsRead = true,  ReferenceId = match3Id.ToString(), CreatedAt = new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000007"), UserId = rahulId,  Title = "New Message 💬",       Body = "Priya sent you a message",        Type = "message", IsRead = false, CreatedAt = new DateTime(2024,1,2,1,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000008"), UserId = priyaId,  Title = "Coins Added 🪙",       Body = "Welcome bonus: +100 coins",       Type = "coins",   IsRead = true,  CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000009"), UserId = arjunId,  Title = "Call Missed 📞",       Body = "You missed a call from Aisha",    Type = "call",    IsRead = false, CreatedAt = new DateTime(2024,1,3,4,0,0,DateTimeKind.Utc) },
            new Notification { Id = Guid.Parse("e1000001-0000-0000-0000-000000000010"), UserId = aishaId,  Title = "Incoming Call 📹",     Body = "Arjun is calling — video call",   Type = "call",    IsRead = true,  CreatedAt = new DateTime(2024,1,3,3,0,0,DateTimeKind.Utc) }
        );

        mb.Entity<UserSubscription>().HasData(
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000001"), UserId = arjunId,  PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000002"), UserId = priyaId,  PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000003"), UserId = shreyaId, PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000003"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc), IsActive = true, AutoRenew = true },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000004"), UserId = amanId,   PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000001"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,6,30,0,0,0,DateTimeKind.Utc),  IsActive = true, AutoRenew = false },
            new UserSubscription { Id = Guid.Parse("f1000001-0000-0000-0000-000000000005"), UserId = ritaId,   PlanId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), StartDate = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc), EndDate = new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc), IsActive = true, AutoRenew = true }
        );
    }
}
