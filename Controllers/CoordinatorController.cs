
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
    /* Microsoft. 2024.
     * Asynchronous programming with async and await
     * Microsoft Learn
     * https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
     * Accessed: October 20, 2024
     */

    /*Microsoft Corporation. (2024)
TempData in ASP.NET Core
Microsoft Learn
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state#tempdata
Date Accessed: October 20, 2025
*/


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


    // POST: Verify (Approve)
    [HttpPost]
    public async Task<IActionResult> Verify(int claimId)
    {
        try // Error handling for all claims
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

    // GET: Download Document
    public async Task<IActionResult> DownloadDocument(int documentId)
    {
        try
        { // Error handling for document downloads
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