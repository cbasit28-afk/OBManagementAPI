using System;
using System.Collections.Generic;

namespace OBManagementAPI.Models;

public partial class OfficeBoyLeave
{
    public int Id { get; set; }

    public int OfficeBoyAccountId { get; set; }

    public DateTime RequestedStartDate { get; set; }

    public DateTime RequestedEndDate { get; set; }

    public int RequestedDays { get; set; }

    public string? Reason { get; set; }

    public DateTime? ApprovedStartDate { get; set; }

    public DateTime? ApprovedEndDate { get; set; }

    public int? ApprovedDays { get; set; }

    public string? SupervisorRemarks { get; set; }

    public string Status { get; set; } = null!;

    public int? ReplacementOfficeBoyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account OfficeBoyAccount { get; set; } = null!;

    public virtual Account? ReplacementOfficeBoy { get; set; }
}
