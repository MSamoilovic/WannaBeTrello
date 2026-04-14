using Feezbow.Domain.Events.Household_Events;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

/// <summary>
/// Extends a <see cref="Project"/> with household-specific metadata.
/// One Project can have at most one HouseholdProfile (1:1 relationship).
/// </summary>
public class HouseholdProfile : AuditableEntity
{
    public long ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string Timezone { get; private set; } = "Europe/Belgrade";
    public DayOfWeek ShoppingDay { get; private set; } = DayOfWeek.Saturday;

    private HouseholdProfile() { }

    public static HouseholdProfile Create(long projectId, long createdBy, string? address = null,
        string? city = null, string? timezone = null, DayOfWeek? shoppingDay = null)
    {
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        var profile = new HouseholdProfile
        {
            ProjectId = projectId,
            Address = address?.Trim(),
            City = city?.Trim(),
            Timezone = string.IsNullOrWhiteSpace(timezone) ? "Europe/Belgrade" : timezone.Trim(),
            ShoppingDay = shoppingDay ?? DayOfWeek.Saturday,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        profile.AddDomainEvent(new HouseholdProfileCreatedEvent(profile.Id, projectId, createdBy));

        return profile;
    }

    public void Update(string? address, string? city, string? timezone, DayOfWeek? shoppingDay, long updatedBy)
    {
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var changed = false;

        var normalizedAddress = address?.Trim();
        if (!string.Equals(Address, normalizedAddress, StringComparison.Ordinal))
        {
            oldValues[nameof(Address)] = Address;
            Address = normalizedAddress;
            newValues[nameof(Address)] = Address;
            changed = true;
        }

        var normalizedCity = city?.Trim();
        if (!string.Equals(City, normalizedCity, StringComparison.Ordinal))
        {
            oldValues[nameof(City)] = City;
            City = normalizedCity;
            newValues[nameof(City)] = City;
            changed = true;
        }

        var normalizedTimezone = string.IsNullOrWhiteSpace(timezone) ? null : timezone.Trim();
        if (normalizedTimezone is not null && !string.Equals(Timezone, normalizedTimezone, StringComparison.Ordinal))
        {
            oldValues[nameof(Timezone)] = Timezone;
            Timezone = normalizedTimezone;
            newValues[nameof(Timezone)] = Timezone;
            changed = true;
        }

        if (shoppingDay.HasValue && ShoppingDay != shoppingDay.Value)
        {
            oldValues[nameof(ShoppingDay)] = ShoppingDay;
            ShoppingDay = shoppingDay.Value;
            newValues[nameof(ShoppingDay)] = ShoppingDay;
            changed = true;
        }

        if (!changed) return;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;

        AddDomainEvent(new HouseholdProfileUpdatedEvent(Id, ProjectId, updatedBy, oldValues, newValues));
    }
}
