using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebServer;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var httpListener = new HttpListener();
        var dbContext = new DbContext();
        try
        {
            dbContext._dbConnection.Open();
            Console.WriteLine("Успешно открылось соединение");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        httpListener.Prefixes.Add("http://localhost:5000/");
        httpListener.Start();
        var weatherApiClient = new WeatherApiClient("075fa28a9b1197747f725b24ab92748c");

        while (httpListener.IsListening)
        {
            Console.WriteLine("Началось прослушивание");
            var context = await httpListener.GetContextAsync();
            var response = context.Response;
            var request = context.Request;
            var ctx = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                switch (request.Url?.LocalPath)
                {
                    case "/registration" when request.HttpMethod == "GET":
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/html";
                        var registrationFile = await File.ReadAllBytesAsync("public/registration.html", ctx.Token);
                        await context.Response.OutputStream.WriteAsync(registrationFile, ctx.Token);
                        break;

                    case "/registration/saveUser" when request.HttpMethod == "POST":
                        
                        var user = await SaveUser(context, ctx.Token);
                        Console.WriteLine("*");
                        if (user == null)
                            break;
                        await response.OutputStream.WriteAsync(
                            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            })), ctx.Token);
                        break;

                    case "/login" when request.HttpMethod == "GET":
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/html";
                        var loginFile = await File.ReadAllBytesAsync("public/login.html", ctx.Token);
                        await context.Response.OutputStream.WriteAsync(loginFile, ctx.Token);
                        break;

                    case "/login/user" when request.HttpMethod == "POST":
                        break;

                    case "/home" when request.HttpMethod == "GET":
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/html";
                        var file = await File.ReadAllBytesAsync("public/index.html", ctx.Token);
                        await context.Response.OutputStream.WriteAsync(file, ctx.Token);
                        break;

                    case "/weather" when request?.HttpMethod == "GET":

                        var query = request.Url?.Query.Split("?")[^1].Split("&") ?? ["0", "0"];
                        var output = weatherApiClient.GetWeatherAsync(ctx.Token, query[0], query[1]);

                        Console.WriteLine(output.Result.visibility);

                        response.StatusCode = 200;
                        response.ContentType = "application/json";

                        await response.OutputStream.WriteAsync(
                            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(output, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            })), ctx.Token);

                        break;

                    default:
                        await ShowResourseFile(context, ctx.Token);
                        break;
                }
                response.OutputStream.Close();
                response.Close();
            });
        }
        httpListener.Stop();
        httpListener.Close();

        static async Task ShowResourseFile(HttpListenerContext context, CancellationToken token)
        {
            if (context.Request.Url is null)
            {
                context.Response.StatusCode = 404;
                return;
            }
            var path = context.Request.Url.LocalPath.Split('/')[^1];

            context.Response.StatusCode = 200;
            context.Response.ContentType = path.Split('.')[^1] switch
            {
                "html" => "text/html",
                "css" => "text/css",
                "js" => "text/javascript",
                "svg" => "image/svg+xml",
                _ => throw new ArgumentOutOfRangeException()
            };

            var file = await File.ReadAllBytesAsync($"public/{path}", token);
            await context.Response.OutputStream.WriteAsync(file, token);
        }

        

        async Task<User?> SaveUser(HttpListenerContext context, CancellationToken token)
        {   //читаем входящий поток байтов
            using var sr = new StreamReader(context.Request.InputStream);
            var jsonString= await sr.ReadToEndAsync(token);
            Console.WriteLine(jsonString);
            //десериализуем в UserLoginModel;
            var userLoginModel = JsonSerializer.Deserialize<UserLoginModel>
                (jsonString, 
                new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

            var userValidatorRule = new UserValidationRules();
            var userValidationRulesExample = userValidatorRule.Validate(userLoginModel);

            if (!userValidationRulesExample.IsValid)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                context.Response.ContentEncoding = Encoding.UTF8;

                await context.Response.OutputStream.WriteAsync(
                Encoding.UTF8.GetBytes($"Ваши данные не прошли проверку! Информация по ошибке: " +
                                       $"{string.Join("\n\r", userValidationRulesExample.Errors.Select(er => er.ErrorMessage))}"),
                token);
                return null;
            }

            userLoginModel!.Password = MyPasswordHasher.Hash(userLoginModel.Password);
            var user = await dbContext.CreateUser(userLoginModel!.Login, userLoginModel.Password, token);
            Console.WriteLine($"Login:{user.Login}, Password:{user.Password}");
            return user;

        }

        /*async Task Login(HttpListenerContext context, CancellationToken cancellationToken)
        {
            using var sr= new StreamReader(context.Request.InputStream);
            var userLoginModel = JsonSerializer.Deserialize<UserLoginModel>(
                await sr.ReadToEndAsync(cancellationToken).ConfigureAwait(false),
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive= true
                }
                );// Достаем из инпут потока логин пользователя, десириализуем его в экземпляр 
            if (userLoginModel == null)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync("Не переданы данны логина и пароля"u8.ToArray(), cancellationToken)
                    .ConfigureAwait(false);
                return;
                //обработка, если такого пользователя нет
            }

            var user = await dbContext.GetUser(userLoginModel!.Login, cancellationToken);// достаем пользователя из бд по логину
            if (user == null)// если нет такого пльзователя, обработка 
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes($"Не найден пользователь с логином {userLoginModel.Login}"),
                    cancellationToken).ConfigureAwait(false);
                return;
            }

            if (!MyPasswordHasher.Validate(user.Password, userLoginModel.Password))
            {
                context.Response.StatusCode = 203;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Пароль не верный"));

            }//сравниваются пароли из бд и из request

            var sessionId = Guid.NewGuid().ToString();
            LocalCache.Cache.Add(sessionId, new CacheItem
            {
                UserId = user.Id,
                Role=user.Role,
            });

            context.Response.StatusCode = 200;
            context.Response.ContentType = "txt/html";
            context.Response.Headers["Set-Cookie"]= $"sessionId={sessionId};path=/;expires={DateTime.Now.AddHours(10)}";
        }*/

        async Task<CacheItem?> Autorization(HttpListenerContext context, CancellationToken cancellationToken)
        {
            var cookie = context.Request.Headers["Cookie"];
            if (cookie == null)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "txt/html";
                context.Response.OutputStream.WriteAsync("Вы давно не использовали сайт, перезайдите заново"u8.ToArray(), cancellationToken).ConfigureAwait(false);
                return null;
            }

            var session = cookie.Split(";")
                .FirstOrDefault(x => x.Contains("id="))?.Split('=');
            if (session!=null && LocalCache.Cache.TryGetValue(session[1], out var cacheItem))
            {
                return cacheItem;
            }

            context.Response.ContentType = "txt/html";
            context.Response.ContentEncoding = Encoding.UTF8;

            await context.Response.OutputStream.WriteAsync(
                "Вы давно не использовали сайт, повторно войдите!"u8.ToArray(),
                cancellationToken).ConfigureAwait(false);

            return null;



        }
    }
}

