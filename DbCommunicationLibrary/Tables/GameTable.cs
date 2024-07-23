using DbCommunicationLibrary.Entities;
using DbCommunicationLibrary.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DbCommunicationLibrary.Tables
{
    public class GameTable
    {
        public void InsertGames(List<Game> games)
        {
            using (MySqlConnection connection = DbConnector.GetConnection())
            {
                connection.Open();

                using (MySqlCommand command = connection.CreateCommand())
                {
                    try
                    {
                        for (int i = 0; i < games.Count; i++)
                        {
                            command.CommandText = $"INSERT INTO `game`(`title`, `avgRating`, `picture`, `date`) VALUES ('{games[i].Title}', '{games[i].AvgRating.ToString()}', @image, '{games[i].Date}');";
                            command.Parameters.Add("@image", MySqlDbType.LongBlob).Value = DownLoadFile(games[i].ImgSrc);
                            command.ExecuteNonQuery();

                            command.CommandText = "SELECT last_insert_id();";
                            int gameId = Convert.ToInt32(command.ExecuteScalar());

                            if (games[i].Platforms != null)
                            {
                                for (int j = 0; j < games[i].Platforms.Count; j++)
                                {
                                    command.CommandText = $"SELECT * FROM `platform` WHERE `title` = '{games[i].Platforms[j]}';";

                                    MySqlDataReader reader = command.ExecuteReader();

                                    int platformId = 0;

                                    while (reader.Read())
                                        platformId = reader.GetInt32("id");

                                    reader.Close();

                                    command.CommandText = $"INSERT INTO `game_platfrorm` (`game_id`,`platform_id`) VALUES ({gameId},{platformId});";
                                    command.ExecuteNonQuery();

                                }

                            }

                            if (games[i].Genre != null)
                            {
                                for (int j = 0; j < games[i].Genre.Count; j++)
                                {
                                    command.CommandText = $"SELECT * FROM `jenre` WHERE `title` = '{games[i].Genre[j]}';";

                                    MySqlDataReader reader = command.ExecuteReader();

                                    int genreId = 0;

                                    while (reader.Read())
                                        genreId = reader.GetInt32("id");

                                    reader.Close();

                                    command.CommandText = $"INSERT INTO `game_jenre` (`game_id`,`jenre_id`) VALUES ({gameId},{genreId});";
                                    command.ExecuteNonQuery();

                                }
                            }

                            if (games[i].Themes != null)
                            {
                                for (int j = 0; j < games[i].Themes.Count; j++)
                                {
                                    command.CommandText = $"INSERT INTO `theme` (`title`, `gameId`) VALUES ('{games[i].Themes[j].Title}', {gameId});";
                                    command.ExecuteNonQuery();

                                    command.CommandText = "SELECT last_insert_id();";
                                    int themeId = Convert.ToInt32(command.ExecuteScalar());

                                    if (games[i].Themes[j].Comments != null)
                                    {
                                        for (int z = 0; z < games[i].Themes[j].Comments.Count; z++)
                                        {
                                            command.CommandText = $"INSERT INTO `comment` (`userId`,`themeId`,`text`, `commentId`, `date`) VALUES (0, {themeId}, '{games[i].Themes[j].Comments[z].Text}',0, '{games[i].Themes[j].Comments[z].DateTime}');";
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            command.CommandText = $"UPDATE `gamelink` SET `date` = '{DateTime.Now.ToString()}' WHERE `pagelink` = '{games[i].PageLink}'";

                            Console.WriteLine("Success.");

                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                connection.Close();
            }
        }
        public static byte[] DownLoadFile(string address)
        {
            WebClient client = new WebClient();

            byte[] data = client.DownloadData(address);

            return data;
        }
    }
}
