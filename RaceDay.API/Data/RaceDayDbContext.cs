using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RaceDay.API.Data;

public partial class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Enrolment> Enrolments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventParticipant> EventParticipants { get; set; }

    public virtual DbSet<Organiser> Organisers { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<RouteInformation> RouteInformations { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__23CAF1F80583C1A4");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("categoryID");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("categoryName");
            entity.Property(e => e.DistanceKm)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("distanceKM");
            entity.Property(e => e.EventId).HasColumnName("eventID");
            entity.Property(e => e.MaxParticipants).HasColumnName("maxParticipants");

            entity.HasOne(d => d.Event).WithMany(p => p.Categories)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Category_Event");

            entity.HasMany(d => d.Events).WithMany(p => p.CategoriesNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "EventCategory",
                    r => r.HasOne<Event>().WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EC_Event"),
                    l => l.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EC_Category"),
                    j =>
                    {
                        j.HasKey("CategoryId", "EventId");
                        j.ToTable("EventCategory");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("categoryID");
                        j.IndexerProperty<int>("EventId").HasColumnName("eventID");
                    });
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(e => e.EnrolmentId).HasName("PK__Enrolmen__34687B2B3F6B36D9");

            entity.ToTable("Enrolment");

            entity.Property(e => e.EnrolmentId).HasColumnName("enrolmentID");
            entity.Property(e => e.CategoryId).HasColumnName("categoryID");
            entity.Property(e => e.EnrolmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("enrolmentDate");
            entity.Property(e => e.ParticipantId).HasColumnName("participantID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active")
                .HasColumnName("status");

            entity.HasOne(d => d.Category).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolment_Category");

            entity.HasOne(d => d.Participant).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolment_Participant");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Event__2DC7BD698D9DED1A");

            entity.ToTable("Event");

            entity.Property(e => e.EventId).HasColumnName("eventID");
            entity.Property(e => e.CategoryId).HasColumnName("categoryID");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.EventDate)
                .HasColumnType("datetime")
                .HasColumnName("eventDate");
            entity.Property(e => e.Location)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("location");
            entity.Property(e => e.OrganiserId).HasColumnName("organiserID");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.Organiser).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrganiserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Event_Organiser");
        });

        modelBuilder.Entity<EventParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ParticipantId, e.EventId });

            entity.ToTable("EventParticipant");

            entity.Property(e => e.ParticipantId).HasColumnName("participantID");
            entity.Property(e => e.EventId).HasColumnName("eventID");
            entity.Property(e => e.Field)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Event).WithMany(p => p.EventParticipants)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EP_Event");

            entity.HasOne(d => d.Participant).WithMany(p => p.EventParticipants)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EP_Participant");
        });


        modelBuilder.Entity<Organiser>(entity =>
        {
            entity.HasKey(e => e.OrganiserID);

            entity.ToTable("Organiser");

            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.OrganiserID)
                .HasColumnName("OrganiserID");

            entity.Property(e => e.CreatedAT)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAT");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fullName");

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("passwordHash");
        });


        modelBuilder.Entity<Participant>(entity =>
        {

            entity.HasKey(e => e.ParticipantID).HasName("PK__Particip__4EE7923021C6D7E4");

            entity.ToTable("Participant");

            entity.Property(e => e.ParticipantID).HasColumnName("participantID");
            entity.Property(e => e.Field)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ParticipantName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("participantName");
            entity.Property(e => e.ParticipantSurname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("participantSurname");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Email");

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PasswordHash");

        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Result__C6EADC7B880F6357");

            entity.ToTable("Result");

            entity.HasIndex(e => e.EnrolmentId, "UQ__Result__34687B2A1DD8029E").IsUnique();

            entity.Property(e => e.ResultId).HasColumnName("resultID");
            entity.Property(e => e.EnrolmentId).HasColumnName("enrolmentID");
            entity.Property(e => e.FinishTime).HasColumnName("finishTime");
            entity.Property(e => e.Position).HasColumnName("position");

            entity.HasOne(d => d.Enrolment).WithOne(p => p.Result)
                .HasForeignKey<Result>(d => d.EnrolmentId)
                .HasConstraintName("FK_Result_Enrolment");
        });

        modelBuilder.Entity<RouteInformation>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK__RouteInf__BAC024277BF9BB7C");

            entity.ToTable("RouteInformation");

            entity.Property(e => e.RouteId).HasColumnName("routeID");
            entity.Property(e => e.EndPoint)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("endPoint");
            entity.Property(e => e.EventId).HasColumnName("eventID");
            entity.Property(e => e.RouteName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("routeName");
            entity.Property(e => e.StartPoint)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("startPoint");

            entity.HasOne(d => d.Event).WithMany(p => p.RouteInformations)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Route_Event");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
