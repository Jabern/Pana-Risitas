

using Launcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Launcher
{
  internal class Mod
  {
    public string name = string.Empty;
    public string description = string.Empty;
    public ServerMods.Version version = new ServerMods.Version(-1, -1, -1);
    public string relativeRemotePath = "mods/";
    private string localPath = string.Empty;
    private List<Tuple<string, bool>> listOfCreatedEntriesDuringExtraction = new List<Tuple<string, bool>>();
    private Mod.MOD_STATE state;
    private byte[] modContentBytes;

    public Mod()
    {
    }

    public Mod(string path)
    {
      if (!File.Exists(path))
        return;
      System.Text.RegularExpressions.Match match = Regex.Match(path, ".+[/\\\\]([a-zA-Z]+)_(.+).mod");
      this.name = match.Groups[1].ToString();
      this.version = new ServerMods.Version(match.Groups[2].ToString(), '_');
      this.state = Mod.MOD_STATE.MOD_DOWNLOADED;
      this.modContentBytes = File.ReadAllBytes(path);
      this.localPath = path;
    }

    public async Task<byte[]> downloadMod()
    {
      Downloader downloader = new Downloader();
      byte[] numArray;
      try
      {
        string[] strArray = new string[7]
        {
          "http://lm-rp.fr/launcher/",
          this.relativeRemotePath,
          this.name,
          "/",
          this.name + "_" + this.version.ToString(),
          "/",
          this.name
        };
        if (await downloader.downloadAsync(string.Concat(strArray)) == E_DOWNLOAD_STATE.STATE_DOWNLOADED)
        {
          numArray = downloader.result;
          this.modContentBytes = numArray;
          if (numArray != null)
            this.state = Mod.MOD_STATE.MOD_DOWNLOADED;
        }
        else
          numArray = (byte[]) null;
      }
      catch (Exception ex)
      {
        numArray = (byte[]) null;
      }
      return numArray;
    }

    public bool saveMod(string destinationPath)
    {
      bool flag = true;
      try
      {
        if (!Directory.Exists(destinationPath))
          Directory.CreateDirectory(destinationPath);
        string path = Path.Combine(destinationPath, this.name + "_" + (object) this.version + ".mod");
        if (File.Exists(path))
        {
          File.SetAttributes(path, FileAttributes.Normal);
          File.Delete(path);
        }
        File.WriteAllBytes(path, this.modContentBytes);
        File.SetAttributes(path, FileAttributes.ReadOnly);
        this.localPath = path;
      }
      catch (Exception ex)
      {
        flag = false;
      }
      return flag;
    }

    public async Task<bool> downloadAndSaveMod(string destinationPath)
    {
      bool returnValue = false;
      try
      {
        if (await this.downloadMod() != null)
          returnValue = this.saveMod(destinationPath);
      }
      catch (Exception ex)
      {
        returnValue = false;
      }
      return returnValue;
    }

    public bool extractMod(string pathDestination)
    {
      bool flag1 = true;
      try
      {
        using (ZipArchive zipArchive = ZipFile.OpenRead(this.localPath))
        {
          foreach (ZipArchiveEntry entry in zipArchive.Entries)
          {
            string str = pathDestination + "\\" + entry.FullName;
            bool flag2 = false;
            if (!File.Exists(str) && !Directory.Exists(str))
              flag2 = true;
            this.listOfCreatedEntriesDuringExtraction.Add(Tuple.Create<string, bool>(str, flag2));
            if (entry.Name == string.Empty)
            {
              Directory.CreateDirectory(str);
            }
            else
            {
              try
              {
                entry.ExtractToFile(str, false);
              }
              catch (IOException ex)
              {
              }
            }
          }
          this.state = Mod.MOD_STATE.MOD_EXTRACTED;
        }
      }
      catch (Exception ex)
      {
        flag1 = false;
      }
      return flag1;
    }

    public bool deleteExtractedFilesAndDirectories()
    {
      bool flag = true;
      try
      {
        foreach (Tuple<string, bool> tuple in this.listOfCreatedEntriesDuringExtraction)
        {
          string path = tuple.Item1;
          if (tuple.Item2)
          {
            if (Directory.Exists(path))
              Directory.Delete(path, true);
            if (File.Exists(path))
            {
              File.SetAttributes(path, FileAttributes.Normal);
              File.Delete(path);
            }
          }
        }
      }
      catch (Exception ex)
      {
        flag = false;
      }
      return flag;
    }

    public Mod.MOD_STATE getModState()
    {
      return this.state;
    }

    public enum MOD_STATE
    {
      MOD_UNINSTALLED,
      MOD_DOWNLOADED,
      MOD_EXTRACTED,
    }
  }
}
