using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace LOANS.services
{
    public class createPassword
    {
        private dbServices ds = new dbServices();

        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();


        public async Task<responseData> CreatePassword(requestData req, string newPassword)
        {
            responseData resData = new responseData();
            resData.rData["rCode"] = 0;
            resData.eventID = req.eventID;

            try
            {
                string input = req.addInfo["UserId"].ToString();
                bool isMobileNumber = IsValidMobileNumber(input);

                if (isMobileNumber)
                {
                    MySqlParameter[] checkParams = new MySqlParameter[] {
                new MySqlParameter("@UserId", input)
            };

                    var checkQuery = $"SELECT * FROM m_subr WHERE MOBILE1 = @UserId";
                    var checkData = ds.ExecuteSQLName(checkQuery, checkParams);

                    if (checkData == null || checkData[0].Count() == 0)
                    {
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "User not found";
                    }
                    else
                    {
                        var subscriberId = checkData[0][0]["SUBSR_ID"].ToString();
                        var passwordInDatabase = checkData[0][0]["U_PASSCODE"].ToString();

                        if (string.IsNullOrEmpty(passwordInDatabase))
                        {
                            MySqlParameter[] updateParams = new MySqlParameter[] {
                        new MySqlParameter("@SubscriberId", subscriberId),
                        new MySqlParameter("@NewPassword", newPassword)
                    };

                            var updateQuery = $"UPDATE m_subr SET U_PASSCODE = @NewPassword WHERE SUBSR_ID = @SubscriberId";
                            ds.ExecuteSQLName(updateQuery, updateParams);

                            resData.rData["rMessage"] = "Password created and updated successfully";
                        }
                        else
                        {
                            resData.rData["rCode"] = 1;
                            resData.rData["rMessage"] = "User already has a password";
                        }
                    }
                }
                else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Invalid UserId";
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


        public static bool IsValidMobileNumber(string phoneNumber)
        {
            string pattern = @"^[0-9]{7,15}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }
}
