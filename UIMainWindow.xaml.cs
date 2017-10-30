

using ControlzEx;
using Launcher.UI.UserControls;
using Launcher.Utils;
using MaterialDesignThemes.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Launcher.UI
{
  public partial class MainWindow : Window, IComponentConnector
  {
    internal Image logo;
    internal Button btnConnect;
    internal Button btnAddFavorites;
    internal Button btnRemoveFavorites;
    internal Button btnSettings;
    internal TextBox textBoxUserName;
    internal ServerListView serversListsView;
    internal Button btnFavorites;
    internal PackIcon gameIndicator;
    internal ProgressBar progressBarDownload;
    private bool _contentLoaded;

    public MainWindow()
    {
      this.InitializeComponent();
      this.btnFavorites.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
      this.textBoxUserName.Text = Registry.getValue("username");
      this.serversListsView.owner = (Window) this;
      BitmapFrame bitmapFrame = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("Launcher.Resources.icon.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
      this.logo.Source = (ImageSource) bitmapFrame;
      this.Icon = (ImageSource) bitmapFrame;
    }

    private void onUsernameHasBeenChanged(object sender, RoutedEventArgs e)
    {
      Registry.setValue("username", this.textBoxUserName.Text);
    }

    private async void onClickBtnConnect(object sender, RoutedEventArgs e)
    {
      if (Registry.getValue("mods") == null || Registry.getValue("gta_sa") == null || (Registry.getValue("gta_sa") == string.Empty || Registry.getValue("mods") == string.Empty))
      {
        SettingsWindow settingsWindow = new SettingsWindow();
        settingsWindow.Owner = (Window) this;
        settingsWindow.ShowDialog();
      }
      else
      {
        ServerListView.ServerListViewBinding selectedItem = (ServerListView.ServerListViewBinding) this.serversListsView.serverListsView.SelectedItem;
        if (selectedItem == null)
          return;
        string password = string.Empty;
        if (selectedItem.serverPassword == 937)
        {
          DialogInput dialogInput = new DialogInput("Mot de passe", "Un mot de passe est nécessaire pour se connecter au serveur:");
          dialogInput.Owner = (Window) this;
          try
          {
            dialogInput.ShowDialog();
            string result = dialogInput.result;
            if (!(result != string.Empty))
              return;
            password = result;
          }
          catch
          {
          }
          finally
          {
            dialogInput.Close();
          }
        }
        this.btnConnect.IsEnabled = false;
        this.btnSettings.IsEnabled = false;
        SampUtils.ServerHost serverHost = new SampUtils.ServerHost(selectedItem.serverAddress, selectedItem.serverPort);
        ServerMods serverMods = new ServerMods(serverHost);
        ServerMods.SERVER_REQUIRES_MODS serverRequiresMods = await serverMods.isServerRequiresMods();
        if (serverRequiresMods == ServerMods.SERVER_REQUIRES_MODS.OPTIONAL)
        {
          DialogAsk dialogAsk = new DialogAsk("Gestion des mods", "Des mods sont disponible pour ce serveur mais ils sont optionnels, souhaitez-vous les utiliser ?");
          dialogAsk.Owner = (Window) this;
          dialogAsk.ShowDialog();
          serverRequiresMods = !dialogAsk.result ? ServerMods.SERVER_REQUIRES_MODS.NO : ServerMods.SERVER_REQUIRES_MODS.YES;
          dialogAsk.Close();
        }
        if (serverRequiresMods == ServerMods.SERVER_REQUIRES_MODS.YES)
        {
          if (!await serverMods.isPlayerHasMods())
          {
            DialogVersionUpdate dialog = new DialogVersionUpdate("Mise à jour", await serverMods.getUrlToVersionDescription());
            dialog.Owner = (Window) this;
            try
            {
              dialog.ShowDialog();
              bool flag = dialog.result;
              if (flag)
                flag = await serverMods.loadModsFromServer();
              if (flag)
              {
                this.progressBarDownload.IsIndeterminate = true;
                int num = await serverMods.downloadLoadedMods() ? 1 : 0;
                this.progressBarDownload.IsIndeterminate = false;
              }
            }
            catch
            {
            }
            finally
            {
              dialog.Close();
            }
            dialog = (DialogVersionUpdate) null;
          }
          else
          {
            int num1 = await serverMods.loadModsFromLocal() ? 1 : 0;
          }
          serverMods.extractLoadedMods();
        }
        ((PackIconBase<PackIconKind>) this.gameIndicator).set_Kind((PackIconKind) 44);
        ((Control) this.gameIndicator).Foreground = (Brush) Brushes.OrangeRed;
        string username = this.textBoxUserName.Text;
        new Task((Action) (() =>
        {
          Game.LaunchGTAInjected(serverHost.address, serverHost.port, username, password, false, "\\samp.dll");
          serverMods.cleanLoadedMods();
          ((DispatcherObject) this.gameIndicator).Dispatcher.Invoke((Action) (() =>
          {
            ((PackIconBase<PackIconKind>) this.gameIndicator).set_Kind((PackIconKind) 332);
            ((Control) this.gameIndicator).Foreground = (Brush) Brushes.Green;
          }));
          this.btnConnect.Dispatcher.Invoke((Action) (() => this.btnConnect.IsEnabled = true));
          this.btnSettings.Dispatcher.Invoke((Action) (() => this.btnSettings.IsEnabled = true));
        })).Start();
      }
    }

    private void onClickBtnSettings(object sender, RoutedEventArgs e)
    {
      SettingsWindow settingsWindow = new SettingsWindow();
      settingsWindow.Owner = (Window) this;
      settingsWindow.ShowDialog();
    }

    private void onClickBtnAddFavorites(object sender, RoutedEventArgs e)
    {
      DialogInput dialogInput = new DialogInput("Ajout de favoris", "Entrez l'ip du serveur :");
      dialogInput.Owner = (Window) this;
      try
      {
        dialogInput.ShowDialog();
        string result = dialogInput.result;
        if (!(result != string.Empty))
          return;
        string[] strArray = result.Split(':');
        if (strArray.Length != 2)
          return;
        strArray[0] = SampQuery.GetServerAddressByDomain(strArray[0]);
        if (!(strArray[0] != string.Empty))
          return;
        FavoritesFile.AddServer(strArray[0] + ":" + strArray[1]);
        this.btnFavorites.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
      }
      catch
      {
      }
      finally
      {
        dialogInput.Close();
      }
    }

    private void onClickBtnRemoveFavorites(object sender, RoutedEventArgs e)
    {
      ServerListView.ServerListViewBinding selectedItem = (ServerListView.ServerListViewBinding) this.serversListsView.serverListsView.SelectedItem;
      if (selectedItem != null)
        FavoritesFile.RemoveServer(selectedItem.serverAddress + ":" + selectedItem.serverPort);
      this.btnFavorites.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }

    private async void onTabSelected(object sender, RoutedEventArgs e)
    {
      SampUtils.E_TYPE_LIST typeList = (SampUtils.E_TYPE_LIST) int.Parse((string) (sender as Button).Tag);
      bool showPendingInformation = false;
      if (typeList == SampUtils.E_TYPE_LIST.LIST_FAVOURITES)
      {
        showPendingInformation = true;
        this.btnAddFavorites.IsEnabled = true;
        this.btnRemoveFavorites.IsEnabled = true;
      }
      else
      {
        this.btnAddFavorites.IsEnabled = false;
        this.btnRemoveFavorites.IsEnabled = false;
      }
      string[] remoteServerList = await SampUtils.GetRemoteServerList(typeList);
      if (remoteServerList == null)
        return;
      this.serversListsView.updateWithServersList(remoteServerList, typeList, showPendingInformation);
    }

    private void onClickBtnWebSite(object sender, RoutedEventArgs e)
    {
      Process.Start((string) ((FrameworkElement) sender).ToolTip);
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

    private void onClickOnMinimizeButton(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Minimized;
    }

    private void onClickOnMaximizeAndRestoreButton(object sender, RoutedEventArgs e)
    {
      this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void onMouseDoubleClickOnWindowHeader(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Maximized;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Launcher;component/ui/mainwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
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
          break;
        case 2:
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.onMouseDoubleClickOnWindowHeader);
          break;
        case 3:
          this.logo = (Image) target;
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.onClickOnQuitButton);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.onClickOnMaximizeAndRestoreButton);
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.onClickOnMinimizeButton);
          break;
        case 7:
          this.btnConnect = (Button) target;
          this.btnConnect.Click += new RoutedEventHandler(this.onClickBtnConnect);
          break;
        case 8:
          this.btnAddFavorites = (Button) target;
          this.btnAddFavorites.Click += new RoutedEventHandler(this.onClickBtnAddFavorites);
          break;
        case 9:
          this.btnRemoveFavorites = (Button) target;
          this.btnRemoveFavorites.Click += new RoutedEventHandler(this.onClickBtnRemoveFavorites);
          break;
        case 10:
          this.btnSettings = (Button) target;
          this.btnSettings.Click += new RoutedEventHandler(this.onClickBtnSettings);
          break;
        case 11:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.onClickBtnWebSite);
          break;
        case 12:
          this.textBoxUserName = (TextBox) target;
          this.textBoxUserName.TextChanged += new TextChangedEventHandler(this.onUsernameHasBeenChanged);
          break;
        case 13:
          this.serversListsView = (ServerListView) target;
          break;
        case 14:
          this.btnFavorites = (Button) target;
          this.btnFavorites.Click += new RoutedEventHandler(this.onTabSelected);
          break;
        case 15:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.onTabSelected);
          break;
        case 16:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.onTabSelected);
          break;
        case 17:
          this.gameIndicator = (PackIcon) target;
          break;
        case 18:
          this.progressBarDownload = (ProgressBar) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
