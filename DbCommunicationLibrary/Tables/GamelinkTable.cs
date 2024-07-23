using DbCommunicationLibrary.Entities;
using DbCommunicationLibrary.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCommunicationLibrary.Tables
{
    public class GamelinkTable
    {
        public List<Game> GetGameLinks()
        {
            List<Game> gamesWithLinks = new List<Game>();

            using (MySqlConnection connection = DbConnector.GetConnection())
            {
                connection.Open();


                using (MySqlCommand command = connection.CreateCommand())
                {

                    try
                    {
                        command.CommandText = $@"SELECT * FROM `gamelink` WHERE `date` IS NULL";
                        MySqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            gamesWithLinks.Add(new Game() { PageLink = reader.GetString("link") });
                        }

                        reader.Close();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                connection.Close();
            }
            return gamesWithLinks;
        }

        public void InsertGameLinks(List<string> links)
        {
            List<string> dbLinks = new List<string>();

            using (MySqlConnection connection = DbConnector.GetConnection())
            {
                connection.Open();


                using (MySqlCommand command = connection.CreateCommand())
                {

                    try
                    {
                        command.CommandText = $@"SELECT * FROM `gamelink`";
                        MySqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            dbLinks.Add(reader.GetString("link"));
                        }

                        reader.Close();

                        for (int i = 0; i < links.Count; i++)
                        {
                            bool result = false;
                            for (int j = 0; j < dbLinks.Count; j++)
                            {
                                if (links[i] == dbLinks[j])
                                {
                                    result = true;
                                    break;
                                }
                            }

                            if (result == false)
                            {
                                command.CommandText = $"INSERT INTO `gamelink`(`link`) VALUE ('{links[i]}');";
                                command.ExecuteNonQuery();
                            }
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
    }
}
