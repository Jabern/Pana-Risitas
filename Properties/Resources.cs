
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Launcher.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  public class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static ResourceManager ResourceManager
    {
      get
      {
        if (Launcher.Properties.Resources.resourceMan == null)
          Launcher.Properties.Resources.resourceMan = new ResourceManager("Launcher.Properties.Resources", typeof (Launcher.Properties.Resources).Assembly);
        return Launcher.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static CultureInfo Culture
    {
      get
      {
        return Launcher.Properties.Resources.resourceCulture;
      }
      set
      {
        Launcher.Properties.Resources.resourceCulture = value;
      }
    }

    public static Bitmap icon
    {
      get
      {
        return (Bitmap) Launcher.Properties.Resources.ResourceManager.GetObject(nameof (icon), Launcher.Properties.Resources.resourceCulture);
      }
    }
  }
}
