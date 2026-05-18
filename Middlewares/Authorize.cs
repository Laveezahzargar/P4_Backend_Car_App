namespace P4_Backend_Car_App.Middlewares
{
    public class Authorize
    {
        private readonly RequestDelegate _next;

        public Authorize(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var role = context.Request.Headers["Role"].ToString();

            if (context.Request.Path.StartsWithSegments("/api/admin"))
            {
                if (role != "admin")
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access Denied");
                    return;
                }
            }

            await _next(context);
        }
    }
}
