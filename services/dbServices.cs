using MySql.Data.MySqlClient;
using RestSharp;




public class dbServices
{
    private readonly Dictionary<string, string> _sms = new Dictionary<string, string>();
    IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    //MySqlConnection conn = null; // this will store the connection which will be persistent 
    MySqlConnection connPrimary = null; // this will store the connection which will be persistent 
    MySqlConnection connReadOnly = null;

    public dbServices() // constructor
    {
        //_appsettings=appsettings;
        connectDBPrimary();
        connectDBReadOnly();
    }


    private void connectDBPrimary()
    {

        try
        {
            connPrimary = new MySqlConnection(appsettings["db:connStrPrimary"]);
            connPrimary.Open();
        }
        catch (Exception ex)
        {
            //throw new ErrorEventArgs(ex); // check as this will throw exception error
            Console.WriteLine(ex);
        }
    }
    private void connectDBReadOnly()
    {

        try
        {
            connReadOnly = new MySqlConnection(appsettings["db:connStrReadOnly"]);
            connReadOnly.Open();
        }
        catch (Exception ex)
        {
            //throw new ErrorEventArgs(ex); // check as this will throw exception error
            Console.WriteLine(ex);
        }
    }




    public List<List<Object[]>> executeSQL(string sq, MySqlParameter[] prms) // this will return the database response the last partameter is to allow selection of connectio id
    {
        MySqlTransaction trans = null;
        //ArrayList allTables=new ArrayList();
        List<List<Object[]>> allTables = new List<List<Object[]>>();

        try
        {
            if (connPrimary == null || connPrimary.State == 0)
                connectDBPrimary();

            trans = connPrimary.BeginTransaction();

            var cmd = connPrimary.CreateCommand();
            cmd.CommandText = sq;
            if (prms != null)
                cmd.Parameters.AddRange(prms);


            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                do
                {
                    List<Object[]> tblRows = new List<Object[]>();
                    while (dr.Read())
                    {
                        object[] values = new object[dr.FieldCount]; // create an array with sixe of field count
                        dr.GetValues(values); // save all values here
                        tblRows.Add(values); // add this to the list array
                    }
                    allTables.Add(tblRows);
                } while (dr.NextResult());
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            trans.Rollback(); // check these functions
            return null; // if error return null
        }
        Console.Write("Database Operation Completed Successfully");
        trans.Commit(); // check thee functions
        connPrimary.Close(); //here is close the connection
        return allTables; // if success return allTables
    }
    public List<Dictionary<string, object>[]> ExecuteSQLName(string query, MySqlParameter[] parameters)
    {
        MySqlTransaction transaction = null;
        List<Dictionary<string, object>[]> allTables = new List<Dictionary<string, object>[]>();

        try
        {
            if (connPrimary == null || connPrimary.State == 0)
                connectDBPrimary();

            transaction = connPrimary.BeginTransaction();

            using (MySqlCommand cmd = new MySqlCommand(query, connPrimary))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    do
                    {
                        List<Dictionary<string, object>> tblRows = new List<Dictionary<string, object>>();

                        while (reader.Read())
                        {
                            Dictionary<string, object> values = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object columnValue = reader.GetValue(i);
                                values[columnName] = columnValue;
                            }

                            tblRows.Add(values);
                        }

                        allTables.Add(tblRows.ToArray());
                    } while (reader.NextResult());
                }
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            if (transaction != null)
                transaction.Rollback();
            return null;
        }


        Console.WriteLine("Database Operation Completed Successfully");
        return allTables;
    }

    public int ExecuteInsertAndGetLastId(string sq, MySqlParameter[] prms)
    {
        MySqlTransaction trans = null;
        int lastInsertedId = -1;

        try
        {
            if (connPrimary == null || connPrimary.State == 0)
                connectDBPrimary();

            trans = connPrimary.BeginTransaction();

            var cmd = connPrimary.CreateCommand();
            cmd.CommandText = sq;
            if (prms != null)
                cmd.Parameters.AddRange(prms);

            // Execute the INSERT query
            cmd.ExecuteNonQuery();

            // Get the last inserted ID
            cmd.CommandText = "SELECT LAST_INSERT_ID();";
            lastInsertedId = Convert.ToInt32(cmd.ExecuteScalar());

        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            trans.Rollback();
        }

        trans.Commit();
        connPrimary.Close();

        return lastInsertedId;
    }


    public List<List<Object[]>> executeSQLpcmdb(string sq, MySqlParameter[] prms) // this will return the database response the last partameter is to allow selection of connectio id
    {

        MySqlTransaction trans = null;
        List<List<Object[]>> allTables = new List<List<Object[]>>();

        try
        {
            if (connReadOnly == null)
                connectDBReadOnly();

            trans = connReadOnly.BeginTransaction();

            var cmd = connReadOnly.CreateCommand();
            cmd.CommandText = sq;
            if (prms != null)
                cmd.Parameters.AddRange(prms);

            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                do
                {
                    List<Object[]> tblRows = new List<Object[]>();
                    while (dr.Read())
                    {
                        object[] values = new object[dr.FieldCount]; // create an array with sixe of field count
                        dr.GetValues(values); // save all values here
                        tblRows.Add(values); // add this to the list array
                    }
                    allTables.Add(tblRows);
                } while (dr.NextResult());
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            trans.Rollback(); // check these functions
            return null; // if error return null
        }
        Console.Write("Database Operation Completed Successfully");
        trans.Commit(); // check thee functions
        connReadOnly.Close(); //here is close the connection
        return allTables; // if success return allTables
    }
    public int commonAuditTrans(Dictionary<string, object> data)
    {
        int transData = 0;
        try
        {
            var events = data["EVENT"];
            var currentDateTime = $"{DateTime.UtcNow:yyyy-MM-dd} {(DateTime.UtcNow.TimeOfDay + new TimeSpan(5, 30, 0)):hh\\:mm}";
            MySqlParameter[] myparams = new MySqlParameter[]
               {
                    new MySqlParameter("@uId",data["uId"]),
                    new MySqlParameter("@roleId",data["roleId"]),
                    new MySqlParameter("@tDate",currentDateTime),
                    new MySqlParameter("@auditRemarks",data["AUDIT_REMARKS"])
               };

            if (events == "UPDATE_ENTRY")
            {
                var tId = data["tId"];

                var auditTarns = $"insert into e_com_audit_dets_dependra(T_ID ,U_ID,ROLE_ID,MNU_ID,REMARKS,ENTRY_DATE,AUDIT_REMARK) values({tId},@uId,@roleId,0,'{events}',@tDate,@auditRemarks)";
                var dbData = executeSQL(auditTarns, myparams);
                return transData;
            }
            else if (events == "DELETE_ENTRY")
            {

            }
            else
            {

                var uId = data["uId"];
                var sqTrans = @"insert into e_acc_trans_dependra(MNU_ID,ROLE_ID,T_DATE,U_ID,REF_DATE) values(0,@roleId,@tDate,@uId,@tDate)";
                // var sqTrans="insert into hlfppt.e_acc_trans(MNU_ID,SUB_MENU_ID,ROLE_ID,T_TYPE,T_NO,T_DATE,U_ID,ACC_ID,ALT_ACC_ID,REMARKS,MODE,REF_DATE,DRCR,FUND_ID,TRANS_STATUS,LAST_SEQ_NO,CUR_SEQ_NO,AUDIT_TYPE_ID,ACTION_TYPE_ID,VERIFY_STARTED,TOTAL_AMOUNT,INTEREST_RATE,RTGS_STATUS,BS_ID) values(@patId,@docId,@apptDate,@bookingDate,@facId,@slot,0)";
                transData = ExecuteInsertAndGetLastId(sqTrans, myparams);
                var auditTarns = $"insert into e_com_audit_dets_dependra(T_ID,U_ID,ROLE_ID,MNU_ID,REMARKS,ENTRY_DATE) values({transData},@uId,@roleId,0,'{events}',@tDate)";
                var auditData = executeSQL(auditTarns, myparams);
            }


        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
        }



        return transData;
    }
    public async Task<string> commonFunction(Dictionary<string, object> mydict)
    {

        try
        {
            var currentDateTime = $"{DateTime.UtcNow:yyyy-MM-dd} {(DateTime.UtcNow.TimeOfDay + new TimeSpan(5, 30, 0)):hh\\:mm}";
            MySqlParameter[] myParam = new MySqlParameter[]
            {
                    new MySqlParameter("@_event_category",mydict["_event_category"].ToString()),
                    new MySqlParameter("@_event_category_record_id",mydict["_event_category_record_id"]),
                    new MySqlParameter("@_to_user_id",mydict["_to_user_id"]),
                    new MySqlParameter("@_by_user_id",mydict["_by_user_id"]),
                    new MySqlParameter("@_entry_dateTime",currentDateTime),
                    new MySqlParameter("@_status",0),
            };
            var query = @"INSERT INTO e_notification_dependra(EVENT_CATEGORY, EVENT_CATEGORY_RECORD_ID, TO_USER_ID, BY_USER_ID, ENTRY_DATETIME, STATUS) VALUES(@_event_category,@_event_category_record_id,@_to_user_id,@_by_user_id,@_entry_dateTime,@_status)";
            var dbData = executeSQL(query, myParam);
        }
        catch (Exception ex)
        {

            Console.WriteLine($"An error occurred: {ex.Message}");
            // Optionally, rethrow the exception if you want it to propagate further
            throw;
        }
        return "Success";
    }

}