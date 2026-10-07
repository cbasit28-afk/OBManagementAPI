using System;
using System.Collections.Generic;

namespace OBManagementAPI.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }

    public virtual ICollection<FacultyMemberOffice> FacultyMemberOffices { get; set; } = new List<FacultyMemberOffice>();

    public virtual ICollection<FacultyTracking> FacultyTrackingFacultyAccounts { get; set; } = new List<FacultyTracking>();

    public virtual ICollection<FacultyTracking> FacultyTrackingOfficeBoyAccounts { get; set; } = new List<FacultyTracking>();

    public virtual ICollection<OfficeBoyAssignedFloor> OfficeBoyAssignedFloors { get; set; } = new List<OfficeBoyAssignedFloor>();

    public virtual ICollection<OfficeBoyLeave> OfficeBoyLeaveOfficeBoyAccounts { get; set; } = new List<OfficeBoyLeave>();

    public virtual ICollection<OfficeBoyLeave> OfficeBoyLeaveReplacementOfficeBoys { get; set; } = new List<OfficeBoyLeave>();

    public virtual ICollection<OfficeBoyLocation> OfficeBoyLocations { get; set; } = new List<OfficeBoyLocation>();

    public virtual ICollection<Task> TaskFacultyAccounts { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskOfficeBoyAccounts { get; set; } = new List<Task>();
}
