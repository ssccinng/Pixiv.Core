using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Pixiv.Core
{
    public enum SMode
    {
        [Description("s_tag_full")]
        TagFull,
        [Description("s_tag")]
        Tag
    }
    public class SearchResult
    {
        public List<string> Ids = new List<string>();
        public int Total;
    }

    public class PixivClawer
    {
        /// <summary>
        /// 搜索url
        /// </summary>
        //private string _searchUrl = "https://www.pixiv.net/ajax/search/illustrations/{0}?word={0}&order=date_d&mode=all&p={1}&s_mode=s_tag_full&type=illust_and_ugoira&lang=zh";
        private string _searchUrl = "https://www.pixiv.net/ajax/search/illustrations/{0}?word={0}&order=date_d&mode=all&p={1}&s_mode={2}&type=illust_and_ugoira&lang=zh";
        private string _artworksUrl = "https://www.pixiv.net/artworks/{0}";

        private HttpClient _httpClient;
        private string header = @"accept: image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8
accept-encoding: gzip
accept-language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6
cache-control: no-cache
pragma: no-cache
referer: https://www.pixiv.net/
sec-ch-ua: "" Not;A Brand"";v=""99"", ""Microsoft Edge"";v=""103"", ""Chromium"";v=""103""
sec-ch-ua-mobile: ?0
sec-ch-ua-platform: ""Windows""
sec-fetch-dest: image
sec-fetch-mode: no-cors
sec-fetch-site: cross-site
user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36 Edg/103.0.1264.37";

        Regex reg = new Regex(@"""original"":""(.+?)""");



        public PixivClawer()
        {
            _httpClient = new HttpClient();
            foreach (var hd in header.Split('\n'))
            {
                var hk = hd.Split(": ");
                _httpClient.DefaultRequestHeaders.Add(hk[0].Trim(), hk[1].Trim());
            }
        }

        public async Task<SearchResult> SearchTagAsync(string tag, int page = 1, SMode sMode = SMode.TagFull)
        {
            List<string> ids = new();
            Console.WriteLine(string.Format(_searchUrl, tag, page, sMode.ToString()));
            var res = await _httpClient.GetAsync(string.Format(_searchUrl, tag, page, sMode == SMode.TagFull ? "s_tag_full" : "s_tag"));
            //Console.WriteLine(await res.Content.ReadAsStringAsync());
            GZipStream gZipStream = new GZipStream(await res.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gZipStream.CopyTo(resultStream);
            var html = Encoding.UTF8.GetString(resultStream.ToArray());
            //Console.WriteLine(html);
            var jsonData = JsonDocument.Parse(html).RootElement;
            var imgList = jsonData.GetProperty("body").GetProperty("illust").GetProperty("data");
            var total = jsonData.GetProperty("body").GetProperty("illust").GetProperty("total").GetInt32();
            for (int i = 0; i < imgList.GetArrayLength(); ++i)
            {
                // Console.WriteLine(imgList[i]);
                ids.Add(imgList[i].GetProperty("id").GetString());
            }

            return new SearchResult
            {
                 Ids = ids,
                 Total = total,
            };
        }


        public async Task<byte[]> GetImageByteArray(string id)
        {
            var htmlStream = await (await _httpClient.GetAsync(string.Format(_artworksUrl, id))).Content.ReadAsStreamAsync();
            GZipStream gZipStream = new GZipStream(htmlStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gZipStream.CopyTo(resultStream);
            var html = Encoding.UTF8.GetString(resultStream.ToArray());
            //Console.WriteLine(html);
            var matches = reg.Matches(html);
            foreach (Match match in matches)
            {
                Console.WriteLine(match.Groups[1].Value);
                var gg1 = await _httpClient.GetAsync(match.Groups[1].Value);
                return await gg1.Content.ReadAsByteArrayAsync();
                //File.WriteAllBytes($"b{i}.png", await gg1.Content.ReadAsByteArrayAsync());
            }
            return new byte[0];
        }
    }
}