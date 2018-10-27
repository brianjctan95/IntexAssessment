using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace IntexAssessment.Controllers
{
    public class Image
    {
        public int id { get; set; }
        public string name { get; set; }
        public string src { get; set; }
        public int page { get; set; }
        public int requestCount { get; set; }
    }
    public class Url
    {
        public int id { get; set; }
        public string url { get; set; }
        public int start { get; set; }
        public int end { get; set; }
    }

    //[Authorize]
    public class ValuesController : ApiController
    {
        [Route("meme/test/{input}"), HttpGet]
        public string test(string input)
        {
            return input;
        }

        [Route("meme/all"), HttpGet]
        public List<Image> getAll()
        {
            var path = HttpContext.Current.Server.MapPath("~/images.json");
            List<Image> imageList = new List<Image>();
            using (StreamReader file = File.OpenText(path))
            {
                string json = file.ReadToEnd();
                imageList = JsonConvert.DeserializeObject<List<Image>>(json);
            }

            foreach (Image i in imageList)
                i.requestCount++;

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, imageList);
            }

            return imageList;
        }

        [Route("meme/id/{id}"), HttpGet]
        public Image getById(int id)
        {
            var path = HttpContext.Current.Server.MapPath("~/images.json");
            List<Image> imageList = new List<Image>();
            Image output = new Image();

            using (StreamReader file = File.OpenText(path))
            {
                string json = file.ReadToEnd();
                imageList = JsonConvert.DeserializeObject<List<Image>>(json);
            }

            for (int i = 0; i < imageList.Count; i++)
            {
                if (imageList[i].id == id)
                {
                    output = imageList[i];
                    imageList[i].requestCount++;
                    break;
                }
            }

            //if (output.Equals(null))


            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, imageList);
            }
            return output;
        }

        [Route("meme/page/{page}"), HttpGet]
        public List<Image> getByPage(int page)
        {
            var path = HttpContext.Current.Server.MapPath("~/images.json");
            List<Image> imageList = new List<Image>();
            List<Image> output = new List<Image>();

            using (StreamReader file = File.OpenText(path))
            {
                string json = file.ReadToEnd();
                imageList = JsonConvert.DeserializeObject<List<Image>>(json);
            }

            for (int i = 0; i < imageList.Count; i++)
            {
                if (imageList[i].page == page)
                {
                    output.Add(imageList[i]);
                    imageList[i].requestCount++;
                }
            }

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, imageList);
            }
            return output;
        }

        [Route("meme/popular"), HttpGet]
        public List<Image> getMostPopular()
        {
            var path = HttpContext.Current.Server.MapPath("~/images.json");
            List<Image> imageList = new List<Image>();
            List<Image> output = new List<Image>();
            List<int> idList = new List<int>();
            int currentBest = -1;

            using (StreamReader file = File.OpenText(path))
            {
                string json = file.ReadToEnd();
                imageList = JsonConvert.DeserializeObject<List<Image>>(json);
            }

            for (int i = 0; i < imageList.Count; i++)
            {
                if (currentBest == -1)
                {
                    currentBest = imageList[i].requestCount;
                    output.Add(imageList[i]);
                    idList.Add(i);
                }
                else
                {
                    if (imageList[i].requestCount > currentBest)
                    {
                        currentBest = imageList[i].requestCount;
                        output.Clear();
                        idList.Clear();
                        output.Add(imageList[i]);
                        idList.Add(i);
                    }
                    else if (imageList[i].requestCount == currentBest)
                    {
                        output.Add(imageList[i]);
                        idList.Add(i);
                    }
                }
            }

            foreach (int i in idList)
                imageList[i].requestCount++;

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, imageList);
            }
            return output;
        }

        [Route("meme/create"), HttpPost]
        public Image create([FromBody]Image image)
        {
            var path = HttpContext.Current.Server.MapPath("~/images.json");
            List<Image> imageList = new List<Image>();

            int id = 0;
            int page = 0;

            using (StreamReader file = File.OpenText(path))
            {
                string json = file.ReadToEnd();
                imageList = JsonConvert.DeserializeObject<List<Image>>(json);
            }
            id = imageList[imageList.Count() - 1].id + 1;
            page = imageList[imageList.Count() - 9].page + 1;

            Image newImage = new Image();
            newImage.id = id;
            newImage.name = image.name;
            newImage.src = image.src;
            newImage.page = page;
            newImage.requestCount = 0;
            imageList.Add(newImage);

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, imageList);
            }
            return newImage;
        }

        [Route("meme/scrape"), HttpPut]
        public string Put()
        {
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            string url = "";
            int start = 0;
            int end = 0;

            List<string> name = new List<string>();
            List<string> src = new List<string>();
            List<int> page = new List<int>();

            var path = HttpContext.Current.Server.MapPath("~/url.json");
            using (StreamReader file = File.OpenText(path))
            {
                string json = file.ReadToEnd();
                List<Url> urlList = JsonConvert.DeserializeObject<List<Url>>(json);
                url = urlList[0].url;
                start = urlList[0].start;
                end = urlList[0].end;
            }
            for (int i = start; i <= end; i++)
            {
                HtmlAgilityPack.HtmlDocument doc = web.Load(url + i);

                var memeName = doc.DocumentNode.SelectNodes("//div[@class='meme-name']").ToList();
                foreach (var item in memeName)
                {
                    System.Diagnostics.Debug.WriteLine(item.InnerText.Trim());
                    name.Add(item.InnerText.Trim());
                }

                var memeImage = doc.DocumentNode.SelectNodes("//img[@class='meme-img center-block']").ToList();
                foreach (var item in memeImage)
                {
                    System.Diagnostics.Debug.WriteLine(item.Attributes["src"].Value);
                    src.Add(item.Attributes["src"].Value);
                    page.Add(i);
                }
            }

            path = HttpContext.Current.Server.MapPath("~/images.json");

            List<Image> imageList = new List<Image>();
            int j = 0;
            foreach (var item in name)
            {
                Image newImage = new Image();
                newImage.name = name[j];
                newImage.src = src[j];
                newImage.page = page[j];
                newImage.id = ++j;
                newImage.requestCount = 0;
                imageList.Add(newImage);
            }
            
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, imageList);
            }
            return "Success!!!";
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
