

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace Launcher
{
  public class App : Application, IComponentConnector
  {
    private bool _contentLoaded;

    private App()
    {
      this.InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
      {
        int num = (int) MessageBox.Show("Vous ne pouvez lancer qu'une seule instance du launcher PanaMonde.", "Impossible de lancer le launcher", MessageBoxButton.OK, MessageBoxImage.Hand);
        Application.Current.Shutdown();
      }
      else
        base.OnStartup(e);
    }

    [STAThread]
    public static void Main()
    {
      AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(App.OnResolveAssembly);
      App app = new App();
      try
      {
        app.InitializeComponent();
        app.Run();
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.ToString() + "\n\n\nPrenez un screenshot et envoyez-le à l'administration.\nToutes nos excuses pour la gêne occasionnée, l'équipe de PanaMonde", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Hand);
        app.Shutdown();
      }
    }

    private static Assembly OnResolveAssembly(object sender, ResolveEventArgs e)
    {
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      string dllName = new AssemblyName(e.Name).Name + ".dll";
      IEnumerable<string> source = ((IEnumerable<string>) executingAssembly.GetManifestResourceNames()).Where<string>((Func<string, bool>) (s => s.EndsWith(dllName)));
      if (!source.Any<string>())
        return (Assembly) null;
      string name = source.First<string>();
      using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name))
      {
        if (manifestResourceStream == null)
          return (Assembly) null;
        byte[] numArray = new byte[manifestResourceStream.Length];
        try
        {
          manifestResourceStream.Read(numArray, 0, numArray.Length);
          return Assembly.Load(numArray);
        }
        catch (IOException ex)
        {
          return (Assembly) null;
        }
        catch (BadImageFormatException ex)
        {
          return (Assembly) null;
        }
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Launcher;component/ui/app.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      this._contentLoaded = true;
    }
  }
}
