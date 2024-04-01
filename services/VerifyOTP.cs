using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace LOANS.services
{
    public class VerifyOTP
    {
        private readonly dbServices db = new dbServices();

        private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();
        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public  async Task<responseData>verifyOTP(requestData req)
        {
            responseData resData = new responseData();
            resData.rData["rcode"] =0;

            try
            {
                MySqlParameter []verify = new MySqlParameter[]{
                new MySqlParameter("@OTP",req.addInfo["OTP"].ToString()),
                new MySqlParameter("Mobile_no",req.addInfo["Mobile_no"].ToString()),
                
                
                };

                var sq = @"SELECT * FROM agif_app.OTP_reg WHERE Mobile_no=@Mobile_no AND OTP=@OTP ";
                var data =db.executeSQL(sq,verify);

                if(data[0].Count()==0)
                {
                    resData.rData["rCode"] =1;
                    resData.rData["rMessage"] ="Invalid OTP";
                }
                else
                {
                    MySqlParameter[] myPar = new MySqlParameter[] {
                        new MySqlParameter("@Mobile_no", req.addInfo["Mobile_no"]),
                        new MySqlParameter("@Status",2)
                        
                    };

                    var query = @"UPDATE agif_app.OTP_reg SET Status=@Status WHERE Mobile_no=@Mobile_no ";
                    var dbData = db.executeSQL(query, myPar);

                    resData.rData["rMessage"]="OTP Valid  Only for 10 min";
                    
                }

            }
            catch(Exception ex)
            {
                resData.rData["rcode"]=1;
                resData.rData["rMessage"]=ex.Message;

            }
            return resData;
        }
       
    }
}