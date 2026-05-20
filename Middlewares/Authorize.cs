//using P4_Backend_Car_App.Interfaces;
//using P4_Backend_Car_App.Services;
//using System.Security.Claims;

//namespace P4_Backend_Car_App.Middlewares
//{
//    public class Authorize
//    {
//        private readonly RequestDelegate _next;

//        public Authorize(RequestDelegate next)
//        {
//            _next = next;
//        }

//        public async Task Invoke(HttpContext context, ITokenService tokenService)
//        {
//            var token = context.Request.Cookies["car_app_token"];

//            if (!string.IsNullOrEmpty(token))
//            {
//                var userId = tokenService.VerifyToken(token);

//                var claims = new List<Claim>
//        {
//            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
//        };

//                context.User = new ClaimsPrincipal(
//                    new ClaimsIdentity(claims, "custom")
//                );
//            }

//            // OPTIONAL: admin protection
//            if (context.Request.Path.StartsWithSegments("/api/admin"))
//            {
//                var role = context.User?.FindFirst(ClaimTypes.Role)?.Value;

//                if (role != "Admin")
//                {
//                    context.Response.StatusCode = 403;
//                    await context.Response.WriteAsync("Access Denied");
//                    return;
//                }
//            }
//            await _next(context);
//        }
//    }
//}
