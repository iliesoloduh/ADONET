using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ADONET
{
    class Program
    {
        static DataSet dataSet = new DataSet("HomeworkDBSet");
        static string connectionString = ConfigurationManager.ConnectionStrings["sqlconnection"].ConnectionString;
        static SqlDataAdapter usersAdapter = new SqlDataAdapter();
        static DataTable usersTable;
        static DataTable resortsTable;
        
        static void Main(string[] args)
        {
            //CreateTwoTables();

            usersTable = GetUsersTable();
            resortsTable = GetResortsTable();

            usersAdapter = GetUsersAdapter();
            usersAdapter.Fill(usersTable);
            PrintUsersData();
            //usersAdapter.Dispose();
            Console.WriteLine("After updating Users table");

            UpdateRowInUsersTable(new User { FirstName = "Porumb", LastName = "Mihai", PhoneNumber = "060777888"}, 6);
            PrintUsersData();
            Console.WriteLine("After inserting into Users table");

            InsertRowInUsersTable(new User { FirstName = "Ciuperca", LastName = "Ion", PhoneNumber = "069555888" });
            PrintUsersData();
            Console.WriteLine("After deleting from Users table");

            DeleteRowInUsersTable(14);
            PrintUsersData();
        }

        public static void CreateTwoTables()
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var createTableUsers = "CREATE TABLE Users (" +
                                       "Id INT IDENTITY (1,1) PRIMARY KEY, " +
                                       "FirstName NVARCHAR(50) NOT NULL, " +
                                       "LastName NVARCHAR(50) NOT NULL, " +
                                       "PhoneNumber NVARCHAR(20) NOT NULL);";

                var createTableResorts = "CREATE TABLE Resorts (" +
                                         "Id INT IDENTITY (1,1) PRIMARY KEY, " +
                                         "Name NVARCHAR(50) NOT NULL, " +
                                         "Address NVARCHAR(50), " +
                                         "Capacity INT, " +
                                         "Price INT, " +
                                         "OwnerId INT, " +
                                         "CONSTRAINT FK_Resorts_Users FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE NO ACTION);";

                using (var sqlCommand = new SqlCommand(createTableUsers, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
                using (var sqlCommand = new SqlCommand(createTableResorts, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static DataTable GetUsersTable()
        {
            DataTable template = new DataTable("Users");
            template.Columns.Add("Id", typeof(int));
            template.Columns.Add("FirstName", typeof(string));
            template.Columns.Add("LastName", typeof(string));
            template.Columns.Add("PhoneNumber", typeof(string));

            return template;
        }

        private static DataTable GetResortsTable()
        {
            DataTable template = new DataTable("Resorts");
            template.Columns.Add("Id", typeof(int));
            template.Columns.Add("Name", typeof(string));
            template.Columns.Add("Address", typeof(string));
            template.Columns.Add("Capacity", typeof(int));
            template.Columns.Add("Price", typeof(int));
            template.Columns.Add("OwnerId", typeof(int));

            return template;
        }

        private static SqlDataAdapter GetUsersAdapter()
        {
            SqlDataAdapter tempAdapter = new SqlDataAdapter("SELECT * FROM Users;", connectionString);

            //setting up insert command
            tempAdapter.InsertCommand = new SqlCommand("INSERT INTO Users(FirstName, LastName, PhoneNumber) " + 
                "VALUES (@FirstName, @LastName, @PhoneNumber);");
            tempAdapter.InsertCommand.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50, "FirstName");
            tempAdapter.InsertCommand.Parameters.Add("@LastName", SqlDbType.NVarChar, 50, "LastName");
            tempAdapter.InsertCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20, "PhoneNumber");

            //setting up update command
            tempAdapter.UpdateCommand = new SqlCommand("UPDATE Users " + 
                "SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber " +
                "WHERE Id = @Id;");
            tempAdapter.UpdateCommand.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50, "FirstName");
            tempAdapter.UpdateCommand.Parameters.Add("@LastName", SqlDbType.NVarChar, 50, "LastName");
            tempAdapter.UpdateCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20, "PhoneNumber");
            SqlParameter updateParameter = tempAdapter.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int);
            updateParameter.SourceColumn = "Id";
            updateParameter.SourceVersion = DataRowVersion.Original;

            //setting up delete command
            tempAdapter.DeleteCommand = new SqlCommand("DELETE FROM Users WHERE Id = @Id;");
            SqlParameter deleteParameter = tempAdapter.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int);
            deleteParameter.SourceColumn = "Id";
            deleteParameter.SourceVersion = DataRowVersion.Original;

            return tempAdapter;
        }

        public static void PrintUsersData()
        {
            foreach (DataRow row in usersTable.Rows)
            {
                Console.WriteLine($"{row["Id"]} {row["FirstName"]} {row["LastName"]} {row["PhoneNumber"]}");
            }
        }

        public static void UpdateRowInUsersTable(User updatedUser, int id)
        {
            foreach (DataRow row in usersTable.Rows)
            {
                if ((int)row["Id"] == id)
                {
                    row["FirstName"] = updatedUser.FirstName;
                    row["LastName"] = updatedUser.LastName;
                    row["PhoneNumber"] = updatedUser.PhoneNumber;
                }
            }
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                usersAdapter.UpdateCommand.Connection = sqlConnection;
                usersAdapter.Update(usersTable);
            }
        }

        public static void InsertRowInUsersTable(User insertedUser)
        {
            DataRow row = usersTable.NewRow();
            row["FirstName"] = insertedUser.FirstName;
            row["LastName"] = insertedUser.LastName;
            row["PhoneNumber"] = insertedUser.PhoneNumber;
            usersTable.Rows.Add(row);

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                usersAdapter.InsertCommand.Connection = sqlConnection;
                usersAdapter.Update(usersTable);
            }
        }

        public static void DeleteRowInUsersTable(int id)
        {
            var row = usersTable.Select($"Id = {id}");
            row[0].Delete();
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                usersAdapter.DeleteCommand.Connection = sqlConnection;
                usersAdapter.Update(usersTable);
            }
        }
    }
}
