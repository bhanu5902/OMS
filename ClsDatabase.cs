using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterSamples
{
    public class clsDatabase
    {

        public SqlConnection sqlCon;
        public SqlCommand sqlCmd;
        public DataTable dtTable;
        public DataSet dSet;

        public clsDatabase()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public SqlConnection getOpenConnection()
        {
            sqlCon = new SqlConnection(ConfigurationManager.AppSettings["sqlCon"].ToString());
            sqlCon.Open();
            return sqlCon;
        }

        public void getCloseConnection()
        {
            sqlCmd.Dispose();
            sqlCon.Close();
        }

        public DataTable GetEmployeeAttributes(String empPK, string ProcName)
        {
            clsDatabase clsDB = new clsDatabase();
            try
            {

                Hashtable hTable = new Hashtable();
                hTable.Add("@EmployeePK", Convert.ToInt32(empPK));

                DataTable dTbl = clsDB.GetDataTable(ProcName, hTable);
                return dTbl;

            }
            catch
            {
                //Catch an exception here and throw the error on the screen

            }
            finally
            {
            }

            return null;
        }

        public DataTable GetClientAttributes(String clientPK, string ProcName)
        {
            clsDatabase clsDB = new clsDatabase();
            try
            {

                Hashtable hTable = new Hashtable();
                hTable.Add("@ClientPK", Convert.ToInt32(clientPK));

                DataTable dTbl = clsDB.GetDataTable(ProcName, hTable);
                return dTbl;

            }
            catch
            {
                //Catch an exception here and throw the error on the screen

            }
            finally
            {
            }

            return null;
        }

        #region Public Functions/Members

        /// <summary>
        ///  Connection String Fetch from web.config AppSettings
        /// </summary>
        public static string GetSqlConnectionString()
        {
            return ConfigurationManager.AppSettings["sqlCon"].ToString();// ConfigurationManager.AppSettings["_SqlConnection"];
        }

        /// <summary>
        /// Fetch values by using any select query
        /// </summary>
        /// <param name="selectQuery"></param>
        public static DataSet GetDatasetValue(string selectQuery)
        {
            //selectQuery = "select * from table" : Example
            //int MyFeedback = 0;
            SqlConnection MyConnection = null;
            SqlDataAdapter MyCommand = null;
            DataSet ds = new DataSet();
            try
            {
                String selectCmd = selectQuery;
                MyConnection = new SqlConnection(GetSqlConnectionString());
                MyCommand = new SqlDataAdapter(selectCmd, MyConnection);
                MyCommand.Fill(ds);
                return ds;
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                ds = null;
                throw new Exception(ex.ToString());
            }
            finally
            {
                if ((MyCommand != null))
                    MyCommand.Dispose();
                if ((MyConnection != null))
                    MyConnection.Dispose();
            }
        }

        /// <summary>
        /// Excute values by using DML operations
        /// </summary>
        /// <param name="dmlString"></param>
        public static int SetDatasetValue(string dmlString)
        {
            //dmlString = "Insert, Update and Delete on table" : Example
            String dmlCmd = dmlString.Replace("''", "null");
            int MyFeedback = 0;
            SqlConnection MyConnection = null;
            SqlCommand MyCommand = null;

            try
            {
                MyConnection = new SqlConnection(GetSqlConnectionString());
                MyCommand = MyConnection.CreateCommand();
                MyCommand.CommandText = dmlCmd;
                MyCommand.CommandType = CommandType.Text;
                MyConnection.Open();
                MyCommand.ExecuteNonQuery();

                //TODO: Return Feedback
                MyFeedback = 1;

            }
            catch
            {
                MyFeedback = 0;
                //throw new Exception(ex.ToString());
            }
            finally
            {
                if ((MyCommand != null))
                    MyCommand.Dispose();
                if ((MyConnection != null))
                    MyConnection.Dispose();
            }

            return MyFeedback;
        }

       


        public static DataSet GetDataFromSP(string MyStoredProcedure, List<SqlParameter> inputParams)
        {
            SqlConnection MyConnection = null;
            SqlCommand MyCommand = null;

            MyConnection = new SqlConnection(GetSqlConnectionString());
            MyCommand = MyConnection.CreateCommand();
            MyCommand.CommandText = MyStoredProcedure;
            MyCommand.CommandType = CommandType.StoredProcedure;
            MyConnection.Open();

            if (inputParams.Count > 0)
            {
                foreach (SqlParameter param in inputParams)
                {
                    MyCommand.Parameters.Add(param);
                }
            }

            SqlDataAdapter da = new SqlDataAdapter(MyCommand);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;
        }

        public static int SetExecuteSP(string MyStoredProcedure, string MyPrameterIP, string MyValueIP, string MyDataTypeIP)
        {

            string MyPrmOP = "@MyReturn";

            int MyExecuteFeedback = 0;

            SqlConnection MyConnection = null;
            SqlCommand MyCommand = null;

            SqlParameter MySqlParameterIP = default(SqlParameter);
            SqlParameter MySqlParameterOP = default(SqlParameter);
            char sv = clsDatabase.GetSeperator();
            string[] MyArrPrm = MyPrameterIP.Split(sv);
            string[] MyArrVal = MyValueIP.Split(sv);
            string[] MyArrTyp = MyDataTypeIP.Split(sv);

            if (MyArrPrm.Length != MyArrVal.Length & MyArrPrm.Length != MyArrTyp.Length)
                return MyExecuteFeedback;

            int vCnt = MyArrPrm.Length - 1;
            int vCtr = 0;
            SqlDbType vMySqlDBType = 0;

            try
            {
                MyConnection = new SqlConnection(GetSqlConnectionString());
                MyCommand = MyConnection.CreateCommand();
                MyCommand.CommandText = MyStoredProcedure;
                MyCommand.CommandType = CommandType.StoredProcedure;
                MyConnection.Open();

                //adding ipput parameter
                for (vCtr = 0; vCtr <= vCnt; vCtr++)
                {
                    if (MyArrTyp[vCtr] == "V")
                        vMySqlDBType = SqlDbType.VarChar;
                    if (MyArrTyp[vCtr] == "DT")
                        vMySqlDBType = SqlDbType.DateTime;
                    //If myArrTyp(vCtr) = "CH" Then vMySqlDBType = MySqlDbType.Char
                    if (MyArrTyp[vCtr] == "N")
                        vMySqlDBType = SqlDbType.Decimal;
                    if (MyArrTyp[vCtr] == "B")
                        vMySqlDBType = SqlDbType.Bit;
                    if (MyArrTyp[vCtr] == "NT")
                        vMySqlDBType = SqlDbType.NText;
                    if (MyArrTyp[vCtr] == "F")
                        vMySqlDBType = SqlDbType.Float;
                    MySqlParameterIP = MyCommand.Parameters.Add(MyArrPrm[vCtr], vMySqlDBType);
                    MySqlParameterIP.Direction = ParameterDirection.Input;
                    if ((MyArrVal[vCtr].ToLower()) != "{null}" && !string.IsNullOrEmpty(MyArrVal[vCtr]))
                    {
                        MySqlParameterIP.Value = MyArrVal[vCtr];
                    }
                }

                //adding output parameter
                MySqlParameterOP = MyCommand.Parameters.Add(MyPrmOP, SqlDbType.BigInt);
                MySqlParameterOP.Direction = ParameterDirection.Output;

                //executing
                MyCommand.ExecuteNonQuery();

                //getting feedback
                MyExecuteFeedback = int.Parse(MySqlParameterOP.Value.ToString());
            }
            catch
            {
                MyExecuteFeedback = 0;

            }
            finally
            {
                if ((MyCommand != null))
                    MyCommand.Dispose();
                if ((MyConnection != null))
                    MyConnection.Dispose();
            }

            return MyExecuteFeedback;
        }

        public static char GetSeperator()
        {
            return ';';
        }
        #endregion

        # region Private Data Members

        DataSet _psmstdataset;

        DataTable _psmstdatatable;

        SqlCommand _psmstcommandObject;

        SqlConnection _psmstconnectionObject;

        SqlDataAdapter _psmstdataAdapterObject;

        IDictionaryEnumerator _psmststoredProceduresParameters;

        //SqlParameter _psmstparameter;

        #endregion

        # region Public Data Members
        static SqlConnection GetConnectionObject()
        {
            SqlConnection scon = new SqlConnection();
            String strConn = ConfigurationManager.AppSettings["sqlCon"].ToString();//ConfigurationManager.ConnectionStrings["_SQLConStr"].ConnectionString;
            scon = new SqlConnection(strConn);
            //scon.Open();
            return scon;
        }

        /// <summary>
        /// This function accepts a sql stored procedure name and its parameters. Returns a dataset. 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="storedProcedureParameters"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string storedProcedure, Hashtable storedProcedureParameters)
        {
            if (string.IsNullOrEmpty(storedProcedure))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            if (storedProcedureParameters == null)
                throw new ArgumentNullException("Stored Procedure Parameters cannot be null", "storedProcedureParameters");

            if (storedProcedureParameters.Count == 0)
                throw new ArgumentException("Stored Procedure Parameters count cannot be zero", "storedProcedureParameters");


            _psmststoredProceduresParameters = storedProcedureParameters.GetEnumerator();

            _psmstcommandObject = new SqlCommand();
            _psmstcommandObject.CommandText = storedProcedure;
            _psmstcommandObject.CommandType = CommandType.StoredProcedure;

            while (_psmststoredProceduresParameters.MoveNext())
            {
                _psmstcommandObject.Parameters.AddWithValue(_psmststoredProceduresParameters.Key.ToString(), _psmststoredProceduresParameters.Value.ToString());

            }

            _psmstdataset = new DataSet();

            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject.CommandTimeout = 3600;
            _psmstcommandObject.Connection = _psmstconnectionObject;

            _psmstdataAdapterObject = new SqlDataAdapter();
            _psmstdataAdapterObject.SelectCommand = _psmstcommandObject;

            try
            {

                _psmstdataAdapterObject.Fill(_psmstdataset);

                return _psmstdataset;
            }
            catch (SqlException se)
            {
                //ErrorLog.LogThisError(se);

                throw;
            }
            catch (InvalidOperationException ioe)
            {
                //ErrorLog.LogThisError(ioe);

                throw;
            }

            finally
            {

            }
        }

        /// <summary>
        /// This function accepts a sql stored procedure name and returns a dataset. 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string storedProcedure)
        {

            if (string.IsNullOrEmpty(storedProcedure))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            _psmstcommandObject = new SqlCommand();
            _psmstcommandObject.CommandText = storedProcedure;
            _psmstcommandObject.CommandType = CommandType.StoredProcedure;

            _psmstdataset = new DataSet();

            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject.CommandTimeout = 3600;
            _psmstcommandObject.Connection = _psmstconnectionObject;

            _psmstdataAdapterObject = new SqlDataAdapter();
            _psmstdataAdapterObject.SelectCommand = _psmstcommandObject;

            try
            {

                _psmstdataAdapterObject.Fill(_psmstdataset);

                return _psmstdataset;
            }
            catch (SqlException se)
            {
                //ErrorLog.LogThisError(se);

                throw;
            }
            catch (InvalidOperationException ioe)
            {
                //ErrorLog.LogThisError(ioe);

                throw;
            }

            finally
            {
                _psmstconnectionObject.Close();
            }
        }

        /// <summary>
        /// This function accepts a sql stored procedure name and its parameters. Returns a datatable.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="storedProcedureParameters"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string storedProcedure, Hashtable storedProcedureParameters)
        {
            if (string.IsNullOrEmpty(storedProcedure))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            if (storedProcedureParameters == null)
                throw new ArgumentNullException("storedProcedureParameters cannot be null", "storedProcedureParameters");

            if (storedProcedureParameters.Count == 0)
                throw new ArgumentException("storedProcedureParameters count cannot be zero", "storedProcedureParameters");

            _psmststoredProceduresParameters = storedProcedureParameters.GetEnumerator();

            _psmstcommandObject = new SqlCommand();
            _psmstcommandObject.CommandText = storedProcedure;
            _psmstcommandObject.CommandType = CommandType.StoredProcedure;

            while (_psmststoredProceduresParameters.MoveNext())
            {
                _psmstcommandObject.Parameters.AddWithValue(_psmststoredProceduresParameters.Key.ToString(), _psmststoredProceduresParameters.Value.ToString());

            }
            _psmstdatatable = new DataTable();
            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject.CommandTimeout = 3600;
            _psmstcommandObject.Connection = _psmstconnectionObject;

            _psmstdataAdapterObject = new SqlDataAdapter();
            _psmstdataAdapterObject.SelectCommand = _psmstcommandObject;
            try
            {
                _psmstdataAdapterObject.Fill(_psmstdatatable);

                return _psmstdatatable;
            }
            catch (SqlException se)
            {
                //ErrorLog.LogThisError(se);

                throw;
            }
            catch (InvalidOperationException ioe)
            {
                //ErrorLog.LogThisError(ioe);

                throw;
            }

            finally
            {
                _psmstconnectionObject.Dispose();
                _psmstconnectionObject.Close();
            }
        }

        /// <summary>
        /// This function accepts a sql stored procedure name and returns a datatable. 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string storedProcedure)
        {
            if (string.IsNullOrEmpty(storedProcedure))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            _psmstcommandObject = new SqlCommand();
            _psmstcommandObject.CommandText = storedProcedure;
            _psmstcommandObject.CommandType = CommandType.StoredProcedure;

            _psmstdatatable = new DataTable();
            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject.CommandTimeout = 3600;
            _psmstcommandObject.Connection = _psmstconnectionObject;

            _psmstdataAdapterObject = new SqlDataAdapter();
            _psmstdataAdapterObject.SelectCommand = _psmstcommandObject;
            try
            {
                _psmstdataAdapterObject.Fill(_psmstdatatable);

                return _psmstdatatable;
            }
            catch (SqlException se)
            {
                // ErrorLog.LogThisError(se);

                throw;
            }
            catch (InvalidOperationException ioe)
            {
                //ErrorLog.LogThisError(ioe);

                throw;
            }

            finally
            {
                _psmstconnectionObject.Dispose();
                _psmstconnectionObject.Close();
            }
        }

        /// <summary>
        /// This functions accepts stored procedure and its parameters and returns the number of rows effected. 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="storedProcedureParameters"></param>
        public int ExecuteNonQuery(string storedProcedure, Hashtable storedProcedureParameters)
        {

            if (string.IsNullOrEmpty(storedProcedure))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            if (storedProcedureParameters == null)
                throw new ArgumentNullException("StoredProcedureParameters cannot be null", "storedProcedureParameters");

            if (storedProcedureParameters.Count == 0)
                throw new ArgumentException("StoredProcedureParameters count cannot be zero", "storedProcedureParameters");


            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject = new SqlCommand(storedProcedure, _psmstconnectionObject);
            _psmstcommandObject.CommandType = CommandType.StoredProcedure;
            _psmstcommandObject.CommandTimeout = 3600;

            _psmststoredProceduresParameters = storedProcedureParameters.GetEnumerator();

            while (_psmststoredProceduresParameters.MoveNext())
            {
                SqlParameter _sqlParam;

                if (_psmststoredProceduresParameters.Value == null)
                {
                    _psmstcommandObject.Parameters.AddWithValue(_psmststoredProceduresParameters.Key.ToString(), System.DBNull.Value);
                }
                else if (_psmststoredProceduresParameters.Value.GetType() == typeof(byte[]))
                {
                    _sqlParam = _psmstcommandObject.Parameters.Add(_psmststoredProceduresParameters.Key.ToString(), SqlDbType.Binary, ((byte[])_psmststoredProceduresParameters.Value).Length);
                    _sqlParam.Value = _psmststoredProceduresParameters.Value;
                }
                else if (_psmststoredProceduresParameters.Value.GetType() == typeof(DataTable))
                {
                    _sqlParam = _psmstcommandObject.Parameters.Add(_psmststoredProceduresParameters.Key.ToString(), SqlDbType.Structured);
                    _sqlParam.Value = _psmststoredProceduresParameters.Value;
                }
                else
                {
                    _psmstcommandObject.Parameters.AddWithValue(_psmststoredProceduresParameters.Key.ToString(), _psmststoredProceduresParameters.Value.ToString());
                }
            }

            try
            {
                _psmstconnectionObject.Open();
                return _psmstcommandObject.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                //ErrorLog.LogThisError(ex);

                throw;
            }
            finally
            {
                _psmstconnectionObject.Close();
            }
        }

        public int ExecuteNonQuery(string storedProcedure)
        {

            if (string.IsNullOrEmpty(storedProcedure))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject = new SqlCommand(storedProcedure, _psmstconnectionObject);
            _psmstcommandObject.CommandType = CommandType.StoredProcedure;
            _psmstcommandObject.CommandTimeout = 3600;

            try
            {
                _psmstconnectionObject.Open();
                return _psmstcommandObject.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                //ErrorLog.LogThisError(ex);

                throw;
            }
            finally
            {
                _psmstconnectionObject.Close();
            }
        }

        /// <summary>
        /// This function accepts stored procedures and returns the first column of the first row in the result set returned by the query. 
        /// </summary>
        /// <param name="storedProcedure">Stored procedure to be executed</param>
        /// <returns></returns>
        public string ExecuteScalar(string storedProcedure)
        {
            if (string.IsNullOrEmpty(storedProcedure))
                throw new ArgumentNullException("SqlQuery cannot be null or empty", storedProcedure);

            _psmstconnectionObject = GetConnectionObject();

            _psmstcommandObject = new SqlCommand(storedProcedure, _psmstconnectionObject);

            _psmstcommandObject.CommandTimeout = 0;

            _psmstcommandObject.CommandType = CommandType.StoredProcedure;

            try
            {
                _psmstconnectionObject.Open();

                return Convert.ToString(_psmstcommandObject.ExecuteScalar());
            }
            catch (SqlException ex)
            {
                //ErrorLog.LogThisError(ex);

                throw;

            }
            finally
            {
                _psmstconnectionObject.Close();
            }

        }

        /// <summary>
        /// This function accepts stored procedures its parameters and returns the first column of the first row in the result set returned by the query. 
        /// </summary>
        /// <param name="storedProcedure">Stored procedure to be executed</param>
        /// <param name="storedProcedureParameters">Paramaters for the stored procedures</param>
        /// <returns></returns>
        public string ExecuteScalar(string storedProcedure, Hashtable storedProcedureParameters)
        {
            if (string.IsNullOrEmpty(storedProcedure))
                throw new ArgumentNullException("SqlQuery cannot be null or empty", storedProcedure);

            if (storedProcedureParameters == null)
                throw new ArgumentNullException("StoredProcedureParameters cannot be null", "storedProcedureParameters");

            if (storedProcedureParameters.Count == 0)
                throw new ArgumentException("StoredProcedureParameters count cannot be zero", "storedProcedureParameters");

            _psmstconnectionObject = GetConnectionObject();

            _psmstcommandObject = new SqlCommand(storedProcedure, _psmstconnectionObject);

            _psmstcommandObject.CommandTimeout = 0;

            _psmstcommandObject.CommandType = CommandType.StoredProcedure;

            _psmststoredProceduresParameters = storedProcedureParameters.GetEnumerator();

            while (_psmststoredProceduresParameters.MoveNext())
            {

                SqlParameter _sqlParam;

                if (_psmststoredProceduresParameters.Value == null)
                {
                    _psmstcommandObject.Parameters.AddWithValue(_psmststoredProceduresParameters.Key.ToString(), System.DBNull.Value);
                }
                else if (_psmststoredProceduresParameters.Value.GetType() == typeof(byte[]))
                {
                    _sqlParam = _psmstcommandObject.Parameters.Add(_psmststoredProceduresParameters.Key.ToString(), SqlDbType.Binary, ((byte[])_psmststoredProceduresParameters.Value).Length);
                    _sqlParam.Value = _psmststoredProceduresParameters.Value;
                }
                else if (_psmststoredProceduresParameters.Value.GetType() == typeof(DataTable))
                {
                    _sqlParam = _psmstcommandObject.Parameters.Add(_psmststoredProceduresParameters.Key.ToString(), SqlDbType.Structured);
                    _sqlParam.Value = _psmststoredProceduresParameters.Value;
                }
                else
                {
                    _psmstcommandObject.Parameters.AddWithValue(_psmststoredProceduresParameters.Key.ToString(), _psmststoredProceduresParameters.Value.ToString());
                }
            }

            try
            {
                _psmstconnectionObject.Open();

                return Convert.ToString(_psmstcommandObject.ExecuteScalar());
            }
            catch (SqlException ex)
            {
                //ErrorLog.LogThisError(ex);

                throw;

            }
            finally
            {
                _psmstconnectionObject.Close();
            }

        }



        //Calling procedure datatavke

        ////public DataTable Goals(int GoalID)
        ////{
        ////    Hashtable htGoal = new Hashtable();
        ////    htGoal.Add("@ID", GoalID);
        ////    try
        ////    {
        ////        DataTable dTbl = objDataAccessLayer.GetDataTable("PMT_frmEmpIN_GOALS_GetEmpInGoalsByID", htGoal);
        ////        return dTbl;
        ////    }
        ////    catch (SqlException)
        ////    {
        ////        throw;
        ////    }
        ////    catch (InvalidOperationException)
        ////    {
        ////        throw;
        ////    }
        ////}

        ///Update/ Insert / Delete Procedure

        ////public int UpdateGoalStatusofEmp()
        ////{
        ////    try
        ////    {
        ////        Hashtable htUpdate = new Hashtable();

        ////        htUpdate.Add("@EmpID", EmpId);
        ////        htUpdate.Add("@GoalID", GoalID);
        ////        htUpdate.Add("@mgrID", MgrID);
        ////        htUpdate.Add("GoalStatus", Status);

        ////        return objDataAccessLayer.ExecuteNonQuery("UpdateGoalStatus", htUpdate);
        ////    }
        ////    catch (SqlException)
        ////    {
        ////        throw;
        ////    }
        ////    catch (InvalidOperationException)
        ////    {
        ////        throw;
        ////    }
        ////}



        /// <summary>
        /// This function accepts a sql stored procedure name and its parameters. Returns a dataset. 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="storedProcedureParameters"></param>
        /// <returns></returns>
        public DataSet GetDataSetForQuery(string strQuery, Hashtable strQueryParameters)
        {
            if (string.IsNullOrEmpty(strQuery))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            if (strQueryParameters == null)
                throw new ArgumentNullException("Stored Procedure Parameters cannot be null", "storedProcedureParameters");

            if (strQueryParameters.Count == 0)
                throw new ArgumentException("Stored Procedure Parameters count cannot be zero", "storedProcedureParameters");


            _psmststoredProceduresParameters = strQueryParameters.GetEnumerator();

            _psmstcommandObject = new SqlCommand();
            _psmstcommandObject.CommandText = strQuery;
            _psmstcommandObject.CommandType = CommandType.Text;

            while (_psmststoredProceduresParameters.MoveNext())
            {
                _psmstcommandObject.Parameters.AddWithValue(_psmststoredProceduresParameters.Key.ToString(), _psmststoredProceduresParameters.Value.ToString());

            }

            _psmstdataset = new DataSet();

            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject.CommandTimeout = 3600;
            _psmstcommandObject.Connection = _psmstconnectionObject;

            _psmstdataAdapterObject = new SqlDataAdapter();
            _psmstdataAdapterObject.SelectCommand = _psmstcommandObject;

            try
            {

                _psmstdataAdapterObject.Fill(_psmstdataset);

                return _psmstdataset;
            }
            catch (SqlException se)
            {
                //ErrorLog.LogThisError(se);

                throw;
            }
            catch (InvalidOperationException ioe)
            {
                //ErrorLog.LogThisError(ioe);

                throw;
            }

            finally
            {

            }
        }

        /// <summary>
        /// This function accepts a sql stored procedure name and returns a dataset. 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        public DataSet GetDataSetForQuery(string strQuery)
        {

            if (string.IsNullOrEmpty(strQuery))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            _psmstcommandObject = new SqlCommand();
            _psmstcommandObject.CommandText = strQuery;
            _psmstcommandObject.CommandType = CommandType.Text;

            _psmstdataset = new DataSet();

            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject.CommandTimeout = 3600;
            _psmstcommandObject.Connection = _psmstconnectionObject;

            _psmstdataAdapterObject = new SqlDataAdapter();
            _psmstdataAdapterObject.SelectCommand = _psmstcommandObject;

            try
            {

                _psmstdataAdapterObject.Fill(_psmstdataset);

                return _psmstdataset;
            }
            catch (SqlException se)
            {
                //ErrorLog.LogThisError(se);

                throw;
            }
            catch (InvalidOperationException ioe)
            {
                //ErrorLog.LogThisError(ioe);

                throw;
            }

            finally
            {
                _psmstconnectionObject.Close();
            }
        }

        /// <summary>
        /// This function accepts a sql stored procedure name and its parameters. Returns a datatable.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="storedProcedureParameters"></param>
        /// <returns></returns>
        public DataTable GetDataTableForQuery(string strQuery, Hashtable strQueryParameters)
        {
            if (string.IsNullOrEmpty(strQuery))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            if (strQueryParameters == null)
                throw new ArgumentNullException("storedProcedureParameters cannot be null", "storedProcedureParameters");

            if (strQueryParameters.Count == 0)
                throw new ArgumentException("storedProcedureParameters count cannot be zero", "storedProcedureParameters");

            _psmststoredProceduresParameters = strQueryParameters.GetEnumerator();

            _psmstcommandObject = new SqlCommand();
            _psmstcommandObject.CommandText = strQuery;
            _psmstcommandObject.CommandType = CommandType.Text;

            while (_psmststoredProceduresParameters.MoveNext())
            {
                _psmstcommandObject.Parameters.AddWithValue(_psmststoredProceduresParameters.Key.ToString(), _psmststoredProceduresParameters.Value.ToString());

            }
            _psmstdatatable = new DataTable();
            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject.CommandTimeout = 3600;
            _psmstcommandObject.Connection = _psmstconnectionObject;

            _psmstdataAdapterObject = new SqlDataAdapter();
            _psmstdataAdapterObject.SelectCommand = _psmstcommandObject;
            try
            {
                _psmstdataAdapterObject.Fill(_psmstdatatable);

                return _psmstdatatable;
            }
            catch (SqlException se)
            {
                //ErrorLog.LogThisError(se);

                throw;
            }
            catch (InvalidOperationException ioe)
            {
                //ErrorLog.LogThisError(ioe);

                throw;
            }

            finally
            {

            }
        }

        /// <summary>
        /// This function accepts a sql stored procedure name and returns a datatable. 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        public DataTable GetDataTableForQuery(string strQuery)
        {
            if (string.IsNullOrEmpty(strQuery))
                throw new ArgumentNullException("Stored Procedure cannot be null or empty", "storedProcedure");

            _psmstcommandObject = new SqlCommand();
            _psmstcommandObject.CommandText = strQuery;
            _psmstcommandObject.CommandType = CommandType.Text;

            _psmstdatatable = new DataTable();
            _psmstconnectionObject = GetConnectionObject();
            _psmstcommandObject.CommandTimeout = 3600;
            _psmstcommandObject.Connection = _psmstconnectionObject;

            _psmstdataAdapterObject = new SqlDataAdapter();
            _psmstdataAdapterObject.SelectCommand = _psmstcommandObject;
            try
            {
                _psmstdataAdapterObject.Fill(_psmstdatatable);

                return _psmstdatatable;
            }
            catch (SqlException se)
            {
                // ErrorLog.LogThisError(se);

                throw;
            }
            catch (InvalidOperationException ioe)
            {
                //ErrorLog.LogThisError(ioe);

                throw;
            }

            finally
            {
                _psmstconnectionObject.Close();
            }
        }
        #endregion
    }
}
