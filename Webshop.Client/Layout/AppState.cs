using Webshop.Shared.Models;

namespace Webshop.Client.Layout
{
    

    public class AppState
    {
        public string SelectedMethod { get; private set; } = "rest";
        public event Action? OnMethodChanged;
        public event Action? OnCartChanged;

        public Dictionary<CartKey, int> Cart { get; } = new();
        public Dictionary<CartKey, decimal> CartPrices { get; } = new();
        public Dictionary<CartKey, Dictionary<int, string>> CartOptionValues { get; } = new();

        // Veiliger hier dan in URL als parameter voor de paginatie te onthouden.
        public string? LastGraphQLCursor { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public void SetMethod(string method)
        {
            SelectedMethod = method;
            OnMethodChanged?.Invoke();
        }

        public void AddToCart(
            int productId,
            int quantity,
            decimal price,
            IReadOnlyList<int> optionIds,
            Dictionary<int, string> optionValues)
        {
            var key = new CartKey(productId, optionIds);
            Cart[key] = quantity;
            CartPrices[key] = price;
            CartOptionValues[key] = optionValues;
            OnCartChanged?.Invoke();
        }

        public void RemoveFromCart(CartKey key)
        {
            if (Cart.Remove(key) || CartPrices.Remove(key) || CartOptionValues.Remove(key))
            {
                OnCartChanged?.Invoke();
            }
        }

        public void ClearCart()
        {
            Cart.Clear();
            CartPrices.Clear();
            CartOptionValues.Clear();
            OnCartChanged?.Invoke();
        }

        public int GetTotalCartItems() => Cart.Values.Sum();
    }
}
