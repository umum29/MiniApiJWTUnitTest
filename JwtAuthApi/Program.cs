using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GuidService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//

builder.Services.AddSingleton<JwtHelpers>();

builder.Services
    //.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
        options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            // 一般我們都會驗證 Issuer
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

            // 通常不太需要驗證 Audience
            ValidateAudience = false,
            //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

            // 一般我們都會驗證 Token 的有效期間
            ValidateLifetime = true,

            // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
            ValidateIssuerSigningKey = false,

            // "1234567890123456" 應該從 IConfiguration 取得
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
        };
    });

builder.Services.AddAuthorization();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//in NET 7, it will automatically add Authentication/authorization based on builder.services.AddAuthentiction.....
//app.UseAuthentication();
//app.UseAuthorization();

// 登入並取得 JWT Token
app.MapPost("/signin", (LoginViewModel login, JwtHelpers jwt) =>
{
    if (ValidateUser(login))
    {
        var token = jwt.GenerateToken(login.Username);
        return Results.Ok(new { token });
    }
    else
    {
        return Results.BadRequest();
    }
})
    .WithName("SignIn")
    .AllowAnonymous();

// 取得 JWT Token 中的所有 Claims
app.MapGet("/claims", (ClaimsPrincipal user) =>
{
    return Results.Ok(user.Claims.Select(p => new { p.Type, p.Value }));
})
    .WithName("Claims")
    .RequireAuthorization();

// 取得 JWT Token 中的使用者名稱
app.MapGet("/username", (ClaimsPrincipal user) =>
{
    return Results.Ok(user.Identity?.Name);
})
    .WithName("Username")
    .RequireAuthorization();

// 取得使用者是否擁有特定角色
app.MapGet("/isInRole", (ClaimsPrincipal user, string name) =>
{
    return Results.Ok(user.IsInRole(name));
})
    .WithName("IsInRole")
    .RequireAuthorization();

// 取得 JWT Token 中的 JWT ID
app.MapGet("/jwtid", (ClaimsPrincipal user) =>
{
    return Results.Ok(user.Claims.FirstOrDefault(p => p.Type == "jti")?.Value);
})
    .WithName("JwtId")
    .RequireAuthorization();

//for unit testing
app.MapGet("/", () => "Hello World!");
app.MapGet("/guid", (GuidService guidService) => guidService.GetGuid());



app.Run();

bool ValidateUser(LoginViewModel login)
{
    return true;
}
record LoginViewModel(string Username, string Password);


// 增加 Program 類別公開宣告 for unit testing
public partial class Program { }

public class GuidService
{
    private string _src;
    public GuidService(string src = "SUT")
    {
        _src = src;
    }
    public string GetGuid() => $"{_src}:{Guid.NewGuid()}";
}