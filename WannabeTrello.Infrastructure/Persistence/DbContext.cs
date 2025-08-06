using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events;

namespace WannabeTrello.Infrastructure.Persistence;

public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        IMediator? mediator = null,
        ICurrentUserService? currentUserService = null)
    : IdentityDbContext<User, IdentityRole<long>, long>(options)
{
    private readonly IMediator? _mediator = mediator;
    
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<BoardTask> Tasks { get; init; } = null!;
    public DbSet<Column> Columns { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
    public DbSet<BoardMember> BoardMembers { get; set; } = null!;
    public DbSet<ActivityTracker> ActivityTrackers { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Dodato zbog toga da bi MB koristio konfiguracije iz trenutnog Assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        modelBuilder.Ignore<DomainEvent>(); 
        
        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var domainEntities = ChangeTracker.Entries<AuditableEntity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Count != 0)
            .Select(x => x.Entity)
            .ToList();
        
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            
            // if(entry.Entity is User) continue;
            
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    //Dodati logiku za CreatedBy, sa currentUserService
                    entry.Entity.CreatedBy = currentUserService?.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = DateTime.UtcNow;
                    // Isto za LastModifiedBy
                    entry.Entity.LastModifiedBy = currentUserService?.UserId;
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (mediator == null) return result;
        
        foreach (var entity in domainEntities)
        {
            foreach (var domainEvent in entity.DomainEvents.ToList())
            {
                await mediator.Publish(domainEvent, cancellationToken);
            }
                
            entity.ClearDomainEvents();
        }

        return result;
    }
};