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

    public class SearchUserResult
    {
        public string Username { get; set; }
        public string Id { get; set; }
        public string Url { get; set; }
    }

    public class PixivClawer
    {
        /// <summary>
        /// 搜索url
        /// </summary>
        //private string _searchUrl = "https://www.pixiv.net/ajax/search/illustrations/{0}?word={0}&order=date_d&mode=all&p={1}&s_mode=s_tag_full&type=illust_and_ugoira&lang=zh";
        private string _searchUrl = "https://www.pixiv.net/ajax/search/illustrations/{0}?word={0}&order=date_d&mode=all&p={1}&s_mode={2}&type=illust_and_ugoira&lang=zh";
        private string _artworksUrl = "https://www.pixiv.net/artworks/{0}";
        private string _searchUserUrl = "https://www.pixiv.net/search_user.php?nick={0}&s_mode=s_usr";
        private string _userArtworksUrl = "https://www.pixiv.net/ajax/user/{0}/profile/all?lang=zh";

        private HttpClient _httpClient;
        private HttpClient _httpClientUser;
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


        private string header1 =
            @"accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
accept-encoding: gzip, deflate, br
accept-language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6
cache-control: no-cache
cookie: first_visit_datetime_pc=2022-06-29+10%3A01%3A23; p_ab_id=4; p_ab_id_2=9; p_ab_d_id=584911464; yuid_b=KVaQEQU; _fbp=fb.1.1656464485194.1188669955; _gcl_au=1.1.1838878230.1656464570; __utmz=235335808.1656464571.2.2.utmcsr=bing|utmccn=(organic)|utmcmd=organic|utmctr=(not%20provided); _im_vid=01G6PFQEE7X9SW89FGHKVQSF6P; adr_id=SpOD3KCW0DSoXzX9UGH65piQ8eSJ1kwugjrm6FZa3HMc1NcD; PHPSESSID=25421434_1CcJn2YFlw4D293vfuynZOGfbRgJ9NQk; device_token=4dce52f6b1bd6b45afa2970cfaf2634b; c_type=23; privacy_policy_agreement=0; privacy_policy_notification=0; a_type=0; b_type=1; login_ever=yes; __utmv=235335808.|2=login%20ever=yes=1^3=plan=normal=1^5=gender=male=1^6=user_id=25421434=1^9=p_ab_id=4=1^10=p_ab_id_2=9=1^11=lang=zh=1; tag_view_ranking=AL_Ixyn11N~t4uEwuIPF8~RpwyIZWsuY~_u5h-CMrqp~BR3jJZcEHk~9778Z-Sx3f~7DhgHp69L8~GVtOo5MeUD~WKX_qbfZtF~MibNz6NZNO~cRc4aP7hy6~kHxzNu5fgo~ptbDpN0XQz~KEXlaLY683~W0P8k76Ful~VEoBvQxqnq~HgUosxtViU~v2cFCcYEDF~yMNRqWm-Ug~Sdp_c1eMDE; cto_bundle=vwgs2l9aUERld0ZwJTJGdE1GSkd5dyUyRlExMzRVMFphR1A3a0pGJTJCZEw2aGJxbFAxZ3BwQ3paWHNNbERCbDlXRnBUUllwSEhFN0dmMHNxQWhoYUw1JTJCODh6JTJGSG1jbElTTXcwWm9WYUZyS1ZnUnk4ciUyQjVEbXBnWGppc254cHNWNzN0Z1BoQ2k3ZWJGM3pRQ045bUZiblY3S3BnNUNuJTJCQSUzRCUzRA; __utma=235335808.1131938266.1656464485.1656479502.1657259541.6; __utmc=235335808; __utmt=1; __cf_bm=3.uiHAqQhxl69K8Lo6JidTg3R2pdKyTJFgDoGwnpLJo-1657259541-0-Aau2QOEaUrpbycBIbu+Z1FiRA8wfTfRD3CitnrY6/PjjmmNg8Xcq13WVRUEmGgaHCXnyIEyJv0xkdcjZQfl9EstJbQyPFT/O4fYjT2WRIZWKbuJ16Jm5qiEPpZmEtJYsQHCAofMwYqe37jjxZK+PYugYqwoCW8NzSQzIWjWgZ4RmLHgROzj2YfzCBsBITdxI6w==; tags_sended=1; categorized_tags=bXMh6mBhl8~ptbDpN0XQz; __utmb=235335808.2.10.1657259541; _ga_75BBYNYN9J=GS1.1.1657259540.6.1.1657259546.0; _ga=GA1.2.1131938266.1656464485; _gid=GA1.2.610816895.1657259546; _gat_UA-1830249-3=1
pragma: no-cache
sec-ch-ua: "" Not;A Brand"";v=""99"", ""Microsoft Edge"";v=""103"", ""Chromium"";v=""103""
sec-ch-ua-mobile: ?0
sec-ch-ua-platform: ""Windows""
sec-fetch-dest: document
sec-fetch-mode: navigate
sec-fetch-site: none
sec-fetch-user: ?1
upgrade-insecure-requests: 1
user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.66 Safari/537.36 Edg/103.0.1264.44";

        Regex reg = new Regex(@"""original"":""(.+?)""");



        public PixivClawer()
        {
            _httpClient = new HttpClient();
            _httpClientUser = new HttpClient();
            foreach (var hd in header.Split('\n'))
            {
                var hk = hd.Split(": ");
                _httpClient.DefaultRequestHeaders.Add(hk[0].Trim(), hk[1].Trim());
            }
            
            foreach (var hd in header1.Split('\n'))
            {
                var hk = hd.Split(": ");
                _httpClientUser.DefaultRequestHeaders.Add(hk[0].Trim(), hk[1].Trim());
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

        public async Task<SearchUserResult[]> SearchUserAsync(string val)
        {
            // var handler = new HttpClientHandler()
            // {
            //     MaxAutomaticRedirections = 10
            // };
            // HttpClient client = new(handler);
            
            Regex reg = new (@"<a href=""(?<url>/users/\d+?)"" target=""_blank"" class=""title"">(?<author>.*?)</a>");
            var res = await _httpClientUser.GetAsync(string.Format(_searchUserUrl, val));
            
            GZipStream gZipStream = new GZipStream(res.Content.ReadAsStream(), CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gZipStream.CopyTo(resultStream);
            var html = Encoding.UTF8.GetString(resultStream.ToArray());
            // Console.WriteLine(11);
            // Console.WriteLine(res.StatusCode);
            //
            // Console.WriteLine(await res.Content.ReadAsStringAsync());
            var matches = reg.Matches(html);
            SearchUserResult[] userResults = new SearchUserResult[matches.Count];
            int idx = 0;
            foreach (Match match in matches)
            {
                userResults[idx++] = new SearchUserResult()
                {
                    Username = match.Groups["author"].ToString(),
                    Url = "https://www.pixiv.net/" + (match.Groups["url"].ToString()),
                    Id = match.Groups["url"].ToString()[7..]
                };
                // Console.WriteLine(match.Groups["author"]);
                // Console.WriteLine(match.Groups["url"]);
            }

            return userResults;
        }

        public async Task<string[]> GetUserArtworksAsync(string uid)
        {
            List<string> ids = new();
            var res = await _httpClient.GetAsync(string.Format(_userArtworksUrl, uid));
            GZipStream gZipStream = new GZipStream(res.Content.ReadAsStream(), CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gZipStream.CopyTo(resultStream);
            var html = Encoding.UTF8.GetString(resultStream.ToArray());
            var data = JsonDocument.Parse(html).RootElement;
            var illusts = data.GetProperty("body").GetProperty("illusts");
            foreach (var id in illusts.EnumerateObject())
            {
                ids.Add(id.Name);
            }
            return ids.ToArray();
        }
    }
}