using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Transport_Management_System.Services;

namespace Transport_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // GET: /Search?query=...
        public async Task<IActionResult> Index(string query)
        {
            ViewData["Title"] = $"Search Results for: \"{query}\"";
            var results = await _searchService.SearchAllAsync(query);
            return View(results);
        }

        // GET: /Search/Suggestions?query=...
        [HttpGet]
        public async Task<IActionResult> Suggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new object[] { });
            }

            var suggestions = await _searchService.GetSearchSuggestionsAsync(query);
            return Json(suggestions);
        }
    }
}
