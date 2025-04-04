using Microsoft.AspNetCore.Components;
using Webshop.Shared.Models;

namespace Webshop.Client.Pages
{
    public partial class ProductDetail
    {
        [Parameter] public int id { get; set; }
        Product? product;

        protected override async Task OnInitializedAsync()
        {
            product = await RestService.GetProduct(id);
        }
    }
}