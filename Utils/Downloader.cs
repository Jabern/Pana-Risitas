
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Launcher.Utils
{
  internal class Downloader : IDisposable
  {
    private static HttpClient httpClient = new HttpClient();

    public byte[] result { get; set; }

    public async Task<E_DOWNLOAD_STATE> downloadAsync(string url)
    {
      E_DOWNLOAD_STATE returnValue = E_DOWNLOAD_STATE.STATE_ERROR_DOWNLOADED;
      try
      {
        HttpResponseMessage async = await Downloader.httpClient.GetAsync(new Uri(url));
        long? contentLength = async.Content.Headers.ContentLength;
        if (async.IsSuccessStatusCode)
        {
          this.result = await async.Content.ReadAsByteArrayAsync();
          returnValue = E_DOWNLOAD_STATE.STATE_DOWNLOADED;
        }
      }
      catch
      {
        returnValue = E_DOWNLOAD_STATE.STATE_ERROR_DOWNLOADED;
      }
      return returnValue;
    }

    public void Dispose()
    {
      this.result = (byte[]) null;
    }
  }
}
