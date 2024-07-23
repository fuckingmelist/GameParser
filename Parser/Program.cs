using DbCommunicationLibrary;
using HtmlAgilityPack;
using Parser.Entities;
using System.Net;

using System.Text;

namespace GamesParser
{

    public class Program
    {

        static void Main(string[] args)
        {
            const string originalLink = "https://www.playground.ru";
            const string originalPageLink = "/games?release=all&sort=follow_month&p=";

            WebClient webClient = new WebClient();

            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705)");

            string htmlData = webClient.DownloadString("https://www.playground.ru/games");

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            string secondPageLink = document.DocumentNode.SelectSingleNode("//a[@rel='next']").GetAttributeValue("href", "def");

            string htmlDataForAllCount = webClient.DownloadString($"{originalLink}{secondPageLink}");

            HtmlDocument documentForAllCount = new HtmlDocument();
            documentForAllCount.LoadHtml(htmlDataForAllCount);

            HtmlNodeCollection linkNodes = documentForAllCount.DocumentNode.SelectNodes("//ul[@class='pagination']/li");

            string[] arrayLinks = new string[int.Parse(linkNodes[linkNodes.Count - 2].InnerText)];
            List<Gamelink> gameLinks = new List<Gamelink>();

            //for (int i = 0; i != arrayLinks.Length; i++)
            //{
            //    arrayLinks[i] = $"{originalLink}{originalPageLink}{i + 1}";

            //    HtmlDocument documentData = new HtmlDocument();
            //    documentData.LoadHtml(webClient.DownloadString($"{arrayLinks[i]}"));

            //    HtmlNodeCollection currentGamesNodes = documentData.DocumentNode.SelectNodes("//div[@class='item']");
            //    for (int j = 0; j < currentGamesNodes.Count; j++)
            //    {
            //        var htmlDoc = new HtmlDocument();
            //        htmlDoc.LoadHtml(currentGamesNodes[j].InnerHtml);

            //        var currentGame = htmlDoc.DocumentNode;

            //        string gamePageLink = "";
            //        gamePageLink = $"{originalLink}{currentGame.SelectSingleNode("//div[@class='gp-game-cover']/a").GetAttributeValue("href", "")}";

            //        gameLinks.Add(new Gamelink() { Link = gamePageLink });
            //    }
            //}

            //for (int i = 0; i < gameLinks.Count; i++)
            //{
            //    using (GamechatContext dbContext = new GamechatContext())
            //    {
            //        Gamelink gl = dbContext.Gamelinks.Where(x => x.Link == gameLinks[i].Link).FirstOrDefault();
            //        if (gl == null)
            //        {
            //            dbContext.Gamelinks.Add(new Gamelink() { Link = gameLinks[i].Link });
            //            dbContext.SaveChanges();
            //        }
            //    }
            //}
            //Console.WriteLine(1);

            List<Gamelink> gamesLinks;

            using (GamechatContext db = new GamechatContext())
            {
                gamesLinks = db.Gamelinks.Where(x => x.Parsingdate == null).ToList();
            }

            Task<bool>[] tasks = new Task<bool>[Environment.ProcessorCount];

            int gamePackForParsing = 100;
            int gamesInWork = 0;
            int minGamePack = 0;
            int gamesCount = gamesLinks.Count;
            for (int i = 0; i < tasks.Length; i++)
            {
                gamesInWork += minGamePack;
                tasks[i] = new Task<bool>(() => Parse(gamesLinks, minGamePack, minGamePack += gamePackForParsing, originalLink));
            }

            for (int i = 0; i < tasks.Length; i++)
                tasks[i].Start();

            while (gamesInWork < gamesCount)
            {
                int indexOfFreeTask = Task.WaitAny(tasks);

                if (minGamePack + gamePackForParsing > gamesCount)
                {
                    gamesInWork += gamesCount - gamesInWork;
                    tasks[indexOfFreeTask] = new Task<bool>(() => Parse(gamesLinks, minGamePack, gamesCount, originalLink));
                    tasks[indexOfFreeTask].Start();
                }
                else
                {
                    gamesInWork += gamePackForParsing;
                    tasks[indexOfFreeTask] = new Task<bool>(() => Parse(gamesLinks, minGamePack, minGamePack += gamePackForParsing, originalLink));
                    tasks[indexOfFreeTask].Start();
                }
            }

            Task.WaitAll(tasks);

            Console.WriteLine(1);
        }

        public static bool Parse(List<Gamelink> gamesLinks, int minGamesCount, int maxGamesCount, string originalLink)
        {
            List<Game> currentParsedGames = new List<Game>();

            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705)");

                for (int i = minGamesCount; i < maxGamesCount; i++)
                    Parsing(gamesLinks, i, webClient, originalLink);
            }

            return true;
        }

        public static byte[] GetImageBytes(string url)
        {
            WebClient webClient = new WebClient();
            return webClient.DownloadData(url);
        }

        private static void Parsing(List<Gamelink> games, int i, WebClient webClient, string originalLink)
        {

            Game game = new Game();
            HtmlDocument currentGameDocument = new HtmlDocument();
            currentGameDocument.LoadHtml(webClient.DownloadString(games[i].Link));

            HtmlNodeCollection links = currentGameDocument.DocumentNode.SelectNodes("//ul[@class='list-unstyled']/li[@class='item']/a");
            string forumLink = $"{originalLink}{links[6].GetAttributeValue("href", "")}";

            HtmlDocument forumGameDocument = new HtmlDocument();
            forumGameDocument.LoadHtml(webClient.DownloadString(forumLink));
            HtmlNodeCollection themes = forumGameDocument.DocumentNode.SelectNodes("//div[@class='content-block']/div[@class='first']/span/a");

            List<string> themeLinks = new List<string>();

            if (themes != null)
            {
                for (int j = 0; j < themes.Count; j++)
                {
                    string link = themes[j].GetAttributeValue("href", "def");
                    string themeTitle = themes[j].InnerText.Replace("&quot;", " ").TrimStart().TrimEnd();
                    game.Themes.Add(new Theme { Title = themeTitle });
                    themeLinks.Add(link);

                    HtmlDocument commentsDocument = new HtmlDocument();
                    commentsDocument.LoadHtml(webClient.DownloadString(themeLinks[j]));

                    HtmlNodeCollection commentsNodes = commentsDocument.DocumentNode.SelectNodes("//div[@class='comments-block js-comments-block']/div[@class='comments-item js-comments-item\n']/div[@class='comments-item-main js-comments-item-main']");

                    if (commentsNodes != null)
                    {
                        for (int z = 0; z < commentsNodes.Count; z++)
                        {
                            try
                            {
                                string text = commentsNodes[z].SelectSingleNode("div[@class='comments-item-content js-comments-item-content']/p").InnerText.Replace("&quot;", " ").TrimStart().TrimEnd();
                                string userNickname = commentsNodes[z].SelectSingleNode("div[@class='comments-item-header js-comments-item-header']/a").InnerText.TrimStart().TrimEnd();
                                string dateTime = commentsNodes[z].SelectSingleNode("div[@class='comments-item-header js-comments-item-header']/a/time").InnerText.TrimStart().TrimEnd();
                                if (text != "" && userNickname != "")
                                    game.Themes[j].Comments.Add(new Comment() { Text = text, UserNickname = userNickname, Date = dateTime });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                    }
                }
            }

            HtmlNodeCollection platformsNodes = currentGameDocument.DocumentNode.SelectNodes("//div[@class='releases']/div[@class='release-item']/span[@class='platform']");
            HtmlNodeCollection genresNodes = currentGameDocument.DocumentNode.SelectNodes("//div[@class='genres']/a");

            string title = forumGameDocument.DocumentNode.SelectSingleNode("//div[@class='mezzanine-info']/a").InnerText.Replace("\n\r", "").Replace("&#039;", "'").TrimStart().TrimEnd();
            string date = forumGameDocument.DocumentNode.SelectSingleNode("//div[@class='mezzanine-info']/span").InnerText.Replace("\n\r", "").TrimStart().TrimEnd();
            double avgRating = double.Parse(forumGameDocument.DocumentNode.SelectSingleNode("//span[@class='value users js-game-rating-value']").InnerText.Replace("\n\r", "").TrimStart().TrimEnd().Replace(".", ","));

            List<string> platforms = new List<string>();
            List<string> genres = new List<string>();
            string imgSrc = currentGameDocument.DocumentNode.SelectSingleNode("//div[@class='gp-game-cover']/div/img").GetAttributeValue("src", "def");

            if (genresNodes != null)
            {
                for (int j = 0; j < genresNodes.Count; j++)
                {
                    genres.Add(genresNodes[j].InnerText.Replace("\n", "").Replace(",", "").TrimStart().TrimEnd());
                }
            }

            if (platformsNodes != null)
            {
                for (int j = 0; j < platformsNodes.Count; j++)
                {
                    string platformString = platformsNodes[j].InnerText.Replace("\n", "").TrimStart().TrimEnd();

                    if (platformString.Contains(","))
                    {
                        string[] platformArray = platformString.Split(',');
                        for (int k = 0; k < platformArray.Length; k++)
                        {
                            platforms.Add(platformArray[k].TrimStart().TrimEnd());
                        }
                    }
                    else
                    {
                        platforms.Add(platformString.TrimStart().TrimEnd());
                    }
                }
            }
            else
            {
                platforms.Add("TBA");
            }

            game.AvgRating = avgRating.ToString();
            game.Date = date;
            game.Title = title;
            List<string> gameGenres = genres;
            List<string> gamePlatforms = platforms;

            Console.WriteLine(game.Title);

            if (games[i].Requestdate != null)
            {
                using (GamechatContext db = new GamechatContext())
                {
                    Game gameFromDb = db.Games.Where(x => x.Title == game.Title).FirstOrDefault();
                    int id;

                    if (gameFromDb == null)
                    {
                        db.Games.Add(new Game() { Title = game.Title, AvgRating = game.AvgRating, Date = game.Date, Picture = GetImageBytes(imgSrc) });
                        db.SaveChanges();
                        Console.WriteLine(game.Title);

                        id = db.Games.Where(x => x.Title == game.Title).FirstOrDefault().Id;
                    }
                    else
                    {
                        id = gameFromDb.Id;
                    }

                    Gamelink currentLink = db.Gamelinks.Where(X => X.Link == games[i].Link).FirstOrDefault();
                    currentLink.Requestdate = DateTime.Now;
                    db.Gamelinks.Update(currentLink);
                    db.SaveChanges();

                    for (int j = 0; j < gameGenres.Count; j++)
                    {
                        using (GamechatContext dbJenre = new GamechatContext())
                        {
                            int currentJenreId = dbJenre.Jenres.Where(x => x.Title == gameGenres[j]).FirstOrDefault().Id;

                            GameJenre gameJenre = dbJenre.GameJenres.Where(x => x.GameId == id && x.JenreId == currentJenreId).FirstOrDefault();

                            if (gameJenre == null)
                            {
                                dbJenre.GameJenres.Add(new GameJenre() { GameId = id, JenreId = currentJenreId });
                                dbJenre.SaveChanges();
                            }
                        }
                    }
                    for (int j = 0; j < gamePlatforms.Count; j++)
                    {
                        using (GamechatContext dbPlatform = new GamechatContext())
                        {
                            int currentPlatformId = dbPlatform.Platforms.Where(x => x.Title == gamePlatforms[j]).FirstOrDefault().Id;

                            GamePlatform gamePlatform = dbPlatform.GamePlatforms.Where(x => x.GameId == id && x.PlatformId == currentPlatformId).FirstOrDefault();

                            if (gamePlatform == null)
                            {
                                dbPlatform.GamePlatforms.Add(new GamePlatform() { GameId = id, PlatformId = currentPlatformId });
                                dbPlatform.SaveChanges();
                            }
                        }
                    }
                    for (int j = 0; j < game.Themes.Count; j++)
                    {
                        using (GamechatContext dbTheme = new GamechatContext())
                        {
                            Theme theme = dbTheme.Themes.Where(x => x.Title == game.Themes[j].Title).FirstOrDefault();

                            if (theme == null)
                            {
                                dbTheme.Themes.Add(new Theme() { UserNickname = CreateRandomValueField(8), GameId = id, Title = game.Themes[j].Title.Replace(" &quot;", "") });
                                dbTheme.SaveChanges();
                                int currentThemeId = dbTheme.Themes.Where(x => x.Title == game.Themes[j].Title).FirstOrDefault().Id;

                                for (int z = 0; z < game.Themes[j].Comments.Count; z++)
                                {
                                    using (GamechatContext dbComment = new GamechatContext())
                                    {
                                        Comment comment = dbComment.Comments.Where(x => x.Text == game.Themes[j].Comments[z].Text).FirstOrDefault();

                                        if (comment == null)
                                        {
                                            try
                                            {
                                                dbComment.Comments.Add(new Comment() { Date = game.Themes[j].Comments[z].Date, CommentId = 0, Text = game.Themes[j].Comments[z].Text, UserNickname = game.Themes[j].Comments[z].UserNickname, ThemeId = currentThemeId });
                                                dbComment.SaveChanges();
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e.Message);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    currentLink.Parsingdate = DateTime.Now;
                    db.Gamelinks.Update(currentLink);
                    db.SaveChanges();
                }
            }
            else
            {
                using (GamechatContext db = new GamechatContext())
                {

                    db.Games.Add(new Game() { Title = game.Title, AvgRating = game.AvgRating, Date = game.Date, Picture = GetImageBytes(imgSrc) });
                    db.SaveChanges();

                    Gamelink currentLink = db.Gamelinks.Where(X => X.Link == games[i].Link).FirstOrDefault();
                    currentLink.Requestdate = DateTime.Now;
                    db.Gamelinks.Update(currentLink);
                    db.SaveChanges();

                    int id = db.Games.Where(x => x.Title == game.Title).FirstOrDefault().Id;
                    for (int j = 0; j < gameGenres.Count; j++)
                    {
                        using (GamechatContext dbJenre = new GamechatContext())
                        {
                            int currentJenreId = dbJenre.Jenres.Where(x => x.Title == gameGenres[j]).FirstOrDefault().Id;
                            dbJenre.GameJenres.Add(new GameJenre() { GameId = id, JenreId = currentJenreId });
                            dbJenre.SaveChanges();
                        }
                    }
                    for (int j = 0; j < gamePlatforms.Count; j++)
                    {
                        using (GamechatContext dbPlatform = new GamechatContext())
                        {
                            int currentPlatformId = dbPlatform.Platforms.Where(x => x.Title == gamePlatforms[j]).FirstOrDefault().Id;
                            dbPlatform.GamePlatforms.Add(new GamePlatform() { GameId = id, PlatformId = currentPlatformId });
                            dbPlatform.SaveChanges();
                        }
                    }
                    for (int j = 0; j < game.Themes.Count; j++)
                    {
                        using (GamechatContext dbTheme = new GamechatContext())
                        {
                            dbTheme.Themes.Add(new Theme() { UserNickname = CreateRandomValueField(8), GameId = id, Title = game.Themes[j].Title.Replace(" &quot;", "") });
                            dbTheme.SaveChanges();
                            int currentThemeId = dbTheme.Themes.Where(x => x.Title == game.Themes[j].Title).FirstOrDefault().Id;

                            for (int z = 0; z < game.Themes[j].Comments.Count; z++)
                            {
                                using (GamechatContext dbComment = new GamechatContext())
                                {
                                    try
                                    {
                                        dbComment.Comments.Add(new Comment() { Date = game.Themes[j].Comments[z].Date, CommentId = 0, Text = game.Themes[j].Comments[z].Text, UserNickname = game.Themes[j].Comments[z].UserNickname, ThemeId = currentThemeId });
                                        dbComment.SaveChanges();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
                                }
                            }
                        }
                    }

                    currentLink.Parsingdate = DateTime.Now;
                    db.Gamelinks.Update(currentLink);
                    db.SaveChanges();
                }
            }
        }

        public static string CreateRandomValueField(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }
}



