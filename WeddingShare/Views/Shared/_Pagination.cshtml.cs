using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WeddingShare.Views.Shared
{
    public class PaginationModel : PageModel
    {
        public void OnGet()
        {
        }

        public int TotalItems { get; set; } = 0;
        public int ItemsPerPage { get; set; } = 5;
        public int CurrentPage { get; set; } = 1;
        public int PagesToShow { get; set; } = 9;

        public int StartPage
        {
            get
            {
                var value = 0;

                var pagesToShow = this.PagesToShow % 2 == 1 ? this.PagesToShow : this.PagesToShow - 1;
                if (pagesToShow > 1)
                {
                    var pageSplit = (int)Math.Floor((double)(pagesToShow - 1) / 2.0);

                    if (this.CurrentPage + pageSplit > this.LastPage)
                    {
                        value = this.LastPage - (pagesToShow - 1);
                    }
                    else
                    { 
                        value = this.CurrentPage - pageSplit;
                    }
                }

                return value >= this.FirstPage ? value : this.FirstPage;
            }
        }

        public int EndPage
        {
            get
            {
                var value = 0;

                var pagesToShow = this.PagesToShow % 2 == 1 ? this.PagesToShow : this.PagesToShow - 1;
                if (pagesToShow > 1)
                {
                    var pageSplit = (int)Math.Floor((double)(pagesToShow - 1) / 2.0);

                    if (this.CurrentPage - pageSplit < this.StartPage)
                    {
                        value = this.StartPage + (pagesToShow - 1);
                    }
                    else
                    {
                        value = this.CurrentPage + pageSplit;
                    }
                }

                return value <= this.LastPage ? value : this.LastPage;
            }
        }

        public int FirstPage
        {
            get
            {
                return 1;
            }
        }

        public int LastPage
        {
            get
            {
                return (int)Math.Ceiling((double)this.TotalItems / (double)this.ItemsPerPage);
            }
        }
    }
}