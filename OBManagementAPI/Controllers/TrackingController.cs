using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBManagementAPI.Models;


namespace OBManagementAPI.Controllers
{
    [ApiController]
    [Route("api/tracking")]
    public class TrackingController : ControllerBase
    {
        private readonly ObmanagementContext _context;

        // University ka fixed center point (campus)
        private const double UNIVERSITY_LAT = 33.706485;
        private const double UNIVERSITY_LNG = 73.086977;

        public TrackingController(ObmanagementContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────
        // 1. Teacher geofence task banata hai
        // POST /api/tracking/set-location-task
        // ─────────────────────────────────────────────────────
        [HttpPost("set-location-task")]
        public async Task<IActionResult> SetLocationTask(
            [FromBody] FacultyTaskRequest req)
        {
            // Basic validation
            if (req.OfficeBoyAccountId == null)
                return BadRequest(new { message = "OfficeBoyAccountId is required" });

            if (req.TriggerType != "IN" && req.TriggerType != "OUT")
                return BadRequest(new { message = "TriggerType must be IN or OUT" });

            var officeBoy = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == req.OfficeBoyAccountId && a.Role == 1);

            if (officeBoy == null)
                return BadRequest(new { message = "OfficeBoy not found" });

            // Naya geofence task record — sab kuch FacultyTracking table mein
            var tracking = new FacultyTracking
            {
                FacultyAccountId = req.FacultyAccountId,
                OfficeBoyAccountId = req.OfficeBoyAccountId,
                TaskType = "Geofence",
                TaskDescription = req.TaskDescription,
                SelectedLatitude = UNIVERSITY_LAT,   // campus fixed point
                SelectedLongitude = UNIVERSITY_LNG,
                RadiusMeters = req.RadiusMeters ?? 300,
                TriggerType = req.TriggerType,
                Status = "Pending",
                IsInsideRegion = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.FacultyTrackings.Add(tracking);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Geofence task created successfully",
                id = tracking.Id
            });
        }

        // ─────────────────────────────────────────────────────
        // 2. Teacher app periodic location bhejti hai
        //    Backend check karta hai geofence trigger hua ya nahi
        // POST /api/tracking/check-region
        // ─────────────────────────────────────────────────────
        [HttpPost("check-region")]
        public async Task<IActionResult> CheckRegion(
            [FromBody] RegionCheckRequest req)
        {
            // Is teacher ke sare "Pending" geofence tasks nikalo
            var pendingTasks = await _context.FacultyTrackings
                .Where(f =>
                    f.FacultyAccountId == req.FacultyAccountId &&
                    f.Status == "Pending")
                .ToListAsync();

            if (!pendingTasks.Any())
            {
                return Ok(new
                {
                    triggered = false,
                    message = "No pending geofence tasks"
                });
            }

            var triggeredList = new List<object>();

            foreach (var task in pendingTasks)
            {
                var distance = CalculateDistance(
                    req.CurrentLatitude,
                    req.CurrentLongitude,
                    task.SelectedLatitude,
                    task.SelectedLongitude);

                double radius = task.RadiusMeters ?? 300;

                bool shouldTrigger =
                    (task.TriggerType == "IN" && distance <= radius) ||
                    (task.TriggerType == "OUT" && distance > radius);

                if (shouldTrigger)
                {
                    // Status "Triggered" karo — sirf ek baar trigger ho
                    task.Status = "Triggered";
                    task.IsInsideRegion = distance <= radius;
                    task.UpdatedAt = DateTime.Now;

                    triggeredList.Add(new
                    {
                        id = task.Id,
                        officeBoyId = task.OfficeBoyAccountId,
                        taskDescription = task.TaskDescription,
                        triggerType = task.TriggerType,
                        distanceMeters = Math.Round(distance, 2)
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                triggered = triggeredList.Any(),
                triggeredCount = triggeredList.Count,
                tasks = triggeredList
            });
        }

        // ─────────────────────────────────────────────────────
        // 3. OfficeBoy ke "Triggered" geofence tasks
        // GET /api/tracking/ob-geofence-tasks/{officeBoyId}
        // ─────────────────────────────────────────────────────
        [HttpGet("ob-geofence-tasks/{officeBoyId}")]
        public async Task<IActionResult> GetOBGeofenceTasks(int officeBoyId)
        {
            var tasks = await _context.FacultyTrackings
        .Include(f => f.FacultyAccount)
        .Where(f =>
            f.OfficeBoyAccountId == officeBoyId &&
            f.Status == "Triggered")
        .OrderByDescending(f => f.UpdatedAt)
                .Select(f => new
                {
                    id = f.Id,
                    facultyName = f.FacultyAccount.Name,
                    taskDescription = f.TaskDescription,
                    triggerType = f.TriggerType,
                    radiusMeters = f.RadiusMeters,
                    status = f.Status,
                    updatedAt = f.UpdatedAt
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // ─────────────────────────────────────────────────────
        // 4. Faculty ke sare geofence tasks (apni history dekhne ke liye)
        // GET /api/tracking/faculty-task/{facultyId}
        // ─────────────────────────────────────────────────────
        [HttpGet("faculty-task/{facultyId}")]
        public async Task<IActionResult> GetFacultyTask(int facultyId)
        {
            var tasks = await _context.FacultyTrackings
                .Include(f => f.OfficeBoyAccount)
                .Where(f => f.FacultyAccountId == facultyId)
                .OrderByDescending(f => f.UpdatedAt)
                .Select(f => new
                {
                    id = f.Id,
                    officeBoyName = f.OfficeBoyAccount != null ? f.OfficeBoyAccount.Name : null,
                    taskType = f.TaskType,
                    taskDescription = f.TaskDescription,
                    triggerType = f.TriggerType,
                    radiusMeters = f.RadiusMeters,
                    status = f.Status,
                    isInsideRegion = f.IsInsideRegion,
                    updatedAt = f.UpdatedAt
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // ─────────────────────────────────────────────────────
        // 5. OfficeBoy task complete kare
        // PUT /api/tracking/complete-geofence/{id}
        // ─────────────────────────────────────────────────────
        [HttpPut("complete-geofence/{id}")]
        public async Task<IActionResult> CompleteGeofenceTask(int id)
        {
            var task = await _context.FacultyTrackings.FindAsync(id);

            if (task == null)
                return NotFound(new { message = "Geofence task not found" });

            if (task.Status == "Completed")
                return BadRequest(new { message = "Task already completed" });

            task.Status = "Completed";
            task.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Geofence task marked as completed" });
        }

        // ─── Haversine Formula — do points ke beech distance ───
        private double CalculateDistance(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius meters
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRad(double deg) => deg * Math.PI / 180;
    }

    // ─── Request Models ─────────────────────────────────────
    public class FacultyTaskRequest
    {
        public int FacultyAccountId { get; set; }
        public int? OfficeBoyAccountId { get; set; }
        public string TaskDescription { get; set; } = null!;
        public double? RadiusMeters { get; set; }
        public string TriggerType { get; set; } = null!;
    }

    public class RegionCheckRequest
    {
        public int FacultyAccountId { get; set; }
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
    }
}