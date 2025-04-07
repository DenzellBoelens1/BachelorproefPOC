using Microsoft.AspNetCore.Components;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;

namespace Webshop.Client.Pages
{
    public partial class ProductDetail
    {
        [Parameter] public int id { get; set; }
        ProductDTO.Index? product;

        protected override async Task OnInitializedAsync()
        {
            product = await RestService.GetProduct(id);
        }
    }
}