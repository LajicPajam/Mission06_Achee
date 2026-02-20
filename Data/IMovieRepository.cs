using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Data;

public interface IMovieRepository
{
    List<Movie> GetMovies();
    Movie? GetMovieById(int id);
    List<Category> GetCategories();
    void AddMovie(Movie movie);
    void UpdateMovie(Movie movie);
    void DeleteMovie(int id);
}
