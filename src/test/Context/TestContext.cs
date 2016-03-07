using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codentia.Common.Data;

namespace Codentia.Common.Data.Test.Context
{
    /// <summary>
    /// Wrapper around DbContext to permit testing
    /// </summary>
    public class TestContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestContext"/> class.
        /// </summary>
        public TestContext() 
            : base("test")
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
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Procedures the string.
        /// </summary>
        /// <returns>Test Data</returns>
        public string ProcedureString()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Procedures the bool.
        /// </summary>
        /// <returns>Test Data</returns>
        public bool ProcedureBool()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Procedures the int.
        /// </summary>
        /// <returns>Test Data</returns>
        public bool ProcedureInt()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Procedures the no return.
        /// </summary>
        public void ProcedureNoReturn()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Queries the data table.
        /// </summary>
        /// <returns>Test Data</returns>
        public DataTable QueryDataTable()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Queries the data set.
        /// </summary>
        /// <returns>Test Data</returns>
        public DataSet QueryDataSet()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Queries the string.
        /// </summary>
        /// <returns>Test Data</returns>
        public string QueryString()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Queries the bool.
        /// </summary>
        /// <returns>Test Data</returns>
        public bool QueryBool()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Queries the int.
        /// </summary>
        /// <returns>Test Data</returns>
        public bool QueryInt()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Queries the no return.
        /// </summary>
        public void QueryNoReturn()
        {
            throw new System.NotImplementedException();
        }
    }
}
