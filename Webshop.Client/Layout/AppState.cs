namespace Webshop.Client.Layout
{
    public class AppState
    {
        public string SelectedMethod { get; private set; } = "rest";

        public event Action? OnMethodChanged;

        public void SetMethod(string method)
        {
            SelectedMethod = method;
            OnMethodChanged?.Invoke();
        }
    }
}
