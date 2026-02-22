using Microsoft.AspNetCore.Mvc.RazorPages;
using WeddingShare.Models.Database;

namespace WeddingShare.Views.Account.Tabs
{
    public class UsersModel : PageModel
    {
        public UsersModel() 
        {
        }

        public List<UserModel>? Users { get; set; }
        public int TotalItems { get; set; } = 0;
        public int TotalItemsPerPage { get; set; } = 50;

        public void OnGet()
        {
        }
    }
}