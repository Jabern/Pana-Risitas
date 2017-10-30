

using Launcher.Utils;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;

namespace Launcher.UI
{
  public partial class SettingsWindow : Window, IComponentConnector
  {
    internal System.Windows.Controls.TextBox textBoxGTADirectory;
    internal System.Windows.Controls.TextBox textBoxModsDirectory;
    private bool _contentLoaded;

    public SettingsWindow()
    {
      this.InitializeComponent();
    }

    private void onWindowLoaded(object sender, RoutedEventArgs e)
    {
      this.textBoxGTADirectory.Text = Registry.getValue("gta_sa");
      this.textBoxModsDirectory.Text = Registry.getValue("mods");
    }

    private void onWindowMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left)
        return;
      this.DragMove();
    }

    private void onClickOnQuitButton(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void onClickOnGTAFolder(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.CheckFileExists = true;
      openFileDialog.CheckPathExists = true;
      openFileDialog.Title = "Chemin d'accès vers gta_sa.exe";
      openFileDialog.Multiselect = false;
      openFileDialog.FileName = "gta_sa*";
      openFileDialog.Filter = "Exe Files (.exe)|*.exe";
      openFileDialog.FilterIndex = 1;
      if (openFileDialog.ShowDialog() != DialogResult.OK)
        return;
      string str = openFileDialog.FileName.Replace("\\gta_sa.exe", "");
      Registry.setValue("gta_sa", str);
      this.textBoxGTADirectory.Text = str;
    }

    private void onClickOnModsFolder(object sender, RoutedEventArgs e)
    {
      FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
      if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
        return;
      string selectedPath = folderBrowserDialog.SelectedPath;
      Registry.setValue("mods", selectedPath);
      this.textBoxModsDirectory.Text = selectedPath;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/Launcher;component/ui/settingswindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseDown += new MouseButtonEventHandler(this.onWindowMouseDown);
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.onWindowLoaded);
          break;
        case 2:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.onClickOnQuitButton);
          break;
        case 3:
          this.textBoxGTADirectory = (System.Windows.Controls.TextBox) target;
          break;
        case 4:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.onClickOnGTAFolder);
          break;
        case 5:
          this.textBoxModsDirectory = (System.Windows.Controls.TextBox) target;
          break;
        case 6:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.onClickOnModsFolder);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
