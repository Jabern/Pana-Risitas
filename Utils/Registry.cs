

namespace Launcher.Utils
{
  internal class Registry
  {
    private static string REGISTRY_PATH = "HKEY_CURRENT_USER\\SOFTWARE\\PanaMonde";

    public static string getValue(string key)
    {
      string empty = string.Empty;
      try
      {
        return (string) Microsoft.Win32.Registry.GetValue(Registry.REGISTRY_PATH, key, (object) string.Empty);
      }
      catch
      {
        return string.Empty;
      }
    }

    public static bool setValue(string key, string value)
    {
      try
      {
        Microsoft.Win32.Registry.SetValue(Registry.REGISTRY_PATH, key, (object) value);
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
