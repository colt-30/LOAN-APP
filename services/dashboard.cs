using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace LOANS.services
{
    public class dashboard
    {
        dbServices ds = new dbServices();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();

        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        public dashboard(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<responseData> GetUserDashboard(requestData req)
        {
            responseData resData = new responseData();
            resData.rData["rCode"] = 0;
            resData.eventID = req.eventID;

            try
            {
                var list = new List<Dictionary<string, object>>();


                if (req.addInfo.ContainsKey("SUBSR_ID"))
                {
                    string subscriberId = req.addInfo["SUBSR_ID"].ToString();

                    MySqlParameter[] myParams = new MySqlParameter[] {
                new MySqlParameter("@SubscriberId", subscriberId)
            };


                    var sq = $"SELECT * FROM m_subr WHERE SUBSR_ID = @SubscriberId";
                    // ...
                    var userData = ds.executeSQL(sq, myParams);

                    if (userData != null && userData[0].Count() > 0)
                    {
                        resData.eventID = req.eventID;
                        resData.rData["rMessage"] = "User data retrieved successfully";

                        var userDict = new Dictionary<string, object>();

                        userDict.Add("SUBSR_ID", userData[0][0][0].ToString());
                        userDict.Add("MNU_ID", userData[0][0][1].ToString());
                        userDict.Add("SUB_MENU_ID", userData[0][0][2].ToString());
                        userDict.Add("SERV_PRE", userData[0][0][3].ToString());
                        userDict.Add("SERV_NO", userData[0][0][4].ToString());
                        userDict.Add("SERV_SUF", userData[0][0][5].ToString());
                        userDict.Add("OLD_ARMY_PRE", userData[0][0][6].ToString());
                        userDict.Add("OLD_ARMY_NO", userData[0][0][7].ToString());
                        userDict.Add("OLD_ARMY_NO_SUF", userData[0][0][8].ToString());
                        userDict.Add("SubscriberName", userData[0][0][9].ToString());
                        userDict.Add("Rank", userData[0][0][10].ToString());
                        userDict.Add("EMAIL", userData[0][0][11].ToString());
                        userDict.Add("ALTERNATE_EMAIL", userData[0][0][12].ToString());
                        userDict.Add("MOBILE1", userData[0][0][13].ToString());
                        userDict.Add("MOBILE2", userData[0][0][14].ToString());
                        userDict.Add("AADHAR_NO", userData[0][0][15].ToString());
                        userDict.Add("PAN_NO", userData[0][0][16].ToString());
                        userDict.Add("CDA_AC_NO", userData[0][0][17].ToString());
                        userDict.Add("RANK_NAME", userData[0][0][18].ToString());
                        userDict.Add("PREFIX", userData[0][0][19].ToString());
                        userDict.Add("ARMY_NO", userData[0][0][20].ToString());
                        userDict.Add("SUFFIX", userData[0][0][21].ToString());
                        userDict.Add("LOAN_SERV_NO", userData[0][0][22].ToString());
                        userDict.Add("IC_NO_PFX", userData[0][0][23].ToString());
                        userDict.Add("IC_NUMBER", userData[0][0][24].ToString());
                        userDict.Add("IC_NO_SFX", userData[0][0][25].ToString());
                        userDict.Add("JC_NO_PFX", userData[0][0][26].ToString());
                        userDict.Add("JC_NUMBER", userData[0][0][27].ToString());
                        userDict.Add("JC_NO_SFX", userData[0][0][28].ToString());
                        userDict.Add("OR_NO_PFX", userData[0][0][29].ToString());
                        userDict.Add("OR_NUMBER", userData[0][0][30].ToString());
                        userDict.Add("OR_NO_SFX", userData[0][0][31].ToString());
                        userDict.Add("OLD_IC_NO_PFX", userData[0][0][32].ToString());
                        userDict.Add("OLD_IC_NUMBER", userData[0][0][33].ToString());
                        userDict.Add("OLD_IC_NO_SFX", userData[0][0][34].ToString());
                        userDict.Add("OLD_JC_NO_PFX", userData[0][0][35].ToString());
                        userDict.Add("OLD_JC_NUMBER", userData[0][0][36].ToString());
                        userDict.Add("OLD_JC_NO_SFX", userData[0][0][37].ToString());
                        userDict.Add("OLD_OR_NO_PFX", userData[0][0][38].ToString());
                        userDict.Add("OLD_OR_NUMBER", userData[0][0][39].ToString());
                        userDict.Add("OLD_OR_NO_SFX", userData[0][0][40].ToString());
                        userDict.Add("GC_NO_PFX", userData[0][0][41].ToString());
                        userDict.Add("GC_NUMBER", userData[0][0][42].ToString());
                        userDict.Add("GC_NO_SFX", userData[0][0][43].ToString());
                        userDict.Add("GC_PERIOD_FROM", userData[0][0][44].ToString());
                        userDict.Add("GC_PERIOD_TO", userData[0][0][45].ToString());
                        userDict.Add("GC_ENTRY_TYPE", userData[0][0][46].ToString());
                        userDict.Add("SL_NO_PFX", userData[0][0][47].ToString());
                        userDict.Add("SL_NUMBER", userData[0][0][48].ToString());
                        userDict.Add("SL_NO_SFX", userData[0][0][49].ToString());
                        userDict.Add("U_PASSCODE", userData[0][0][50].ToString());
                        list.Add(userDict);


                    }

                    else
                    {
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "User not found";
                    }
                    resData.rData["UserData"] = list;

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

    }
}