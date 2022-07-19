using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using TreeProject.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.IO;

namespace TreeProject.Models
{
    public class DataConnection
    {
        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; set; }
    }

    public interface IMyDatabase
    {
        void CheckTables();
        ObservableCollection<ITreeComponent> GetData();
        void RemoveData(ITreeComponent treeComponent);
        void EditData(ITreeComponent treeComponent);
        void AddData(ITreeComponent treeComponent);
    }
    public class MyDatabase : IMyDatabase
    {
        string connectionString;
        //переменные для получения  данных
        int CurrentID = -1;
        int ID;
        string Type, Product;
        int IDPARENT = 0;
        string AttributeName = String.Empty;
        string Attribute = String.Empty;
        string LinkName = String.Empty;
        ITreeComponent currentComponent = null;
        public MyDatabase()
        {
            //Строка подключения к БД
            try
            {
                string jsonString = String.Empty;
                using (StreamReader reader = File.OpenText("appsettings.json"))
                {
                    jsonString = reader.ReadToEnd();
                }
                DataConnection dataConnection = Newtonsoft.Json.JsonConvert.DeserializeObject<DataConnection>(jsonString);
                connectionString = dataConnection.ConnectionString;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //Проверка наличия таблиц, если их нет, то они создаются
        public void CheckTables()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                string sql = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ObjectsTable_ForTree') " +
                "CREATE TABLE  ObjectsTable_ForTree (id INT PRIMARY KEY IDENTITY, type TEXT NOT NULL, product TEXT NOT NULL); " +
                "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LinksTable_ForTree') " +
                "CREATE TABLE  LinksTable_ForTree (idparent INT, idchild INT, linkname TEXT NOT NULL, " +
                                "FOREIGN KEY (idparent)  REFERENCES ObjectsTable_ForTree (id)  ON DELETE CASCADE); " +
                "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AttributesTable_ForTree') " +
                "CREATE TABLE  AttributesTable_ForTree (id INT, name TEXT NOT NULL, value TEXT NOT NULL " +
                                "FOREIGN KEY (id)  REFERENCES ObjectsTable_ForTree (id)  ON DELETE CASCADE) ";
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = sql;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        #region Получение данных
        //получение данных из БД
        public ObservableCollection<ITreeComponent> GetData()
        {
            ObservableCollection<ITreeComponent> ComponentsFromDB = new ObservableCollection<ITreeComponent>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT ObjectsTable_ForTree.id AS ID, ObjectsTable_ForTree.type AS Type, " +
                        "ObjectsTable_ForTree.product AS Product, LinksTable_ForTree.idparent AS IDPARENT, LinksTable_ForTree.linkname AS LinkName, " +
                        "AttributesTable_ForTree.name AS AttributeName, AttributesTable_ForTree.value AS Attribute FROM ObjectsTable_ForTree " +
                        "LEFT OUTER  JOIN LinksTable_ForTree ON ObjectsTable_ForTree.id = LinksTable_ForTree.idchild " +
                        "LEFT OUTER  JOIN AttributesTable_ForTree ON ObjectsTable_ForTree.id = AttributesTable_ForTree.id " +
                        "ORDER BY ObjectsTable_ForTree.id";
                    SqlDataReader reader = command.ExecuteReader();
                    CurrentID = -1;
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ID = reader.GetInt32(0);
                            Type = reader.GetString(1);
                            Product = reader.GetString(2);
                            IDPARENT = 0;
                            AttributeName = String.Empty;
                            Attribute = String.Empty;
                            LinkName = String.Empty;
                            try
                            {
                                IDPARENT = reader.GetInt32(3);
                                LinkName = reader.GetString(4);
                            }
                            catch { }
                            try
                            {
                                AttributeName = reader.GetString(5);
                                Attribute = reader.GetString(6);
                            }
                            catch { }
                            if (ID != CurrentID)
                            {
                                currentComponent = new TreeComponent(ID, LinkName, Product, Type) { IdParent = IDPARENT };
                                ComponentsFromDB.Add(currentComponent);
                            }
                            if (AttributeName != String.Empty)
                            {
                                currentComponent?.Attributes.Add(new AttributesComponent() { Name = AttributeName, Value = Attribute });

                            }
                            CurrentID = ID;
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return ComponentsFromDB;
        }
        #endregion

        #region Удаление данных
        //удаление данных
        public void RemoveData(ITreeComponent treeComponent)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "DELETE FROM ObjectsTable_ForTree WHERE id = @id; DELETE FROM LinksTable_ForTree WHERE idchild = @id ";
                    SqlParameter idParametr = new SqlParameter("id", treeComponent.Id);
                    command.Parameters.Add(idParametr);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        #endregion

        #region Редактирование данных
        //редактирование существующих данных
        public void EditData(ITreeComponent treeComponent)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "UPDATE ObjectsTable_ForTree SET type = @type, product = @product  WHERE id = @id; ";
                    SqlParameter idParametr = new SqlParameter("id", treeComponent.Id);
                    SqlParameter typeParametr = new SqlParameter("type", treeComponent.TypeProduct);
                    SqlParameter productParametr = new SqlParameter("product", treeComponent.Product);
                    command.Parameters.Add(idParametr);
                    command.Parameters.Add(typeParametr);
                    command.Parameters.Add(productParametr);
                    command.CommandText += "DELETE FROM LinksTable_ForTree WHERE idchild = @id; ";
                    if (treeComponent.IdParent > 0)
                    {
                        command.CommandText += "INSERT INTO LinksTable_ForTree (idparent, idchild, linkname) VALUES (@idparent, @id, @linkname); ";
                        SqlParameter IdParentParametr = new SqlParameter("idparent", treeComponent.IdParent);
                        SqlParameter linknameParametr = new SqlParameter("linkname", treeComponent.Linkname);
                        command.Parameters.Add(IdParentParametr);
                        command.Parameters.Add(linknameParametr);
                    }
                    command.CommandText += "DELETE FROM AttributesTable_ForTree WHERE id = @id; ";
                    command.CommandText += addAttributes(treeComponent, treeComponent.Id);

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        #endregion

        #region Добавление данных
        public void AddData(ITreeComponent treeComponent)
        {
            int newID = 0;
            //Добавление объекта в главную таблицу
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "INSERT INTO ObjectsTable_ForTree (type, product) VALUES (@type, @product);SET @id=SCOPE_IDENTITY()";
                    SqlParameter typePatametr = new SqlParameter("type", treeComponent.TypeProduct);
                    SqlParameter productParametr = new SqlParameter("product", treeComponent.Product);

                    SqlParameter idParametr = new SqlParameter
                    {
                        ParameterName = "@id",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(typePatametr);
                    command.Parameters.Add(productParametr);
                    command.Parameters.Add(idParametr);

                    command.ExecuteNonQuery();
                    newID = (int)idParametr.Value;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }

            //Добавление связей и атрибутов
            if (treeComponent.IdParent > 0 || treeComponent.Attributes.Count() > 0)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = "";
                        if (treeComponent.IdParent > 0)
                        {
                            command.CommandText = "INSERT INTO LinksTable_ForTree (idparent, idchild, linkname) VALUES (@idParentPatametr, @idPatametr, @linkParametr)";
                            SqlParameter idParentPatametr = new SqlParameter("idParentPatametr", treeComponent.IdParent);
                            SqlParameter idPatametr = new SqlParameter("idPatametr", newID);
                            SqlParameter linkParametr = new SqlParameter("linkParametr", treeComponent.Linkname);

                            command.Parameters.Add(idParentPatametr);
                            command.Parameters.Add(idPatametr);
                            command.Parameters.Add(linkParametr);
                        }
                        command.CommandText += addAttributes(treeComponent, newID);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }
            }
        }
        #endregion

        private string addAttributes(ITreeComponent treeComponent, int id)
        {
            string sql = "";
            if (treeComponent.Attributes.Count() > 0)
            {
                int CountAttr = treeComponent.Attributes.Count();
                sql = "INSERT INTO AttributesTable_ForTree (id, name, value) VALUES";
                foreach (AttributesComponent attribute in treeComponent.Attributes)
                {
                    sql += $"({id},'{attribute.Name}', '{attribute.Name}')";
                    if (treeComponent.Attributes.IndexOf(attribute) != CountAttr - 1)
                    {
                        sql += ",";
                    }
                }
            }
            return sql;
        }
    }
}
