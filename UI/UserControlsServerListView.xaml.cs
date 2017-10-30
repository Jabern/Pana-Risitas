
using Launcher.Utils;
using MaterialDesignThemes.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Launcher.UI.UserControls
{
  public partial class ServerListView : UserControl, IComponentConnector
  {
    private ConcurrentDictionary<SampUtils.E_TYPE_LIST, Tuple<List<ServerListView.ServerListViewBinding>, SampUtils.E_OPCODE[]>> serverQueue = new ConcurrentDictionary<SampUtils.E_TYPE_LIST, Tuple<List<ServerListView.ServerListViewBinding>, SampUtils.E_OPCODE[]>>();
    public Window owner;
    internal Grid mainLayout;
    internal ListView serverListsView;
    internal ListView playersListView;
    internal ListView rulesListView;
    private bool _contentLoaded;

    public ServerListView()
    {
      this.InitializeComponent();
      Task.Factory.StartNew(new Action(this.processSelectedServerUpdate));
      Task.Factory.StartNew(new Action(this.processQueue));
    }

    private void OnSelectServer(object sender, SelectionChangedEventArgs e)
    {
      this.playersListView.Dispatcher.Invoke((Action) (() => this.playersListView.Items.Clear()));
      this.rulesListView.Dispatcher.Invoke((Action) (() => this.rulesListView.Items.Clear()));
      this.updateSelectedServer();
    }

    private void updateSelectedServer()
    {
      ServerListView.ServerListViewBinding serverData = (ServerListView.ServerListViewBinding) null;
      this.serverListsView.Dispatcher.Invoke((Action) (() => serverData = (ServerListView.ServerListViewBinding) this.serverListsView.SelectedItem));
      if (serverData == null)
        return;
      SampUtils.E_OPCODE[] eOpcodeArray = new SampUtils.E_OPCODE[4]
      {
        SampUtils.E_OPCODE.SERVER_INFORMATION,
        SampUtils.E_OPCODE.SERVER_PING,
        SampUtils.E_OPCODE.SERVER_PLAYERS,
        SampUtils.E_OPCODE.SERVER_RULES
      };
      foreach (SampUtils.E_OPCODE eOpcode in eOpcodeArray)
      {
        SampUtils.E_OPCODE opcode = eOpcode;
        serverData.updateServer(opcode, (Action) null, (Action) (() =>
        {
          switch (opcode)
          {
            case SampUtils.E_OPCODE.SERVER_PLAYERS:
              this.playersListView.Dispatcher.Invoke(closure_0 ?? (closure_0 = (Action) (() =>
              {
                this.playersListView.Items.Clear();
                foreach (object player in serverData.players)
                  this.playersListView.Items.Add(player);
              })));
              break;
            case SampUtils.E_OPCODE.SERVER_RULES:
              this.rulesListView.Dispatcher.Invoke(closure_1 ?? (closure_1 = (Action) (() =>
              {
                this.rulesListView.Items.Clear();
                foreach (object rule in serverData.rules)
                  this.rulesListView.Items.Add(rule);
              })));
              break;
          }
        }));
      }
    }

    private void processSelectedServerUpdate()
    {
      while (true)
      {
        this.updateSelectedServer();
        Thread.Sleep(1000);
      }
    }

    private void processQueue()
    {
      while (true)
      {
        Tuple<List<ServerListView.ServerListViewBinding>, SampUtils.E_OPCODE[]> queueData = (Tuple<List<ServerListView.ServerListViewBinding>, SampUtils.E_OPCODE[]>) null;
        bool queryIsInRightContext = false;
        SampUtils.E_TYPE_LIST typeList = SampUtils.E_TYPE_LIST.LIST_INTERNET;
        this.serverListsView.Dispatcher.Invoke((Action) (() =>
        {
          if (this.mainLayout.Tag == null)
            return;
          queryIsInRightContext = this.serverQueue.TryRemove((SampUtils.E_TYPE_LIST) this.mainLayout.Tag, out queueData);
          if (!queryIsInRightContext)
            return;
          typeList = (SampUtils.E_TYPE_LIST) this.mainLayout.Tag;
        }));
        if (queryIsInRightContext && queueData != null)
        {
          queryIsInRightContext = true;
          foreach (ServerListView.ServerListViewBinding serverListViewBinding in queueData.Item1)
          {
            ServerListView.ServerListViewBinding server = serverListViewBinding;
            this.serverListsView.Dispatcher.Invoke(closure_0 ?? (closure_0 = (Action) (() =>
            {
              if (typeList == (SampUtils.E_TYPE_LIST) this.mainLayout.Tag)
                return;
              queryIsInRightContext = false;
            })));
            if (queryIsInRightContext)
            {
              foreach (SampUtils.E_OPCODE opcode in queueData.Item2)
                server.updateServer(opcode, (Action) (() => this.mainLayout.Dispatcher.Invoke((Action) (() =>
                {
                  if ((SampUtils.E_TYPE_LIST) this.mainLayout.Tag != typeList)
                    return;
                  this.serverListsView.Dispatcher.Invoke((Action) (() =>
                  {
                    if (this.isServerIsOnList(server))
                      return;
                    this.serverListsView.Items.Add((object) server);
                  }));
                }))), (Action) null);
              Thread.Sleep(50);
            }
            else
              break;
          }
        }
        Thread.Sleep(50);
      }
    }

    private bool isServerIsOnList(ServerListView.ServerListViewBinding server)
    {
      bool flag = false;
      foreach (object obj in (IEnumerable) this.serverListsView.Items)
      {
        if (obj != null)
        {
          ServerListView.ServerListViewBinding serverListViewBinding = (ServerListView.ServerListViewBinding) obj;
          if (serverListViewBinding.serverAddress.Equals(server.serverAddress) && serverListViewBinding.serverPort.Equals(server.serverPort))
          {
            flag = true;
            break;
          }
        }
      }
      return flag;
    }

    public void updateWithServersList(string[] serversHostInfos, SampUtils.E_TYPE_LIST typeList, bool showPendingInformation)
    {
      this.serverListsView.Items.Clear();
      this.mainLayout.Tag = (object) typeList;
      this.serverQueue.Clear();
      List<ServerListView.ServerListViewBinding> serverListViewBindingList = new List<ServerListView.ServerListViewBinding>();
      foreach (string serversHostInfo in serversHostInfos)
      {
        string[] strArray = serversHostInfo.Split(':');
        if (strArray.Length == 2)
        {
          ServerListView.ServerListViewBinding serverListViewBinding = new ServerListView.ServerListViewBinding();
          serverListViewBinding.serverAddress = strArray[0];
          serverListViewBinding.serverPort = strArray[1];
          if (showPendingInformation)
          {
            serverListViewBinding.serverName = "(Retrieving info...) " + serversHostInfo;
            serverListViewBinding.serverPlayers = "0/0";
            this.serverListsView.Items.Add((object) serverListViewBinding);
          }
          serverListViewBindingList.Add(serverListViewBinding);
        }
      }
      if (serverListViewBindingList == null || serverListViewBindingList.Count <= 0)
        return;
      Tuple<List<ServerListView.ServerListViewBinding>, SampUtils.E_OPCODE[]> tuple = new Tuple<List<ServerListView.ServerListViewBinding>, SampUtils.E_OPCODE[]>(serverListViewBindingList, new SampUtils.E_OPCODE[2]
      {
        SampUtils.E_OPCODE.SERVER_INFORMATION,
        SampUtils.E_OPCODE.SERVER_PING
      });
      this.serverQueue.TryAdd(typeList, tuple);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Launcher;component/ui/usercontrols/serverlistview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.mainLayout = (Grid) target;
          break;
        case 2:
          this.serverListsView = (ListView) target;
          this.serverListsView.SelectionChanged += new SelectionChangedEventHandler(this.OnSelectServer);
          break;
        case 3:
          this.playersListView = (ListView) target;
          break;
        case 4:
          this.rulesListView = (ListView) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public abstract class IListViewBinding : INotifyPropertyChanged
    {
      public object tag { get; set; }

      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
        // ISSUE: reference to a compiler-generated field
        if (this.PropertyChanged == null)
          return;
        // ISSUE: reference to a compiler-generated field
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public class ServerListViewBinding : ServerListView.IListViewBinding
    {
      private PackIconKind _serverPassword = (PackIconKind) 938;
      private string _serverName = string.Empty;
      private string _serverPlayers = string.Empty;
      private string _serverPing = string.Empty;
      private string _serverMode = string.Empty;
      private string _serverLanguage = string.Empty;
      public string serverAddress = string.Empty;
      public string serverPort = string.Empty;
      public List<ServerListView.RulesListBinding> rules = new List<ServerListView.RulesListBinding>();
      public List<ServerListView.PlayersListBinding> players = new List<ServerListView.PlayersListBinding>();

      public PackIconKind serverPassword
      {
        get
        {
          return this._serverPassword;
        }
        set
        {
          this._serverPassword = value;
          this.OnPropertyChanged(nameof (serverPassword));
        }
      }

      public string serverName
      {
        get
        {
          return this._serverName;
        }
        set
        {
          this._serverName = value;
          this.OnPropertyChanged(nameof (serverName));
        }
      }

      public string serverPlayers
      {
        get
        {
          return this._serverPlayers;
        }
        set
        {
          this._serverPlayers = value;
          this.OnPropertyChanged(nameof (serverPlayers));
        }
      }

      public string serverPing
      {
        get
        {
          return this._serverPing;
        }
        set
        {
          this._serverPing = value;
          this.OnPropertyChanged(nameof (serverPing));
        }
      }

      public string serverMode
      {
        get
        {
          return this._serverMode;
        }
        set
        {
          this._serverMode = value;
          this.OnPropertyChanged(nameof (serverMode));
        }
      }

      public string serverLanguage
      {
        get
        {
          return this._serverLanguage;
        }
        set
        {
          this._serverLanguage = value;
          this.OnPropertyChanged(nameof (serverLanguage));
        }
      }

      public void updateServer(SampUtils.E_OPCODE opcode, Action beforeUpdate = null, Action afterUpdate = null)
      {
        SampQuery sampQuery = new SampQuery((object) null);
        try
        {
          sampQuery.Send(this.serverAddress, int.Parse(this.serverPort), opcode, (Action<string[], object>) ((result, state) =>
          {
            if (beforeUpdate != null)
              beforeUpdate();
            this.__updateValues(result, opcode);
            if (afterUpdate == null)
              return;
            afterUpdate();
          }));
        }
        catch (Exception ex)
        {
          sampQuery.Dispose();
        }
      }

      private void __updateValues(string[] result, SampUtils.E_OPCODE opcode)
      {
        if (opcode <= SampUtils.E_OPCODE.SERVER_INFORMATION)
        {
          if (opcode != SampUtils.E_OPCODE.SERVER_PLAYERS)
          {
            if (opcode != SampUtils.E_OPCODE.SERVER_INFORMATION)
              return;
            this.serverPassword = result[0] == "0" ? (PackIconKind) 938 : (PackIconKind) 937;
            this.serverName = result[3];
            this.serverPlayers = result[1] + " / " + result[2];
            this.serverMode = result[4];
            this.serverLanguage = result[5];
          }
          else
          {
            this.players.Clear();
            int index = 0;
            while (index < result.Length - 1)
            {
              if (int.Parse(result[index + 1]) < 0)
                result[index + 1] = "0";
              this.players.Add(new ServerListView.PlayersListBinding()
              {
                playerName = result[index],
                playerLevel = result[index + 1]
              });
              index += 2;
            }
          }
        }
        else if (opcode != SampUtils.E_OPCODE.SERVER_PING)
        {
          if (opcode != SampUtils.E_OPCODE.SERVER_RULES)
            return;
          this.rules.Clear();
          int index = 0;
          while (index < result.Length - 1)
          {
            this.rules.Add(new ServerListView.RulesListBinding()
            {
              ruleName = result[index],
              ruleValue = result[index + 1]
            });
            index += 2;
          }
        }
        else
          this.serverPing = result[0];
      }
    }

    public class PlayersListBinding : ServerListView.IListViewBinding
    {
      private string _playerName = string.Empty;
      private string _playerLevel = string.Empty;

      public string playerName
      {
        get
        {
          return this._playerName;
        }
        set
        {
          this._playerName = value;
          this.OnPropertyChanged(nameof (playerName));
        }
      }

      public string playerLevel
      {
        get
        {
          return this._playerLevel;
        }
        set
        {
          this._playerLevel = value;
          this.OnPropertyChanged(nameof (playerLevel));
        }
      }
    }

    public class RulesListBinding : ServerListView.IListViewBinding
    {
      private string _ruleName = string.Empty;
      private string _ruleValue = string.Empty;

      public string ruleName
      {
        get
        {
          return this._ruleName;
        }
        set
        {
          this._ruleName = value;
          this.OnPropertyChanged(nameof (ruleName));
        }
      }

      public string ruleValue
      {
        get
        {
          return this._ruleValue;
        }
        set
        {
          this._ruleValue = value;
          this.OnPropertyChanged(nameof (ruleValue));
        }
      }
    }
  }
}
