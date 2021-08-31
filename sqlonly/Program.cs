using System;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

namespace sqlonly
{
    class Program
    {

        static void Main(string[] args)
        {
            string connectionString = System.Environment.GetEnvironmentVariable("sqlconnectionstring");
            int numWorkers = 100;
            if (false == (int.TryParse(System.Environment.GetEnvironmentVariable("numworkers"), out numWorkers)))
            {
                numWorkers = 100;
            }            
            List<Worker> workerList = new List<Worker>();
            List<Task> taskList = new List<Task>();
            for(int i = 0; i < numWorkers; i++)
            {
                Worker w = new Worker() { ConnectionString= connectionString, WorkerId = i};
                workerList.Add(w);
                taskList.Add(Task.Factory.StartNew(w.RunTask));
            }
            
            Task.WaitAll(taskList.ToArray(), 500 * numWorkers);
            while(true)
            {
                long totalCount = 0;
                double longestWorkerDuration = 0;
                foreach(Worker w in workerList)
                {
                    totalCount += w.State.Count;
                    longestWorkerDuration = Math.Max(longestWorkerDuration, w.State.Duration);
                }
                if (longestWorkerDuration > 0)
                {
                    Console.WriteLine($"{longestWorkerDuration};{totalCount}; {totalCount/(longestWorkerDuration)}");
                }
                Task.WaitAll(taskList.ToArray(), 10000);
            }
        }


    }

    public class Worker
    {
        const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()[]{}-=_+;':,.<>";

        public string ConnectionString {get; set;} 
        public int WorkerId {get; set;}

        public WorkerState State = new WorkerState() {Count = 0, StartTime = DateTime.UtcNow };
    
        public void RunTask()
        {

            Random rand = new Random();

            using SqlConnection conn = new SqlConnection(ConnectionString);
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

                lock(State)
                {
                    State.Count++;
                }

            }
        }
    }

    public class WorkerState
    {
        public DateTime StartTime {get; set;}
        public long Count { get; set;}
        public double Duration {get { 
            double x = (DateTime.UtcNow.Subtract(StartTime).TotalSeconds);
            return x;
            }}

    }
}
