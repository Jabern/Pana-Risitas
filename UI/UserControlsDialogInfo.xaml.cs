
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace Launcher.UI.UserControls
{
  public partial class DialogInfo : Window, IComponentConnector
  {
    internal TextBlock title;
    internal TextBlock description;
    private bool _contentLoaded;

    public DialogInfo(string title, string description)
    {
      this.InitializeComponent();
      this.title.Text = title;
      this.description.Text = description;
      this.MouseDown += new MouseButtonEventHandler(this.onWindowMouseDown);
    }

    private void onClickOnValidButton(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void onWindowMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left)
        return;
      this.DragMove();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Launcher;component/ui/usercontrols/dialoginfo.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.title = (TextBlock) target;
          break;
        case 2:
          this.description = (TextBlock) target;
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.onClickOnValidButton);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
