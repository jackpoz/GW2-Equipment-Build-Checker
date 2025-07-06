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

    private void InitialLook()
    {
        CharacterComboBox.Visibility = Visibility.Collapsed;
        CharacterComboBox.ItemsSource = null;

        BuildComboBox.Visibility = Visibility.Collapsed;
        BuildComboBox.ItemsSource = null;

        BuildUrlTextBox.Visibility = Visibility.Collapsed;

        CompareButton.Visibility = Visibility.Collapsed;

        DifferencesTextBlock.Visibility = Visibility.Collapsed;
        DifferencesTextBlock.Text = "";
    }

    private async void LoadGW2Button_Click(object sender, RoutedEventArgs e)
    {
        InitialLook();

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
        CharacterComboBox.SelectedIndex = -1;
        CharacterComboBox.SelectedValue = null;
        CharacterComboBox.ItemsSource = characterNames;
        CharacterComboBox.Visibility = Visibility.Visible;
    }

    private async void CharacterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BuildComboBox.Visibility = Visibility.Collapsed;
        BuildComboBox.ItemsSource = null;

        BuildUrlTextBox.Visibility = Visibility.Collapsed;

        CompareButton.Visibility = Visibility.Collapsed;

        DifferencesTextBlock.Visibility = Visibility.Collapsed;
        DifferencesTextBlock.Text = "";

        if (CharacterComboBox.SelectedItem == null)
        {
            return;
        }

        var selectedCharacterName = CharacterComboBox.SelectedItem.ToString();
        var builds = await GW2API.GetBuildsAsync(selectedCharacterName);

        BuildComboBox.SelectedIndex = -1;
        BuildComboBox.SelectedValue = null;
        BuildComboBox.ItemsSource = builds;
        BuildComboBox.Visibility = Visibility.Visible;
    }
}
