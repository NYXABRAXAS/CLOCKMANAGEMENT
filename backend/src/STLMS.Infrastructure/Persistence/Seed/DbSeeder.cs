using Microsoft.EntityFrameworkCore;
using STLMS.Application.ReligionCalculators;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Seed;

/// <summary>Modules referenced by permission seeding. Expanded as later milestones introduce
/// new modules (Alarms, Habits, Religion, etc.) - each just adds another entry here.</summary>
public static class PermissionModules
{
    public static readonly string[] All =
    [
        "DASHBOARD", "USERS", "ROLES", "RELIGIONS", "SETTINGS", "AUDIT_LOGS", "PROFILE", "WORLD_CLOCK", "ALARMS", "TIMERS", "CALENDAR", "HEALTH",
        "PRAYER_CENTER", "PRODUCTIVITY",
    ];
}

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(context);
        await SeedPermissionsAsync(context);
        await SeedRolePermissionsAsync(context);
        await SeedReligionsAsync(context);
        await SeedCitiesAsync(context);
        await SeedAchievementsAsync(context);
        await SeedFestivalsAsync(context);
        await SeedQuotesAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        context.Roles.AddRange(
            new Role { Code = RoleCodes.SuperAdmin, Name = "Super Admin", Description = "Full unrestricted access.", IsSystem = true, SortOrder = 1 },
            new Role { Code = RoleCodes.Admin, Name = "Admin", Description = "Manages users, religions, and application settings.", IsSystem = true, SortOrder = 2 },
            new Role { Code = RoleCodes.PremiumUser, Name = "Premium User", Description = "Full feature access on a paid subscription.", IsSystem = true, SortOrder = 3 },
            new Role { Code = RoleCodes.StandardUser, Name = "Standard User", Description = "Free-tier feature access.", IsSystem = true, SortOrder = 4 },
            new Role { Code = RoleCodes.Guest, Name = "Guest", Description = "Read-only trial access.", IsSystem = true, SortOrder = 5 }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedPermissionsAsync(AppDbContext context)
    {
        var existing = await context.Permissions.Select(p => p.Module + ":" + p.Action).ToListAsync();
        var toAdd = new List<Permission>();
        foreach (var module in PermissionModules.All)
        {
            foreach (var action in new[] { "view", "create", "edit", "delete" })
            {
                var key = $"{module}:{action}";
                if (!existing.Contains(key)) toAdd.Add(new Permission { Module = module, Action = action });
            }
        }
        if (toAdd.Count != 0)
        {
            context.Permissions.AddRange(toAdd);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRolePermissionsAsync(AppDbContext context)
    {
        var superAdmin = await context.Roles.SingleAsync(r => r.Code == RoleCodes.SuperAdmin);
        var admin = await context.Roles.SingleAsync(r => r.Code == RoleCodes.Admin);
        var premium = await context.Roles.SingleAsync(r => r.Code == RoleCodes.PremiumUser);
        var standard = await context.Roles.SingleAsync(r => r.Code == RoleCodes.StandardUser);
        var guest = await context.Roles.SingleAsync(r => r.Code == RoleCodes.Guest);

        var allPermissions = await context.Permissions.ToListAsync();
        var existingPairs = (await context.RolePermissions.Select(rp => new { rp.RoleId, rp.PermissionId }).ToListAsync())
            .Select(x => (x.RoleId, x.PermissionId)).ToHashSet();

        void Grant(Role role, IEnumerable<Permission> permissions)
        {
            foreach (var permission in permissions)
            {
                if (existingPairs.Contains((role.Id, permission.Id))) continue;
                context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }
        }

        // Super Admin and Admin: everything.
        Grant(superAdmin, allPermissions);
        Grant(admin, allPermissions.Where(p => p.Module != "ROLES" || p.Action == "view"));

        // Premium/Standard: full self-service access to their own data, no admin modules.
        var selfServiceModules = new[]
        {
            "DASHBOARD", "SETTINGS", "PROFILE", "WORLD_CLOCK", "ALARMS", "TIMERS", "CALENDAR", "HEALTH", "PRAYER_CENTER", "PRODUCTIVITY",
        };
        Grant(premium, allPermissions.Where(p => selfServiceModules.Contains(p.Module)));
        Grant(standard, allPermissions.Where(p => selfServiceModules.Contains(p.Module)));

        // Everyone can read the religions list (it's reference data used by the Settings picker) -
        // only Admin/SuperAdmin can manage the list itself (grant above already covers that).
        Grant(premium, allPermissions.Where(p => p.Module == "RELIGIONS" && p.Action == "view"));
        Grant(standard, allPermissions.Where(p => p.Module == "RELIGIONS" && p.Action == "view"));
        Grant(guest, allPermissions.Where(p => p.Module == "RELIGIONS" && p.Action == "view"));

        // Guest: view-only.
        Grant(guest, allPermissions.Where(p => selfServiceModules.Contains(p.Module) && p.Action == "view"));

        await context.SaveChangesAsync();
    }

    private static async Task SeedReligionsAsync(AppDbContext context)
    {
        if (await context.Religions.AnyAsync()) return;

        context.Religions.AddRange(
            new Religion { Code = ReligionCodes.Islam, Name = "Islam", IsSystem = true, SortOrder = 1 },
            new Religion { Code = ReligionCodes.Hinduism, Name = "Hinduism", IsSystem = true, SortOrder = 2 },
            new Religion { Code = ReligionCodes.Christianity, Name = "Christianity", IsSystem = true, SortOrder = 3 },
            new Religion { Code = ReligionCodes.Sikhism, Name = "Sikhism", IsSystem = true, SortOrder = 4 },
            new Religion { Code = ReligionCodes.Buddhism, Name = "Buddhism", IsSystem = true, SortOrder = 5 },
            new Religion { Code = ReligionCodes.Jainism, Name = "Jainism", IsSystem = true, SortOrder = 6 },
            new Religion { Code = ReligionCodes.Judaism, Name = "Judaism", IsSystem = true, SortOrder = 7 },
            new Religion { Code = ReligionCodes.Other, Name = "Other", IsSystem = true, SortOrder = 8 }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedCitiesAsync(AppDbContext context)
    {
        if (await context.Cities.AnyAsync()) return;

        City C(string name, string country, string countryCode, string tz, double lat, double lon) =>
            new() { Name = name, Country = country, CountryCode = countryCode, TimezoneId = tz, Latitude = lat, Longitude = lon, IsSystem = true };

        context.Cities.AddRange(
            C("New York", "United States", "US", "America/New_York", 40.7128, -74.0060),
            C("Washington D.C.", "United States", "US", "America/New_York", 38.9072, -77.0369),
            C("Boston", "United States", "US", "America/New_York", 42.3601, -71.0589),
            C("Miami", "United States", "US", "America/New_York", 25.7617, -80.1918),
            C("Chicago", "United States", "US", "America/Chicago", 41.8781, -87.6298),
            C("Houston", "United States", "US", "America/Chicago", 29.7604, -95.3698),
            C("Dallas", "United States", "US", "America/Chicago", 32.7767, -96.7970),
            C("Denver", "United States", "US", "America/Denver", 39.7392, -104.9903),
            C("Phoenix", "United States", "US", "America/Phoenix", 33.4484, -112.0740),
            C("Los Angeles", "United States", "US", "America/Los_Angeles", 34.0522, -118.2437),
            C("San Francisco", "United States", "US", "America/Los_Angeles", 37.7749, -122.4194),
            C("Seattle", "United States", "US", "America/Los_Angeles", 47.6062, -122.3321),
            C("Las Vegas", "United States", "US", "America/Los_Angeles", 36.1699, -115.1398),
            C("Anchorage", "United States", "US", "America/Anchorage", 61.2181, -149.9003),
            C("Honolulu", "United States", "US", "Pacific/Honolulu", 21.3069, -157.8583),
            C("Toronto", "Canada", "CA", "America/Toronto", 43.6532, -79.3832),
            C("Montreal", "Canada", "CA", "America/Toronto", 45.5017, -73.5673),
            C("Vancouver", "Canada", "CA", "America/Vancouver", 49.2827, -123.1207),
            C("Mexico City", "Mexico", "MX", "America/Mexico_City", 19.4326, -99.1332),
            C("Havana", "Cuba", "CU", "America/Havana", 23.1136, -82.3666),
            C("Lima", "Peru", "PE", "America/Lima", -12.0464, -77.0428),
            C("Bogota", "Colombia", "CO", "America/Bogota", 4.7110, -74.0721),
            C("Caracas", "Venezuela", "VE", "America/Caracas", 10.4806, -66.9036),
            C("Santiago", "Chile", "CL", "America/Santiago", -33.4489, -70.6693),
            C("Buenos Aires", "Argentina", "AR", "America/Argentina/Buenos_Aires", -34.6037, -58.3816),
            C("Sao Paulo", "Brazil", "BR", "America/Sao_Paulo", -23.5505, -46.6333),
            C("Reykjavik", "Iceland", "IS", "Atlantic/Reykjavik", 64.1466, -21.9426),
            C("London", "United Kingdom", "GB", "Europe/London", 51.5074, -0.1278),
            C("Dublin", "Ireland", "IE", "Europe/Dublin", 53.3498, -6.2603),
            C("Lisbon", "Portugal", "PT", "Europe/Lisbon", 38.7223, -9.1393),
            C("Madrid", "Spain", "ES", "Europe/Madrid", 40.4168, -3.7038),
            C("Paris", "France", "FR", "Europe/Paris", 48.8566, 2.3522),
            C("Brussels", "Belgium", "BE", "Europe/Brussels", 50.8503, 4.3517),
            C("Amsterdam", "Netherlands", "NL", "Europe/Amsterdam", 52.3676, 4.9041),
            C("Berlin", "Germany", "DE", "Europe/Berlin", 52.5200, 13.4050),
            C("Zurich", "Switzerland", "CH", "Europe/Zurich", 47.3769, 8.5417),
            C("Vienna", "Austria", "AT", "Europe/Vienna", 48.2082, 16.3738),
            C("Rome", "Italy", "IT", "Europe/Rome", 41.9028, 12.4964),
            C("Prague", "Czech Republic", "CZ", "Europe/Prague", 50.0755, 14.4378),
            C("Warsaw", "Poland", "PL", "Europe/Warsaw", 52.2297, 21.0122),
            C("Budapest", "Hungary", "HU", "Europe/Budapest", 47.4979, 19.0402),
            C("Stockholm", "Sweden", "SE", "Europe/Stockholm", 59.3293, 18.0686),
            C("Oslo", "Norway", "NO", "Europe/Oslo", 59.9139, 10.7522),
            C("Copenhagen", "Denmark", "DK", "Europe/Copenhagen", 55.6761, 12.5683),
            C("Helsinki", "Finland", "FI", "Europe/Helsinki", 60.1699, 24.9384),
            C("Athens", "Greece", "GR", "Europe/Athens", 37.9838, 23.7275),
            C("Kyiv", "Ukraine", "UA", "Europe/Kyiv", 50.4501, 30.5234),
            C("Moscow", "Russia", "RU", "Europe/Moscow", 55.7558, 37.6173),
            C("Istanbul", "Turkey", "TR", "Europe/Istanbul", 41.0082, 28.9784),
            C("Casablanca", "Morocco", "MA", "Africa/Casablanca", 33.5731, -7.5898),
            C("Cairo", "Egypt", "EG", "Africa/Cairo", 30.0444, 31.2357),
            C("Lagos", "Nigeria", "NG", "Africa/Lagos", 6.5244, 3.3792),
            C("Accra", "Ghana", "GH", "Africa/Accra", 5.6037, -0.1870),
            C("Nairobi", "Kenya", "KE", "Africa/Nairobi", -1.2921, 36.8219),
            C("Addis Ababa", "Ethiopia", "ET", "Africa/Addis_Ababa", 9.0300, 38.7400),
            C("Johannesburg", "South Africa", "ZA", "Africa/Johannesburg", -26.2041, 28.0473),
            C("Jerusalem", "Israel", "IL", "Asia/Jerusalem", 31.7683, 35.2137),
            C("Tel Aviv", "Israel", "IL", "Asia/Jerusalem", 32.0853, 34.7818),
            C("Amman", "Jordan", "JO", "Asia/Amman", 31.9454, 35.9284),
            C("Baghdad", "Iraq", "IQ", "Asia/Baghdad", 33.3152, 44.3661),
            C("Tehran", "Iran", "IR", "Asia/Tehran", 35.6892, 51.3890),
            C("Dubai", "United Arab Emirates", "AE", "Asia/Dubai", 25.2048, 55.2708),
            C("Riyadh", "Saudi Arabia", "SA", "Asia/Riyadh", 24.7136, 46.6753),
            C("Doha", "Qatar", "QA", "Asia/Qatar", 25.2854, 51.5310),
            C("Kuwait City", "Kuwait", "KW", "Asia/Kuwait", 29.3759, 47.9774),
            C("Muscat", "Oman", "OM", "Asia/Muscat", 23.5859, 58.4059),
            C("Karachi", "Pakistan", "PK", "Asia/Karachi", 24.8607, 67.0011),
            C("Lahore", "Pakistan", "PK", "Asia/Karachi", 31.5497, 74.3436),
            C("Islamabad", "Pakistan", "PK", "Asia/Karachi", 33.6844, 73.0479),
            C("Mumbai", "India", "IN", "Asia/Kolkata", 19.0760, 72.8777),
            C("New Delhi", "India", "IN", "Asia/Kolkata", 28.6139, 77.2090),
            C("Bengaluru", "India", "IN", "Asia/Kolkata", 12.9716, 77.5946),
            C("Kolkata", "India", "IN", "Asia/Kolkata", 22.5726, 88.3639),
            C("Chennai", "India", "IN", "Asia/Kolkata", 13.0827, 80.2707),
            C("Hyderabad", "India", "IN", "Asia/Kolkata", 17.3850, 78.4867),
            C("Kathmandu", "Nepal", "NP", "Asia/Kathmandu", 27.7172, 85.3240),
            C("Colombo", "Sri Lanka", "LK", "Asia/Colombo", 6.9271, 79.8612),
            C("Dhaka", "Bangladesh", "BD", "Asia/Dhaka", 23.8103, 90.4125),
            C("Almaty", "Kazakhstan", "KZ", "Asia/Almaty", 43.2220, 76.8512),
            C("Tashkent", "Uzbekistan", "UZ", "Asia/Tashkent", 41.2995, 69.2401),
            C("Yangon", "Myanmar", "MM", "Asia/Yangon", 16.8661, 96.1951),
            C("Bangkok", "Thailand", "TH", "Asia/Bangkok", 13.7563, 100.5018),
            C("Phnom Penh", "Cambodia", "KH", "Asia/Phnom_Penh", 11.5564, 104.9282),
            C("Hanoi", "Vietnam", "VN", "Asia/Ho_Chi_Minh", 21.0285, 105.8542),
            C("Ho Chi Minh City", "Vietnam", "VN", "Asia/Ho_Chi_Minh", 10.8231, 106.6297),
            C("Jakarta", "Indonesia", "ID", "Asia/Jakarta", -6.2088, 106.8456),
            C("Kuala Lumpur", "Malaysia", "MY", "Asia/Kuala_Lumpur", 3.1390, 101.6869),
            C("Singapore", "Singapore", "SG", "Asia/Singapore", 1.3521, 103.8198),
            C("Manila", "Philippines", "PH", "Asia/Manila", 14.5995, 120.9842),
            C("Hong Kong", "Hong Kong", "HK", "Asia/Hong_Kong", 22.3193, 114.1694),
            C("Beijing", "China", "CN", "Asia/Shanghai", 39.9042, 116.4074),
            C("Shanghai", "China", "CN", "Asia/Shanghai", 31.2304, 121.4737),
            C("Taipei", "Taiwan", "TW", "Asia/Taipei", 25.0330, 121.5654),
            C("Seoul", "South Korea", "KR", "Asia/Seoul", 37.5665, 126.9780),
            C("Ulaanbaatar", "Mongolia", "MN", "Asia/Ulaanbaatar", 47.8864, 106.9057),
            C("Tokyo", "Japan", "JP", "Asia/Tokyo", 35.6762, 139.6503),
            C("Osaka", "Japan", "JP", "Asia/Tokyo", 34.6937, 135.5023),
            C("Perth", "Australia", "AU", "Australia/Perth", -31.9505, 115.8605),
            C("Adelaide", "Australia", "AU", "Australia/Adelaide", -34.9285, 138.6007),
            C("Sydney", "Australia", "AU", "Australia/Sydney", -33.8688, 151.2093),
            C("Melbourne", "Australia", "AU", "Australia/Melbourne", -37.8136, 144.9631),
            C("Brisbane", "Australia", "AU", "Australia/Brisbane", -27.4698, 153.0251),
            C("Port Moresby", "Papua New Guinea", "PG", "Pacific/Port_Moresby", -9.4438, 147.1803),
            C("Suva", "Fiji", "FJ", "Pacific/Fiji", -18.1248, 178.4501),
            C("Auckland", "New Zealand", "NZ", "Pacific/Auckland", -36.8485, 174.7633),
            C("Wellington", "New Zealand", "NZ", "Pacific/Auckland", -41.2865, 174.7762),
            C("Nuku'alofa", "Tonga", "TO", "Pacific/Tongatapu", -21.1789, -175.1982)
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedAchievementsAsync(AppDbContext context)
    {
        if (await context.Achievements.AnyAsync()) return;

        context.Achievements.AddRange(
            new Achievement { Code = AchievementCodes.HabitFirstCheckIn, Title = "First Step", Description = "Completed your first habit check-in.", Emoji = "🌱", IsSystem = true },
            new Achievement { Code = AchievementCodes.HabitStreak7, Title = "Week Strong", Description = "Kept a habit streak for 7 days in a row.", Emoji = "🔥", IsSystem = true },
            new Achievement { Code = AchievementCodes.HabitStreak30, Title = "Month Master", Description = "Kept a habit streak for 30 days in a row.", Emoji = "🏆", IsSystem = true },
            new Achievement { Code = AchievementCodes.HabitCheckIns100, Title = "Centurion", Description = "Logged 100 total habit check-ins.", Emoji = "💯", IsSystem = true }
        );
        await context.SaveChangesAsync();
    }

    /// <summary>Christianity's dates are genuinely computed (Easter via the Computus algorithm,
    /// see ChristianFeastCalculator) - real math, not guesses. Sikhism/Buddhism/Jainism follow
    /// lunar/lunisolar calendars with no free API or .NET-portable calculator available, so those
    /// are seeded as reasonable best-effort dates for 2026-2027 - genuinely approximate and in
    /// need of periodic admin re-verification/re-seeding for future years, exactly as the plan
    /// for this milestone anticipates (same honesty standard as PanchangCalculator).</summary>
    private static async Task SeedFestivalsAsync(AppDbContext context)
    {
        if (await context.FestivalCalendarEntries.AnyAsync()) return;

        var christianity = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Christianity);
        var sikhism = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Sikhism);
        var buddhism = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Buddhism);
        var jainism = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Jainism);

        var entries = new List<FestivalCalendarEntry>();

        foreach (var year in new[] { 2026, 2027 })
        {
            foreach (var feast in ChristianFeastCalculator.MovableFeastsForYear(year).Concat(ChristianFeastCalculator.FixedFeastsForYear(year)))
            {
                entries.Add(new FestivalCalendarEntry { ReligionId = christianity.Id, Name = feast.Name, Date = feast.Date, Emoji = feast.Emoji, IsSystem = true });
            }
        }

        FestivalCalendarEntry F(Religion religion, string name, string description, DateOnly date, string emoji) =>
            new() { ReligionId = religion.Id, Name = name, Description = description, Date = date, Emoji = emoji, IsSystem = true };

        entries.AddRange(
        [
            F(sikhism, "Vaisakhi", "Khalsa Foundation Day - fixed solar date on the Nanakshahi calendar.", new DateOnly(2026, 4, 14), "🪈"),
            F(sikhism, "Vaisakhi", "Khalsa Foundation Day - fixed solar date on the Nanakshahi calendar.", new DateOnly(2027, 4, 14), "🪈"),
            F(sikhism, "Guru Nanak Gurpurab", "Birth anniversary of Guru Nanak Dev Ji (lunar date - approximate).", new DateOnly(2026, 11, 24), "🙏"),
            F(sikhism, "Bandi Chhor Divas", "Coincides with Diwali (lunar date - approximate).", new DateOnly(2026, 11, 8), "🪔"),

            F(buddhism, "Magha Puja", "Full moon of the third lunar month (approximate).", new DateOnly(2026, 3, 3), "🪷"),
            F(buddhism, "Vesak (Buddha Purnima)", "Buddha's birth, enlightenment, and death - full moon of May (approximate).", new DateOnly(2026, 5, 31), "🪷"),
            F(buddhism, "Asalha Puja (Dharma Day)", "Full moon of the eighth lunar month (approximate).", new DateOnly(2026, 7, 29), "🪷"),

            F(jainism, "Mahavir Jayanti", "Birth anniversary of Lord Mahavira (lunar date - approximate).", new DateOnly(2026, 3, 31), "🕉️"),
            F(jainism, "Paryushana (start)", "Eight/ten-day period of reflection and fasting (lunar date - approximate).", new DateOnly(2026, 8, 22), "🕉️"),
            F(jainism, "Diwali (Jain)", "Marks Lord Mahavira's attainment of nirvana - coincides with Diwali (approximate).", new DateOnly(2026, 11, 8), "🪔"),
        ]);

        context.FestivalCalendarEntries.AddRange(entries);
        await context.SaveChangesAsync();
    }

    private static async Task SeedQuotesAsync(AppDbContext context)
    {
        if (await context.DailyQuotes.AnyAsync()) return;

        var islam = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Islam);
        var hinduism = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Hinduism);
        var christianity = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Christianity);
        var sikhism = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Sikhism);
        var buddhism = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Buddhism);
        var jainism = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Jainism);
        var judaism = await context.Religions.SingleAsync(r => r.Code == ReligionCodes.Judaism);

        context.DailyQuotes.AddRange(
            new DailyQuote { ReligionId = null, Text = "The best among you are those who have the best manners and character.", Source = "Universal wisdom" },
            new DailyQuote { ReligionId = null, Text = "Time is precious - use it wisely, for it never returns.", Source = "Universal wisdom" },
            new DailyQuote { ReligionId = null, Text = "Small daily habits compound into a life well lived.", Source = "Universal wisdom" },
            new DailyQuote { ReligionId = islam.Id, Text = "Indeed, with hardship comes ease.", Source = "Qur'an 94:6" },
            new DailyQuote { ReligionId = islam.Id, Text = "The most beloved deeds to Allah are those done consistently, even if small.", Source = "Hadith, Sahih al-Bukhari" },
            new DailyQuote { ReligionId = hinduism.Id, Text = "You have the right to work, but never to the fruit of work.", Source = "Bhagavad Gita 2:47" },
            new DailyQuote { ReligionId = hinduism.Id, Text = "As a man acts, so he becomes.", Source = "Brihadaranyaka Upanishad" },
            new DailyQuote { ReligionId = christianity.Id, Text = "I can do all things through Christ who strengthens me.", Source = "Philippians 4:13" },
            new DailyQuote { ReligionId = christianity.Id, Text = "Love your neighbor as yourself.", Source = "Mark 12:31" },
            new DailyQuote { ReligionId = sikhism.Id, Text = "Recognize the whole human race as one.", Source = "Guru Gobind Singh Ji" },
            new DailyQuote { ReligionId = sikhism.Id, Text = "Truth is high, but higher still is truthful living.", Source = "Guru Nanak Dev Ji" },
            new DailyQuote { ReligionId = buddhism.Id, Text = "Peace comes from within. Do not seek it without.", Source = "The Buddha" },
            new DailyQuote { ReligionId = buddhism.Id, Text = "What we think, we become.", Source = "The Dhammapada" },
            new DailyQuote { ReligionId = jainism.Id, Text = "Non-violence is the highest religion.", Source = "Ahimsa Paramo Dharma" },
            new DailyQuote { ReligionId = jainism.Id, Text = "Live and let live.", Source = "Jain principle" },
            new DailyQuote { ReligionId = judaism.Id, Text = "Whoever saves a single life is considered to have saved an entire world.", Source = "Mishnah, Sanhedrin 4:5" },
            new DailyQuote { ReligionId = judaism.Id, Text = "You are not obligated to complete the work, but neither are you free to abandon it.", Source = "Pirkei Avot 2:16" }
        );
        await context.SaveChangesAsync();
    }
}
