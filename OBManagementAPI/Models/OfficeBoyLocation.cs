using System;
using System.Collections.Generic;

namespace OBManagementAPI.Models;

public partial class OfficeBoyLocation
{
    public int Id { get; set; }

    public int OfficeBoyAccountId { get; set; }

    public int TaskId { get; set; }

    public double CurrentLatitude { get; set; }

    public double CurrentLongitude { get; set; }

    public string LocationName { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public virtual Account OfficeBoyAccount { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
