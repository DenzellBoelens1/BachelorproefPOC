using Webshop.Shared.Models;

namespace Webshop.Client.Layout
{
    public class AppState
    {
        public string SelectedMethod { get; private set; } = "rest";
        public event Action? OnMethodChanged;
        public event Action? OnCartChanged;

        

        public Dictionary<CartKey, int> Cart { get; private set; } = new(); // unieke key -> aantal
        public Dictionary<CartKey, decimal> CartPrices { get; private set; } = new();
        public Dictionary<CartKey, string> CartDescriptions { get; private set; } = new(); // tekstuele beschrijving

        public void SetMethod(string method)
        {
            SelectedMethod = method;
            OnMethodChanged?.Invoke();
        }

        public void AddToCart(int productId, int quantity, decimal unitPrice, Dictionary<string, string> selectedOptions, string customText)
        {
            string signature = GenerateOptionSignature(selectedOptions, customText);
            var key = new CartKey(productId, signature);

            if (Cart.ContainsKey(key))
                Cart[key] += quantity;
            else
                Cart[key] = quantity;

            CartPrices[key] = unitPrice;

            // Beschrijving voor UI
            var descriptionParts = selectedOptions.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToList();
            if (!string.IsNullOrWhiteSpace(customText))
                descriptionParts.Add($"Tekst: {customText}");

            CartDescriptions[key] = string.Join(", ", descriptionParts);

            OnCartChanged?.Invoke();
        }
        private string GenerateOptionSignature(Dictionary<string, string> selectedOptions, string customText)
        {
            var sorted = selectedOptions.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}");
            return string.Join("|", sorted) + $"|text={customText}";
        }

        public void RemoveFromCart(CartKey key)
        {
            Cart.Remove(key);
            CartPrices.Remove(key);
            CartDescriptions.Remove(key);
            OnCartChanged?.Invoke();
        }

        public int GetTotalCartItems() => Cart.Values.Sum();
    }
}
