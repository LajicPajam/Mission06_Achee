using System.Diagnostics;
using System.Text.Json;
using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Data;

public class SqliteMovieRepository(IWebHostEnvironment env) : IMovieRepository
{
    private readonly string _dbPath = Path.Combine(env.ContentRootPath, "App_Data", "movies.db");

    public List<Movie> GetMovies()
    {
        const string sql = """
            SELECT MovieId, CategoryId, Title, Year, Director, Rating, Edited, LentTo, CopiedToPlex, Notes
            FROM Movies
            ORDER BY Title;
            """;

        return RunJsonQuery(sql).Select(ParseMovie).ToList();
    }

    public Movie? GetMovieById(int id)
    {
        var sql = $"""
            SELECT MovieId, CategoryId, Title, Year, Director, Rating, Edited, LentTo, CopiedToPlex, Notes
            FROM Movies
            WHERE MovieId = {id}
            LIMIT 1;
            """;

        return RunJsonQuery(sql).Select(ParseMovie).FirstOrDefault();
    }

    public List<Category> GetCategories()
    {
        const string sql = """
            SELECT CategoryId, CategoryName
            FROM Categories
            ORDER BY CategoryName;
            """;

        return RunJsonQuery(sql)
            .Select(row => new Category
            {
                CategoryId = ReadInt(row, "CategoryId"),
                CategoryName = ReadString(row, "CategoryName") ?? string.Empty
            })
            .ToList();
    }

    public void AddMovie(Movie movie)
    {
        var sql = $"""
            INSERT INTO Movies (CategoryId, Title, Year, Director, Rating, Edited, LentTo, CopiedToPlex, Notes)
            VALUES (
                {NullableInt(movie.CategoryId)},
                {Quoted(movie.Title)},
                {movie.Year},
                {NullableText(movie.Director)},
                {NullableText(movie.Rating)},
                {BoolToInt(movie.Edited)},
                {NullableText(movie.LentTo)},
                {BoolToInt(movie.CopiedToPlex)},
                {NullableText(movie.Notes)}
            );
            """;

        RunNonQuery(sql);
    }

    public void UpdateMovie(Movie movie)
    {
        var sql = $"""
            UPDATE Movies
            SET CategoryId = {NullableInt(movie.CategoryId)},
                Title = {Quoted(movie.Title)},
                Year = {movie.Year},
                Director = {NullableText(movie.Director)},
                Rating = {NullableText(movie.Rating)},
                Edited = {BoolToInt(movie.Edited)},
                LentTo = {NullableText(movie.LentTo)},
                CopiedToPlex = {BoolToInt(movie.CopiedToPlex)},
                Notes = {NullableText(movie.Notes)}
            WHERE MovieId = {movie.MovieId};
            """;

        RunNonQuery(sql);
    }

    public void DeleteMovie(int id)
    {
        RunNonQuery($"DELETE FROM Movies WHERE MovieId = {id};");
    }

    private IEnumerable<JsonElement> RunJsonQuery(string sql)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sqlite3",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.StartInfo.ArgumentList.Add("-json");
        process.StartInfo.ArgumentList.Add(_dbPath);
        process.StartInfo.ArgumentList.Add(sql);

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"SQLite query failed: {error}");
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            return Enumerable.Empty<JsonElement>();
        }

        using var doc = JsonDocument.Parse(output);
        return doc.RootElement.EnumerateArray().Select(el => el.Clone()).ToList();
    }

    private void RunNonQuery(string sql)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sqlite3",
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.StartInfo.ArgumentList.Add(_dbPath);
        process.StartInfo.ArgumentList.Add(sql);

        process.Start();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"SQLite update failed: {error}");
        }
    }

    private static Movie ParseMovie(JsonElement row)
    {
        return new Movie
        {
            MovieId = ReadInt(row, "MovieId"),
            CategoryId = ReadNullableInt(row, "CategoryId"),
            Title = ReadString(row, "Title") ?? string.Empty,
            Year = ReadInt(row, "Year"),
            Director = ReadString(row, "Director"),
            Rating = ReadString(row, "Rating"),
            Edited = ReadInt(row, "Edited") == 1,
            LentTo = ReadString(row, "LentTo"),
            CopiedToPlex = ReadInt(row, "CopiedToPlex") == 1,
            Notes = ReadString(row, "Notes")
        };
    }

    private static int ReadInt(JsonElement row, string propertyName)
    {
        return row.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetInt32()
            : 0;
    }

    private static int? ReadNullableInt(JsonElement row, string propertyName)
    {
        if (!row.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.GetInt32();
    }

    private static string? ReadString(JsonElement row, string propertyName)
    {
        if (!row.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.GetString();
    }

    private static int BoolToInt(bool value) => value ? 1 : 0;

    private static string NullableInt(int? value) => value.HasValue ? value.Value.ToString() : "NULL";

    private static string NullableText(string? value) => string.IsNullOrWhiteSpace(value) ? "NULL" : Quoted(value);

    private static string Quoted(string value) => $"'{value.Replace("'", "''")}'";
}
