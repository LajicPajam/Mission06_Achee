using System.Diagnostics;
using System.Text.Json;
using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Data;

public class SqliteMovieRepository(IWebHostEnvironment env) : IMovieRepository
{
    // Keep the SQLite file in App_Data so it ships with the project and works on any machine.
    private readonly string _dbPath = Path.Combine(env.ContentRootPath, "App_Data", "movies.db");

    public List<Movie> GetMovies()
    {
        // Load the full collection used by the list page.
        const string sql = """
            SELECT MovieId, CategoryId, Title, Year, Director, Rating, Edited, LentTo, CopiedToPlex, Notes
            FROM Movies
            ORDER BY Title;
            """;

        return RunJsonQuery(sql).Select(ParseMovie).ToList();
    }

    public Movie? GetMovieById(int id)
    {
        // Load one record by ID for edit and delete screens.
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
        // Category options displayed in the create/edit dropdown.
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
        // Build and run an INSERT statement from the posted form values.
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
        // Update the matching row with the latest form values.
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
        // Delete the row after the user confirms on the delete screen.
        RunNonQuery($"DELETE FROM Movies WHERE MovieId = {id};");
    }

    private IEnumerable<JsonElement> RunJsonQuery(string sql)
    {
        // Execute a SELECT and parse sqlite's JSON output so we can map rows strongly.
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
        // Shared executor for write statements that do not return result sets.
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
        // Convert one JSON row into the Movie object used by controllers/views.
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
        // Defensive parse: if a numeric field is missing/null, default to 0.
        return row.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetInt32()
            : 0;
    }

    private static int? ReadNullableInt(JsonElement row, string propertyName)
    {
        // CategoryId is nullable, so null should stay null instead of becoming 0.
        if (!row.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.GetInt32();
    }

    private static string? ReadString(JsonElement row, string propertyName)
    {
        // Convert SQL NULL to C# null for optional text fields.
        if (!row.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.GetString();
    }

    // SQLite stores booleans as integers, so true/false becomes 1/0.
    private static int BoolToInt(bool value) => value ? 1 : 0;

    private static string NullableInt(int? value) => value.HasValue ? value.Value.ToString() : "NULL";

    // Optional text fields are stored as NULL when blank.
    private static string NullableText(string? value) => string.IsNullOrWhiteSpace(value) ? "NULL" : Quoted(value);

    // Escape apostrophes so titles like Schindler's List do not break SQL.
    private static string Quoted(string value) => $"'{value.Replace("'", "''")}'";
}
