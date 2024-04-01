using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;

namespace LOANS.services
{
    public class Login
    {
        
     dbServices ds = new dbServices();
        
        private readonly Dictionary<string, string> _sms = new Dictionary<string, string>();

        private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _service_config = new Dictionary<string, string>();
        private serviceSmsSource _ss_sdc = new serviceSmsSource("sourceSMS");
        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        
        public Login()
        {
            // _sms["sender"] = appsettings["sourceSMS:sender"].ToString();
            // _sms["authkey"] = appsettings["sourceSMS:authkey"].ToString();
            // _sms["SmsServerUrl"] = appsettings["sourceSMS:SmsServerUrl"].ToString();
            // _sms["TemplateId"] = appsettings["sourceSMS:TemplateId"].ToString();

            jwt_config["Key"] = appsettings["jwt_config:Key"].ToString();
            jwt_config["Issuer"] = appsettings["jwt_config:Issuer"].ToString();
            jwt_config["Audience"] = appsettings["jwt_config:Audience"].ToString();
            jwt_config["Subject"] = appsettings["jwt_config:Subject"].ToString();
            jwt_config["ExpiryDuration_app"] = appsettings["jwt_config:ExpiryDuration_app"].ToString();
            jwt_config["ExpiryDuration_web"] = appsettings["jwt_config:ExpiryDuration_web"].ToString();
        }
        public async Task<responseData> login(requestData req)
        {
            responseData resData= new responseData();
            resData.rData["rCode"]=0;
            resData.eventID = req.eventID;
            try
            {
                string input = req.addInfo["UserId"].ToString();
                bool isMobileNumber = IsValidMobileNumber(input);
                string columnName;
                    if (isMobileNumber)
                    {
                        columnName = "MOBILE1";
                    }
                    else
                    {
                        columnName = "";
                    }

                    MySqlParameter[] myParams = new MySqlParameter[] {
                    new MySqlParameter("@UserId", input),
                    new MySqlParameter("@U_PASSCODE", req.addInfo["U_PASSCODE"].ToString())
                    };
                    var sq = $"SELECT * FROM agif_app.m_subr WHERE {columnName} = @UserId";
                    var data = ds.ExecuteSQLName(sq, myParams);
                    
                    if (data==null || data[0].Count()==0)
                    {
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "Invalid Credentials";
                    }
                    else
                    {
                        var subscriberId = data[0][0]["SUBSR_ID"].ToString();
                        var passwordInDatabase = data[0][0]["U_PASSCODE"].ToString();

                        if (!string.IsNullOrEmpty(passwordInDatabase))
                        {
                            if (VerifyPassword(req.addInfo["U_PASSCODE"].ToString(), passwordInDatabase))
                            {
                                var claims = new[]
                                {
                                    new Claim("SUBSR_ID", subscriberId),
                                    new Claim("guid", cf.CalculateSHA256Hash(req.addInfo["guid"].ToString())),
                                };
                        
                            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_config["Key"]));
                            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
                            var tokenDescriptor = new JwtSecurityToken(issuer: jwt_config["Issuer"], audience: jwt_config["Audience"], claims: claims,
                                expires: DateTime.Now.AddMinutes(Int32.Parse(jwt_config["ExpiryDuration_app"])), signingCredentials: credentials);
                            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
                    
                            resData.eventID = req.eventID;
                            resData.rData["rMessage"] = "Login Successfully";
                            resData.rData["id"]=data[0][0]["SUBSR_ID"];
                            resData.rData["Token"] = token;
                            
                            }
                            else
                            {
                                resData.rData["rCode"]=1;
                                resData.rData["rMessage"]="Check Your Password";
                            }
                        
                        }
                        else
                        {
                            resData.rData["rCode"]=2;
                            var otp = new Random().Next(100000,999999).ToString();
                            var MobNo = req.addInfo["UserId"].ToString();
                            DateTime currentTime = DateTime.Now;
                            DateTime expirationTime =currentTime.AddMinutes(10);
                            
                            var TemplateId="65af50f8d6fc0577c3686ef2";
                            // var isOtpSent = await ds.SendOtpLoan(MobNo, otp,TemplateId);

                            MySqlParameter[] par = new MySqlParameter[] {
                            new MySqlParameter("@Mobile_no", MobNo),
                            new MySqlParameter("@Valid", expirationTime),
                            new MySqlParameter("@Status",1),
                            new MySqlParameter("@OTP",otp),
                          
                        };
                        var check = $"SELECT U_PASSCODE FROM agif_app.m_subr WHERE {columnName} = @UserId";
                        var isEmpty= ds.ExecuteSQLName(check,myParams);

                        if(isEmpty !=null)
                        {
                            var otpInsert = $"insert into agif_app.OTP_reg(Mobile_no,Valid,Status,OTP)values(@Mobile_no,@Valid,@Status,@OTP)";
                            var insert=ds.executeSQL(otpInsert,par);
                             var msg = " OTP for Medskey Registration is "+otp.ToString()+". - SOURCEDOTCOM PVT LTD";
                               _ss_sdc.SendSMS(MobNo, msg, otp.ToString());

                        }
                        
                        resData.rData["rMessage"] = $"OTP generated Successfully";
                        
                    }
                }
            }
            catch (Exception ex)
            {
                resData.rData["rCode"]=1;
                resData.rData["rMessage"]=ex.Message;
            }
            return resData;
        }

        public async Task<responseData> CheckPassword(requestData req)
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
                    MySqlParameter[] myParams = new MySqlParameter[] {
                    new MySqlParameter("@UserId", input)
                };

                    var sq = $"SELECT U_PASSCODE FROM m_subr WHERE MOBILE1 = @UserId";
                    var data = ds.ExecuteSQLName(sq, myParams);

                    if (data == null || data[0].Count() == 0)
                    {
                        resData.rData["rCode"] = 2;
                        resData.rData["rMessage"] = "User not found";
                    }
                    else
                    {
                        var passwordInDatabase = data[0][0]["U_PASSCODE"].ToString();

                        if (!string.IsNullOrEmpty(passwordInDatabase))
                        {
                            resData.rData["rCode"] = 0;
                            resData.rData["rMessage"] = "Password exists for the user";

                        }
                        else
                        {
                            resData.rData["rCode"] = 1;

                            resData.rData["rMessage"] = "Password not created for the user";
                        }
                    }
                }
                else
                {
                    resData.rData["rCode"] = 2;
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

        


        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return enteredPassword == storedPassword;
        }
          public static bool IsValidEmail(string Email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(Email, pattern);
        }
        public static bool IsValidMobileNumber(string Mobile)
        {
            string pattern = @"^[0-9]{7,15}$";
            return Regex.IsMatch(Mobile, pattern);
        }
    }
}