using GW2EquipmentBuildChecker.Core;
using GW2EquipmentBuildChecker.Core.GW2;
using GW2EquipmentBuildChecker.Core.GW2.Entities.Characters;
using GW2EquipmentBuildChecker.Core.GW2Skills;
using System.Text;

namespace GW2EquipmentBuildChecker.Uno;

public sealed partial class MainPage : Page
{
    GW2API GW2API;
#if __WASM__
    GW2SkillsAPI GW2SkillsAPI = new GW2SkillsAPI("https://gw2equipmentbuilderchecker-apimgmt.azure-api.net/cors");
#else
    GW2SkillsAPI GW2SkillsAPI = new GW2SkillsAPI();
#endif

    private bool Processing
    {
        set
        {
            ProcessingPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public MainPage()
    {
        InitializeComponent();
        InitialLook();

        if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(ApiKeyTextBox), out object apiKey))
        {
            ApiKeyTextBox.Text = apiKey.ToString();
        }
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

        Processing = false;
    }

    private async void LoadGW2Button_Click(object sender, RoutedEventArgs e)
    {
        await Utilities.TryExecute(async () =>
        {
            InitialLook();

            if (string.IsNullOrEmpty(ApiKeyTextBox.Text))
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Title = "API Key Required",
                    Content = "Please enter your GW2 API key.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
                return;
            }

            using var loader = new Loader(this);

            ApplicationData.Current.LocalSettings.Values[nameof(ApiKeyTextBox)] = ApiKeyTextBox.Text;

            GW2API = new GW2API(ApiKeyTextBox.Text);

            var characterNames = await GW2API.GetCharactersNamesAsync();
            CharacterComboBox.SelectedIndex = -1;
            CharacterComboBox.SelectedValue = null;
            CharacterComboBox.ItemsSource = characterNames;
            CharacterComboBox.Visibility = Visibility.Visible;
        });
    }

    private async void CharacterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await Utilities.TryExecute(async () =>
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

            using var loader = new Loader(this);

            var selectedCharacterName = CharacterComboBox.SelectedItem.ToString();
            var builds = await GW2API.GetBuildsAsync(selectedCharacterName);

            BuildComboBox.SelectedIndex = -1;
            BuildComboBox.SelectedValue = null;
            BuildComboBox.ItemsSource = builds;
            BuildComboBox.Visibility = Visibility.Visible;
        });
    }

    private async void BuildComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await Utilities.TryExecute(() =>
        {
            DifferencesTextBlock.Visibility = Visibility.Collapsed;
            DifferencesTextBlock.Text = "";

            if (BuildComboBox.SelectedItem == null)
            {
                BuildUrlTextBox.Visibility = Visibility.Collapsed;

                CompareButton.Visibility = Visibility.Collapsed;
                return;
            }

            BuildUrlTextBox.Visibility = Visibility.Visible;

            CompareButton.Visibility = Visibility.Visible;
        });
    }

    private async void CompareButton_Click(object sender, RoutedEventArgs e)
    {
        await Utilities.TryExecute(async () =>
        {
            DifferencesTextBlock.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(BuildUrlTextBox.Text))
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Title = "GW2Skills.net Build URL Required",
                    Content = "Please enter a GW2Skills.net build URL.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
                return;
            }

            using var loader = new Loader(this);

            var gw2skillsBuild = await GW2SkillsAPI.GetBuildAsync(BuildUrlTextBox.Text);

            // 6. Compare and find differences
            var buildDifferences = await BuildComparer.CompareBuilds(((BuildContainer)(BuildComboBox.SelectedItem)).Build, gw2skillsBuild);

            var differencesText = new StringBuilder();
            // 7. Tell what to change
            if (buildDifferences.Count == 0)
            {
                differencesText.AppendLine("No differences found");
            }
            else
            {
                foreach (var difference in buildDifferences)
                {
                    differencesText.AppendLine(difference);
                    differencesText.AppendLine();
                }
            }

            DifferencesTextBlock.Text = differencesText.ToString();
            DifferencesTextBlock.Visibility = Visibility.Visible;
        });
    }

    private class Loader : IDisposable
    {
        private MainPage _mainPage;

        public Loader(MainPage mainPage)
        {
            _mainPage = mainPage;
            _mainPage.Processing = true;
        }

        public void Dispose()
        {
            _mainPage.Processing = false;
        }
    }
}
