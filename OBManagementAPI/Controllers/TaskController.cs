using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBManagementAPI.Models;

namespace OBManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ObmanagementContext _context;

        public TasksController(ObmanagementContext context)
        {
            _context = context;
        }

        // -----------------------------------------------------------------------
        // GET api/tasks
        // -----------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _context.Tasks
                .Select(t => new
                {
                    taskId = t.Id,
                    description = t.Description,
                    location = t.Location.Name,
                    faculty = t.FacultyAccount.Name,
                    officeBoy = t.OfficeBoyAccount.Name,
                    status = t.Status,
                    taskTime = t.TaskTime,
                    scheduledAt = t.ScheduledAt,
                    isScheduled = t.IsScheduled,
                    rating = t.Rating,
                    remarks = t.Remarks
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // -----------------------------------------------------------------------
        // POST api/tasks/createTask
        // Updated to support Scheduling (Now/Later)
        // -----------------------------------------------------------------------
        [HttpPost("createTask")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            // Validate Faculty
            var faculty = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.FacultyAccountId && a.Role == 2);

            if (faculty == null)
                return BadRequest(new { message = "Faculty not found" });

            // Validate OfficeBoy
            var officeBoy = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.OfficeBoyAccountId && a.Role == 1);

            if (officeBoy == null)
                return BadRequest(new { message = "OfficeBoy not found" });

            // Validate Location
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == request.LocationId);

            if (location == null)
                return BadRequest(new { message = "Location not found" });

            // Validate Task Mode
            if (string.IsNullOrEmpty(request.TaskMode))
                return BadRequest(new { message = "TaskMode is required (now/later)" });

            bool isScheduled = request.TaskMode.ToLower() == "later";



            DateTime? scheduledTime = null;  // Nullable banaya

            if (isScheduled)
            {
                if (!request.ScheduledAt.HasValue)
                    return BadRequest(new { message = "ScheduledAt is required for later tasks" });

                if (request.ScheduledAt <= DateTime.Now)
                    return BadRequest(new { message = "Scheduled time must be in future" });

                scheduledTime = request.ScheduledAt.Value;
            }
            // else mein kuch nahi, scheduledTime null hi rahega

            //DateTime scheduledTime;
            //if (isScheduled)
            //{
            //    if (!request.ScheduledAt.HasValue)
            //        return BadRequest(new { message = "ScheduledAt is required for later tasks" });

            //    if (request.ScheduledAt <= DateTime.Now)
            //        return BadRequest(new { message = "Scheduled time must be in future" });

            //    scheduledTime = request.ScheduledAt.Value;
            //}
            //else
            //{
            //    scheduledTime = DateTime.Now;
            //}

            // Create Task Object
            var task = new OBManagementAPI.Models.Task
            {
                FacultyAccountId = request.FacultyAccountId,
                OfficeBoyAccountId = request.OfficeBoyAccountId,
                LocationId = request.LocationId,
                Description = request.Description,
                TaskTime = DateTime.Now,
                ScheduledAt = scheduledTime,
                IsScheduled = isScheduled,
                Status = "Pending",
                Rating = null,
                Remarks = null
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = isScheduled ? "Task scheduled successfully" : "Task created successfully",
                taskId = task.Id,
                scheduledAt = task.ScheduledAt
            });
        }
        // yeh sari locations ge

        [HttpGet("Locations")]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _context.Locations
                .Select(l => new
                {
                    id = l.Id,
                    name = l.Name,
                    latitude = l.Latitude,
                    longitude = l.Longitude
                })
                .ToListAsync();

            return Ok(locations);
        }




        // -----------------------------------------------------------------------
        // GET api/tasks/{id}
        // -----------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.Tasks
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    taskId = t.Id,
                    description = t.Description,
                    location = t.Location.Name,
                    faculty = t.FacultyAccount.Name,
                    officeBoy = t.OfficeBoyAccount.Name,
                    status = t.Status,
                    taskTime = t.TaskTime,
                    scheduledAt = t.ScheduledAt,
                    rating = t.Rating,
                    remarks = t.Remarks
                })
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new { message = "Task not found" });

            return Ok(task);
        }


        [HttpGet("byfaculty/{facultyId}")]
        public async Task<IActionResult> GetOfficeBoysByFaculty(int facultyId)
        {
            // Step 1 — Check faculty exists
            var faculty = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == facultyId && a.Role == 2);

            if (faculty == null)
                return NotFound(new { message = "Faculty not found" });

            // Step 2 — Get ALL floor numbers of this faculty's offices
            var facultyFloorIds = await _context.FacultyMemberOffices
                .Where(f => f.FacultyAccountId == facultyId)
                .Select(f => f.Office.BuildingFloorId)
                .Distinct()
                .ToListAsync();

            if (!facultyFloorIds.Any())
                return NotFound(new { message = "No floor assigned to this faculty" });

            // Step 3 — Get OfficeBoys assigned to those floors
            var officeBoys = await _context.OfficeBoyAssignedFloors
                .Where(o => facultyFloorIds.Contains(o.FloorId) && o.Status == "Active")
                .GroupBy(o => new { o.OfficeBoyAccount.Id, o.OfficeBoyAccount.Name, o.Floor.Number })
                .Select(g => new
                {
                    id = g.Key.Id,
                    name = g.Key.Name,
                    floor = g.Key.Number,
                    assignedOffices = g.Select(x => x.Office.OfficeName).ToList()
                })
                .ToListAsync();

            return Ok(officeBoys);
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound(new { message = "Task not found" });
            if (task.Status == "Completed") return BadRequest(new { message = "Task is already completed" });

            task.Status = "Completed";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Task marked as completed" });
        }

        [HttpPut("{id}/rate")]
        public async Task<IActionResult> RateTask(int id, [FromBody] RateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound(new { message = "Task not found" });
            if (task.Status != "Completed") return BadRequest(new { message = "Task must be completed before rating" });
            if (request.Rating < 1 || request.Rating > 5) return BadRequest(new { message = "Rating must be between 1 and 5" });

            task.Rating = request.Rating;
            task.Remarks = request.Remarks;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Task rated successfully" });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingTasks()
        {
            // Filter tasks where Status = "Pending"
            var tasks = await _context.Tasks
                .Where(t => t.Status == "Pending")
                .Select(t => new
                {
                    taskId = t.Id,
                    description = t.Description,
                    location = t.Location.Name,
                    faculty = t.FacultyAccount.Name,
                    officeBoy = t.OfficeBoyAccount.Name,
                    status = t.Status,
                    taskTime = t.TaskTime
                })
                .ToListAsync();

            return Ok(tasks);
        }
        // -----------------------------------------------------------------------
        // GET api/tasks/completed
        //
        // PURPOSE:
        //   Returns all tasks that have been completed.
        //   Used by Supervisor or Faculty to see finished tasks with ratings.
        //
        // RESPONSE (200): list of completed tasks (same fields as GetAllTasks)
        // -----------------------------------------------------------------------
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedTasks()
        {
            // Filter tasks where Status = "Completed"
            var tasks = await _context.Tasks
                .Where(t => t.Status == "Completed")
                .Select(t => new
                {
                    taskId = t.Id,
                    description = t.Description,
                    location = t.Location.Name,
                    faculty = t.FacultyAccount.Name,
                    officeBoy = t.OfficeBoyAccount.Name,
                    status = t.Status,
                    taskTime = t.TaskTime,
                    rating = t.Rating,
                    remarks = t.Remarks
                })
                .ToListAsync();

            return Ok(tasks);
        }


        // GET /api/tasks/poor-feedback
        // yeh jo poor feedback hain wo supervisior ko show kry ga
        [HttpGet("poor-feedback")]
        public async Task<IActionResult> GetPoorFeedback()
        {
            try
            {
                var poorFeedback = await _context.Tasks
                    .Include(t => t.FacultyAccount)
                    .Include(t => t.OfficeBoyAccount)
                    .Include(t => t.Location)
                    .Where(t => t.Rating == 1 && t.Status == "Completed")
                    .OrderByDescending(t => t.TaskTime)
                    .Select(t => new
                    {
                        taskId = t.Id,
                        description = t.Description,
                        faculty = t.FacultyAccount.Name,
                        officeBoy = t.OfficeBoyAccount.Name,
                        location = t.Location.Name,
                        rating = t.Rating,
                        remarks = t.Remarks,
                        taskTime = t.TaskTime,
                        status = t.Status
                    })
                    .ToListAsync();

                if (poorFeedback == null || !poorFeedback.Any())
                {
                    return Ok(new { message = "No poor feedback records found." });
                }

                return Ok(poorFeedback);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching data", error = ex.Message });
            }
        }
    



        [HttpGet("TaskAllShow/{facultyId}")]
        public async Task<IActionResult> GetFacultyTasks(int facultyId)
        {
            try
            {
                // 1. Database se is Faculty ke saare tasks fetch karein
                // NOTE: Include ki zaroorat nahi kyunki Select ke andar Navigation Properties use ho rahi hain
                var rawTasks = await _context.Tasks
                    .Where(t => t.FacultyAccountId == facultyId)
                    .OrderByDescending(t => t.TaskTime) // Latest task upar aayega
                    .Select(t => new {
                        taskId = t.Id,
                        description = t.Description,
                        locationName = t.Location.Name,           // ← Auto JOIN ho jayega
                        officeBoyName = t.OfficeBoyAccount.Name,   // ← Auto JOIN ho jayega
                        status = t.Status,
                        taskTime = t.TaskTime,
                        rating = t.Rating,
                        remarks = t.Remarks
                    })
                    .ToListAsync();

                // 2. Logic Implementation (In-Memory Filtering)

                // ALL Tab: Aaj ke tasks + Purane pending + Completed without rating
                var today = DateTime.Today;

                var allTab = rawTasks
                    .Where(t => t.status == "Pending" || (t.status == "Completed" && t.rating == null))
                    .ToList();

                // PENDING Tab: Sirf pending tasks (chahe kitne bhi purane hon)
                var pendingTab = rawTasks
                    .Where(t => t.status == "Pending")
                    .ToList();

                // COMPLETED Tab: Completed tasks with NULL rating (Feedback Needed)
                var completedTab = rawTasks
                    .Where(t => t.status == "Completed" && t.rating == null)
                    .ToList();

                // 3. Final Response JSON format mein
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        all = allTab,
                        pending = pendingTab,
                        completed = completedTab
                    },
                    stats = new
                    {
                        allCount = allTab.Count,
                        pendingCount = pendingTab.Count,
                        completedCount = completedTab.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


    }

    // --- Request Models ---

    public class CreateTaskRequest
    {
        public int FacultyAccountId { get; set; }
        public int OfficeBoyAccountId { get; set; }
        public int LocationId { get; set; }
        public string Description { get; set; }
        public string TaskMode { get; set; } // "now" or "later"
        public DateTime? ScheduledAt { get; set; } // Required if TaskMode is "later"
    }

    public class RateTaskRequest
    {
        public int Rating { get; set; }
        public string Remarks { get; set; }
    }
}