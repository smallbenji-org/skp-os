using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SKP.OS.Base.Models;

namespace SKP.OS.Base;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<InstructorProfile> InstructorProfiles { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTemplate> ProjectTemplates { get; set; }
    public DbSet<LogbookEntry> LogbookEntries { get; set; }
    public DbSet<FFEntry> FFEntries { get; set; }
    public DbSet<CheckIn> CheckIns { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<InfoEntry> InfoEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<StudentProfile>()
            .HasOne(sp => sp.User)
            .WithMany()
            .HasForeignKey(sp => sp.ApplicationUserId);

        builder.Entity<StudentProfile>()
            .Property(sp => sp.CompletedHauls)
            .HasConversion(
                hauls => string.Join(',', hauls.Select(h => (int)h)),
                value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => (ProjectHaul)int.Parse(v))
                    .ToList());

        builder.Entity<InstructorProfile>()
            .HasOne(ip => ip.User)
            .WithMany()
            .HasForeignKey(ip => ip.ApplicationUserId);

        builder.Entity<StudentProfile>()
            .HasMany(sp => sp.Instructors)
            .WithMany(ip => ip.Students)
            .UsingEntity(j => j.ToTable("StudentInstructors"));

        builder.Entity<Project>()
            .HasMany(p => p.Students)
            .WithMany(sp => sp.Projects)
            .UsingEntity(j => j.ToTable("ProjectStudents"));

        builder.Entity<Project>()
            .HasOne(p => p.ProjectTemplate)
            .WithMany(pt => pt.Projects)
            .HasForeignKey(p => p.ProjectTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ProjectTemplate>()
            .HasOne(pt => pt.InstructorProfile)
            .WithMany()
            .HasForeignKey(pt => pt.InstructorProfileId);

        builder.Entity<LogbookEntry>()
            .HasOne(l => l.StudentProfile)
            .WithMany()
            .HasForeignKey(l => l.StudentProfileId);

        builder.Entity<FFEntry>()
            .HasOne(f => f.StudentProfile)
            .WithMany()
            .HasForeignKey(f => f.StudentProfileId);

        builder.Entity<CheckIn>()
            .HasOne(c => c.StudentProfile)
            .WithMany()
            .HasForeignKey(c => c.StudentProfileId);

        builder.Entity<CheckIn>()
            .HasOne(c => c.Room)
            .WithMany(r => r.CheckIns)
            .HasForeignKey(c => c.RoomId);

        builder.Entity<InfoEntry>()
            .HasOne(i => i.InstructorProfile)
            .WithMany()
            .HasForeignKey(i => i.InstructorProfileId);
    }
}
