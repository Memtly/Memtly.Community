using Microsoft.AspNetCore.Mvc.RazorPages;
using WeddingShare.Models.Database;

namespace WeddingShare.Views.Account.Tabs
{
    public class ResourcesModel : PageModel
    {
        public ResourcesModel() 
        {
        }

        public List<CustomResourceModel>? CustomResources { get; set; }
        public int TotalItems { get; set; } = 0;
        public int TotalItemsPerPage { get; set; } = 50;

        public void OnGet()
        {
        }
    }
}