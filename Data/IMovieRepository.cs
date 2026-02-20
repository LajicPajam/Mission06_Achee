using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Data;

public interface IMovieRepository
{
    // Read operations.
    List<Movie> GetMovies();
    Movie? GetMovieById(int id);
    List<Category> GetCategories();

    // Write operations.
    void AddMovie(Movie movie);
    void UpdateMovie(Movie movie);
    void DeleteMovie(int id);
}
