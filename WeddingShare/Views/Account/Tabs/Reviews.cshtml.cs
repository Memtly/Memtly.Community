using Microsoft.AspNetCore.Mvc.RazorPages;
using WeddingShare.Models;

namespace WeddingShare.Views.Account.Tabs
{
    public class ReviewsModel : PageModel
    {
        public ReviewsModel()
        {
        }

        public List<PhotoGallery>? PendingRequests { get; set; }
        public int TotalItems { get; set; } = 0;
        public int TotalItemsPerPage { get; set; } = 50;

        public void OnGet()
        {
        }
    }
}