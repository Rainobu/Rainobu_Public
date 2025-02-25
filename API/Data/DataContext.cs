using System;
using System.Reflection;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int,
        IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<UserStory> LikeStory { get; set; }
        public DbSet<UserHistory> HistoryUsers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Genre> Genres {get;set;}
        public DbSet<Status> Statuses {get; set;}
        public DbSet<Language> Languages { get; set; }
        public DbSet<Story> Stories {get; set;}
        public DbSet<StoryChapter> StoryChapters { get; set; }
        public DbSet<Published> Publishes { get; set; }
        public DbSet<Tag> Tags {get;set;}
        public DbSet<TagStory> TagStories { get; set; }
        //public DbSet<StoryTag> StoryTags { get; set; }
        public DbSet<StoryComment> StoryComments {get;set;}
        public DbSet<PhotoStory> PhotoStories { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<Report> Reports {get;set;}
        public DbSet<ReportTopic> ReportTopics {get; set;}
        public DbSet<News> Newses {get; set;}
        public DbSet<LikeComment> LikeComments {get; set;}
        public DbSet<LikeChapter> LikeChapters { get; set; }
        public DbSet<Activities> Activities { get; set; }
        public DbSet<RecievePoint> RecievePoints { get; set; }
        public DbSet<ActivitiesPoint> ActivitiesPoints { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<TitleName> titleName { get; set; }
        public DbSet<TitleActive> titleActives { get; set; }
        public DbSet<PhotoScreen>  PhotoScreens { get; set; }
        public DbSet<PhotoSlide> PhotoSlides { get; set; }
        public DbSet<PhotoSlide2> PhotoSlides2 { get; set; }
        public DbSet<BannerChapter> BannerChapters { get; set; }
        public DbSet<BannerDialog> BannerDialogs { get; set; }
        public DbSet<VipUser> VipUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            
            builder.Entity<Group>()
                .HasMany(x => x.Connections)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            builder.Entity<UserLike>()
                .HasKey(k => new { k.SourceUserId, k.LikedUserId });

            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(s => s.LikedUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserStory>()
                .HasKey(k => new { k.SourceUserId, k.LikedStoryId });

            builder.Entity<UserStory>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedStoryByUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserStory>()
                .HasOne(s => s.LikedStory)
                .WithMany(l => l.StoryLiked)
                .HasForeignKey(s => s.LikedStoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserHistory>()
                .HasKey(k => new { k.SourceUserId, k.HistoryStoryId });

            builder.Entity<UserHistory>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.UserHistory)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserHistory>()
                .HasOne(s => s.HistoryStory)
                .WithMany(l => l.StoryHistory)
                .HasForeignKey(s => s.HistoryStoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TagStory>()
                .HasKey(k => new { k.TagId , k.StoryId });

            builder.Entity<TagStory>()
                .HasOne(s => s.Tags)
                .WithMany(l => l.TagStories)
                .HasForeignKey(s => s.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TagStory>()
                .HasOne(s => s.Stories)
                .WithMany(l => l.StoryTags)
                .HasForeignKey(s => s.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // builder.Entity<StoryTag>()
            //     .HasKey(k => new {k.StoryId,k.TagId});

            // builder.Entity<StoryTag>()
            //     .HasOne(s => s.Story)
            //     .WithMany(t => t.StoryTags)
            //     .HasForeignKey(s => s.StoryId)
            //     .OnDelete(DeleteBehavior.Restrict);

            // builder.Entity<StoryTag>()
            //     .HasOne(s => s.Tag)
            //     .WithMany(t => t.StoryTags)
            //     .HasForeignKey(s => s.TagId)
            //     .OnDelete(DeleteBehavior.Restrict);
                
            builder.ApplyUtcDateTimeConverter();
        }
    }

    public static class UtcDateAnnotation
    {
        private const String IsUtcAnnotation = "IsUtc";
        private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
          new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
          new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
          builder.HasAnnotation(IsUtcAnnotation, isUtc);

        public static Boolean IsUtc(this IMutableProperty property) =>
          ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

        /// <summary>
        /// Make sure this is called after configuring all your entities.
        /// </summary>
        public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (!property.IsUtc())
                    {
                        continue;
                    }

                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(UtcConverter);
                    }

                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(UtcNullableConverter);
                    }
                }
            }
        }
    }
}