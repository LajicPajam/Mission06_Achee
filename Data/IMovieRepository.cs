using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Data;

public interface IMovieRepository
{
    void InitializeDatabase();
    void SeedFavoriteMovies();
    void AddMovie(Movie movie);
}
