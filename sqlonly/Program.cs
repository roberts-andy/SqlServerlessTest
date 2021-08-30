﻿using System;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;


namespace sqlonly
{
    class Program
    {

        static void Main(string[] args)
        {

            List<Task> taskList = new List<Task>();
            
            for(int i = 0; i < 100; i++)
            {
                taskList.Add(Task.Factory.StartNew(new Worker().RunTask));
            }
            Task.WaitAll(taskList.ToArray());
        }

    }

    public class Worker
    {
        const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()[]{}-=_+;':,.<>";
        public void RunTask()
        {

            Random rand = new Random();

            using SqlConnection conn = new SqlConnection(System.Environment.GetEnvironmentVariable("sqlconnectionstring"));
            using SqlCommand cmd = new SqlCommand("uspNewTransaction", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(new SqlParameter[] 
            {
                new SqlParameter("transactionId", SqlDbType.UniqueIdentifier)
                , new SqlParameter("transactionTime", SqlDbType.DateTime2)
                , new SqlParameter("transactionAmount", SqlDbType.Decimal)
                , new SqlParameter("paymentToken", SqlDbType.NVarChar, 256)
            });

            while(true)
            {

                cmd.Parameters["transactionId"].Value = Guid.NewGuid();
                cmd.Parameters["transactionTime"].Value = DateTime.UtcNow;
                cmd.Parameters["transactionAmount"].Value = Math.Round(rand.NextDouble() * 1000, 2);
                cmd.Parameters["paymentToken"].Value = new string(Enumerable.Repeat(characters, 256).Select(s => s[rand.Next(s.Length)]).ToArray());

                try
                {
                    conn.Open();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Exception {e.Message} opening connection retrying in 10 seconds");
                    System.Threading.Thread.Sleep(10000);
                }

                try
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Exception {e.Message} opening connection retrying in 10 seconds");
                }

                conn.Close();


            }

        }
    }
}
