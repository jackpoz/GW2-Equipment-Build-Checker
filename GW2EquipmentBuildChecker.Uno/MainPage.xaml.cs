using GW2EquipmentBuildChecker.Core.GW2;

namespace GW2EquipmentBuildChecker.Uno;

public sealed partial class MainPage : Page
{
    GW2API GW2API;

    public MainPage()
    {
        InitializeComponent();
        InitialLook();
    }

    public void InitialLook()
    {
        CharacterComboBox.Visibility = Visibility.Collapsed;
        CharacterComboBox.Items.Clear();

        BuildComboBox.Visibility = Visibility.Collapsed;
        BuildComboBox.Items.Clear();

        BuildUrlTextBox.Visibility = Visibility.Collapsed;

        CompareButton.Visibility = Visibility.Collapsed;

        DifferencesTextBlock.Visibility = Visibility.Collapsed;
        DifferencesTextBlock.Text = "";
    }

    public async void LoadGW2Button_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ApiKeyTextBox.Text))
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "API Key Required",
                Content = "Please enter your GW2 API key.",
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
            return;
        }

        GW2API = new GW2API(this.ApiKeyTextBox.Text);

        var characterNames = await GW2API.GetCharactersNamesAsync();
        CharacterComboBox.Items.Clear();
        CharacterComboBox.SelectedIndex = -1;
        CharacterComboBox.SelectedValue = null;
        CharacterComboBox.ItemsSource = characterNames;
        CharacterComboBox.Visibility = Visibility.Visible;
    }
}
