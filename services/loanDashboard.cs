
using MySql.Data.MySqlClient;

namespace LOANS.services
{
    public class loanDashboard
    {
        dbServices ds = new dbServices();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();

        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public loanDashboard(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<responseData> GetLoanDashboard(requestData req)
        {
            responseData resData = new responseData();
            resData.rData["rCode"] = 0;
            resData.eventID = req.eventID;

            try
            {
                var list = new List<Dictionary<string, object>>();

                if (req.addInfo.ContainsKey("SUBSID"))
                {
                    string loanId = req.addInfo["SUBSID"].ToString();
                    

                    MySqlParameter[] myParams = new MySqlParameter[] {
                        new MySqlParameter("@LoanId", loanId),
                        
                    };

                    var sq = $"SELECT * FROM e_loan WHERE SUBSID = @LoanId";
                    var loanData = ds.executeSQL(sq, myParams);
                    if (loanData != null && loanData[0].Count() > 0){
                        resData.eventID = req.eventID;
                        resData.rData["rMessage"] = "Loan data retrieved successfully";
                        var loanDictList = new List<Dictionary<string, object>>();
                        foreach (var row in loanData[0])
                        {
                            var loanDict = new Dictionary<string, object>
                            {
                                {"LOAN_TYPE", row[2].ToString()},
                                {"SUBSID", row[3].ToString()},
                                {"FolioNumber", row[5].ToString()},
                                {"Amount_San", row[26].ToString()},
                                 
                                {"STATUS", row[37].ToString()},
                                {"RnD_Date", ConvertToDateString(row[40])}, 
                                };
                                if(row[37].ToString() == "0"){
                                    loanDict["Message"] = "In Progress";
                                }else{
                                    loanDict["Message"] = "Loan verfified";
                                }
                                loanDictList.Add(loanDict);
                             }
                                resData.rData["LoanData"] = loanDictList;
                            }else{
                                resData.rData["rCode"] = 1;
                                resData.rData["rMessage"] = "Loan not found";
                            }
                        }else{
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "SUBSID not provided in the request";                      
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



         public async Task<responseData> GetLoanType(requestData req ){
            responseData resData = new responseData();
            resData.rData["rCode"] = 0;
            resData.eventID = req.eventID;

            try
            {
                var list = new List<Dictionary<string, object>>();

                if (req.addInfo.ContainsKey("SUBSR_ID"))
                {
                    string loanType = req.addInfo["SUBSR_ID"].ToString();

                    MySqlParameter[] myParams = new MySqlParameter[] {
                        new MySqlParameter("@SUBSR_ID", loanType)
                    };


                   var sq = $"SELECT m_loan_type.LTCode,m_loan_type.LGCode,m_loan_type.LTName FROM m_subr INNER JOIN e_loan ON e_loan.SUBSID = m_subr.SUBSR_ID INNER JOIN m_loan_type ON m_loan_type.LTCode = e_loan.LoanType WHERE SUBSR_ID = @SUBSR_ID";
                    
                    var lt = ds.executeSQL(sq, myParams);

                    if (lt != null && lt[0].Count() > 0)
                    {
                        resData.eventID = req.eventID;
                        resData.rData["rMessage"] = " data retrieved successfully";
                        foreach (var row in lt[0])
                        {
                            
                            var typeDict = new Dictionary<string, object>
                            {
                                {"LTCode",row[0].ToString()}, 
                                {"LGCode",row[1]},
                                {"LTName", row[2].ToString()},
                              
                                };
                               
                                list.Add(typeDict);
                                }
                                resData.rData["ClaimData"] = list;
                                }
                                else
                                {
                                    resData.rData["rCode"] = 1;
                                    resData.rData["rMessage"] = "Loan Type not found";
                                }
                            }
                            else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "SUBSR_ID not provided in the request";
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
         private string ConvertToDateString(object date){
            if (date is DateTime)
            {
                return ((DateTime)date).ToString("dd-MM-yyyy");
            }
            return string.Empty;
        }

        
    }
    
}


