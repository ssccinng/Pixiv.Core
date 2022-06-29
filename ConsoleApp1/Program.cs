// See https://aka.ms/new-console-template for more information
using Pixiv.Core;

PixivClawer PixivClawer = new PixivClawer();

var ids = await PixivClawer.SearchTagAsync("primarina");

var img = await PixivClawer.GetImageByteArray(ids[0]);

File.WriteAllBytes("test.png", img);
