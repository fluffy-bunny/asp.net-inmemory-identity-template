using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jsonplaceholder.service;
using jsonplaceholder.service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InMemoryIdentityApp.Pages
{
 
    public class JsonPlaceHolderModel : PageModel
    {
        private IJsonPlaceholderService _jsonPlaceholderService;

        public JsonPlaceHolderModel(IJsonPlaceholderService jsonPlaceholderService)
        {
            _jsonPlaceholderService = jsonPlaceholderService;
        }

        public IEnumerable<User> Users { get; private set; }

        public async Task OnGetAsync()
        {
            Users = await _jsonPlaceholderService.GetUsersAsync();
        }
    }
}
