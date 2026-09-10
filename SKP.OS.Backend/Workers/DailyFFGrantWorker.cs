using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SKP.OS.Base;
using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Workers;

/// <summary>
/// Grants every student 3 FF hours each day with the note
/// "FF justrering for: {month}". Runs once per day at 03:00.
/// </summary>
public class DailyFFGrantWorker : BackgroundService
{
    private static readonly TimeSpan MaxFfBalance = TimeSpan.FromHours(37);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyFFGrantWorker> _logger;

    public DailyFFGrantWorker(IServiceScopeFactory scopeFactory, ILogger<DailyFFGrantWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.Now;
            var nextRun = new DateTimeOffset(now.Year, now.Month, now.Day, 3, 0, 0, now.Offset);
            if (nextRun <= now)
            {
                nextRun = nextRun.AddDays(1);
            }
            var delay = nextRun - now;

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await GrantTodayIfNeededAsync(stoppingToken);
        }
    }

    private async Task GrantTodayIfNeededAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTimeOffset.Now;
            var dayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
            var dayEnd = dayStart.AddDays(1);
            var monthName = CultureInfo.GetCultureInfo("da-DK")
                .DateTimeFormat.GetMonthName(now.Month);
            var note = $"FF justrering for: {monthName}";

            var entryTotals = await context.FFEntries
                .Select(f => new { f.StudentProfileId, f.Duration })
                .ToListAsync(stoppingToken);
            var totals = entryTotals
                .GroupBy(e => e.StudentProfileId)
                .ToDictionary(g => g.Key, g => TimeSpan.FromSeconds(g.Sum(e => e.Duration.TotalSeconds)));

            var students = await context.StudentProfiles.ToListAsync(stoppingToken);
            var added = 0;
            var skipped = 0;
            foreach (var student in students)
            {
                var alreadyGranted = await context.FFEntries
                    .AnyAsync(
                        f => f.StudentProfileId == student.Id
                            && f.Date >= dayStart
                            && f.Date < dayEnd
                            && f.Note.StartsWith("FF justrering for"),
                        stoppingToken);
                if (alreadyGranted)
                {
                    continue;
                }

                var total = totals.TryGetValue(student.Id, out var t) ? t : TimeSpan.Zero;
                if (total >= MaxFfBalance)
                {
                    skipped++;
                    continue;
                }

                context.FFEntries.Add(new FFEntry
                {
                    Date = now,
                    Duration = TimeSpan.FromHours(3),
                    Note = note,
                    StudentProfileId = student.Id,
                    InstructorProfileId = null
                });
                added++;
            }

            if (added > 0)
            {
                await context.SaveChangesAsync(stoppingToken);
            }

            _logger.LogInformation(
                "Daily FF grant finished. Granted {Count} student(s), skipped {Skipped} at the {Max} cap on {Date}.",
                added, skipped, MaxFfBalance, dayStart.Date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Daily FF grant failed.");
        }
    }
}