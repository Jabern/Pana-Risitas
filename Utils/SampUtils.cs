

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Utils
{
  public class SampUtils
  {
    public static async Task<string[]> GetRemoteServerList(SampUtils.E_TYPE_LIST typeList)
    {
      string[] results = (string[]) null;
      Downloader downloader = new Downloader();
      try
      {
        switch (typeList)
        {
          case SampUtils.E_TYPE_LIST.LIST_INTERNET:
          case SampUtils.E_TYPE_LIST.LIST_HOSTED:
            E_DOWNLOAD_STATE eDownloadState1;
            if (typeList == SampUtils.E_TYPE_LIST.LIST_INTERNET)
            {
              eDownloadState1 = await downloader.downloadAsync("http://lists.sa-mp.com/0.3.7/servers");
            }
            else
            {
              E_DOWNLOAD_STATE eDownloadState2;
              if (typeList == SampUtils.E_TYPE_LIST.LIST_HOSTED)
                eDownloadState2 = await downloader.downloadAsync("http://lists.sa-mp.com/0.3.7/hosted");
              else
                eDownloadState2 = E_DOWNLOAD_STATE.STATE_ERROR_DOWNLOADED;
              eDownloadState1 = eDownloadState2;
            }
            if (eDownloadState1 == E_DOWNLOAD_STATE.STATE_DOWNLOADED)
            {
              results = Encoding.Default.GetString(downloader.result).Split('\n');
              break;
            }
            break;
          case SampUtils.E_TYPE_LIST.LIST_FAVOURITES:
            results = FavoritesFile.GetServerList();
            break;
        }
      }
      finally
      {
        if (downloader != null)
          downloader.Dispose();
      }
      downloader = (Downloader) null;
      return results;
    }

    public static byte[] __buildPacket(string hostname, int port, SampUtils.E_OPCODE opcode)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream))
        {
          binaryWriter.Write("SAMP".ToCharArray());
          string[] strArray = hostname.ToString().Split('.');
          binaryWriter.Write(Convert.ToByte(Convert.ToInt32(strArray[0])));
          binaryWriter.Write(Convert.ToByte(Convert.ToInt32(strArray[1])));
          binaryWriter.Write(Convert.ToByte(Convert.ToInt32(strArray[2])));
          binaryWriter.Write(Convert.ToByte(Convert.ToInt32(strArray[3])));
          binaryWriter.Write((ushort) port);
          binaryWriter.Write((char) opcode);
          if ((int) (ushort) opcode == 112)
            binaryWriter.Write("8493".ToCharArray());
          return memoryStream.ToArray();
        }
      }
    }

    public static string[] __parseResponse(byte[] response)
    {
      string[] strArray1 = new string[0];
      int num1 = 0;
      using (MemoryStream memoryStream = new MemoryStream(response))
      {
        using (BinaryReader binaryReader = new BinaryReader((Stream) memoryStream))
        {
          if (memoryStream.Length <= 10L)
            return strArray1;
          binaryReader.ReadBytes(10);
          char ch = binaryReader.ReadChar();
          int num2;
          if ((uint) ch <= 100U)
          {
            if ((int) ch != 99)
            {
              if ((int) ch == 100)
              {
                int num3 = (int) binaryReader.ReadInt16();
                strArray1 = new string[num3 * 4];
                for (int index1 = 0; index1 < num3; ++index1)
                {
                  string[] strArray2 = strArray1;
                  int index2 = num1;
                  int num4 = 1;
                  int num5 = index2 + num4;
                  string str1 = Convert.ToString(binaryReader.ReadByte());
                  strArray2[index2] = str1;
                  int count = (int) binaryReader.ReadByte();
                  string[] strArray3 = strArray1;
                  int index3 = num5;
                  int num6 = 1;
                  int num7 = index3 + num6;
                  string str2 = Encoding.UTF7.GetString(binaryReader.ReadBytes(count));
                  strArray3[index3] = str2;
                  string[] strArray4 = strArray1;
                  int index4 = num7;
                  int num8 = 1;
                  int num9 = index4 + num8;
                  string str3 = Convert.ToString(binaryReader.ReadInt32());
                  strArray4[index4] = str3;
                  string[] strArray5 = strArray1;
                  int index5 = num9;
                  int num10 = 1;
                  num1 = index5 + num10;
                  string str4 = Convert.ToString(binaryReader.ReadInt32());
                  strArray5[index5] = str4;
                }
              }
            }
            else
            {
              int num3 = (int) binaryReader.ReadInt16();
              strArray1 = new string[num3 * 2];
              for (int index1 = 0; index1 < num3; ++index1)
              {
                int count = (int) binaryReader.ReadByte();
                string[] strArray2 = strArray1;
                int index2 = num1;
                int num4 = 1;
                int num5 = index2 + num4;
                string str1 = Encoding.UTF7.GetString(binaryReader.ReadBytes(count));
                strArray2[index2] = str1;
                string[] strArray3 = strArray1;
                int index3 = num5;
                int num6 = 1;
                num1 = index3 + num6;
                string str2 = Convert.ToString(binaryReader.ReadInt32());
                strArray3[index3] = str2;
              }
            }
          }
          else if ((int) ch != 105)
          {
            if ((int) ch != 112)
            {
              if ((int) ch == 114)
              {
                int num3 = (int) binaryReader.ReadInt16();
                strArray1 = new string[num3 * 2];
                for (int index1 = 0; index1 < num3; ++index1)
                {
                  int count1 = (int) binaryReader.ReadByte();
                  string[] strArray2 = strArray1;
                  int index2 = num1;
                  int num4 = 1;
                  int num5 = index2 + num4;
                  string str1 = Encoding.UTF7.GetString(binaryReader.ReadBytes(count1));
                  strArray2[index2] = str1;
                  int count2 = (int) binaryReader.ReadByte();
                  string[] strArray3 = strArray1;
                  int index3 = num5;
                  int num6 = 1;
                  num1 = index3 + num6;
                  string str2 = Encoding.UTF7.GetString(binaryReader.ReadBytes(count2));
                  strArray3[index3] = str2;
                }
              }
            }
            else
            {
              strArray1 = new string[1];
              string[] strArray2 = strArray1;
              int index = num1;
              int num3 = 1;
              num2 = index + num3;
              string str = "10";
              strArray2[index] = str;
            }
          }
          else
          {
            strArray1 = new string[6];
            string[] strArray2 = strArray1;
            int index1 = num1;
            int num3 = 1;
            int num4 = index1 + num3;
            string str1 = Convert.ToString(binaryReader.ReadByte());
            strArray2[index1] = str1;
            string[] strArray3 = strArray1;
            int index2 = num4;
            int num5 = 1;
            int num6 = index2 + num5;
            string str2 = Convert.ToString(binaryReader.ReadInt16());
            strArray3[index2] = str2;
            string[] strArray4 = strArray1;
            int index3 = num6;
            int num7 = 1;
            int num8 = index3 + num7;
            string str3 = Convert.ToString(binaryReader.ReadInt16());
            strArray4[index3] = str3;
            int count1 = binaryReader.ReadInt32();
            string[] strArray5 = strArray1;
            int index4 = num8;
            int num9 = 1;
            int num10 = index4 + num9;
            string str4 = Encoding.UTF7.GetString(binaryReader.ReadBytes(count1));
            strArray5[index4] = str4;
            int count2 = binaryReader.ReadInt32();
            string[] strArray6 = strArray1;
            int index5 = num10;
            int num11 = 1;
            int num12 = index5 + num11;
            string str5 = Encoding.UTF7.GetString(binaryReader.ReadBytes(count2));
            strArray6[index5] = str5;
            int count3 = binaryReader.ReadInt32();
            string[] strArray7 = strArray1;
            int index6 = num12;
            int num13 = 1;
            num2 = index6 + num13;
            string str6 = Encoding.UTF7.GetString(binaryReader.ReadBytes(count3));
            strArray7[index6] = str6;
          }
        }
      }
      return strArray1;
    }

    public class ServerHost
    {
      public string address = string.Empty;
      public string port = string.Empty;

      public ServerHost(string address, string port)
      {
        this.address = address;
        this.port = port;
      }

      public override string ToString()
      {
        return string.Format("{0}:{1}", (object) this.address, (object) this.port);
      }
    }

    public enum E_OPCODE
    {
      SERVER_PLAYERS = 99,
      SERVER_INFORMATION = 105,
      SERVER_PING = 112,
      SERVER_RULES = 114,
    }

    public enum E_TYPE_LIST
    {
      LIST_INTERNET,
      LIST_HOSTED,
      LIST_FAVOURITES,
    }
  }
}
