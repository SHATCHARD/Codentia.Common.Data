using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Codentia.Common.Data;
using Codentia.Common.Data.Provider;

namespace Codentia.Common.Data.Test.Context
{
    /// <summary>
    /// Wrapper around DbContext to permit testing
    /// </summary>
    public class TestContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestContext" /> class.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        public TestContext(string dbName) 
            : base(dbName)
        {
        }

        /// <summary>
        /// Procedures the data table.
        /// </summary>
        /// <returns>Test Data</returns>
        public DataTable ProcedureDataTable()
        {
            return this.ExecuteProcedure<DataTable>("TestProc003", null).Result;
        }

        /// <summary>
        /// Procedures the data set.
        /// </summary>
        /// <returns>Test Data</returns>
        public DataSet ProcedureDataSet()
        {
            return this.ExecuteProcedure<DataSet>("TestProc005", null).Result;
        }

        /// <summary>
        /// Procedures the string.
        /// </summary>
        /// <returns>Test Data</returns>
        public string ProcedureString()
        {
            return this.ExecuteProcedure<string>("TestProc_050", null).Result;
        }

        /// <summary>
        /// Procedures the bool.
        /// </summary>
        /// <returns>Test Data</returns>
        public bool ProcedureBool()
        {
            return ExecuteProcedure<bool>("TestProc_051", null).Result;
        }

        /// <summary>
        /// Procedures the int.
        /// </summary>
        /// <returns>Test Data</returns>
        public int ProcedureInt()
        {
            return this.ExecuteProcedure<int>("TestProc_052", null).Result;
        }

        /// <summary>
        /// Procedures the no return.
        /// </summary>
        public void ProcedureNoReturn()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("param1", DbType.Boolean, ParameterDirection.Output, false)
            };

            this.ExecuteProcedure<DBNull>("TestProc_010", parameters).Wait();
        }

        /// <summary>
        /// Queries the data table.
        /// </summary>
        /// <returns>Test Data</returns>
        public DataTable QueryDataTable()
        {
            return this.ExecuteQuery<DataTable>("SELECT 1, 2, 3", null).Result;
        }

        /// <summary>
        /// Queries the data set.
        /// </summary>
        /// <returns>Test Data</returns>
        public DataSet QueryDataSet()
        {
            return this.ExecuteQuery<DataSet>("SELECT 1, 2, 3", null).Result;
        }

        /// <summary>
        /// Queries the string.
        /// </summary>
        /// <returns>Test Data</returns>
        public string QueryString()
        {
            return this.ExecuteQuery<string>("SELECT 'test'", null).Result;
        }

        /// <summary>
        /// Queries the bool.
        /// </summary>
        /// <returns>Test Data</returns>
        public bool QueryBool()
        {
            return this.ExecuteQuery<bool>("SELECT 1", null).Result;
        }

        /// <summary>
        /// Queries the int.
        /// </summary>
        /// <returns>Test Data</returns>
        public int QueryInt()
        {
            return this.ExecuteQuery<int>("SELECT 42", null).Result;
        }

        /// <summary>
        /// Queries the no return.
        /// </summary>
        public void QueryNoReturn()
        {
            this.ExecuteQuery<DBNull>("UPDATE Table001 SET Column1 = Column 1", null).Wait();
        }

        /// <summary>
        /// Primes the test database.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void PrimeTestDatabase(string filename)
        {
            string text = System.IO.File.ReadAllText(filename);

            string splitter = text.IndexOf("GO\n") == -1 ? "~~" : "GO";
            string[] commands = Regex.Split(text, splitter);

            foreach (string command in commands)
            {
                this.ExecuteQuery<DBNull>(command, null).Wait();
            }
        }
    }
}
