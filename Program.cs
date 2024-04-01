using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;

using LOANS.services;


WebHost.CreateDefaultBuilder().
ConfigureServices(s =>
{
    IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    s.AddSingleton<VerifyOTP>();
    s.AddSingleton<Login>();
    s.AddSingleton<createPassword>();
    s.AddSingleton<dashboard>();
    s.AddSingleton<loanDashboard>();
    s.AddSingleton<claimDashboard>();
    s.AddSingleton<document>();





    s.AddHttpContextAccessor();
    s.AddAuthorization();
    s.AddAuthentication("SourceJWT").AddScheme<SourceJwtAuthenticationSchemeOptions, SourceJwtAuthenticationHandler>("SourceJWT", options =>
    {
        options.SecretKey = appsettings["jwt_config:Key"].ToString();
        options.ValidIssuer = appsettings["jwt_config:Issuer"].ToString();
        options.ValidAudience = appsettings["jwt_config:Audience"].ToString();
        options.Subject = appsettings["jwt_config:Subject"].ToString();
    });
    s.AddCors();
    s.AddControllers();

}).
Configure(app =>
 {
     app.UseStaticFiles();
     app.UseRouting();



     app.UseCors(options =>
         options.WithOrigins("https://localhost:7186", "http://localhost:5281")
         .AllowAnyHeader().AllowAnyMethod().AllowCredentials());

     app.UseAuthentication();
     app.UseAuthorization();
     app.UseEndpoints(e =>
     {
         var logins = e.ServiceProvider.GetRequiredService<Login>();
         var otp=e.ServiceProvider.GetRequiredService<VerifyOTP>();
         var password = e.ServiceProvider.GetRequiredService<createPassword>();
         var dash = e.ServiceProvider.GetRequiredService<dashboard>();
         var ldash = e.ServiceProvider.GetRequiredService<loanDashboard>();
         var cdash = e.ServiceProvider.GetRequiredService<claimDashboard>();
         var documents = e.ServiceProvider.GetRequiredService<document>();

         
         e.MapPost("document",
         [AllowAnonymous] async (HttpContext http) =>
         {
             var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
             requestData rData = JsonSerializer.Deserialize<requestData>(body);
             if (rData.eventID == "1001")
                 await http.Response.WriteAsJsonAsync(await documents.GetDocument(rData));

         });

         e.MapPost("login",
         [AllowAnonymous] async (HttpContext http) =>
         {
             var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
             requestData rData = JsonSerializer.Deserialize<requestData>(body);
             if (rData.eventID == "1001")
                 await http.Response.WriteAsJsonAsync(await logins.login(rData));
            else if(rData.eventID == "1002")
                await http.Response.WriteAsJsonAsync(await logins.CheckPassword(rData));

         });

         e.MapPost("otpVerify",
       [AllowAnonymous] async (HttpContext http) =>
       {
           var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
           requestData rData = JsonSerializer.Deserialize<requestData>(body);
           if (rData.eventID == "1001")
               await http.Response.WriteAsJsonAsync(await otp.verifyOTP(rData));

       });



         e.MapPost("createPassword",
          [AllowAnonymous] async (HttpContext http) =>
          {
              var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
              requestData rData = JsonSerializer.Deserialize<requestData>(body);

              if (rData.eventID == "1001")
              {
                  // Extract the new password from the requestData
                  string newPassword = rData.addInfo.ContainsKey("NewPassword")
                                       ? rData.addInfo["NewPassword"].ToString()
                                       : string.Empty;

                  await http.Response.WriteAsJsonAsync(await password.CreatePassword(rData, newPassword));
              }
          });

         e.MapPost("dashboard",
       [Authorize(AuthenticationSchemes = "SourceJWT")] async (HttpContext http) =>
       {
           var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
           requestData rData = JsonSerializer.Deserialize<requestData>(body);
           if (rData.eventID == "1001")
               await http.Response.WriteAsJsonAsync(await dash.GetUserDashboard(rData));

       });


         e.MapPost("ldashboard",
       [AllowAnonymous] async (HttpContext http) =>
       {
           var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
           requestData rData = JsonSerializer.Deserialize<requestData>(body);
           if (rData.eventID == "1001")
               await http.Response.WriteAsJsonAsync(await ldash.GetLoanDashboard(rData));
            else if(rData.eventID == "1002")
                await http.Response.WriteAsJsonAsync(await ldash.GetLoanType(rData));

       });

         e.MapPost("cdashboard",
       [AllowAnonymous] async (HttpContext http) =>
       {
           var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
           requestData rData = JsonSerializer.Deserialize<requestData>(body);
           if (rData.eventID == "1001")
               await http.Response.WriteAsJsonAsync(await cdash.GetClaimDashboard(rData));
            else if(rData.eventID=="1002")
                await http.Response.WriteAsJsonAsync(await cdash.GetClaimLoanType(rData));
            else if (rData.eventID=="1003")
                await http.Response.WriteAsJsonAsync(await cdash.GetClaim(rData));

       });
         e.MapGet("/bing",
                //async c => await c.Response.WriteAsJsonAsync(await contactService.GetAll()));
                //async c => await c.Response.WriteAsync("Hello how are you"));
                async c => await c.Response.WriteAsJsonAsync("{'Name':'Anish','Age':'26','Project':'Diagnostic'}"));

     });

 }).Build().Run();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
public record requestData
{
    [Required]
    public string eventID { get; set; }
    [Required]
    public IDictionary<string, object> addInfo { get; set; }

}

public record responseData
{
    public responseData()
    {
        eventID = "";
        rStatus = 0;
        rData = new Dictionary<string, object>();
    }
    [Required]
    public int rStatus { get; set; } = 0;
    public string eventID { get; set; }
    public IDictionary<string, object> addInfo { get; set; }
    public IDictionary<string, object> rData { get; set; }
}













