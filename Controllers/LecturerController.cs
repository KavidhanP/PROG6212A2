using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROGA22025.Data;
using PROGA22025.Models;
using PROGA22025.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class LecturerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly FileUploadService _fileUploadService;

    public LecturerController(ApplicationDbContext context)
    {
        _context = context;
        _fileUploadService = new FileUploadService();
    }
    /* Microsoft. 2024.
 * Asynchronous programming with async and await
 * Microsoft Learn
 * https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
 * Accessed: October 20, 2024
 */
    // GET: Lecturer Dashboard - View All Claims
    public async Task<IActionResult> Index(int? lecturerId)
    {
        try
        {
            // If no lecturerId, show all lecturers to choose from
            if (lecturerId == null)
            {
                var lecturers = await _context.Lecturers.ToListAsync();
                return View("SelectLecturer", lecturers);
            }

            var lecturer = await _context.Lecturers.FindAsync(lecturerId.Value);
            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "Lecturer not found.";
                return RedirectToAction("Index");
            }

            // Get claims with supporting documents
            var claims = await _context.Claims
                .Include(c => c.SupportingDocuments)
                .Where(c => c.LecturerId == lecturerId.Value)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            ViewBag.LecturerName = lecturer.Name;
            ViewBag.LecturerId = lecturerId.Value;

            return View(claims);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
    /*Microsoft Corporation. (2024)
TempData in ASP.NET Core
Microsoft Learn
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state#tempdata
Date Accessed: October 20, 2025
    */

    // GET: Submit New Claim Form
    public async Task<IActionResult> SubmitClaim(int lecturerId)
    {
        try
        {
            var lecturer = await _context.Lecturers.FindAsync(lecturerId);
            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "Lecturer not found.";
                return RedirectToAction("Index");
            }

            ViewBag.LecturerName = lecturer.Name;
            ViewBag.LecturerId = lecturerId;
            return View();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: Submit Claim with File Upload
    [HttpPost]
    public async Task<IActionResult> SubmitClaim(ClaimViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var lecturer = await _context.Lecturers.FindAsync(model.LecturerId);
                ViewBag.LecturerName = lecturer?.Name;
                ViewBag.LecturerId = model.LecturerId;
                return View(model);
            }

            // Create new claim
            var claim = new Claims
            {
                LecturerId = model.LecturerId,
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                Notes = model.Notes,
                SubmittedDate = DateTime.Now,
                Status = "Pending",
                CoordinatorApproved = null,
                ManagerApproved = null
            };

            // Save claim first to get ClaimId
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Handle file uploads
            if (model.Documents != null && model.Documents.Any())
            {
                foreach (var file in model.Documents)
                {
                    if (file != null && file.Length > 0)
                    {
                        // Upload file
                        var (success, message, storefilename) = _fileUploadService.UploadFile(file);

                        if (!success)
                        {
                            ModelState.AddModelError("Documents", message);
                            var lec = await _context.Lecturers.FindAsync(model.LecturerId);
                            ViewBag.LecturerName = lec?.Name;
                            ViewBag.LecturerId = model.LecturerId;
                            return View(model);
                        }

                        // Create supporting document record
                        var document = new SupportingDocument
                        {
                            ClaimId = claim.ClaimId,
                            OriginalFileName = file.FileName,
                            StoredFileName = storefilename,
                            FileSize = file.Length,
                            FileType = Path.GetExtension(file.FileName),
                            UploadedDate = DateTime.Now
                        };

                        _context.SupportingDocuments.Add(document);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Claim submitted successfully!";
            return RedirectToAction("Index", new { lecturerId = model.LecturerId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error submitting claim: {ex.Message}";
            var lecturer = await _context.Lecturers.FindAsync(model.LecturerId);
            ViewBag.LecturerName = lecturer?.Name;
            ViewBag.LecturerId = model.LecturerId;
            return View(model);
        }
    }
    /*
     * Microsoft Corporation. (2024)
        Switch expression - C# reference
        Microsoft Learn
        https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression
        Date Accessed: October 20, 2025
    */
    /*Mozilla Contributors. (2024)
      Common MIME types
      MDN Web Docs
      https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
      Date Accessed: October 20, 2025
    */
    // GET: Download Supporting Document
    public async Task<IActionResult> DownloadDocument(int documentId)
    {
        try
        {
            var document = await _context.SupportingDocuments.FindAsync(documentId);

            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("Index");
            }

            var (success, message, fileData, originalFileName) =
                _fileUploadService.DownloadFile(document.StoredFileName, document.OriginalFileName);

            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            // Determine content type based on file extension
            string contentType = document.FileType.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            return File(fileData, contentType, originalFileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error downloading file: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // GET: Create Lecturer
    public IActionResult Create()
    {
        return View();
    }

    // POST: Create Lecturer
    [HttpPost]
    public async Task<IActionResult> Create(Lecturers lecturer)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(lecturer);
            }

            // Check for duplicate email
            if (await _context.Lecturers.AnyAsync(l => l.Email.ToLower() == lecturer.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "A lecturer with this email already exists.");
                return View(lecturer);
            }

            lecturer.CreatedDate = DateTime.Now;
            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Lecturer {lecturer.Name} created successfully!";
            return RedirectToAction("Index", new { lecturerId = lecturer.LecturerId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating lecturer: {ex.Message}";
            return View(lecturer);
        }
    }
}
