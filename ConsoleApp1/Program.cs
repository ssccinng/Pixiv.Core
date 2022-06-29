// See https://aka.ms/new-console-template for more information
using Pixiv.Core;

PixivClawer PixivClawer = new PixivClawer();

var ids = await PixivClawer.SearchTagAsync("anya", sMode: SMode.Tag);

//var img = await PixivClawer.GetImageByteArray(ids[0]);
Console.WriteLine(ids.Total);

//File.WriteAllBytes("test1.png", img);
