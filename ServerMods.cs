

using Launcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Launcher
{
  internal class ServerMods
  {
    private List<Mod> _modsList = new List<Mod>();
    private string remoteServerName = string.Empty;
    public const string pathToServer = "http://lm-rp.fr/launcher/";
    private const string localVersionFileName = ".version";
    private SampUtils.ServerHost serverHost;

    public ServerMods(SampUtils.ServerHost serverHost)
    {
      this.serverHost = serverHost;
    }

    public bool cleanLoadedMods()
    {
      try
      {
        foreach (Mod mods in this._modsList)
        {
          if (mods.getModState() == Mod.MOD_STATE.MOD_EXTRACTED)
            mods.deleteExtractedFilesAndDirectories();
        }
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool extractLoadedMods()
    {
      try
      {
        string pathDestination = Registry.getValue("gta_sa");
        foreach (Mod mods in this._modsList)
        {
          if (mods.getModState() == Mod.MOD_STATE.MOD_DOWNLOADED && !mods.extractMod(pathDestination))
            return false;
        }
        return true;
      }
      catch
      {
        return false;
      }
    }

    public async Task<bool> downloadLoadedMods()
    {
      bool flag;
      try
      {
        string destinationPath = await this.getLocalServerPath();
        if (Directory.Exists(destinationPath))
        {
          DirectoryInfo directoryInfo = new DirectoryInfo(destinationPath);
          foreach (FileInfo file in directoryInfo.GetFiles())
          {
            File.SetAttributes(file.FullName, FileAttributes.Normal);
            file.Delete();
          }
          foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            directory.Delete(true);
        }
        foreach (Mod mods in this._modsList)
        {
          if (!await mods.downloadAndSaveMod(destinationPath))
            return false;
        }
        List<Mod>.Enumerator enumerator = new List<Mod>.Enumerator();
        this.createVersionFile((await this.getRemoteVersion()).ToString());
        this.createHiddenHashFile(await this.getLocalModsHash());
        flag = true;
        destinationPath = (string) null;
      }
      catch
      {
        flag = false;
      }
      return flag;
    }

    public async Task<string> getUrlToVersionDescription()
    {
      return "http://lm-rp.fr/launcher/servers/" + await this.getServerName() + "/update.html";
    }

    public async Task<bool> loadModsFromLocal()
    {
      bool returnValue = false;
      try
      {
        string localServerPath = await this.getLocalServerPath();
        foreach (string file in Directory.GetFiles(localServerPath))
        {
          if (file.EndsWith(".mod"))
            this._modsList.Add(new Mod(Path.Combine(localServerPath, file)));
        }
        returnValue = true;
      }
      catch
      {
      }
      return returnValue;
    }

    public async Task<bool> loadModsFromServer()
    {
      bool returnValue = false;
      Downloader downloader = new Downloader();
      try
      {
        string serverName = await this.getServerName();
        if (await downloader.downloadAsync("http://lm-rp.fr/launcher/servers/" + serverName + "/mods.xml") == E_DOWNLOAD_STATE.STATE_DOWNLOADED)
        {
          if (downloader.result != null)
          {
            if (downloader.result.Length != 0)
            {
              string str = Encoding.Default.GetString(downloader.result);
              XmlDocument xmlDocument = new XmlDocument();
              string xml = str;
              xmlDocument.LoadXml(xml);
              string name = "mod";
              foreach (XmlNode xmlNode in xmlDocument.GetElementsByTagName(name))
              {
                this._modsList.Add(new Mod());
                Mod mod = this._modsList.Last<Mod>();
                mod.name = xmlNode.Attributes["name"].Value;
                mod.version = new ServerMods.Version(xmlNode.Attributes["version"].Value, '_');
                if (xmlNode.Attributes["path"] != null)
                  mod.relativeRemotePath = xmlNode.Attributes["path"].Value;
                returnValue = true;
              }
            }
          }
        }
      }
      catch
      {
      }
      return returnValue;
    }

    public async Task<bool> isPlayerHasMods()
    {
      bool returnValue = false;
      if (Directory.Exists(await this.getLocalServerPath()))
      {
        ServerMods.Version localVersion = await this.getLocalVersion();
        ServerMods.Version remoteVersion = await this.getRemoteVersion();
        if (localVersion != null && remoteVersion != null && localVersion.Equals(remoteVersion))
        {
          string localModsHash = await this.getLocalModsHash();
          string hiddenLocalServerPath = await this.getHiddenLocalServerPath();
          if (Directory.Exists(hiddenLocalServerPath))
          {
            string path = Path.Combine(hiddenLocalServerPath, ".mods");
            if (File.Exists(path))
            {
              string str = File.ReadAllText(path);
              if (str.Equals(localModsHash) && str.Length > 0 && localModsHash.Length > 0)
                returnValue = true;
            }
          }
          localModsHash = (string) null;
        }
        localVersion = (ServerMods.Version) null;
      }
      return returnValue;
    }

    public async Task<ServerMods.SERVER_REQUIRES_MODS> isServerRequiresMods()
    {
      ServerMods.SERVER_REQUIRES_MODS returnValue = ServerMods.SERVER_REQUIRES_MODS.NO;
      Downloader downloader = new Downloader();
      try
      {
        if (await downloader.downloadAsync("http://lm-rp.fr/launcher/servers/?ip=" + this.serverHost.ToString()) == E_DOWNLOAD_STATE.STATE_DOWNLOADED)
        {
          if (downloader.result != null && downloader.result.Length != 0)
          {
            string[] strArray = Encoding.Default.GetString(downloader.result).Split(',');
            if (strArray.Length == 2)
              returnValue = int.Parse(strArray[1]) == 0 ? ServerMods.SERVER_REQUIRES_MODS.OPTIONAL : ServerMods.SERVER_REQUIRES_MODS.YES;
          }
          else
            returnValue = ServerMods.SERVER_REQUIRES_MODS.NO;
        }
        else
          returnValue = ServerMods.SERVER_REQUIRES_MODS.NO;
      }
      catch (Exception ex)
      {
        returnValue = ServerMods.SERVER_REQUIRES_MODS.NO;
      }
      return returnValue;
    }

    private async Task<string> getLocalModsHash()
    {
      MemoryStream stream = new MemoryStream();
      try
      {
        string localServerPath = await this.getLocalServerPath();
        if (Directory.Exists(localServerPath))
        {
          foreach (FileInfo file in new DirectoryInfo(localServerPath).GetFiles("*.mod"))
          {
            using (MD5 md5 = MD5.Create())
            {
              using (FileStream fileStream = File.OpenRead(Path.Combine(localServerPath, file.Name)))
              {
                byte[] hash = md5.ComputeHash((Stream) fileStream);
                stream.Write(hash, 0, hash.Length);
              }
            }
          }
        }
      }
      catch
      {
      }
      finally
      {
        stream.Close();
      }
      return BitConverter.ToString(stream.ToArray()).Replace("-", "").ToLowerInvariant();
    }

    private async Task<string> getHiddenLocalServerPath()
    {
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PanaMonde", await this.getServerName());
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      return path;
    }

    private async Task<string> getLocalServerPath()
    {
      return Path.Combine(Registry.getValue("mods"), await this.getServerName());
    }

    private async Task<ServerMods.Version> getRemoteVersion()
    {
      ServerMods.Version returnValue = (ServerMods.Version) null;
      Downloader downloader = new Downloader();
      try
      {
        if (await downloader.downloadAsync("http://lm-rp.fr/launcher/servers/?ip=" + this.serverHost.ToString() + "&version") == E_DOWNLOAD_STATE.STATE_DOWNLOADED)
        {
          if (downloader.result != null)
          {
            if (downloader.result.Length != 0)
              returnValue = new ServerMods.Version(Encoding.Default.GetString(downloader.result).Split(',')[1], '.');
          }
        }
      }
      catch (Exception ex)
      {
        returnValue = (ServerMods.Version) null;
      }
      return returnValue;
    }

    private async Task<ServerMods.Version> getLocalVersion()
    {
      ServerMods.Version localVersion = (ServerMods.Version) null;
      try
      {
        string localServerPath = await this.getLocalServerPath();
        if (Directory.Exists(localServerPath))
        {
          string path = Path.Combine(localServerPath, ".version");
          if (File.Exists(path))
            localVersion = new ServerMods.Version(File.ReadAllText(path), '_');
        }
      }
      catch
      {
      }
      return localVersion;
    }

    private async Task<string> getServerName()
    {
      if (this.remoteServerName != string.Empty)
        return this.remoteServerName;
      string returnValue = string.Empty;
      Downloader downloader = new Downloader();
      try
      {
        if (await downloader.downloadAsync("http://lm-rp.fr/launcher/servers/?ip=" + this.serverHost.ToString() + "&name") == E_DOWNLOAD_STATE.STATE_DOWNLOADED)
        {
          if (downloader.result != null)
          {
            if (downloader.result.Length != 0)
            {
              string[] strArray = Encoding.Default.GetString(downloader.result).Split(',');
              returnValue = strArray[1];
              this.remoteServerName = strArray[1];
            }
          }
        }
      }
      catch (Exception ex)
      {
        returnValue = string.Empty;
      }
      return returnValue;
    }

    private async void createVersionFile(string version)
    {
      string path = Path.Combine(await this.getLocalServerPath(), ".version");
      if (File.Exists(path))
      {
        File.SetAttributes(path, FileAttributes.Normal);
        File.Delete(path);
      }
      using (StreamWriter text = File.CreateText(path))
        text.Write(version);
      File.SetAttributes(path, FileAttributes.Hidden);
    }

    private async void createHiddenHashFile(string hash)
    {
      string path = Path.Combine(await this.getHiddenLocalServerPath(), ".mods");
      if (File.Exists(path))
      {
        File.SetAttributes(path, FileAttributes.Normal);
        File.Delete(path);
      }
      using (StreamWriter text = File.CreateText(path))
        text.Write(hash);
    }

    public enum SERVER_REQUIRES_MODS
    {
      YES,
      OPTIONAL,
      NO,
    }

    public class Version : IEquatable<ServerMods.Version>
    {
      public int major = -1;
      public int minor = -1;
      public int build = -1;
      public const ServerMods.Version None = null;

      public Version(int major = -1, int minor = -1, int build = -1)
      {
        this.major = major;
        this.minor = minor;
        this.build = build;
      }

      public Version(string version, char split = '_')
      {
        string[] strArray = version.Split(split);
        if (strArray.Length != 3)
          return;
        this.major = int.Parse(strArray[0]);
        this.minor = int.Parse(strArray[1]);
        this.build = int.Parse(strArray[2]);
      }

      public override string ToString()
      {
        return string.Format("{0}_{1}_{2}", (object) this.major, (object) this.minor, (object) this.build);
      }

      public bool Equals(ServerMods.Version other)
      {
        return this.ToString() == other.ToString();
      }
    }
  }
}
