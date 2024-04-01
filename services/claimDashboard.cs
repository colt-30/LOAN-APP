
using MySql.Data.MySqlClient;


namespace LOANS.services
{
    public class claimDashboard
    {
        dbServices ds = new dbServices();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();

        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public claimDashboard(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<responseData> GetClaimDashboard(requestData req)
        {
            responseData resData = new responseData();
            resData.rData["rCode"] = 0;
            resData.eventID = req.eventID;

            try
            {
                var list = new List<Dictionary<string, object>>();

                if (req.addInfo.ContainsKey("SUBSID"))
                {
                    string claimId = req.addInfo["SUBSID"].ToString();

                    MySqlParameter[] myParams = new MySqlParameter[] {
                        new MySqlParameter("@SUBSID", claimId)
                    };

                   var sq = $"SELECT * FROM e_claim_details WHERE SUBSID = @SUBSID";
                    
                    var claimData = ds.executeSQL(sq, myParams);

                    if (claimData != null && claimData[0].Count() > 0)
                    {
                        resData.eventID = req.eventID;
                        resData.rData["rMessage"] = "Claim data retrieved successfully";
                        foreach (var row in claimData[0])
                        {
                            var claimDict = new Dictionary<string, object>
                            {
                              
                                {"CLM_TYPE_ID", row[4].ToString()},
                                {"ApplyDate", ConvertToDateString(row[8])},
                                {"ActualAmount", row[16].ToString()},
                                {"STATUS", row[49].ToString()}
                                };
                                // if (row[49].ToString() == "0"){
                                //     claimDict["Message"] = "Pending";
                                // }else{
                                //     claimDict["Message"] = "Claimed";
                                // }

                                var res=$"SELECT CASE WHEN (e_claim_payout.PAY_TID>0) THEN 'Paid' ELSE 'Payment Pending' END PAYMENT_STATUS from e_claim_details  inner join e_claim_payout  on e_claim_details.T_ID = e_claim_payout.CLM_REF_T_ID where SUBSID = @SUBSID";
                                var check = ds.executeSQL(res,myParams);
                                list.Add(claimDict);
                                }
                                resData.rData["ClaimData"] = list;
                                }
                                else
                                {
                                    resData.rData["rCode"] = 1;
                                    resData.rData["rMessage"] = "Claim not found";
                                }
                            }
                            else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "ClmID not provided in the request";
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
                resData.rStatus = 199;
                resData.rData["rMessage"] = "REMOVE THIS ERROR IN PRODUCTION !!!  " + ex.Message.ToString();
            }

            return resData;
        }

   public async Task<responseData> GetClaimLoanType(requestData req)
{
    responseData resData = new responseData();
    resData.rData["rCode"] = 0;
    resData.eventID = req.eventID;

    try
    {
        var list = new List<Dictionary<string, object>>();

if (req.addInfo.ContainsKey("SUBSID"))
{
    string claimType = req.addInfo["SUBSID"].ToString();

    MySqlParameter[] myParams = new MySqlParameter[] {
        new MySqlParameter("@SUBSID", claimType)
    };

    var sq = $"select m_claim_type.CLM_CAT_1,  m_claim_type.CLM_CAT_2 from e_claim_details INNER JOIN m_claim_type on e_claim_details.CLM_TYPE_ID = m_claim_type.CLM_CAT_ID where SUBSID = @SUBSID";

    var ct = ds.executeSQL(sq, myParams);

    if (ct != null && ct.Count > 0 && ct[0].Count > 0)
    {
        resData.eventID = req.eventID;
        resData.rData["rMessage"] = " Claim Type data retrieved successfully";
       foreach (var row in ct[0])
{
    
    var typeDict = new Dictionary<string, object>
    {
        {"CLM_CAT_1", row[0].ToString()}, 
        {"CLM_CAT_2", row[1].ToString()},
 
        
    };

    list.Add(typeDict);
}

        resData.rData["ClaimTypeData"] = list;
    }
    else
    {
        resData.rData["rCode"] = 1;
        resData.rData["rMessage"] = "Claim Type not found or no data returned";
    }
}

        else
        {
            resData.rData["rCode"] = 1;
            resData.rData["rMessage"] = "CLM_CAT_ID not provided in the request";
        }
    }
    catch (Exception ex)
    {
        Console.Write(ex.Message.ToString());
        resData.rStatus = 199;
        resData.rData["rMessage"] = "REMOVE THIS ERROR IN PRODUCTION !!!  " + ex.Message.ToString();
    }

    return resData;
}

 public async Task<responseData> GetClaim(requestData req)
{
    responseData resData = new responseData();
    resData.rData["rCode"] = 0;
    resData.eventID = req.eventID;

    try
    {
        var list = new List<Dictionary<string, object>>();

if (req.addInfo.ContainsKey("SUBSID"))
{
    string claimType = req.addInfo["SUBSID"].ToString();

    MySqlParameter[] myParams = new MySqlParameter[] {
        new MySqlParameter("@SUBSID", claimType)
    };

    var sq = $"SELECT  e_claim_details.ApplyDate, e_claim_details.NetPayableAmount, e_claim_payout.PAY_TID FROM e_claim_details INNER JOIN e_claim_payout ON e_claim_details.T_ID = e_claim_payout.CLM_REF_T_ID WHERE SUBSID = @SUBSID";

    var ctId = ds.executeSQL(sq, myParams);

    if (ctId != null && ctId.Count > 0 && ctId[0].Count > 0)
    {
        resData.eventID = req.eventID;
        resData.rData["rMessage"] = "Data retrieved successfully";
       foreach (var row in ctId[0])
{
    
    var typeDict = new Dictionary<string, object>
    {
   
        {"ApplyDate", ConvertToDateString(row[0])},
        {"NetPayableAmount", row[1].ToString()},
        {"PAY_TID", row[2].ToString()},
    };
    list.Add(typeDict);
}


        resData.rData["ClaimedData"] = list;
    }
    else
    {
        resData.rData["rCode"] = 1;
        resData.rData["rMessage"] = " T_ID not found or no data returned";
    }
}

        else
        {
            resData.rData["rCode"] = 1;
            resData.rData["rMessage"] = "T_ID not provided in the request";
        }
    }
    catch (Exception ex)
    {
        Console.Write(ex.Message.ToString());
        resData.rStatus = 199;
        resData.rData["rMessage"] = "REMOVE THIS ERROR IN PRODUCTION !!!  " + ex.Message.ToString();
    }

    return resData;
}
                private string ConvertToDateString(object date)
                {
                    if (date is DateTime)
                    {
                        return ((DateTime)date).ToString("dd-MM-yyyy");
                    }
                return string.Empty;
        }
    }
}

