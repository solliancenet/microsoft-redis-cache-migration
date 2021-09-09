using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RedisWeb.Pages
{
    public class IndexData
    {
        [Required, StringLength(20)]
        public string Message { get; set; } = "";
    }
    
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public IndexData IndexData { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var data = CacheHelper.GetSetData("RedisWeb.index.message", this.IndexData.Message);

            return RedirectToPage("./Index");
        }

        public void OnGet()
        {
            string data = CacheHelper.GetData<string>("RedisWeb.index.message");

            if (data == null)
            {
                
            }
            else
            {
                if (IndexData == null)
                    IndexData = new IndexData();

                IndexData.Message = data;
            }
        }
    }
}
