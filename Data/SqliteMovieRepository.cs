using System.Diagnostics;
using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Data;

public class SqliteMovieRepository(IWebHostEnvironment env) : IMovieRepository
{
    // Store the app database inside App_Data so it lives with the project files.
    private readonly string _dbPath = Path.Combine(env.ContentRootPath, "App_Data", "movies.db");

    public void InitializeDatabase()
    {
        // Ensure App_Data exists before sqlite tries to create or open the database file.
        var dbDirectory = Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrWhiteSpace(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        // Create the Movies table the first time the app runs.
        ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS Movies (
                MovieId INTEGER PRIMARY KEY AUTOINCREMENT,
                Category TEXT NOT NULL,
                Title TEXT NOT NULL,
                Year TEXT NOT NULL,
                Director TEXT NOT NULL,
                Rating TEXT NOT NULL,
                Edited INTEGER NULL,
                LentTo TEXT NULL,
                Notes TEXT NULL
            );");
    }

    public void SeedFavoriteMovies()
    {
        // If rows already exist, skip seeding so we do not duplicate starter data.
        var existingCount = ExecuteScalarInt("SELECT COUNT(*) FROM Movies;");

        if (existingCount > 0)
        {
            return;
        }

        // Starter records used for initial demo data in Mission 06.
        var favorites = new List<Movie>
        {
            new()
            {
                Category = "Action/Adventure",
                Title = "The Lord of the Rings: The Fellowship of the Ring",
                Year = "2001",
                Director = "Peter Jackson",
                Rating = "PG-13",
                Edited = false,
                Notes = "Extended edition"
            },
            new()
            {
                Category = "Comedy",
                Title = "The Princess Bride",
                Year = "1987",
                Director = "Rob Reiner",
                Rating = "PG",
                Edited = null,
                Notes = "As you wish"
            },
            new()
            {
                Category = "Family",
                Title = "Spider-Man: Into the Spider-Verse",
                Year = "2018",
                Director = "Peter Ramsey",
                Rating = "PG",
                Edited = false,
                Notes = "Great animation"
            }
        };

        foreach (var movie in favorites)
        {
            AddMovie(movie);
        }
    }

    public void AddMovie(Movie movie)
    {
        // SQLite stores nullable booleans as 1, 0, or NULL.
        var editedValue = movie.Edited.HasValue ? (movie.Edited.Value ? "1" : "0") : "NULL";

        var sql = $@"
            INSERT INTO Movies (Category, Title, Year, Director, Rating, Edited, LentTo, Notes)
            VALUES (
                {SqlValue(movie.Category)},
                {SqlValue(movie.Title)},
                {SqlValue(movie.Year)},
                {SqlValue(movie.Director)},
                {SqlValue(movie.Rating)},
                {editedValue},
                {SqlValue(movie.LentTo)},
                {SqlValue(movie.Notes)}
            );";

        ExecuteNonQuery(sql);
    }

    private int ExecuteScalarInt(string sql)
    {
        // Run a query like COUNT(*) and convert the result to an int.
        var output = ExecuteSql(sql).Trim();
        return int.TryParse(output, out var result) ? result : 0;
    }

    private void ExecuteNonQuery(string sql)
    {
        ExecuteSql(sql);
    }

    private string ExecuteSql(string sql)
    {
        // Execute sqlite3 as a process so no extra DB package is required.
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sqlite3",
                ArgumentList = { _dbPath, sql },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            // Bubble up SQL errors with context to make debugging easier.
            throw new InvalidOperationException($"SQLite command failed: {error}");
        }

        return output;
    }

    private static string SqlValue(string? value)
    {
        // Write blank strings as SQL NULL for optional fields.
        if (string.IsNullOrWhiteSpace(value))
        {
            return "NULL";
        }

        // Escape apostrophes so values like O'Brien stay valid SQL literals.
        return $"'{value.Replace("'", "''")}'";
    }
}
