using Microsoft.EntityFrameworkCore;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

/// <summary>
/// Runtime demo-data seeder. Generates a large batch of varied Discover
/// profiles the first time the API starts. Idempotent — checks how many
/// generated demo users already exist and only tops up the difference, so
/// it's safe to leave running on every startup without duplicating data.
///
/// Not for production: these are clearly-tagged test accounts
/// (email pattern "demoseed_*@mingley.app") so they can be found and
/// bulk-deleted later with a single query if needed.
/// </summary>
public static class DemoProfileSeeder
{
    private const int TargetCount = 60;
    private const string EmailPrefix = "demoseed_";
    private const string PasswordHash = "$2b$10$X87YjkGwl4edfI7qVWqI0.KrHBEEn8pc5DxcTf.6WLyTwVGT2SVPq"; // Mingley@123

    private static readonly (string City, string Country, double Lat, double Lng)[] Cities =
    {
        ("Mumbai", "India", 19.0760, 72.8777),
        ("Delhi", "India", 28.6139, 77.2090),
        ("Bangalore", "India", 12.9716, 77.5946),
        ("Pune", "India", 18.5204, 73.8567),
        ("Hyderabad", "India", 17.3850, 78.4867),
        ("Chennai", "India", 13.0827, 80.2707),
        ("Kolkata", "India", 22.5726, 88.3639),
        ("Ahmedabad", "India", 23.0225, 72.5714),
    };

    private static readonly string[] FemaleNames =
    {
        "Ishita Verma", "Riya Malhotra", "Ananya Joshi", "Tanya Chopra", "Meher Bhatia",
        "Simran Kaur", "Diya Menon", "Aditi Nair", "Kavya Reddy", "Sneha Iyer",
        "Naina Kapoor", "Ritika Rao", "Vaishnavi Pillai", "Aarohi Desai", "Kiara Shah",
        "Mahika Bose", "Prisha Agarwal", "Anvi Saxena", "Myra Kulkarni", "Avni Trivedi",
        "Rhea Chatterjee", "Ira Bhatt", "Zoya Khan", "Saanvi Rathore", "Navya Sinha",
        "Amyra Sen", "Ruhi Dutta", "Larisa Fernandes", "Ishaani Mehta", "Kyra Ghosh",
    };

    private static readonly string[] MaleNames =
    {
        "Rohan Malhotra", "Aryan Verma", "Kabir Joshi", "Vihaan Chopra", "Advait Bhatia",
        "Reyansh Kaur", "Arnav Menon", "Sai Nair", "Dhruv Reddy", "Ayaan Iyer",
        "Vivaan Kapoor", "Krishna Rao", "Aditya Pillai", "Yash Desai", "Ishaan Shah",
        "Arjun Bose", "Kartik Agarwal", "Shaurya Saxena", "Veer Kulkarni", "Om Trivedi",
        "Rudra Chatterjee", "Neel Bhatt", "Zaid Khan", "Samar Rathore", "Devansh Sinha",
        "Aarav Sen", "Rian Dutta", "Lucas Fernandes", "Rehan Mehta", "Kian Ghosh",
    };

    private static readonly string[] Professions =
        { "Working Professional", "Student", "Business", "Freelancer" };

    private static readonly string[] FemaleAvatars =
    {
        "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1531123897727-8f129e1688ce?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?w=800&q=90&fit=crop&crop=faces&auto=format",
    };

    private static readonly string[] MaleAvatars =
    {
        "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1552058544-f2b08422138a?w=800&q=90&fit=crop&crop=faces&auto=format",
        "https://images.unsplash.com/photo-1531891437562-4301cf35b7e4?w=800&q=90&fit=crop&crop=faces&auto=format",
    };

    private static readonly string[] Bios =
    {
        "Coffee addict ☕ | Weekend traveler ✈️ | Dog person 🐾",
        "Trying every restaurant in the city, one weekend at a time 🍜",
        "Gym in the morning, Netflix at night 🏋️‍♀️",
        "Bookworm 📚 | Chai over coffee | Bad at karaoke, great at trying",
        "Startup life 💻 | Weekend hiker ⛰️ | Always up for a road trip",
        "Foodie first, everything else second 🍕",
        "Music runs my life 🎧 | Concerts > everything",
        "Adventure seeker | Photography enthusiast 📸",
        "Yoga in the morning, wine in the evening 🍷",
        "Cricket fan, terrible at actually playing it 🏏",
    };

    public static async Task SeedAsync(MingleyDbContext db)
    {
        var existing = await db.Users.IgnoreQueryFilters()
            .CountAsync(u => u.Email != null && u.Email.StartsWith(EmailPrefix));

        var toCreate = TargetCount - existing;
        if (toCreate <= 0) return;

        var interestIds = await db.Interests.Select(i => i.Id).ToListAsync();
        if (interestIds.Count == 0) return; // interests not seeded yet, skip this run

        var rng = new Random();
        var users = new List<User>();
        var locations = new List<UserLocation>();
        var prefs = new List<UserPreference>();
        var images = new List<UserImage>();
        var userInterests = new List<UserInterest>();

        for (int i = 0; i < toCreate; i++)
        {
            var seq = existing + i + 1;
            var isFemale = i % 2 == 0;
            var gender = isFemale ? "female" : "male";
            var names = isFemale ? FemaleNames : MaleNames;
            var avatars = isFemale ? FemaleAvatars : MaleAvatars;
            var name = names[i % names.Length];
            var city = Cities[i % Cities.Length];
            // small jitter so users in the same city aren't stacked on identical coords
            var jitterLat = city.Lat + (rng.NextDouble() - 0.5) * 0.15;
            var jitterLng = city.Lng + (rng.NextDouble() - 0.5) * 0.15;
            var age = rng.Next(20, 38);
            var dob = DateTime.UtcNow.AddYears(-age).AddDays(-rng.Next(0, 365));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = $"{EmailPrefix}{seq}@mingley.app",
                Phone = null,
                PasswordHash = PasswordHash,
                FullName = name,
                Gender = gender,
                DateOfBirth = dob,
                Bio = Bios[i % Bios.Length],
                Profession = Professions[i % Professions.Length],
                Avatar = avatars[i % avatars.Length],
                IsVerified = rng.NextDouble() < 0.4,
                IsActive = true,
                IsPremium = rng.NextDouble() < 0.15,
                CoinBalance = rng.Next(0, 2000),
                ProfileComplete = true,
                IsOnline = rng.NextDouble() < 0.3,
                LastActiveAt = DateTime.UtcNow.AddMinutes(-rng.Next(0, 60 * 24 * 3)),
                CreatedAt = DateTime.UtcNow,
            };
            users.Add(user);

            locations.Add(new UserLocation
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                City = city.City,
                Country = city.Country,
                Lat = jitterLat,
                Lng = jitterLng,
            });

            prefs.Add(new UserPreference
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                InterestedIn = isFemale ? "boys" : "girls",
                MinAge = 18,
                MaxAge = 45,
                MaxDistance = 100,
            });

            images.Add(new UserImage
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Url = avatars[i % avatars.Length],
                SortOrder = 0,
                IsPrimary = true,
            });

            // 2-4 random interests per profile
            var pickCount = rng.Next(2, 5);
            var picked = interestIds.OrderBy(_ => rng.Next()).Take(pickCount);
            foreach (var interestId in picked)
            {
                userInterests.Add(new UserInterest { UserId = user.Id, InterestId = interestId });
            }
        }

        await db.Users.AddRangeAsync(users);
        await db.UserLocations.AddRangeAsync(locations);
        await db.UserPreferences.AddRangeAsync(prefs);
        await db.UserImages.AddRangeAsync(images);
        await db.UserInterests.AddRangeAsync(userInterests);
        await db.SaveChangesAsync();
    }
}