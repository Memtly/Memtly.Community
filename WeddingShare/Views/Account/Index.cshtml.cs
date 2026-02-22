using Microsoft.AspNetCore.Mvc.RazorPages;
using WeddingShare.Enums;
using WeddingShare.Models;
using WeddingShare.Models.Database;

namespace WeddingShare.Views.Account
{
    public class IndexModel : PageModel
    {
        public IndexModel() 
        {
        }

        public AccountTabs ActiveTab { get; set; } = AccountTabs.Reviews;
        public List<PhotoGallery>? PendingRequests { get; set; }
        public List<UserModel>? Users { get; set; }
        public List<GalleryModel>? Galleries { get; set; }
        public List<CustomResourceModel>? CustomResources { get; set; }
        public IEnumerable<AuditLogModel>? AuditLogs { get; set; }
        public IDictionary<string, string>? Settings { get; set; }

        public int TotalItems { get; set; } = 0;
        public int TotalItemsPerPage { get; set; } = 50;

        public void OnGet()
        {
        }
    }
}