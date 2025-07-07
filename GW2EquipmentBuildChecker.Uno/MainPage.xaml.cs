using GW2EquipmentBuildChecker.Core;
using GW2EquipmentBuildChecker.Core.GW2;
using GW2EquipmentBuildChecker.Core.GW2.Entities.Characters;
using GW2EquipmentBuildChecker.Core.GW2Skills;
using System.Text;

namespace GW2EquipmentBuildChecker.Uno;

public sealed partial class MainPage : Page
{
    GW2API GW2API;
    GW2SkillsAPI GW2SkillsAPI = new GW2SkillsAPI();

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

    private void BuildComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
    }

    private async void CompareButton_Click(object sender, RoutedEventArgs e)
    {
        DifferencesTextBlock.Visibility = Visibility.Collapsed;

        if (string.IsNullOrEmpty(BuildUrlTextBox.Text))
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "GW2Skills.net Build URL Required",
                Content = "Please enter a GW2Skills.net build URL.",
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
            return;
        }

        var gw2skillsBuild = await GW2SkillsAPI.GetBuildAsync(BuildUrlTextBox.Text);

        // 6. Compare and find differences
        var buildDifferences = BuildComparer.CompareBuilds(((BuildContainer)(BuildComboBox.SelectedItem)).Build, gw2skillsBuild);

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
            }
        }

        DifferencesTextBlock.Text = differencesText.ToString();
        DifferencesTextBlock.Visibility = Visibility.Visible;
    }
}
