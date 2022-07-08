// See https://aka.ms/new-console-template for more information
using Pixiv.Core;

PixivClawer PixivClawer = new PixivClawer();

var ids = await PixivClawer.SearchTagAsync("anya", sMode: SMode.Tag);
var vv = await PixivClawer.SearchUserAsync("heidi");
var cc = await PixivClawer.GetUserArtworksAsync(vv[1].Id);
for (int i = 0; i < cc.Length; i++)
{
    var img = await PixivClawer.GetImageByteArray(cc[i]);
    File.WriteAllBytes($"heidi{i}.png", img);
}

//var img = await PixivClawer.GetImageByteArray(ids[0]);
Console.WriteLine(ids.Total);

//File.WriteAllBytes("test1.png", img);
