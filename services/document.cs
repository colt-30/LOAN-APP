
using MySql.Data.MySqlClient;


namespace LOANS.services
{
    public class document
    {
        dbServices ds = new dbServices();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();

        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public document(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<responseData> GetDocument(requestData req)
        {
            responseData resData = new responseData();
            resData.rData["rCode"] = 0;
            resData.eventID = req.eventID;

            try
            {
                var list = new List<Dictionary<string, object>>();

                if (req.addInfo.ContainsKey("SUBSR_ID") || req.addInfo.ContainsKey("FILE_NAME"))
                {
                    string documentId = req.addInfo.ContainsKey("SUBSR_ID") ? req.addInfo["SUBSR_ID"].ToString() : null;
                    string fileName = req.addInfo.ContainsKey("FILE_NAME") ? req.addInfo["FILE_NAME"].ToString() : null;

                    if (!string.IsNullOrEmpty(documentId) || !string.IsNullOrEmpty(fileName))
                    {
                        MySqlParameter[] myParams;
                        string sq;

                        if (!string.IsNullOrEmpty(documentId))
                        {
                            myParams = new MySqlParameter[] {
                                new MySqlParameter("@SUBSR_ID", documentId)
                            };
                            sq = $"SELECT e_document.DOCUMENT_NAME, e_document.ATTACHMENT FROM e_document WHERE SUBSR_ID = @SUBSR_ID";
                        }
                        else
                        {
                            myParams = new MySqlParameter[] {
                                new MySqlParameter("@FILE_NAME", fileName)
                            };
                            sq = $"SELECT e_document.DOCUMENT_NAME, e_document.ATTACHMENT FROM e_document WHERE FILE_NAME = @FILE_NAME";
                        }

                        var documentData = ds.executeSQL(sq, myParams);

                        if (documentData != null && documentData[0].Count() > 0)
                        {
                            resData.eventID = req.eventID;
                            resData.rData["rMessage"] = "Document retrieved successfully";

                            foreach (var row in documentData[0])
                            {
                                var documentDict = new Dictionary<string, object>
                                {
                                   {"DOCUMENT_NAME", row[0].ToString()},
                                   {"ATTACHMENT", row[1].ToString()}
                               
                                };
                                list.Add(documentDict);
                            }

                            resData.rData["DocumentData"] = list;
                        }
                        else
                        {
                            resData.rData["rCode"] = 1;
                            resData.rData["rMessage"] = "Document not found";
                        }
                    }
                    else
                    {
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "Document SUBSID and FILE_NAME not provided in the request";
                    }
                }
                else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Document SUBSID and FILE_NAME not provided in the request";
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
