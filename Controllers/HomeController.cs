using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SchwarzesBrett.Models;
using SchwarzesBrett.ViewModels;
using System.Diagnostics;
using SchwarzesBrett.Extensions;

namespace SchwarzesBrett.Controllers
{
    public class HomeController(ILogger<HomeController> _logger, Connection connection) : Controller
    {

        public async Task<IActionResult> Index()
        {
            List<ListingTile> tiles = await connection.ListingsGet();
            ListingLookups lookups = await connection.LookupsGetAll();
            IndexViewModel viewmodel = new IndexViewModel
            {
                Tiles = tiles,
                Lookups = lookups
            };
            return View(viewmodel);
        }

        public async Task<IActionResult> Details(int id)
        {
            Listing? listing = await connection.ListingGetById(id);

            if (listing == null)
                return NotFound();

            return View(listing);
        }
      
        public async Task<IActionResult> My()
        {
            List<ListingTile> tiles = await connection.ListingsGetByUser();
            return View(tiles);
        }

        ///////////////////////////// Create \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

        [HttpGet]
        [RequestSizeLimit(60 * 1024 * 1024)]
        public async Task<IActionResult> Create()
        {
            CreateViewModel viewModel = new()
            {
                Lookups = await connection.LookupsGetAll()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel viewModel)
        {
            string? error = null;

            if (viewModel.Listing.Attachments.Count > FileValidator.MaxCount)
                error = $"Maximal {FileValidator.MaxCount} Dateien.";

            foreach (var file in viewModel.Listing.Attachments)
                if (FileValidator.Validate(file) is string fileError)
                    error = fileError;

            if (error != null)
            {
                viewModel.ErrorMessage = error;
                viewModel.Lookups = await connection.LookupsGetAll();
                return View(viewModel);
            }

            try
            {
                int id = await connection.ListingCreate(viewModel.Listing);
                TempData["Success"] = "Inserat wurde veröffentlicht.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (SqlException ex)
            {
                viewModel.ErrorMessage = ex.Message;
                viewModel.Lookups = await connection.LookupsGetAll();
                return View(viewModel);
            }
        }

        ///////////////////////////// Edit \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Listing? listing = await connection.ListingGetById(id);

            if (listing == null)
                return NotFound();
           
            EditViewModel viewModel = new()
            {
                Listing = new ListingEditRequest
                {
                    Id = listing.Id,
                    Name = listing.Name,
                    Description = listing.Description,
                    ListingTypeId = listing.ListingType.Id,
                    CategoryId = listing.Category.Id,
                    PriceCategoryId = listing.PriceCategory.Id,
                    Price = listing.Price,
                    ContactPhone = listing.ContactPhone,
                    ContactMail = listing.ContactMail
                },
                Lookups = await connection.LookupsGetAll(),
                Attachments = listing.Attachments
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditViewModel viewModel)
        {
            viewModel.Listing.Id = id;   

            try
            {
                await connection.ListingEdit(viewModel.Listing);
                TempData["Success"] = "Änderungen gespeichert.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (SqlException ex)
            {
                viewModel.ErrorMessage = ex.Message;
                viewModel.Lookups = await connection.LookupsGetAll();

                Listing? listing = await connection.ListingGetById(id);
                viewModel.Attachments = listing?.Attachments ?? new();

                return View(viewModel);
            }
        }

        ///////////////////////////// Sonstiges \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await connection.ListingDelete(id);
                TempData["Success"] = "Inserat wurde gelöscht.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("My");
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus(int id, int statusId)
        {
            try
            {
                await connection.ListingSetStatus(id, statusId);
                TempData["Success"] = "Status wurde geändert.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("My");
        }

        [HttpPost]
        public async Task<IActionResult> Extend(int id)
        {
            try
            {
                await connection.ListingExtend(id);
                TempData["Success"] = "Inserat wurde verlängert.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("My");
        }

        ///////////////////////////// Attachments \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
        
        public async Task<IActionResult> Attachment(int id)
        {
            var file = await connection.AttachmentGetContent(id);

            if (file == null)
                return NotFound();

            var (filetype, value, filename) = file.Value;

            if (filetype == "application/pdf")
                return File(value, filetype, filename ?? $"Anhang-{id}.pdf");

            return File(value, filetype);                                     
        }

        [HttpPost]
        [RequestSizeLimit(60 * 1024 * 1024)]
        public async Task<IActionResult> AttachmentAdd(int listingId, IFormFile file)
        {
            string? error = FileValidator.Validate(file);

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Edit), new { id = listingId });
            }

            try
            {
                await connection.AttachmentAdd(listingId, file);
                TempData["Success"] = "Datei hochgeladen.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Edit", new { id = listingId });
        }

        [HttpPost]
        public async Task<IActionResult> AttachmentDelete(int id, int listingId)
        {
            try
            {
                await connection.AttachmentDelete(id);
                TempData["Success"] = "Datei entfernt.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Edit", new { id = listingId });
        }

        [HttpPost]
        public async Task<IActionResult> AttachmentSetTitle(int id, int listingId)
        {
            try
            {
                await connection.AttachmentSetTitle(id);
                TempData["Success"] = "Titelbild geändert.";
            }
            catch (SqlException ex) when (ex.Number >= 50000)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Edit), new { id = listingId });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

 
    }
}
