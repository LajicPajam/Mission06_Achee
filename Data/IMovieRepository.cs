using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Data;

public interface IMovieRepository
{
    // Returns all movies shown in the main table.
    List<Movie> GetMovies();
    // Returns one movie for edit/delete pages.
    Movie? GetMovieById(int id);
    // Returns categories used by the form dropdown.
    List<Category> GetCategories();

    // Persists a new movie row.
    void AddMovie(Movie movie);
    // Saves changes to an existing movie row.
    void UpdateMovie(Movie movie);
    // Removes a movie row permanently.
    void DeleteMovie(int id);
}
