namespace GW2EquipmentBuildChecker.Uno
{
    public static class Utilities
    {
        public static async Task TryExecute(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    XamlRoot = Window.Current.Content.XamlRoot,
                    Title = "Error",
                    Content = ex.Message + Environment.NewLine + ex.StackTrace,
                    CloseButtonText = "OK"
                }.ShowAsync();
            }
        }

        public static async Task TryExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    XamlRoot = Window.Current.Content.XamlRoot,
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK"
                }.ShowAsync();
            }
        }
    }
}
