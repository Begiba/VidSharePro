// VidSharePro.Domain/Common/BaseEntity.cs
namespace VidSharePro.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "System";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsUpdated(string user)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user;
    }

    public void SoftDelete() => IsDeleted = true;

    public void UpdateModifiedDate()
    {
        this.UpdatedAt = DateTime.UtcNow;
    }
}