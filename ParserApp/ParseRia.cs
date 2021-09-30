using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
namespace ParserApp
{
    public class ParseRia : Service1
    {
        public void ParseFromRia()
        {
            string img_src;
            string link;
            string title;
            string date;
            string content;
            string url;
            int category_id;
            HtmlDocument doc = new HtmlDocument();
            HtmlWeb web;
            DB db = new DB();
            string cs = @"server=localhost; userid=admin; password=1488; database=news";
            db.ConnectDB(cs);
            if (!categoriesParsed)
            {
                url = "https://ria.ru/";
                web = new HtmlWeb();
                try
                {
                    doc = web.Load(url);
                    this.EventLog.WriteEntry("Подключение к сайту " + url + " успешно.", EventLogEntryType.SuccessAudit);
                }
                catch
                {
                    this.EventLog.WriteEntry("Не удалось подключиться к сайту " + url + " ! Возможно отсутствует интернет соединение.", EventLogEntryType.Error);
                    return;
                }
                var m = doc.DocumentNode.SelectSingleNode("//div[@class = 'section m-vtv' and @data-section = '1']");
                var mCategories = m.SelectNodes(".//span[@class = 'cell-extension__item-title']");
                foreach (var cat in mCategories)
                {
                    string nameCat = cat.SelectSingleNode(".//span").InnerText;
                    Console.WriteLine(nameCat);
                    db.InsertCategories(nameCat);
                }
                categoriesParsed = true;
            }
            url = "https://ria.ru/lenta/";
            web = new HtmlWeb();
            try
            {
                doc = web.Load(url);
                this.EventLog.WriteEntry("Подключение к сайту " + url + " успешно.", EventLogEntryType.SuccessAudit);
            }
            catch
            {
                this.EventLog.WriteEntry("Не удалось подключиться к сайту" + url + " ! Возможно отсутствует интернет соединение.", EventLogEntryType.Error);
                return;
            }
            var htmlNodes = doc.DocumentNode.SelectNodes("//div[@class = 'list-item']");
            foreach (var node in htmlNodes)
            {
                img_src = node.SelectSingleNode(".//a/picture/img").Attributes["src"].Value;
                link = "https://ria.ru" + node.SelectSingleNode(".//a").Attributes["href"].Value;
                title = node.SelectSingleNode(".//meta[@itemprop = 'name']").Attributes["content"].Value;

                doc = web.Load(link);
                date = doc.DocumentNode.SelectSingleNode("//div[@class = 'article__info-date']/a").InnerText;
                var parsedDate = DateTime.Parse(date);
                var m_htmlNode = doc.DocumentNode.SelectNodes("//div[@data-type = 'text']");
                content = "";
                foreach (var t in m_htmlNode)
                {
                    content += t.SelectSingleNode(".//div[@class = 'article__text']").InnerText + " ";
                }
                content += "\n\n";
                m_htmlNode = doc.DocumentNode.SelectNodes("//a[@class = 'article__tags-item' or @class = 'article__tags-item color-btn-inverse']");
                List<string> categories = db.SelectCategories();
                foreach (var c in m_htmlNode)
                {
                    var category = c.InnerText;
                    if (!categories.Contains(category))
                    {
                        continue;
                    }
                    category_id = db.SelectCategoryId(category);
                    db.InsertDB(parsedDate, title, link, img_src, category_id, content);
                    break;
                }
            }
            if (db.count == 1)
                this.EventLog.WriteEntry("Добавлена " + db.count + " новая запись", EventLogEntryType.Information);
            else
                this.EventLog.WriteEntry("Добавлено " + db.count + " новых записей", EventLogEntryType.Information);
        }
    }

    public class DB:Service1
    {
        public int count;
        string sql;
        MySqlCommand cmd;
        MySqlConnection con;
        public DB()
        {
            count = 0;
        }
        public void ConnectDB(string cs)
        {
            con = new MySqlConnection(cs);
            try
            {
                con.Open();
                this.EventLog.WriteEntry("Соединение с БД установлено.", EventLogEntryType.SuccessAudit);
            }
            catch
            {
                this.EventLog.WriteEntry("Не удалось установить соединение с БД!", EventLogEntryType.Error);
                return;
            }
        }
        public void DisconnectDB()
        {
            con.Close();
            this.EventLog.WriteEntry("Соединение с БД закрыто.", EventLogEntryType.SuccessAudit);
        }
        public void InsertCategories(string category)
        {
            sql = "INSERT INTO categories (category_name) " +
                "SELECT * FROM(SELECT @category) AS tmp " +
                "WHERE NOT EXISTS(" +
                "SELECT category_name FROM categories WHERE category_name = @category)";
            cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@category", category);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
        public List<string> SelectCategories()
        {
            sql = "SELECT category_name FROM categories";
            cmd = new MySqlCommand(sql, con);
            MySqlDataReader reader = cmd.ExecuteReader();
            List<string> categories = new List<string>();
            while (reader.Read())
            {
                categories.Add(reader[0].ToString());
            }
            reader.Close();
            return categories;
        }
        public int SelectCategoryId(string category)
        {
            sql = "SELECT category_id FROM categories " +
                "WHERE category_name = @category";
            cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@category", category);
            cmd.Prepare();
            int category_id = Convert.ToInt32(cmd.ExecuteScalar().ToString());
            return category_id;
        }
        public void InsertDB(DateTime parsedDate, string title, string link, string img_src, int category_id, string content)
        {
            sql = "SELECT href FROM news WHERE href = @link";
            cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@link", link);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader[0].ToString() != string.Empty)
                {
                    this.EventLog.WriteEntry("Запись " + link + " уже существует!", EventLogEntryType.Warning);
                    reader.Close();
                    return;
                }
            }
            reader.Close();

            sql = "INSERT INTO news(date_news, title, href, img_src, category_id, text_news) " +
                "SELECT * FROM (SELECT @date, @title, @link, @img_src, @category_id, @content) AS tmp " +
                "WHERE NOT EXISTS(" +
                "SELECT href FROM news WHERE href = @link)";
            cmd = new MySqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@date", parsedDate);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@link", link);
            cmd.Parameters.AddWithValue("@img_src", img_src);
            cmd.Parameters.AddWithValue("@category_id", category_id);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            this.EventLog.WriteEntry("Запись " + link + " добавлена!", EventLogEntryType.SuccessAudit);
            count++;
        }
    }
}
