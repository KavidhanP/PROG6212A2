
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROGA22025.Data;
using PROGA22025.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CoordinatorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly FileUploadService _fileUploadService;

    public CoordinatorController(ApplicationDbContext context)
    {
        _context = context;
        _fileUploadService = new FileUploadService();
    }

    // GET: View Pending Claims
    public async Task<IActionResult> Index()
    {
        try
        {
            // Get claims where coordinator hasn't reviewed yet
            var pendingClaims = await _context.Claims
                .Include(c => c.SupportingDocuments)
                .Where(c => c.CoordinatorApproved == null)
                .OrderBy(c => c.SubmittedDate)
                .ToListAsync();

            return View(pendingClaims);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading claims: {ex.Message}";
            return View();
        }
    }

    // GET: View History (Reviewed Claims)
    public async Task<IActionResult> History()
    {
        try
        {
            // Get claims that coordinator has already reviewed
            var reviewedClaims = await _context.Claims
                .Include(c => c.SupportingDocuments)
                .Where(c => c.CoordinatorApproved != null)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            return View(reviewedClaims);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading history: {ex.Message}";
            return View();
        }
    }

    // POST: Verify (Approve)
    [HttpPost]
    public async Task<IActionResult> Verify(int claimId)
    {
        try
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Index");
            }

            if (claim.CoordinatorApproved != null)
            {
                TempData["ErrorMessage"] = "This claim has already been reviewed.";
                return RedirectToAction("Index");
            }

            claim.CoordinatorApproved = true;
            claim.Status = "Pending Manager Review";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Claim #{claimId} verified successfully!";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error verifying claim: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: Reject
    [HttpPost]
    public async Task<IActionResult> Reject(int claimId)
    {
        try
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Index");
            }

            if (claim.CoordinatorApproved != null)
            {
                TempData["ErrorMessage"] = "This claim has already been reviewed.";
                return RedirectToAction("Index");
            }

            claim.CoordinatorApproved = false;
            claim.Status = "Rejected by Coordinator";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Claim #{claimId} rejected.";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error rejecting claim: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // GET: Download Document
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
}