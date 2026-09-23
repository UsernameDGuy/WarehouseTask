using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DariusTask
{
    class Program
    {

        static void Main(string[] args)
        {

            string connstr = @"Data Source=LAPTOP-Q3J2E8B8;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=SQL Server Management Studio;Command Timeout=0;Initial Catalog=ICAS_Test";
            try
            {

                var conn = new SqlConnection(connstr);
                conn.Open();

                if(args.Length == 0 || !int.TryParse(args[0], out int argSelect))
                {

                    Console.WriteLine("To use: WarehouseTask.exe {Number}");
                    return;

                }

                string tableName = GetTableName(conn);
                GetTable(conn);


                //We need to get that tablename finder back
                var cmd = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", conn);
                int rows = Convert.ToInt32(cmd.ExecuteScalar());
                string input = args[0]?.Trim();
                bool withinLoop = false;

                //Add a checker if there are duplicate TaskIDs
                //Add them all to a set to check

                //Input
                //SlotIndex has 99 and -4, which should be numerical, TaskID has two eights
                //Ask if they want taskID 8 Second one, to be changed to 9
                while(true)
                {
                    Console.WriteLine("\nType 'Q' to quit\nPlease select a device by its ID to check its details:");
                    if(withinLoop) {

                        input = Console.ReadLine()?.Trim();

                    }

                    if(int.TryParse(input, out int selection))
                    {

                        if(selection <= rows && selection > 0)
                        {

                            //Console.WriteLine($"{rows}");
                            cmd = new SqlCommand($"SELECT * FROM {tableName} WHERE EquipId = {selection}", conn);
                            var reader = cmd.ExecuteReader();
                            ReadReader(reader);
                            reader.Close();

                            CheckErrors(conn, selection, tableName);


                        }
                        else
                        {
                            Console.WriteLine("\nInvalid Selection!");

                        }

                    }
                    else if(input.ToLower() == "q")
                    {

                        return;

                    }
                    else
                    {
                        Console.WriteLine("\nInvalid Selection!");
                    }
                    Console.WriteLine("\n");
                    Console.WriteLine("Press Enter to Continue, or Q to exit");
                    if(Console.ReadLine().ToLower() == "q")
                    {

                        return;

                    }
                    Console.Clear();
                    GetTable(conn);
                    withinLoop = true;


                }

            }
            catch(SqlException error)
            {

                Console.WriteLine($"Connection Failed! {error.Number}: {error.Message}");

            }

        }

        static string GetTableName(SqlConnection conn)
        {

            var cmd = new SqlCommand("SELECT name FROM sys.tables;", conn);
            return cmd.ExecuteScalar()?.ToString();

        }

        static void CheckErrors(SqlConnection conn, int selection, string tableName)
        {

            //Checking for Duplicate TaskIDs and errorneous SlotIndex
            var cmd = new SqlCommand($"SELECT COUNT(*) FROM {tableName} WHERE TaskID = {selection}", conn);
            int rows = Convert.ToInt32(cmd.ExecuteScalar());
            cmd = new SqlCommand($"SELECT EquipID, TaskID FROM {tableName} WHERE TaskID = {selection}", conn);
            var reader = cmd.ExecuteReader();

            //If statement here
            if(rows > 1) {
               
                Console.WriteLine("\n\nDuplicates Detected in TaskID!");
                ReadReader(reader);
                //Fixing TaskID Error
           
                Console.WriteLine("\nWould you like to fix these errors? Type Y to fix");
                if(Console.ReadLine()?.Trim().ToLower() == "y")
                {

                    cmd = new SqlCommand($"UPDATE {tableName} SET TaskID = EquipID WHERE TaskID = {selection}", conn);
                    reader = cmd.ExecuteReader();
                    reader.Close();

                }

            }
            reader.Close();

            //Fix SlotIndex
            cmd = new SqlCommand($"SELECT EquipID, SlotIndex FROM {tableName} WHERE SlotIndex != {selection} - 1 AND EquipID = {selection}", conn);
            rows = Convert.ToInt32(cmd.ExecuteScalar());
            if(rows > 0) {
                Console.WriteLine("\nError with SlotIndex detected!");
                reader = cmd.ExecuteReader();
                ReadReader(reader);
                Console.WriteLine("\nWould you like to fix these errors? Type Y to fix");
                if(Console.ReadLine()?.Trim().ToLower() == "y") {

                    cmd = new SqlCommand($"UPDATE {tableName} SET SlotIndex = EquipId - 1 WHERE EquipID = {selection}", conn);
                    reader = cmd.ExecuteReader();
                    reader.Close();

                } 

            }

            //Ask if they would like to enable
            cmd = new SqlCommand($"SELECT Enable FROM {tableName} WHERE EquipID = {selection}", conn);
            reader = cmd.ExecuteReader();
            ReadReader(reader);
            Console.WriteLine("\nWould you like to enable this device? Type Y to enable Type N to disable");
            switch (Console.ReadLine()?.Trim().ToLower()) 
            {
                case "y":
                    cmd = new SqlCommand($"UPDATE {tableName} SET Enable = 1 WHERE EquipID = {selection}", conn);
                    reader = cmd.ExecuteReader();
                    reader.Close();
                    break;

                case "n":
                    cmd = new SqlCommand($"UPDATE {tableName} SET Enable = 0 WHERE EquipID = {selection}", conn);
                    reader = cmd.ExecuteReader();
                    reader.Close();
                    break;

            }


            //Look for all Nulls, then ask if they want to modify, then provide each one one at a time
            cmd = new SqlCommand($"SELECT * FROM {tableName} WHERE EquipID = {selection}", conn);
            reader = cmd.ExecuteReader();
            reader.Read();
            var names = new List<string>();
            var values = new List<object>();
            var types = new List<Type>();


            for(int i = 0; i < reader.FieldCount; i++) {

                if(reader.GetValue(i) == DBNull.Value) {

                    names.Add(reader.GetName(i));
                    values.Add(reader.GetValue(i));
                    types.Add(reader.GetFieldType(i));

                }

            }
            reader.Close();
            for(int i = 0; i < names.Count; i++)
            {

                if(values[i] != DBNull.Value)
                {

                    continue;

                }

                Console.Write($"{names[i]} is NULL. Input new value (Blank to skip): ");
                string input = Console.ReadLine()?.Trim();

                if(string.IsNullOrEmpty(input))
                {

                    continue;

                }

                Console.WriteLine(names[i] + "SDD");
                cmd = new SqlCommand($"UPDATE {tableName} SET {names[i]} = '{input}' WHERE EquipID = {selection}", conn);

                try
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"  {names[i]} set to {input}.");
                }
                catch(SqlException ex)
                {
                    Console.WriteLine($"  Failed: {ex.Message}");
                }
                
            }




            reader.Close();

        }

        static void ReadReader(SqlDataReader reader)
        {

            while(reader.Read())
            {

                for(int i = 0; i < reader.FieldCount; i++)
                {

                    string name = reader.GetName(i);
                    object value = reader.GetValue(i);
                    if(value == null || value == DBNull.Value)
                    {

                        value = "NULL";

                    }
                    else
                    {

                        value.ToString();

                    }
                    Console.Write($"{name}: {value} | ");

                }

                Console.WriteLine();

            }
            reader.Close();

        }

        static void GetTable(SqlConnection conn)
        {

            var cmd = new SqlCommand($"SELECT * FROM {GetTableName(conn)}", conn);
            var reader = cmd.ExecuteReader();

            for(int i = 0; i < reader.FieldCount; i++) {

                Console.Write($"{reader.GetName(i)}" + "|");

            }
            Console.WriteLine();
            while(reader.Read())
            {

                for(int i = 0; i < reader.FieldCount; i++)
                {

                    object value = reader.GetValue(i);
                    Console.Write(value + " | ");


                }
                Console.WriteLine();

            }

            reader.Close();


        }

    }

}
