// todo: CODE STRUCTURE - restructure code into separate files or classes to improve maintainability and readability. (example: create service classes responsible for handling user registration, authentication, blog post creation..)
using System.Data.SQLite;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// todo: JWT AUTHENTICATION - secret key is hardcoded here, which is insecure. It is better to use environment variables or configuration files for storing sensitive information.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            Validate ()suer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            ValidIssuer = "issuer",
            ValidAudience = "audience",
            IssuerSigningKey = new SymmetricSecurityKey("s3scret"u8.ToArray()),
        };
    });


var app = builder.Build();

app.MapPost("/register", Register);
app.MapPost("/login", Login);
app.MapPost("/new_blog_post", NewBlogPost);
app.MapGet("/blog", Blog);
app.MapGet("/", Hello);

app.Run();

void Register(HttpContext context)
{
    var con = new SQLiteConnection("Data Source=users.db");
    con.Open();

    // todo: CODE DUPLICATION - table creation and database connection commands are repeated in multiple endpoints. These database-related operations should be abstracted into separate methods to avoid duplication and improve maintainability.
    // todo: ERROR HANDLING - there is no proper error handling in the code. If any database operation fails, it will throw an exception which will result in error 500 (Internal Server Error). It's important to provide meaningful error messages.
    var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS users(id INTEGER PRIMARY KEY, username TEXT, password TEXT);", con);
    cmd.ExecuteNonQuery();

    var j = JsonDocument.Parse(context.Request.BodyReader.AsStream()).RootElement;

    var selectCmd = new SQLiteCommand("SELECT * FROM users ORDER BY id DESC LIMIT 1;", con);
    var res = selectCmd.ExecuteReader();
    var id = res.Read() ? res.GetInt32(0) : 0;

    // todo: SQL INJECTION VULNERABILITY - It is better to use parameterized queries instead of directly interpolating user inputs into SQL queries (to prevent possible SQL injection attacks)
    var insertCmd = new SQLiteCommand($"INSERT INTO users VALUES({id + 1}, '{j.GetProperty("username").GetString()}', '{j.GetProperty("password").GetString()}');", con);
    insertCmd.ExecuteNonQuery();
}

void Login(HttpContext context)
{
    var con = new SQLiteConnection("Data Source=users.db");
    con.Open();

    var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS users(id INTEGER PRIMARY KEY, username TEXT, password TEXT);", con);
    cmd.ExecuteNonQuery();

    var j = JsonDocument.Parse(context.Request.BodyReader.AsStream()).RootElement;

    var selectCmd = new SQLiteCommand($"SELECT * FROM users WHERE username='{j.GetProperty("username").GetString()}' AND password='{j.GetProperty("password").GetString()}';", con);
    var res = selectCmd.ExecuteReader();

    if (!res.Read())
    {
        context.Response.StatusCode = 404;
        context.Response.WriteAsync("No such user or password incorrect!");
        return;
    }

    var id = res.GetInt32(0);
    context.Response.WriteAsync($"{{\"id\": {id}}}");
}

void NewBlogPost(HttpContext context)
{
    var con = new SQLiteConnection("Data Source=users.db");
    con.Open();

    var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS blog_posts(id INTEGER PRIMARY KEY, user_id INTEGER, content TEXT);", con);
    cmd.ExecuteNonQuery();

    var j = JsonDocument.Parse(context.Request.BodyReader.AsStream()).RootElement;

    var selectCmd = new SQLiteCommand("SELECT * FROM blog_posts ORDER BY id DESC LIMIT 1;", con);
    var res = selectCmd.ExecuteReader();
    var id = res.Read() ? res.GetInt32(0) : 0;

    var insertCmd = new SQLiteCommand($"INSERT INTO blog_posts VALUES({id + 1}, {j.GetProperty("user_id").GetInt32()}, '{j.GetProperty("content").GetString()}');", con);
    insertCmd.ExecuteNonQuery();
}

void Blog(HttpContext context)
{
    var con = new SQLiteConnection("Data Source=users.db");
    con.Open();

    var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS blog_posts(id INTEGER PRIMARY KEY, user_id INTEGER, content TEXT);", con);
    cmd.ExecuteNonQuery();

    var selectCmd = new SQLiteCommand("SELECT * FROM blog_posts;", con);
    var res = selectCmd.ExecuteReader();

    var posts = new List<string>();
    while (res.Read())
    {
        posts.Add(res.GetString(2));
    }

    context.Response.WriteAsync(string.Join("\n", posts));
}

void Hello(HttpContext context)
{
    context.Response.WriteAsync("<p>Hello, World!</p>");
}