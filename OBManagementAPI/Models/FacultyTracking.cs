using System;
using System.Collections.Generic;

namespace OBManagementAPI.Models;

public partial class FacultyTracking
{
    public int Id { get; set; }

    public int FacultyAccountId { get; set; }

    public string TaskType { get; set; } = null!;

    public string TaskDescription { get; set; } = null!;

    public double SelectedLatitude { get; set; }

    public double SelectedLongitude { get; set; }

    public bool? IsInsideRegion { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? OfficeBoyAccountId { get; set; }

    public double? RadiusMeters { get; set; }

    public string? TriggerType { get; set; }

    public string Status { get; set; } = null!;

    public virtual Account FacultyAccount { get; set; } = null!;

    public virtual Account? OfficeBoyAccount { get; set; }
}
